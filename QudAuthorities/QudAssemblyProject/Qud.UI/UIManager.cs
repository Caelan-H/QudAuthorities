using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[HasModSensitiveStaticCache]
public class UIManager : MonoBehaviour
{
	private Array allKeyCodes;

	public List<WindowBase> windows = new List<WindowBase>();

	public static UIManager instance;

	public Dictionary<string, WindowBase> windowsByName = new Dictionary<string, WindowBase>();

	public WindowBase currentWindow;

	private Stack<WindowBase> windowStack = new Stack<WindowBase>();

	private CanvasScaler _scaler;

	public static int _WindowFramePin;

	public static bool initComplete;

	public static bool UseNewPopups
	{
		get
		{
			if (!Options.ModernUI)
			{
				return Options.OverlayUI;
			}
			return true;
		}
	}

	public static float width
	{
		get
		{
			if (instance?.gameObject == null)
			{
				return 1920f;
			}
			return instance?.gameObject?.GetComponent<RectTransform>()?.sizeDelta.x ?? 1920f;
		}
	}

	public static float height
	{
		get
		{
			if (instance?.gameObject == null)
			{
				return 1080f;
			}
			return instance?.gameObject?.GetComponent<RectTransform>()?.sizeDelta.y ?? 1080f;
		}
	}

	public static float scale => instance._scaler.scaleFactor;

	public static int WindowFramePin
	{
		get
		{
			return _WindowFramePin;
		}
		set
		{
			_WindowFramePin = value;
			PlayerPrefs.SetInt("_WindowFramePin", value);
		}
	}

	public void DestroyWindow(WindowBase window)
	{
		if (!(window == null))
		{
			windowsByName.Where((KeyValuePair<string, WindowBase> kv) => kv.Value == window).ToList().ForEach(delegate(KeyValuePair<string, WindowBase> kv)
			{
				windowsByName.Remove(kv.Key);
			});
			windows.Remove(window);
			UnityEngine.Object.Destroy(window.gameObject);
		}
	}

	public Vector2 WorldspaceToCanvasSpace(Vector3 WorldSpace, Camera camera)
	{
		if (camera == null)
		{
			return Vector2.zero;
		}
		return camera.WorldToScreenPoint(WorldSpace);
	}

	public bool AllowPassthroughInput()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].Visible && !windows[i].AllowPassthroughInput())
			{
				return false;
			}
		}
		return true;
	}

	public static void showWindow(string name, bool aggressive = false)
	{
		if (instance.currentWindow != null && instance.currentWindow.name == name)
		{
			return;
		}
		if (aggressive)
		{
			foreach (WindowBase window in instance.windows)
			{
				window._HideWithoutLeave();
			}
		}
		instance._showWindow(name);
	}

	public static T getWindow<T>(string name) where T : WindowBase
	{
		return instance.windowsByName[name] as T;
	}

	public static WindowBase getWindow(string name)
	{
		if (instance == null)
		{
			return null;
		}
		if (instance.windowsByName == null)
		{
			return null;
		}
		if (!instance.windowsByName.ContainsKey(name))
		{
			return null;
		}
		return instance.windowsByName[name];
	}

	public static WindowBase createWindow(string name, Type scriptType = null, Transform parent = null)
	{
		if (scriptType == null)
		{
			scriptType = typeof(WindowBase);
		}
		if (parent == null)
		{
			parent = instance.transform;
		}
		GameObject obj = new GameObject();
		obj.SetActive(value: false);
		obj.AddComponent(scriptType);
		obj.transform.SetParent(parent, worldPositionStays: false);
		obj.transform.SetAsLastSibling();
		RectTransform component = obj.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.sizeDelta = Vector2.zero;
		WindowBase component2 = obj.GetComponent<WindowBase>();
		instance.windowsByName.Set(name, component2);
		return component2;
	}

	public static WindowBase copyWindow(string name)
	{
		WindowBase windowBase = UnityEngine.Object.Instantiate(instance.windowsByName[name]);
		windowBase.transform.SetParent(instance.transform, worldPositionStays: false);
		windowBase.transform.SetAsLastSibling();
		return windowBase;
	}

	public static void pushWindow(string name, bool hideOld = false)
	{
		GameManager.Instance.uiQueue.awaitTask(delegate
		{
			if (instance.currentWindow != null)
			{
				instance.windowStack.Push(instance.currentWindow);
				if (!hideOld)
				{
					instance.currentWindow = null;
				}
			}
			showWindow(name);
		});
	}

	public static void popWindow()
	{
		if (instance.currentWindow != null)
		{
			instance.currentWindow.Hide();
			instance.currentWindow = null;
		}
		if (instance.windowStack.Count > 0)
		{
			instance.currentWindow = instance.windowStack.Pop();
			instance.currentWindow.Show();
		}
	}

	private void _showWindow(string name)
	{
		DebugConsole.WriteLine("[UIManager] showing window " + name);
		if (currentWindow != null)
		{
			currentWindow.Hide();
		}
		if (name != null)
		{
			currentWindow = windowsByName[name];
			currentWindow.Show();
		}
		else
		{
			currentWindow = null;
		}
	}

	public void DiscoverWindows(string basePath, Transform root)
	{
		foreach (Transform item in root)
		{
			WindowBase component = item.gameObject.GetComponent<WindowBase>();
			if (component != null)
			{
				windows.Add(component);
				windowsByName.Add(basePath + component.name, component);
			}
			else if (basePath == "")
			{
				DiscoverWindows(item.name + "/", item.transform);
			}
		}
	}

	public void Init()
	{
		_WindowFramePin = PlayerPrefs.GetInt("_WindowFramePin", 0);
		instance = this;
		_scaler = GetComponent<CanvasScaler>();
		allKeyCodes = Enum.GetValues(typeof(KeyCode));
		windows = new List<WindowBase>();
		windowsByName = new Dictionary<string, WindowBase>();
		DiscoverWindows("", base.gameObject.transform);
		foreach (WindowBase window in windows)
		{
			try
			{
				if (window.GetComponent<CanvasGroup>() == null)
				{
					window.gameObject.AddComponent<CanvasGroup>();
				}
				window.Init();
				window.Hide();
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Exception intializing window " + window.name, x);
			}
		}
		Update();
		initComplete = true;
	}

	[ModSensitiveCacheInit]
	private static void ModReload()
	{
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			if (instance != null)
			{
				foreach (WindowBase window in instance.windows)
				{
					window.Init();
				}
			}
		});
	}

	private void Update()
	{
		if (Options.StageScale != (double)_scaler.scaleFactor)
		{
			_scaler.scaleFactor = (float)Options.StageScale;
			Canvas.ForceUpdateCanvases();
		}
	}
}

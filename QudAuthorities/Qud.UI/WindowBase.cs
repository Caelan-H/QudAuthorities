using System;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;

namespace Qud.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(Canvas))]
public class WindowBase : MonoBehaviour
{
	public CanvasGroup _canvasGroup;

	public Canvas _canvas;

	public GraphicRaycaster _raycaster;

	public bool _takesInput = true;

	private RectTransform _rectTransform;

	public static long gameTimeMS => XRLCore.runTime.ElapsedMilliseconds;

	public bool Visible
	{
		get
		{
			if (base.gameObject == null)
			{
				return false;
			}
			if (canvas == null)
			{
				return false;
			}
			if (base.gameObject.activeInHierarchy)
			{
				return canvas.enabled;
			}
			return false;
		}
	}

	public CanvasGroup canvasGroup
	{
		get
		{
			if (_canvasGroup == null)
			{
				_canvasGroup = base.gameObject.GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
	}

	public Canvas canvas
	{
		get
		{
			if (_canvas == null)
			{
				_canvas = base.gameObject.GetComponent<Canvas>();
			}
			return _canvas;
		}
	}

	public GraphicRaycaster raycaster
	{
		get
		{
			if (_raycaster == null)
			{
				_raycaster = base.gameObject.GetComponent<GraphicRaycaster>();
			}
			return _raycaster;
		}
	}

	public bool takesInput
	{
		get
		{
			return _takesInput;
		}
		set
		{
			_takesInput = value;
			if (!value)
			{
				raycaster.enabled = false;
			}
			else if (canvas.enabled)
			{
				raycaster.enabled = true;
			}
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.gameObject.GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public static void queueGameAction(Action a)
	{
		GameManager.Instance.gameQueue.queueTask(a);
	}

	public static void queueUIAction(Action a)
	{
		GameManager.Instance.uiQueue.queueTask(a);
	}

	public void DestroyWindow()
	{
		UIManager.instance.DestroyWindow(this);
	}

	public void Toggle()
	{
		if (Visible)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public bool isCurrentWindow()
	{
		return UIManager.instance.currentWindow == this;
	}

	public virtual void Init()
	{
	}

	public virtual bool AllowPassthroughInput()
	{
		return false;
	}

	public void _HideWithoutLeave()
	{
		if (canvas.enabled)
		{
			_ = canvas.gameObject.name == "Keybinds";
		}
		if (canvas.enabled)
		{
			canvas.enabled = false;
		}
		if (raycaster.enabled)
		{
			raycaster.enabled = false;
		}
		if (canvasGroup != null && canvasGroup.interactable)
		{
			canvasGroup.interactable = false;
		}
	}

	public virtual void Hide()
	{
		if (canvas.enabled)
		{
			_ = canvas.gameObject.name == "Keybinds";
		}
		if (canvas.enabled)
		{
			canvas.enabled = false;
		}
		if (raycaster.enabled)
		{
			raycaster.enabled = false;
		}
		if (canvasGroup != null && canvasGroup.interactable)
		{
			canvasGroup.interactable = false;
		}
	}

	public virtual void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (!canvas.enabled)
		{
			canvas.enabled = true;
		}
		if (!raycaster.enabled)
		{
			raycaster.enabled = takesInput;
		}
		if (canvasGroup != null && !canvasGroup.interactable)
		{
			canvasGroup.interactable = true;
		}
	}

	public void BringToFront()
	{
		base.transform.SetAsLastSibling();
	}

	public void ActivateAtWidth(float width, GameObject go)
	{
		if (!(go != null))
		{
			return;
		}
		if (rectTransform.rect.width < width)
		{
			if (go.activeSelf)
			{
				go.SetActive(value: false);
			}
		}
		else if (!go.activeSelf)
		{
			go.SetActive(value: true);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL.CharacterBuilds;

namespace XRL.UI.Framework;

public class FrameworkScroller : MonoBehaviour, IFrameworkContext
{
	public InputAxisTypes NavigationAxis;

	public ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = new ScrollContext<FrameworkDataElement, NavigationContext>();

	public List<FrameworkDataElement> choices = new List<FrameworkDataElement>();

	public bool EnablePaging;

	public GameObject childRoot;

	public FrameworkUnityScrollChild selectionPrefab;

	public LayoutElement contentFitterLayoutElement;

	public GameObject spacerPrefab;

	public UITextSkin titleText;

	public UnityEvent<FrameworkDataElement> onSelected = new UnityEvent<FrameworkDataElement>();

	public UnityEvent<FrameworkDataElement> onHighlight = new UnityEvent<FrameworkDataElement>();

	public int selectedPosition;

	public bool autoHotkey;

	public Dictionary<KeyCode, FrameworkDataElement> hotkeyChoices = new Dictionary<KeyCode, FrameworkDataElement>();

	public ScrollRect scrollRect;

	private GridLayoutGroup _gridLayout;

	public List<FrameworkUnityScrollChild> selectionClones = new List<FrameworkUnityScrollChild>();

	public List<GameObject> spacerClones = new List<GameObject>();

	private List<NavigationContext> lastContexts = new List<NavigationContext>();

	private ScrollViewCalcs scrollCalcs;

	public float lastWidth;

	private RectTransform _rtWidth;

	public bool wasActive = true;

	private bool FirstLateUpdate = true;

	public bool centeringEnabled = true;

	public RectTransform centeringFreeSpace;

	public RectTransform centeringFrame;

	public RectTransform centeringLowestObject;

	public float lastHeight = float.NaN;

	private LayoutElement _layout;

	private RectTransform _contentFitterLayoutElementRectTransform;

	private ScrollViewCalcs scrollViewCalcs = new ScrollViewCalcs();

	public GridLayoutGroup gridLayout
	{
		get
		{
			if (_gridLayout == null)
			{
				_gridLayout = childRoot.GetComponent<GridLayoutGroup>();
			}
			return _gridLayout;
		}
	}

	public InputAxisTypes PageAxis
	{
		get
		{
			if (NavigationAxis == InputAxisTypes.NavigationXAxis)
			{
				return InputAxisTypes.NavigationPageXAxis;
			}
			if (NavigationAxis == InputAxisTypes.NavigationYAxis)
			{
				return InputAxisTypes.NavigationPageYAxis;
			}
			throw new Exception("Unhandled page axis");
		}
	}

	private RectTransform rtWidth => _rtWidth ?? (_rtWidth = scrollRect?.transform.parent?.GetComponent<RectTransform>());

	private LayoutElement layout => _layout ?? (_layout = scrollRect.GetComponent<LayoutElement>());

	private RectTransform contentFitterLayoutElementRectTransform => _contentFitterLayoutElementRectTransform ?? (_contentFitterLayoutElementRectTransform = contentFitterLayoutElement.GetComponent<RectTransform>());

	public virtual NavigationContext GetNavigationContext()
	{
		return scrollContext;
	}

	public void Start()
	{
		selectedPosition = 0;
		if (scrollContext != null)
		{
			scrollContext.data = choices;
		}
	}

	public FrameworkUnityScrollChild GetPrefabForIndex(int x)
	{
		while (selectionClones.Count <= x)
		{
			selectionClones.Add(UnityEngine.Object.Instantiate(selectionPrefab));
		}
		return selectionClones[x];
	}

	public virtual void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		hotkeyChoices.Clear();
		if (titleText != null && descriptor != null)
		{
			titleText.SetText(descriptor.title);
		}
		scrollContext.wraps = false;
		if (selections == null)
		{
			selections = choices;
		}
		choices = (scrollContext.data = new List<FrameworkDataElement>(selections));
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in childRoot.transform)
		{
			if (!selectionClones.Contains(item.GetComponent<FrameworkUnityScrollChild>()) && !spacerClones.Contains(item.gameObject))
			{
				list.Add(item.gameObject);
			}
		}
		if (selectionClones.Count > choices.Count)
		{
			list.AddRange(from clone in selectionClones.Skip(choices.Count)
				select clone.gameObject);
			selectionClones.RemoveRange(choices.Count, selectionClones.Count - choices.Count);
			if (spacerPrefab != null)
			{
				List<GameObject> list2 = spacerClones.Skip(choices.Count - 1).ToList();
				list.AddRange(list2);
				foreach (GameObject item2 in list2)
				{
					spacerClones.Remove(item2);
				}
			}
		}
		list.ForEach(delegate(GameObject child)
		{
			try
			{
				child.DestroyImmediate();
			}
			catch
			{
			}
		});
		scrollContext.SetAxis(NavigationAxis);
		if (EnablePaging)
		{
			scrollContext.axisHandlers.Set(PageAxis, Event.Helpers.Handle(Event.Helpers.Axis(DoPageDown, DoPageUp)));
		}
		lastContexts.Clear();
		lastContexts.AddRange(scrollContext.contexts);
		scrollContext.contexts.Clear();
		if (autoHotkey && CapabilityManager.AllowKeyboardHotkeys)
		{
			List<KeyCode>.Enumerator enumerator3 = ControlManager.GetHotkeySpread(new string[2] { "*default", "Chargen" }).GetEnumerator();
			foreach (IFrameworkDataHotkey item3 in from choice in choices.SelectMany(ChoiceAndChildren)
				select choice as IFrameworkDataHotkey)
			{
				if (item3 != null)
				{
					if (enumerator3.MoveNext())
					{
						KeyCode current3 = enumerator3.Current;
						item3.Hotkey = "[{{W|" + current3.ToString() + "}}]";
						hotkeyChoices.Add(current3, item3 as FrameworkDataElement);
					}
					else
					{
						item3.Hotkey = "";
					}
				}
			}
		}
		foreach (FrameworkDataElement choice in choices)
		{
			int count = scrollContext.contexts.Count;
			if (count > 0 && spacerPrefab != null && spacerClones.Count < count)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(spacerPrefab);
				gameObject.transform.SetParent(childRoot.transform, worldPositionStays: false);
				spacerClones.Add(gameObject);
			}
			ScrollChildContext scrollChildContext = MakeContextFor(choice, count);
			scrollContext.contexts.Add(scrollChildContext);
			NavigationContext navigationContext = lastContexts.ElementAtOrDefault(count);
			if (navigationContext != null && navigationContext.IsActive())
			{
				scrollChildContext.Activate();
			}
			FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(count);
			prefabForIndex.transform.SetParent(childRoot.transform, worldPositionStays: false);
			SetupPrefab(prefabForIndex, scrollChildContext, choice, count);
		}
		scrollContext.Setup();
		Canvas.ForceUpdateCanvases();
	}

	public void DoPageUp()
	{
		MetricsManager.LogEditorInfo("Page up");
		scrollRect.verticalNormalizedPosition += scrollRect.viewport.rect.height / scrollRect.content.rect.height;
		scrollContext.SelectIndex(selectionClones.FindIndex((FrameworkUnityScrollChild c) => ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView));
	}

	public void DoPageDown()
	{
		MetricsManager.LogEditorInfo("Page down");
		scrollRect.verticalNormalizedPosition -= scrollRect.viewport.rect.height / scrollRect.content.rect.height;
		scrollContext.SelectIndex(selectionClones.FindIndex((FrameworkUnityScrollChild c) => ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView));
	}

	public virtual void RefreshDatas()
	{
		for (int i = 0; i < choices.Count; i++)
		{
			FrameworkDataElement data = choices[i];
			FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(i);
			ScrollChildContext context = scrollContext.contexts[i] as ScrollChildContext;
			SetupPrefab(prefabForIndex, context, data, i);
		}
	}

	public virtual void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		newChild.scrollContext = context;
		IFrameworkControl component = newChild.GetComponent<IFrameworkControl>();
		if (component == null)
		{
			MetricsManager.LogError("The control element " + newChild.name + " doesn't have an IFrameworkControl component. It needs one to recieve the framework data.");
		}
		else
		{
			component.setData(data);
		}
	}

	public virtual ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return new ScrollChildContext
		{
			buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				Event.Helpers.Handle(delegate
				{
					onSelected?.Invoke(data);
				})
			} }
		};
	}

	public virtual void UpdateSelection()
	{
		ScrollSelectedIntoView();
		onHighlight?.Invoke(scrollContext.data[selectedPosition]);
	}

	public virtual void UpdateWidth()
	{
	}

	public virtual void LateUpdate()
	{
		if (FirstLateUpdate)
		{
			Canvas.ForceUpdateCanvases();
			UpdateLayout();
			Canvas.ForceUpdateCanvases();
			FirstLateUpdate = false;
		}
	}

	private void UpdateLayout()
	{
		scrollContext.IsActive();
		if (rtWidth != null && rtWidth?.rect.width != lastWidth)
		{
			lastWidth = rtWidth?.rect.width ?? 0f;
			UpdateWidth();
		}
		if (scrollRect != null && gridLayout != null)
		{
			if (layout.preferredHeight != gridLayout.preferredHeight + 10f)
			{
				layout.preferredHeight = gridLayout.preferredHeight + 10f + (float)(autoHotkey ? 16 : 0);
			}
		}
		else if (scrollRect != null && contentFitterLayoutElement != null)
		{
			float num = contentFitterLayoutElementRectTransform.rect.height + 10f + (float)(autoHotkey ? 16 : 0);
			float num2 = contentFitterLayoutElementRectTransform.rect.width + 10f;
			if (layout.preferredHeight != num)
			{
				layout.preferredHeight = num;
			}
			if (layout.preferredWidth != num2)
			{
				layout.preferredWidth = num2;
			}
		}
		UpdateCentering();
	}

	public virtual void Update()
	{
		bool flag = scrollContext.IsActive();
		if (flag != wasActive || (flag && scrollContext.selectedPosition != selectedPosition))
		{
			wasActive = flag;
			selectedPosition = scrollContext.selectedPosition;
			if (flag)
			{
				UpdateSelection();
			}
		}
		UpdateLayout();
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<KeyCode, FrameworkDataElement> hotkeyChoice in hotkeyChoices)
		{
			if (ControlManager.isKeyDown(hotkeyChoice.Key))
			{
				if (SynchronizationContext.Current == The.UiContext)
				{
					Input.ResetInputAxes();
				}
				onSelected?.Invoke(hotkeyChoice.Value);
				break;
			}
		}
	}

	public void Awake()
	{
	}

	public void UpdateCentering()
	{
		if (centeringEnabled && centeringFreeSpace != null && centeringFrame != null && centeringFreeSpace.rect.y != lastHeight)
		{
			Canvas.ForceUpdateCanvases();
			lastHeight = centeringFreeSpace.rect.y;
			float num = Math.Min(centeringFreeSpace.rect.height, Math.Max(0f, centeringFrame.rect.height / 2f - 140f));
			centeringFrame.anchoredPosition = new Vector2(centeringFrame.anchoredPosition.x, 0f - num);
			Canvas.ForceUpdateCanvases();
		}
	}

	public virtual void ScrollSelectedIntoView()
	{
		ScrollViewCalcs.ScrollIntoView(GetPrefabForIndex(selectedPosition).GetComponent<RectTransform>(), scrollViewCalcs);
	}

	public IEnumerable<FrameworkDataElement> ChoiceAndChildren(FrameworkDataElement choice)
	{
		yield return choice;
		IEnumerable<FrameworkDataElement> enumerable = (choice as IFrameworkDataList)?.getChildren().SelectMany(ChoiceAndChildren);
		if (enumerable == null)
		{
			yield break;
		}
		foreach (FrameworkDataElement item in enumerable)
		{
			yield return item;
		}
	}
}

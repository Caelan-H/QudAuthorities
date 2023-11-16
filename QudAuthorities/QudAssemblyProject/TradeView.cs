using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("TradePrerelease", false, true, false, "Trade,Menu", "Trade", false, 0, false)]
public class TradeView : BaseView
{
	public BaseInventoryView list1 = new BaseInventoryView();

	public BaseInventoryView list2 = new BaseInventoryView();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public static TradeView instance;

	public override void OnCreate()
	{
		list1.filterBar = LegacyViewManager.Instance.Views["Trade"].rootObject.GetComponent<TradeViewBehaviour>().filterBar1;
		list1.filterBar.inventory = list1;
		list1.idInventoryContent = "Panel/TraderLeft Panel/Inventory Scroll/Viewport/Content";
		list1.idInventoryScrollView = "Panel/TraderLeft Panel/Inventory Scroll";
		list2.filterBar = LegacyViewManager.Instance.Views["Trade"].rootObject.GetComponent<TradeViewBehaviour>().filterBar2;
		list2.filterBar.inventory = list2;
		list2.idInventoryContent = "Panel/TraderRight Panel/Inventory Scroll/Viewport/Content";
		list2.idInventoryScrollView = "Panel/TraderRight Panel/Inventory Scroll";
		instance = this;
		list1.OnCreate();
		list2.OnCreate();
	}

	public void UpdateObjectList(QudItemList newList, BaseInventoryView listView)
	{
		int num = -1;
		int num2 = -1;
		if (base.EventSystemManager.currentSelectedGameObject != null && listView.InventoryButtons.Contains(base.EventSystemManager.currentSelectedGameObject))
		{
			num = listView.InventoryButtons.IndexOf(base.EventSystemManager.currentSelectedGameObject);
		}
		if (num == -1 && num2 == -1)
		{
			num = 1;
		}
		string text = null;
		if (listView.filterBar.toggledButton != null)
		{
			text = listView.filterBar.toggledButton.category;
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>();
		foreach (KeyValuePair<string, ObjectToggler> toggle in listView.toggles)
		{
			dictionary.Add(toggle.Key, toggle.Value.toggled);
			dictionary2.Add(toggle.Key, toggle.Value.gameObject.activeInHierarchy);
		}
		if (listView.currentList != null && listView.currentList != newList)
		{
			listView.currentList.Clear();
			ObjectPool<QudItemList>.Return(listView.currentList);
			listView.currentList = null;
		}
		listView.currentList = newList;
		listView.currentList.Categorize();
		GameObject childGameObject = GetChildGameObject(listView.idInventoryContent);
		int num3 = 0;
		listView.filterBar.AddCategory("*");
		for (int i = 0; i < listView.currentList.categoryNames.Count; i++)
		{
			num3++;
			List<QudItemListElement> list = listView.currentList.categories[listView.currentList.categoryNames[i]];
			list.Sort(BaseInventoryView.displayNameSorter);
			GameObject gameObject = PooledPrefabManager.Instantiate("ListCollapserLine");
			gameObject.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = listView.currentList.categoryNames[i];
			gameObject.transform.SetParent(childGameObject.transform);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.name = listView.currentList.categoryNames[i];
			listView.toggles.Add(listView.currentList.categoryNames[i], gameObject.GetComponent<ObjectToggler>());
			listView.InventoryButtons.Add(gameObject);
			gameObject.transform.SetSiblingIndex(num3);
			listView.filterBar.AddCategory(listView.currentList.categoryNames[i]);
			for (int j = 0; j < list.Count; j++)
			{
				num3++;
				GameObject gameObject2 = PooledPrefabManager.Instantiate("InventoryItemLine");
				ItemLineManager component = gameObject2.GetComponent<ItemLineManager>();
				component.SetMode("Trade");
				component.SetGameObject(list[j]);
				if (component.onClick != null)
				{
					Debug.LogError("onClick isn't null!");
				}
				listView.InventoryButtons.Add(gameObject2);
				gameObject2.transform.SetParent(childGameObject.transform);
				gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
				gameObject2.transform.SetSiblingIndex(num3);
				gameObject.GetComponent<ObjectToggler>().objects.Add(gameObject2);
				if (j == 0 && i == 0)
				{
					Select(gameObject2);
				}
			}
		}
		if (text != null && listView.filterBar.categoryButtons.ContainsKey(text) && text != "*")
		{
			listView.filterBar.categoryButtons[text].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		else if (text == null || !listView.filterBar.categoryButtons.ContainsKey(text))
		{
			listView.filterBar.categoryButtons["*"].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			if (listView.toggles.ContainsKey(item.Key) && !item.Value)
			{
				if (listView.toggles[item.Key].toggled)
				{
					listView.toggles[item.Key].Toggle();
				}
				if (!dictionary2[item.Key])
				{
					listView.toggles[item.Key].gameObject.SetActive(value: false);
				}
			}
		}
		if (num >= 0)
		{
			bool flag = false;
			for (int k = num; k < listView.InventoryButtons.Count; k++)
			{
				if (k < listView.InventoryButtons.Count && listView.InventoryButtons[k].gameObject.activeInHierarchy)
				{
					base.EventSystemManager.SetSelectedGameObject(listView.InventoryButtons[k]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int num4 = num; num4 >= 0; num4--)
				{
					if (num4 < listView.InventoryButtons.Count && listView.InventoryButtons[num4].gameObject.activeInHierarchy)
					{
						base.EventSystemManager.SetSelectedGameObject(listView.InventoryButtons[num4]);
						flag = true;
						break;
					}
				}
			}
		}
		listView.filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = false;
		listView.filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = true;
	}

	public void UpdateObjectLists(QudItemList newList1, QudItemList newList2)
	{
		Clear();
		UpdateObjectList(newList1, list1);
		UpdateObjectList(newList2, list2);
		Canvas.ForceUpdateCanvases();
		Canvas.ForceUpdateCanvases();
	}

	public void GamesideInventoryUpdate()
	{
		if (TradeViewBehaviour.instance != null)
		{
			QudItemList list1 = ObjectPool<QudItemList>.Checkout();
			list1.Add(TradeUI.Objects[0]);
			QudItemList list2 = ObjectPool<QudItemList>.Checkout();
			list2.Add(TradeUI.Objects[1]);
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				TradeViewBehaviour.instance.infoText.text = Sidebar.FormatToRTF(TradeUI.sReadout);
				UpdateObjectLists(list1, list2);
			});
		}
	}

	public void QueueInventoryUpdate()
	{
		GameManager.Instance.gameQueue.queueTask(delegate
		{
			GamesideInventoryUpdate();
		});
	}

	public override void Enter()
	{
		base.Enter();
	}

	protected void Clear()
	{
		list1.Clear();
		list2.Clear();
	}

	public override void OnLeave()
	{
		Clear();
		list1.OnLeave();
		list2.OnLeave();
	}

	public static bool EnsureVisible(ScrollRect scrollRect, GameObject child)
	{
		Rect target = child.GetComponent<RectTransform>().RectRelativeTo(scrollRect.GetComponent<RectTransform>());
		if (scrollRect.GetComponent<RectTransform>().rect.Contains(target))
		{
			return true;
		}
		float num = scrollRect.GetComponent<RectTransform>().anchorMin.y - child.GetComponent<RectTransform>().anchoredPosition.y;
		num += (float)child.GetComponent<RectTransform>().transform.GetSiblingIndex() / (float)scrollRect.content.transform.childCount;
		num /= 1000f;
		num = (scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1f - num));
		return false;
	}

	public override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(UnityEngine.KeyCode.KeypadMinus))
		{
			foreach (ObjectToggler value in list1.toggles.Values)
			{
				if (value.toggled)
				{
					value.Toggle();
				}
			}
			foreach (ObjectToggler value2 in list2.toggles.Values)
			{
				if (value2.toggled)
				{
					value2.Toggle();
				}
			}
			list1.SelectNearestInventoryToCurrent();
			list2.SelectNearestInventoryToCurrent();
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.KeypadPlus))
		{
			foreach (ObjectToggler value3 in list1.toggles.Values)
			{
				if (!value3.toggled)
				{
					value3.Toggle();
				}
			}
			foreach (ObjectToggler value4 in list2.toggles.Values)
			{
				if (!value4.toggled)
				{
					value4.Toggle();
				}
			}
			list1.SelectNearestInventoryToCurrent();
			list2.SelectNearestInventoryToCurrent();
		}
		int num = 0;
		if (Input.GetKeyDown(UnityEngine.KeyCode.PageDown))
		{
			num = 8;
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.PageUp))
		{
			num = -8;
		}
		if (num == 0)
		{
			return;
		}
		if (base.EventSystemManager.currentSelectedGameObject != null && list1.InventoryButtons.Contains(base.EventSystemManager.currentSelectedGameObject))
		{
			int num2 = list1.InventoryButtons.IndexOf(base.EventSystemManager.currentSelectedGameObject) + num;
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 >= list1.InventoryButtons.Count)
			{
				num2 = list1.InventoryButtons.Count - 1;
			}
			if (list1.InventoryButtons[num2].activeInHierarchy)
			{
				Select(list1.InventoryButtons[num2]);
			}
			else
			{
				for (int i = 0; i < list1.InventoryButtons.Count; i++)
				{
					if (num2 + i < list1.InventoryButtons.Count && list1.InventoryButtons[num2 + i].activeInHierarchy)
					{
						Select(list1.InventoryButtons[num2 + i]);
						break;
					}
					if (num2 - i > 0 && list1.InventoryButtons[num2 - i].activeInHierarchy)
					{
						Select(list1.InventoryButtons[num2 - i]);
						break;
					}
				}
			}
		}
		if (!(base.EventSystemManager.currentSelectedGameObject != null) || !list2.InventoryButtons.Contains(base.EventSystemManager.currentSelectedGameObject))
		{
			return;
		}
		int num3 = list2.InventoryButtons.IndexOf(base.EventSystemManager.currentSelectedGameObject) + num;
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num3 >= list2.InventoryButtons.Count)
		{
			num3 = list2.InventoryButtons.Count - 1;
		}
		if (list2.InventoryButtons[num3].activeInHierarchy)
		{
			Select(list2.InventoryButtons[num3]);
			return;
		}
		for (int j = 0; j < list2.InventoryButtons.Count; j++)
		{
			if (num3 + j < list2.InventoryButtons.Count && list2.InventoryButtons[num3 + j].activeInHierarchy)
			{
				Select(list2.InventoryButtons[num3 + j]);
				break;
			}
			if (num3 - j > 0 && list2.InventoryButtons[num3 - j].activeInHierarchy)
			{
				Select(list2.InventoryButtons[num3 - j]);
				break;
			}
		}
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Offer"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.O, 'o'));
		}
		if (Command.StartsWith("Back"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		base.OnCommand(Command);
	}
}

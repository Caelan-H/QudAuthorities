using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.World;

[UIView("Popup:Item", false, false, false, "Menu", "Popup:Item", false, 0, false)]
public class Popup_Item : BaseInventoryView
{
	public static bool AllowEscape;

	public static Popup_Item instance;

	public Popup_Item()
	{
		instance = this;
		idInventoryContent = "Controls/Inventory Panel/Inventory Scroll/Viewport/Content";
		idInventoryScrollView = "Controls/Inventory Panel/Inventory Scroll";
	}

	public override void OnCreate()
	{
		filterBar = base.rootObject.GetComponent<PopupItemBehaviour>().inventoryPanel.filterBar;
		filterBar.inventory = this;
		base.OnCreate();
	}

	public float CalculateTargetHeight(VerticalLayoutGroup group, List<UnityEngine.GameObject> content)
	{
		int num = 0;
		num += group.padding.top + group.padding.bottom;
		num += (int)group.spacing * content.Count - 1;
		for (int i = 0; i < content.Count; i++)
		{
			num += Math.Max(60, (int)LayoutUtility.GetPreferredHeight(content[i].GetComponent<RectTransform>()));
		}
		return num;
	}

	public override void Enter()
	{
		instance = this;
		if (currentList != null)
		{
			UpdateObjectList(currentList);
		}
		base.Enter();
	}

	public void UpdateObjectList(QudItemList newList)
	{
		int num = -1;
		if (base.EventSystemManager.firstSelectedGameObject != null && InventoryButtons.Contains(base.EventSystemManager.firstSelectedGameObject))
		{
			num = InventoryButtons.IndexOf(base.EventSystemManager.firstSelectedGameObject);
		}
		string text = null;
		if (filterBar.toggledButton != null)
		{
			text = filterBar.toggledButton.category;
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>();
		foreach (KeyValuePair<string, ObjectToggler> toggle in toggles)
		{
			dictionary.Add(toggle.Key, toggle.Value.toggled);
			dictionary2.Add(toggle.Key, toggle.Value.gameObject.activeInHierarchy);
		}
		if (currentList != null && currentList != newList)
		{
			currentList.Clear();
			ObjectPool<QudItemList>.Return(currentList);
			currentList = null;
		}
		currentList = newList;
		Clear();
		currentList.Categorize();
		UnityEngine.GameObject childGameObject = GetChildGameObject(idInventoryContent);
		int num2 = 0;
		filterBar.AddCategory("*");
		for (int i = 0; i < currentList.categoryNames.Count; i++)
		{
			num2++;
			List<QudItemListElement> list = currentList.categories[currentList.categoryNames[i]];
			list.Sort(BaseInventoryView.displayNameSorter);
			UnityEngine.GameObject gameObject = PooledPrefabManager.Instantiate("ListCollapserLine");
			gameObject.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = currentList.categoryNames[i];
			gameObject.transform.SetParent(childGameObject.transform);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.name = currentList.categoryNames[i];
			toggles.Add(currentList.categoryNames[i], gameObject.GetComponent<ObjectToggler>());
			InventoryButtons.Add(gameObject);
			gameObject.transform.SetSiblingIndex(num2);
			filterBar.AddCategory(currentList.categoryNames[i]);
			for (int j = 0; j < list.Count; j++)
			{
				num2++;
				UnityEngine.GameObject newItem = PooledPrefabManager.Instantiate("InventoryItemLine");
				ItemLineManager component = newItem.GetComponent<ItemLineManager>();
				component.SetMode("PickItem");
				component.SetGameObject(list[j]);
				newItem.GetComponent<ItemLineManager>().onClick = delegate(ItemLineManager line)
				{
					base.EventSystemManager.firstSelectedGameObject = newItem;
					XRL.World.GameObject go = line.go;
					Keyboard.PushMouseEvent("ItemSelected", go);
				};
				InventoryButtons.Add(newItem);
				newItem.transform.SetParent(childGameObject.transform);
				newItem.transform.localScale = new Vector3(1f, 1f, 1f);
				newItem.transform.SetSiblingIndex(num2);
				gameObject.GetComponent<ObjectToggler>().objects.Add(newItem);
				if (j == 0 && i == 0)
				{
					Select(newItem);
				}
			}
		}
		if (text != null && filterBar.categoryButtons.ContainsKey(text) && text != "*")
		{
			filterBar.categoryButtons[text].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		else if (text == null || !filterBar.categoryButtons.ContainsKey(text))
		{
			filterBar.categoryButtons["*"].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			if (toggles.ContainsKey(item.Key) && !item.Value)
			{
				if (toggles[item.Key].toggled)
				{
					toggles[item.Key].Toggle();
				}
				if (!dictionary2[item.Key])
				{
					toggles[item.Key].gameObject.SetActive(value: false);
				}
			}
		}
		if (num >= 0)
		{
			bool flag = false;
			for (int k = num; k < InventoryButtons.Count; k++)
			{
				if (k < InventoryButtons.Count && InventoryButtons[k].gameObject.activeInHierarchy)
				{
					base.EventSystemManager.SetSelectedGameObject(InventoryButtons[k]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int num3 = num; num3 >= 0; num3--)
				{
					if (num3 < InventoryButtons.Count && InventoryButtons[num3].gameObject.activeInHierarchy)
					{
						base.EventSystemManager.SetSelectedGameObject(InventoryButtons[num3]);
						flag = true;
						break;
					}
				}
			}
		}
		else if (InventoryButtons.Count > 1)
		{
			Select(InventoryButtons[1]);
		}
		filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = false;
		filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = true;
		Canvas.ForceUpdateCanvases();
		Canvas.ForceUpdateCanvases();
	}

	public override void OnLeave()
	{
		Clear();
		filterBar.toggledButton = null;
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Back"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		if (Command.StartsWith("Store"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.KeypadPlus));
		}
		if (Command.StartsWith("TakeAll"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Tab));
		}
		base.OnCommand(Command);
	}
}

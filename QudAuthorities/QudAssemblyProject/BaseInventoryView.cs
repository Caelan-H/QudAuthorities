using System.Collections.Generic;
using QupKit;
using UnityEngine;

public class BaseInventoryView : BaseView
{
	public class SortQudItemListElementDisplayName : Comparer<QudItemListElement>
	{
		public override int Compare(QudItemListElement x, QudItemListElement y)
		{
			return string.Compare(x.go.GetCachedDisplayNameStripped(), y.go.GetCachedDisplayNameStripped(), ignoreCase: true);
		}
	}

	public string idInventoryContent = "Panel/Inventory Panel/Inventory Scroll/Viewport/Content";

	public string idInventoryScrollView = "Panel/Inventory Scroll";

	public InventoryFilterBarBehaviour filterBar;

	public List<GameObject> InventoryButtons = new List<GameObject>();

	public Dictionary<string, ObjectToggler> toggles = new Dictionary<string, ObjectToggler>();

	public static Dictionary<string, bool> toggleState = new Dictionary<string, bool>();

	public QudItemList currentList;

	public static SortQudItemListElementDisplayName displayNameSorter = new SortQudItemListElementDisplayName();

	public virtual void Clear()
	{
		toggles.Clear();
		for (int i = 0; i < InventoryButtons.Count; i++)
		{
			PooledPrefabManager.Return(InventoryButtons[i]);
		}
		InventoryButtons.Clear();
		if (filterBar != null)
		{
			filterBar.Clear();
		}
	}

	public void SelectNearestInventoryToCurrent()
	{
		if (!InventoryButtons.Contains(base.EventSystemManager.currentSelectedGameObject) || base.EventSystemManager.currentSelectedGameObject.activeInHierarchy)
		{
			return;
		}
		int num = InventoryButtons.IndexOf(base.EventSystemManager.currentSelectedGameObject);
		for (int i = 0; i < InventoryButtons.Count; i++)
		{
			if (num + i < InventoryButtons.Count && InventoryButtons[num + i].activeInHierarchy)
			{
				Select(InventoryButtons[num + i]);
				break;
			}
			if (num - i > 0 && InventoryButtons[num - i].activeInHierarchy)
			{
				Select(InventoryButtons[num - i]);
				break;
			}
		}
	}
}

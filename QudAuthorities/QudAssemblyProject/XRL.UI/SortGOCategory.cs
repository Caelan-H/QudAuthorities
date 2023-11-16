using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class SortGOCategory : Comparer<GameObject>
{
	public override int Compare(GameObject x, GameObject y)
	{
		string inventoryCategory = x.GetInventoryCategory();
		string inventoryCategory2 = y.GetInventoryCategory();
		if (inventoryCategory == inventoryCategory2)
		{
			return string.Compare(x.GetCachedDisplayNameStripped(), y.GetCachedDisplayNameStripped(), ignoreCase: true);
		}
		return string.Compare(inventoryCategory, inventoryCategory2, ignoreCase: true);
	}
}

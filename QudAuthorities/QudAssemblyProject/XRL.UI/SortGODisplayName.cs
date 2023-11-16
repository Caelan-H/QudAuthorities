using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class SortGODisplayName : Comparer<GameObject>
{
	public override int Compare(GameObject x, GameObject y)
	{
		return string.Compare(x.GetCachedDisplayNameStripped(), y.GetCachedDisplayNameStripped(), ignoreCase: true);
	}
}

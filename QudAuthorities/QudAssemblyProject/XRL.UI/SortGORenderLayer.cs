using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class SortGORenderLayer : Comparer<GameObject>
{
	public override int Compare(GameObject x, GameObject y)
	{
		Render pRender = x.pRender;
		Render pRender2 = y.pRender;
		if (x == y)
		{
			return 0;
		}
		if (pRender == null)
		{
			return 1;
		}
		if (pRender2 == null)
		{
			return -1;
		}
		if (pRender.RenderLayer == pRender2.RenderLayer)
		{
			return string.Compare(x.GetCachedDisplayNameStripped(), y.GetCachedDisplayNameStripped(), ignoreCase: true);
		}
		return pRender2.RenderLayer.CompareTo(pRender.RenderLayer);
	}
}

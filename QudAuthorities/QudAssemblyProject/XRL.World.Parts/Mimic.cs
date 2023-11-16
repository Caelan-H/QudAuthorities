using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Wintellect.PowerCollections;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Mimic : IPart
{
	public bool CopyColor = true;

	public bool CopyString;

	public bool CopyBackground = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		MimicNearbyObject();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			MimicNearbyObject();
		}
		return base.FireEvent(E);
	}

	public void MimicNearbyObject()
	{
		if (ParentObject.CurrentCell == null || ParentObject.CurrentCell.ParentZone == null || !ParentObject.CurrentCell.ParentZone.Built)
		{
			return;
		}
		List<Cell> list = new List<Cell>(Algorithms.RandomShuffle(ParentObject.CurrentCell.GetLocalAdjacentCells(), Stat.Rand));
		if (list.Count <= 0)
		{
			return;
		}
		int index = Stat.Random(0, list.Count - 1);
		foreach (GameObject item in list[index].GetObjectsWithPart("Render"))
		{
			if (item == ParentObject)
			{
				continue;
			}
			Render pRender = ParentObject.pRender;
			Render pRender2 = item.pRender;
			if (pRender2.RenderLayer <= 0)
			{
				continue;
			}
			if (CopyColor)
			{
				if (CopyBackground)
				{
					pRender.ColorString = pRender2.ColorString;
					pRender.DetailColor = pRender2.DetailColor;
				}
				else
				{
					pRender.ColorString = ColorUtility.StripBackgroundFormatting(pRender2.ColorString);
					pRender.DetailColor = pRender2.DetailColor;
				}
			}
			if (CopyString)
			{
				pRender.Tile = pRender2.Tile;
				pRender.RenderString = pRender2.RenderString;
			}
			break;
		}
	}
}

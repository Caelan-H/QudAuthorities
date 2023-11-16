using System.Collections.Generic;
using System.Linq;
using Genkit;
using UnityEngine;
using XRL.UI;

namespace XRL;

public class ZTCellFilterOutNode : ZoneTemplateNode
{
	public override bool Execute(ZoneTemplateGenerationContext Context)
	{
		InfluenceMapRegion influenceMapRegion = Context.Regions.Regions[Context.CurrentRegion];
		InfluenceMapRegion influenceMapRegion2 = influenceMapRegion.deepCopy();
		List<Location2D> list = new List<Location2D>();
		string[] array = Filter.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Location2D loc2 in influenceMapRegion.Cells)
			{
				Context.Z.GetCell(loc2);
				string text = array[i].ToLower();
				bool flag = false;
				if (text[0] == '!')
				{
					flag = true;
					text = text.Substring(1);
				}
				List<InfluenceMapRegion> regions = Context.Regions.Regions;
				if (text.StartsWith("regionhassemantictag:"))
				{
					string[] tags = text.Split(':')[1].Split(';');
					if (flag)
					{
						if (!Context.currentSector.Cells.Any((Location2D loc) => tags.Any((string tag) => Context.Z.GetCell(loc).HasSemanticTag(tag))))
						{
							list.Add(loc2);
						}
					}
					else if (Context.currentSector.Cells.Any((Location2D loc) => tags.Any((string tag) => Context.Z.GetCell(loc).HasSemanticTag(tag))))
					{
						list.Add(loc2);
					}
					continue;
				}
				switch (text)
				{
				case "reachable":
					if (flag)
					{
						if (!Context.Z.GetCell(loc2).IsReachable())
						{
							list.Add(loc2);
						}
					}
					else if (Context.Z.GetCell(loc2).IsReachable())
					{
						list.Add(loc2);
					}
					break;
				case "furthest":
					if (!Context.Regions.Regions[0].Contains(loc2))
					{
						list.Add(loc2);
					}
					break;
				case "isolated":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => !R.ConnectsToTag("connection") && !R.HasTag("connected") && R.Contains(loc2)))
						{
							list.Add(loc2);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => (R.HasTag("connected") || R.ConnectsToTag("connection")) && R.Contains(loc2)))
					{
						list.Add(loc2);
					}
					break;
				case "pocket":
					if (flag)
					{
						if (regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count > 0 && R.Contains(loc2)))
						{
							list.Add(loc2);
						}
					}
					else if (regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count == 0 && R.Contains(loc2)))
					{
						list.Add(loc2);
					}
					break;
				case "connection":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => !R.HasTag("connection") && R.Contains(loc2)))
						{
							list.Add(loc2);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => R.HasTag("connection") && R.Contains(loc2)))
					{
						list.Add(loc2);
					}
					break;
				case "deadend":
					if (flag)
					{
						if (!regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count != 1 && R.Contains(loc2)))
						{
							list.Add(loc2);
						}
					}
					else if (!regions.Any((InfluenceMapRegion R) => R.AdjacentRegions.Count == 1 && R.Contains(loc2)))
					{
						list.Add(loc2);
					}
					break;
				default:
					Debug.LogWarning("Unknown criteria: " + text);
					return false;
				}
			}
		}
		foreach (Location2D item in list)
		{
			influenceMapRegion2.removeCell(item);
		}
		Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion2;
		if (Options.DrawInfluenceMaps)
		{
			influenceMapRegion2.draw();
		}
		for (int j = 0; j < Children.Count; j++)
		{
			if (!Children[j].TestCriteria(Context))
			{
				continue;
			}
			int num = Children[j].CheckChance(Context);
			for (int k = 0; k < num; k++)
			{
				if (!Children[j].Execute(Context))
				{
					Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion;
					return false;
				}
			}
		}
		Context.Regions.Regions[Context.CurrentRegion] = influenceMapRegion;
		return true;
	}
}

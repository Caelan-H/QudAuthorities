using System.Collections.Generic;
using Genkit;
using UnityEngine;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class PopTableZoneBuilder
{
	private List<Point2D> Pairs;

	public bool BuildZone(Zone NewZone, string Table, string Density = "minimum", bool bApplyZoneFactionToObjects = false)
	{
		if (!PopulationManager.HasPopulation(Table))
		{
			Debug.LogError("Unknown population table: " + Table);
			return true;
		}
		List<GameObject> list = PopulationManager.Expand(PopulationManager.Generate(Table));
		if (Pairs == null)
		{
			Pairs = new List<Point2D>();
		}
		Pairs.Clear();
		if (1000 - 1 == 0)
		{
			return false;
		}
		List<Cell> cells = NewZone.GetCells();
		List<Cell> list2 = new List<Cell>();
		List<Cell> list3 = new List<Cell>();
		List<Cell> list4 = new List<Cell>();
		foreach (Cell item in cells)
		{
			if (!item.HasObjectWithPart("SpawnBlocker"))
			{
				if (item.HasObjectWithPart("LiquidVolume") && !item.HasBridge())
				{
					list2.Add(item);
				}
				else if (item.HasObjectWithIntProperty("Wall"))
				{
					list3.Add(item);
				}
				else if (!item.IsOccluding() && item.IsReachable() && item.IsEmpty())
				{
					list4.Add(item);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			bool flag = false;
			bool flag2 = false;
			if (list[i].Property.ContainsKey("StartInLiquid"))
			{
				flag = true;
			}
			if (list[i].HasPart("Brain") && ((Brain)list[i].GetPart("Brain")).Aquatic)
			{
				flag = true;
			}
			if (list[i].HasPart("Brain") && ((Brain)list[i].GetPart("Brain")).LivesOnWalls)
			{
				flag2 = true;
			}
			if (bApplyZoneFactionToObjects && NewZone.GetZoneProperty("faction", null) != null && list[i].HasPart("Brain"))
			{
				list[i].pBrain.Factions = "";
				list[i].pBrain.FactionMembership.Clear();
				list[i].pBrain.FactionMembership.Add(NewZone.GetZoneProperty("faction"), 100);
				list[i].pRender.DisplayName = "domesticated " + list[i].pRender.DisplayName;
			}
			List<Cell> list5 = list4;
			if (flag && list2.Count > 0)
			{
				list5 = list2;
			}
			if (flag2 && list3.Count > 0)
			{
				list5 = list3;
			}
			if (list5.Count <= 0)
			{
				list5 = cells;
			}
			int index = Stat.Random(0, list5.Count - 1);
			list5[index].AddObject(list[i]);
			if (list5 != cells)
			{
				list5.Remove(list5[index]);
			}
		}
		return true;
	}
}

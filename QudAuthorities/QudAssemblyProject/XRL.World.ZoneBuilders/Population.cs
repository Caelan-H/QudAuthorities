using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Population : ZoneBuilderSandbox
{
	public string Table;

	public string Density = "lowest";

	public bool BuildZone(Zone NewZone)
	{
		if (!PopulationManager.HasPopulation(Table))
		{
			Debug.LogError("Unknown population table: " + Table);
			return true;
		}
		List<GameObject> list = PopulationManager.Expand(PopulationManager.Generate(Table, "zonetier", NewZone.NewTier.ToString()));
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
			if (list[i].HasTagOrProperty("StartInLiquid"))
			{
				flag = true;
			}
			if (list[i].LimitToAquatic())
			{
				flag = true;
			}
			if (list[i].pBrain != null && list[i].pBrain.LivesOnWalls)
			{
				flag2 = true;
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

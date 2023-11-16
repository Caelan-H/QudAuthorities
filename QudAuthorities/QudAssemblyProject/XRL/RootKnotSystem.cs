using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World;

namespace XRL;

[Serializable]
public class RootKnotSystem : IGameSystem
{
	[NonSerialized]
	public List<GameObject> inventory = new List<GameObject>();

	public bool first = true;

	public override void SaveGame(SerializationWriter writer)
	{
		writer.WriteGameObjectList(inventory);
	}

	public override void LoadGame(SerializationReader reader)
	{
		inventory = new List<GameObject>();
		reader.ReadGameObjectList(inventory);
	}

	public override void NewZoneGenerated(Zone zone)
	{
		if (zone.IsWorldMap())
		{
			return;
		}
		int num = 3;
		if (zone.IsCheckpoint())
		{
			num = 40;
		}
		if (first)
		{
			num = 100;
		}
		if (Stat.Roll(1, 100) > num)
		{
			return;
		}
		first = false;
		if (zone.BaseDisplayName == "Joppa")
		{
			zone.GetCell(24, 21).AddObject("RootPetSummoner");
		}
		else
		{
			Cell cell = zone.GetCellWithEmptyBorder(1);
			if (cell == null)
			{
				cell = zone.GetEmptyCells().FirstOrDefault();
			}
			if (cell == null)
			{
				cell = zone.GetCells().FirstOrDefault();
			}
			Cell cell2 = cell.GetCellFromDirection("NW");
			if (cell2 == null)
			{
				cell2 = cell;
			}
			cell2.AddObject("RootPetSummoner");
		}
		foreach (GameObject @object in zone.GetObjects("RootKnot"))
		{
			@object.pBrain.SetFeeling(The.Player, 1000);
			@object.Inventory.Objects = inventory;
		}
	}
}

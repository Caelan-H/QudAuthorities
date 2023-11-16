using System;
using System.Collections.Generic;
using Genkit;
using XRL.EditorFormats.Map;
using XRL.World.QuestManagers;

namespace XRL.World.Parts;

[Serializable]
public class BearRepair : IPart
{
	public string FileName = "GritGate.rpm";

	public long TurnsPerObject = 50L;

	public long BuildCounter;

	public long LastTurn = long.MinValue;

	[NonSerialized]
	private MapFile map;

	[NonSerialized]
	private List<(Location2D, string)> toBuild;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckBearRepair();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckBearRepair();
		return base.HandleEvent(E);
	}

	public void CheckBearRepair()
	{
		if (LastTurn == long.MinValue && The.Game != null)
		{
			LastTurn = The.Game.TimeTicks;
		}
		BuildCounter += The.Game.TimeTicks - LastTurn;
		LastTurn = The.Game.TimeTicks;
		if (BuildCounter <= TurnsPerObject)
		{
			return;
		}
		long num = Math.Max(1L, BuildCounter / TurnsPerObject);
		BuildCounter = 0L;
		if (map == null)
		{
			map = MapFile.LoadWithMods(FileName);
		}
		if (map == null)
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null)
		{
			return;
		}
		if (toBuild == null)
		{
			toBuild = new List<(Location2D, string)>();
			for (int i = 0; i < currentZone.Height; i++)
			{
				for (int j = 0; j < currentZone.Width; j++)
				{
					foreach (MapFileObjectBlueprint @object in map.Cells[j, i].Objects)
					{
						GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(@object.Name);
						if (blueprint != null && !blueprint.HasTag("Wall") && !blueprint.HasPart("Brain") && !currentZone.GetCell(j, i).HasObject(@object.Name))
						{
							toBuild.Add((Location2D.get(j, i), @object.Name));
						}
					}
				}
			}
			toBuild.ShuffleInPlace();
		}
		for (long num2 = 0L; num2 < num; num2++)
		{
			if (toBuild.Count <= 0)
			{
				break;
			}
			(Location2D, string) tuple = toBuild[0];
			toBuild.RemoveAt(0);
			currentZone.GetCell(tuple.Item1).AddObject(tuple.Item2);
		}
		if (toBuild.Count == 0)
		{
			ParentObject.Destroy();
		}
		GritGateScripts.OpenRank2Doors();
	}
}

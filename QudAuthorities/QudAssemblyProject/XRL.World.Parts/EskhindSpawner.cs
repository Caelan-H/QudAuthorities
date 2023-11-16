using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EskhindSpawner : IPart
{
	public bool spawned;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!spawned && The.Game.HasQuest("Kith and Kin") && !The.Game.HasGameState("EskhindKilled"))
		{
			spawned = true;
			ParentObject.CurrentCell.AddObject("Eskhind").SetStringProperty("RealEskhind", "1");
			The.Game.SetStringGameState("EskhindMoved", "1");
			List<Cell> emptyAdjacentCells = ParentObject.CurrentCell.GetEmptyAdjacentCells(1, 3);
			emptyAdjacentCells.RemoveRandomElement()?.AddObject("Meyehind");
			emptyAdjacentCells.RemoveRandomElement()?.AddObject("Liihart");
		}
		return base.HandleEvent(E);
	}
}

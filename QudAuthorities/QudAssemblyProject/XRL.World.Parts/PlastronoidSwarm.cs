using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PlastronoidSwarm : IPart
{
	public int SpawnTurns = 20;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !ParentObject.IsPlayer() && ParentObject.pPhysics.CurrentCell != null && !ParentObject.pPhysics.CurrentCell.ParentZone.IsWorldMap())
		{
			List<Cell> list = new List<Cell>();
			ParentObject.pPhysics.CurrentCell.GetAdjacentCells(3, list);
			list.Add(ParentObject.pPhysics.CurrentCell);
			foreach (Cell item in list)
			{
				if (item.IsEmpty() && Stat.Random(1, 100) <= 50)
				{
					GameObject gO = GameObjectFactory.Factory.CreateObject("Plastronoid");
					item.AddObject(gO);
					XRLCore.Core.Game.ActionManager.AddActiveObject(gO);
				}
			}
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}

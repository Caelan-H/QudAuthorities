using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class GiantAmoeba : IPart
{
	public int CloneCooldown;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDeathRemoval");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && ParentObject.pPhysics.CurrentCell != null)
		{
			List<Cell> adjacentCells = ParentObject.pPhysics.CurrentCell.GetAdjacentCells();
			adjacentCells.Add(ParentObject.pPhysics.CurrentCell);
			foreach (Cell item in adjacentCells)
			{
				if (!item.IsOccluding() && Stat.Random(1, 100) <= 80)
				{
					item.AddObject(GameObjectFactory.Factory.CreateObject("SlimePuddle"));
				}
			}
		}
		return true;
	}
}

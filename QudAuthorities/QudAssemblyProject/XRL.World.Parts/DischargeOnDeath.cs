using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DischargeOnDeath : IPart
{
	public string Arcs = "1";

	public int Voltage = 3;

	public string Damage = "1d8";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDeathRemoval");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && ParentObject.pPhysics.CurrentCell != null)
		{
			int num = Stat.Roll(Arcs);
			for (int i = 0; i < num; i++)
			{
				List<Cell> adjacentCells = ParentObject.pPhysics.CurrentCell.GetAdjacentCells();
				if (adjacentCells.Count == 0)
				{
					return true;
				}
				Cell randomElement = adjacentCells.GetRandomElement();
				ParentObject.Discharge(randomElement, Voltage, Damage, ParentObject);
			}
		}
		return base.FireEvent(E);
	}
}

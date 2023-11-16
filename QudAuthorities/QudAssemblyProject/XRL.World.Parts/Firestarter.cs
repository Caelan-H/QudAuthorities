using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Firestarter : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
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
			foreach (Cell localAdjacentCell in ParentObject.CurrentCell.ParentZone.GetCell(Stat.Random(0, 79), Stat.Random(0, 24)).GetLocalAdjacentCells())
			{
				if (Stat.Random(1, 100) > 25)
				{
					continue;
				}
				foreach (GameObject item in localAdjacentCell.GetObjectsWithPart("Physics"))
				{
					if (item.HasPart("Render") && !item.HasPart("Combat"))
					{
						item.pPhysics.Temperature = item.pPhysics.FlameTemperature + 25;
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

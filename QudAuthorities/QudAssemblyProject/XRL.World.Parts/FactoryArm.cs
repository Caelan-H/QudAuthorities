using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class FactoryArm : IPart
{
	public string Direction;

	public int Frequency;

	public int Counter;

	public override bool SameAs(IPart p)
	{
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Counter == Frequency - 1)
		{
			int num = XRLCore.CurrentFrame % 30;
			if (num > 0 && num < 15)
			{
				E.Tile = null;
				E.RenderString = Directions.GetArrowForDirection(Direction);
				E.ColorString = "&C";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Counter++;
			if (Counter >= Frequency)
			{
				Counter = 0;
				Cell cellFromDirection = base.currentCell.GetCellFromDirection(Direction);
				Cell cellFromDirection2 = base.currentCell.GetCellFromDirection(Directions.GetOppositeDirection(Direction));
				if (cellFromDirection.IsPassable() && cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: false) == null)
				{
					for (int num = cellFromDirection2.Objects.Count - 1; num >= 0; num--)
					{
						GameObject gameObject = cellFromDirection2.Objects[num];
						if (gameObject.pPhysics != null && gameObject.pPhysics.Takeable && gameObject.CanBeInvoluntarilyMoved() && gameObject.PhaseAndFlightMatches(ParentObject))
						{
							gameObject.DirectMoveTo(cellFromDirection, 0, forced: true, ignoreCombat: true);
							break;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

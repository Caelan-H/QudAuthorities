using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GrabberArm : IPart
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
				E.ColorString = "&R";
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
				GameObject gameObject = null;
				gameObject = cellFromDirection.GetCombatObject();
				if (gameObject == null)
				{
					for (int i = 0; i < cellFromDirection.Objects.Count; i++)
					{
						GameObject gameObject2 = cellFromDirection.Objects[i];
						if (gameObject2.pPhysics != null && gameObject2.pPhysics.Takeable)
						{
							gameObject = gameObject2;
						}
					}
				}
				if (gameObject != null)
				{
					if (gameObject.IsPlayer())
					{
						DidXToY("grab", IComponent<GameObject>.ThePlayer, "and holds you in place", null, null, null, IComponent<GameObject>.ThePlayer);
					}
					gameObject.ApplyEffect(new Stuck(3, 25, "Grab Stuck Restraint"));
				}
			}
		}
		return base.FireEvent(E);
	}
}

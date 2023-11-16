using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingClones : IPart
{
	public int TurnsLeft = -1;

	public string Cooldown = "8";

	public string Clones = "1";

	public string Prefix = "Refracted";

	public string ColorString = "&Y";

	public int RealityStabilizationPenetration;

	public string Sound = "refract";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurnEngulfing");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurnEngulfing")
		{
			if (TurnsLeft < 0)
			{
				TurnsLeft = Stat.Roll(Cooldown);
			}
			else if (TurnsLeft == 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				Cell cell = ParentObject.CurrentCell;
				if (gameObjectParameter != null && cell != null)
				{
					int num = Stat.Roll(Clones);
					int num2 = 0;
					for (int i = 0; i < num; i++)
					{
						Cell randomElement = cell.GetEmptyAdjacentCells().GetRandomElement();
						if (randomElement != null)
						{
							Event @event = Event.New("InitiateRealityDistortionTransit");
							@event.SetParameter("Object", gameObjectParameter);
							@event.SetParameter("Cell", randomElement);
							@event.SetParameter("Mutation", this);
							@event.SetParameter("RealityStabilizationPenetration", RealityStabilizationPenetration);
							if (gameObjectParameter.FireEvent(@event) && randomElement.FireEvent(@event) && EvilTwin.CreateEvilTwin(gameObjectParameter, Prefix, randomElement, null, ColorString, ParentObject))
							{
								num2++;
							}
						}
					}
					if (num2 > 0)
					{
						PlayWorldSound(Sound, 0.5f, 0f, combat: true);
						DidXToY("refract", gameObjectParameter, "into " + Grammar.Cardinal(num2) + " additional " + ((num2 == 1) ? "clone" : "clones"), null, null, null, gameObjectParameter);
					}
					else if (num > 0)
					{
						DidXToY("try", "to refract", gameObjectParameter, "but fails to push through the normality lattice in the local region of spacetime", null, null, null, ParentObject);
					}
					TurnsLeft = Stat.Roll(Cooldown);
				}
			}
			else
			{
				TurnsLeft--;
			}
		}
		return base.FireEvent(E);
	}
}

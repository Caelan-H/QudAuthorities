using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Hoarshroom_Tonic_Applicator : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage != null)
		{
			if (E.Damage.HasAttribute("Cold"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					E.ApplyScore(3 * E.Damage.Amount / E.Actor.baseHitpoints - E.Actor.pPhysics.Temperature / 50);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					E.ApplyScore(3 * E.Damage.Amount / E.Actor.hitpoints - E.Actor.pPhysics.Temperature / 50);
				}
			}
		}
		else
		{
			double num = E.Actor.Health();
			if (E.ForPermission)
			{
				if (num < 1.0)
				{
					E.ApplyScore(1);
				}
			}
			else if (num < 0.2)
			{
				E.ApplyScore((int)(1.0 / num));
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyTonic");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			int num = 180 + Stat.Random(0, 40);
			if (!gameObjectParameter.ApplyEffect(new Hoarshroom_Tonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f))))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ShadeOil_Tonic_Applicator : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

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
			if (E.Damage.Amount >= E.Actor.baseHitpoints * 2 / 3)
			{
				E.ApplyScore((E.Actor.IsTrueKin() ? 10 : 4) * E.Damage.Amount / E.Actor.baseHitpoints);
			}
			else if (E.Damage.Amount >= E.Actor.hitpoints * 3 / 4)
			{
				E.ApplyScore((E.Actor.IsTrueKin() ? 10 : 4) * E.Damage.Amount / E.Actor.hitpoints);
			}
		}
		else
		{
			double num = E.Actor.Health();
			if (num < 0.1)
			{
				E.ApplyScore((int)(0.8 / num));
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
			GameObject gameObject = E.GetParameter("Target") as GameObject;
			int num = Stat.Random(1, 10) + 40;
			return gameObject.ApplyEffect(new ShadeOil_Tonic((int)((float)num * (float)gameObject.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
		}
		return base.FireEvent(E);
	}
}

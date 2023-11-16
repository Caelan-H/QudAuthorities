using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Rubbergum_Tonic_Applicator : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	private void ApplyModifiedScore(GetUtilityScoreEvent E, int Score)
	{
		if (E.Actor.GetEffect("Bleeding") is Bleeding bleeding)
		{
			Score -= (bleeding.Damage.RollMinCached() + bleeding.Damage.RollMaxCached()) / 2;
		}
		if (Score > 0)
		{
			E.ApplyScore(Score);
		}
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage != null)
		{
			if (E.Damage.IsElectricDamage())
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 30 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 30 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
			else if (Rubbergum_Tonic.AffectsDamage(E.Damage))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 20 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 20 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
			else if (E.Damage.HasAttribute("Cold"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 10 * E.Damage.Amount / E.Actor.baseHitpoints - E.Actor.pPhysics.Temperature / 50);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 10 * E.Damage.Amount / E.Actor.hitpoints - E.Actor.pPhysics.Temperature / 50);
				}
			}
		}
		else if (E.Actor.IsFrozen())
		{
			ApplyModifiedScore(E, 100);
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
			int num = Stat.Random(1, 10) + 40;
			return gameObjectParameter.ApplyEffect(new Rubbergum_Tonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
		}
		return base.FireEvent(E);
	}
}

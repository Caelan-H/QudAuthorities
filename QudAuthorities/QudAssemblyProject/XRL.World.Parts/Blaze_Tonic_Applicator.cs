using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Blaze_Tonic_Applicator : IPart
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
			if (E.Damage.HasAttribute("Heat"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					E.ApplyScore(30 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					E.ApplyScore(30 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
		}
		else if (E.Actor.IsFrozen())
		{
			E.ApplyScore(100);
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
			int num = Stat.Random(41, 50);
			if (!gameObjectParameter.ApplyEffect(new Blaze_Tonic((int)((double)(num * gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100)) / 100.0))))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(30, 40);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they get +" + Tier + "% max HP for 1 hour.";
	}

	public override string GetTemplatedDescription()
	{
		return "@they get +30-40% max HP for 1 hour.";
	}

	public override string GetNotification()
	{
		return "@they become heartier.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer() && !go.HasEffect("CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect"))
		{
			go.ApplyEffect(new CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect(Tier));
		}
	}
}
[Serializable]
public class CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public int Tier;

	public int Bonus;

	public CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect()
	{
		base.Duration = 50;
	}

	public CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect(int tier)
	{
		Tier = tier;
		base.Duration = 50;
	}

	public override string GetDetails()
	{
		return "+" + Tier + " % max HP";
	}

	public override bool Apply(GameObject Object)
	{
		Bonus = (int)Math.Ceiling((float)Object.Statistics["Hitpoints"].BaseValue * ((float)Tier * 0.01f));
		Object.Statistics["Hitpoints"].BaseValue += Bonus;
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.Statistics["Hitpoints"].BaseValue -= Bonus;
		Bonus = 0;
	}
}

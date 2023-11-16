using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_FearImmunity_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they become@s immune to fear for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become@s immune to fear.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect("CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect"))
		{
			go.ApplyEffect(new CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect());
		}
	}
}
[Serializable]
public class CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect()
	{
		base.Duration = 300;
	}

	public override string GetDetails()
	{
		return "Immune to fear.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyFear");
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Object.UnregisterEffectEvent(this, "ApplyFear");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyFear")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}

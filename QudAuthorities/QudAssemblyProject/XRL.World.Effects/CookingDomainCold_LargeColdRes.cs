using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 125-175 cold resist for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to cold.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect("CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredAction"))
		{
			go.ApplyEffect(new CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
[Serializable]
public class CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect()
		: base("ColdResistance", "125-175", 50)
	{
	}
}

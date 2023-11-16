using System;

namespace XRL.World.Effects;

[Serializable]
public class Str3_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "+3 Str for the next three hours";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect("Str3_ProceduralCookingTriggeredAction_Effect"))
		{
			go.ApplyEffect(new Str3_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
[Serializable]
public class Str3_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
{
	public string stat = "Strength";

	public int amount = 3;

	public Str3_ProceduralCookingTriggeredAction_Effect()
	{
		base.Duration = 1;
		base.DisplayName = null;
	}

	public override void ApplyEffect(GameObject Object)
	{
		base.DisplayName = "&Wwell fed (+" + amount + " " + stat + ")&y";
		Object.Statistics[stat].Bonus += amount;
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.Statistics[stat].Bonus -= amount;
		amount = 0;
		base.RemoveEffect(Object);
	}
}

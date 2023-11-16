using System;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeStance_Defensive : Effect
{
	public LongbladeStance_Defensive()
	{
		base.DisplayName = "{{G|defensive stance}}";
		base.Duration = 1;
	}

	public override string GetDescription()
	{
		if (base.Object != null && base.Object.HasEffect("Asleep"))
		{
			return null;
		}
		return base.GetDescription();
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override string GetDetails()
	{
		if (base.Object.HasPart("LongBladesImprovedDefensiveStance"))
		{
			return "+3 DV while wielding a long blade in the primary hand.";
		}
		return "+2 DV while wielding a long blade in the primary hand.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect("Defensive Stance");
		Object.RemoveEffect("Aggressive Stance");
		Object.RemoveEffect("Dueling Stance");
		return true;
	}
}

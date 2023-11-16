using System;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeStance_Dueling : Effect
{
	public LongbladeStance_Dueling()
	{
		base.DisplayName = "{{W|dueling stance}}";
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

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect("Defensive Stance");
		Object.RemoveEffect("Aggressive Stance");
		Object.RemoveEffect("Dueling Stance");
		return true;
	}

	public override string GetDetails()
	{
		if (base.Object.HasPart("LongBladesImprovedDuelistStance"))
		{
			return "+3 to hit while wielding a long blade in the primary hand.";
		}
		return "+2 to hit while wielding a long blade in the primary hand.";
	}
}

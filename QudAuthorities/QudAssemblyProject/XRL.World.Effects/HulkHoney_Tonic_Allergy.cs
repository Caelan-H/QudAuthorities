using System;

namespace XRL.World.Effects;

[Serializable]
public class HulkHoney_Tonic_Allergy : Effect
{
	private int applied;

	public HulkHoney_Tonic_Allergy()
	{
		base.DisplayName = "{{G|rage}}";
	}

	public HulkHoney_Tonic_Allergy(int duration)
		: this()
	{
		base.Duration = duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{G|enraged}}";
	}

	public override string GetDetails()
	{
		return "Enraged by hulk honey.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetEffect("HulkHoney_Tonic_Allergy") is HulkHoney_Tonic_Allergy hulkHoney_Tonic_Allergy)
		{
			if (hulkHoney_Tonic_Allergy.Duration < base.Duration)
			{
				hulkHoney_Tonic_Allergy.Duration = base.Duration;
			}
			return false;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
	}
}

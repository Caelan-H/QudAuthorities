using System;

namespace XRL.World.Effects;

[Serializable]
public class Rubbergum_Tonic_Allergy : Effect
{
	public Rubbergum_Tonic_Allergy()
	{
		base.DisplayName = "{{W|floppiness}}";
	}

	public Rubbergum_Tonic_Allergy(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{W|floppy}}";
	}

	public override string GetDetails()
	{
		return "35% chance to fall prone when moving.";
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!base.Object.IsFlying && 35.in100())
		{
			base.Object.ApplyEffect(new Prone());
		}
		return base.HandleEvent(E);
	}
}

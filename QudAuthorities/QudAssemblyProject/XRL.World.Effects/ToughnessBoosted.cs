using System;

namespace XRL.World.Effects;

[Serializable]
public class ToughnessBoosted : Effect
{
	private int Bonus;

	public ToughnessBoosted()
	{
		base.DisplayName = "{{g|vitalized}}";
	}

	public ToughnessBoosted(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		int num = 67108868;
		if (Bonus < 0)
		{
			num |= 0x2000000;
		}
		if (Math.Abs(Bonus) < 6)
		{
			num |= 0x1000000;
		}
		return num;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as ToughnessBoosted).Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "+" + Bonus + " Toughness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("ToughnessBoosted"))
		{
			ToughnessBoosted toughnessBoosted = Object.GetEffect("ToughnessBoosted") as ToughnessBoosted;
			if (toughnessBoosted.Duration < base.Duration)
			{
				toughnessBoosted.Duration = base.Duration;
			}
			toughnessBoosted.UnapplyStats();
			toughnessBoosted.Bonus++;
			toughnessBoosted.ApplyStats();
			return false;
		}
		if (Object.FireEvent(Event.New("ApplyToughnessBoosted", "Duration", base.Duration)))
		{
			Bonus = 1;
			ApplyStats();
			return true;
		}
		return false;
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("Toughness", Bonus);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		Bonus = 0;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}

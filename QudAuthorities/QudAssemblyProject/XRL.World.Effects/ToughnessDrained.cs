using System;

namespace XRL.World.Effects;

[Serializable]
public class ToughnessDrained : Effect
{
	private int Penalty;

	public ToughnessDrained()
	{
		base.DisplayName = "{{r|drained}}";
	}

	public ToughnessDrained(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as ToughnessDrained).Penalty != Penalty)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Penalty + " Toughness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("ToughnessDrained"))
		{
			ToughnessDrained toughnessDrained = Object.GetEffect("ToughnessDrained") as ToughnessDrained;
			if (toughnessDrained.Duration > base.Duration)
			{
				toughnessDrained.Duration = base.Duration;
			}
			toughnessDrained.UnapplyStats();
			toughnessDrained.Penalty++;
			toughnessDrained.ApplyStats();
			return false;
		}
		if (Object.FireEvent(Event.New("ApplyToughnessDrained", "Duration", base.Duration)))
		{
			Penalty = 1;
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		Penalty = 0;
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("Toughness", -Penalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
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

	public override bool Render(RenderEvent E)
	{
		return true;
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

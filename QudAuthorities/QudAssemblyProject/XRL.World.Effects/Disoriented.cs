using System;

namespace XRL.World.Effects;

[Serializable]
public class Disoriented : Effect
{
	public int Level;

	public Disoriented()
	{
		base.DisplayName = "{{r|disoriented}}";
	}

	public Disoriented(int Duration, int Level)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + Level + " DV\n-" + Level + " MA";
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Disoriented).Level != Level)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyDisoriented"))
		{
			return false;
		}
		if (Object.GetEffect("Disoriented") is Disoriented disoriented)
		{
			if (disoriented.Level == Level && disoriented.Duration < base.Duration)
			{
				disoriented.Duration = base.Duration;
			}
			else if (disoriented.Level * disoriented.Duration < Level * base.Duration)
			{
				disoriented.Level = Level;
				disoriented.Duration = base.Duration;
			}
			return false;
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "DV", -Level);
		base.StatShifter.SetStatShift(base.Object, "MA", -Level);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
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

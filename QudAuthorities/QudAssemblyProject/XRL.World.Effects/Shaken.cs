using System;

namespace XRL.World.Effects;

[Serializable]
public class Shaken : Effect
{
	public int Level;

	public Shaken()
	{
		base.DisplayName = "shaken";
	}

	public Shaken(int Duration, int Level)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Shaken).Level != Level)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + Level + " DV";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyShaken"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyShaken"))
		{
			return false;
		}
		if (Object.GetEffect("Shaken") is Shaken shaken)
		{
			bool flag = false;
			if (base.Duration > shaken.Duration)
			{
				shaken.Duration = base.Duration;
				flag = true;
			}
			if (Level > shaken.Level)
			{
				shaken.Level = Level;
				flag = true;
			}
			if (flag)
			{
				shaken.ApplyStats();
			}
			return false;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "DV", -Level);
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

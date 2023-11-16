using System;

namespace XRL.World.Effects;

[Serializable]
public class BoostStatistic : Effect
{
	public string Statistic = "Strength";

	public int Bonus;

	public BoostStatistic()
	{
		base.DisplayName = "boosted " + Statistic.ToLower();
	}

	public BoostStatistic(int Duration, string Statistic, int Amount)
		: this()
	{
		base.Duration = Duration;
		this.Statistic = Statistic;
		Bonus = Amount;
		base.DisplayName = "boosted " + Statistic.ToLower();
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		int num = 0;
		num = ((!XRL.World.Statistic.IsMental(Statistic)) ? (num | 4) : (num | 2));
		if (Bonus < 0)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		BoostStatistic boostStatistic = e as BoostStatistic;
		if (boostStatistic.Statistic != Statistic)
		{
			return false;
		}
		if (boostStatistic.Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "+" + Bonus + " " + Statistic;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyBoostStatistic", "Event", this)))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " " + ((Bonus >= 0) ? "increases" : "decreases") + ".", (Bonus >= 0) ? 'g' : 'r');
			}
			ApplyStats();
			return true;
		}
		return false;
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, Statistic, Bonus);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " returns to normal.", 'r');
		}
		UnapplyStats();
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

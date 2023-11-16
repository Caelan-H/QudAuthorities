using System;

namespace XRL.World.Effects;

[Serializable]
public class Emboldened : Effect
{
	public string Statistic = "Hitpoints";

	public int Bonus;

	public Emboldened()
	{
		base.DisplayName = "Boosted " + Statistic;
	}

	public Emboldened(int Duration, string Statistic, int Amount)
		: this()
	{
		base.Duration = Duration;
		this.Statistic = Statistic;
		Bonus = Amount;
		base.DisplayName = "Boosted " + Statistic;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "emboldened";
	}

	public override string GetDetails()
	{
		return "+" + Bonus + " " + Statistic;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Emboldened"))
		{
			Emboldened emboldened = Object.GetEffect("Emboldened") as Emboldened;
			if (base.Duration > emboldened.Duration)
			{
				emboldened.Duration = base.Duration;
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyBoostStatistic", "Event", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " increases!");
		}
		base.StatShifter.SetStatShift(Statistic, Bonus);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " returns to normal.");
		}
		base.StatShifter.RemoveStatShifts();
	}
}

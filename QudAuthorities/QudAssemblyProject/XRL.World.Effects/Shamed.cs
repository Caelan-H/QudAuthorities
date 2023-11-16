using System;
using System.Text;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Shamed : Effect
{
	public int Penalty = 4;

	public int SpeedPenaltyPercent = 10;

	public int SpeedPenalty;

	public Shamed()
	{
		base.DisplayName = "{{r|shamed}}";
	}

	public Shamed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(-Penalty).Append(" DV\n").Append(-Penalty)
			.Append(" to-hit\n")
			.Append(-Penalty)
			.Append(" Willpower\n")
			.Append(-Penalty)
			.Append(" Ego\n");
		if (base.Object != null)
		{
			if (SpeedPenalty != 0)
			{
				stringBuilder.Append(-SpeedPenalty).Append(" Quickness");
			}
		}
		else
		{
			stringBuilder.Append(-SpeedPenaltyPercent).Append("% Quickness");
		}
		return stringBuilder.ToString();
	}

	public override bool SameAs(Effect e)
	{
		Shamed shamed = e as Shamed;
		if (shamed.Penalty != Penalty)
		{
			return false;
		}
		if (shamed.SpeedPenaltyPercent != SpeedPenaltyPercent)
		{
			return false;
		}
		if (shamed.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyShamed"))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyShamed", "Duration", base.Duration)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Shamed", this))
		{
			return false;
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "ApplyShamed");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanApplyShamed");
		Object.RegisterEffectEvent(this, "RollMeleeToHit");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "ApplyShamed");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanApplyShamed");
		Object.UnregisterEffectEvent(this, "RollMeleeToHit");
		base.Unregister(Object);
	}

	private void ApplyStats()
	{
		SpeedPenalty = base.Object.Stat("Speed") * SpeedPenaltyPercent / 100;
		base.StatShifter.SetStatShift("DV", -Penalty);
		base.StatShifter.SetStatShift("Ego", -Penalty);
		base.StatShifter.SetStatShift("Willpower", -Penalty);
		base.StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 40)
			{
				E.Tile = null;
				E.RenderString = ";";
				E.ColorString = "&r";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RollMeleeToHit")
		{
			if (Penalty != 0)
			{
				E.SetParameter("Result", E.GetIntParameter("Result") - Penalty);
			}
		}
		else
		{
			if (E.ID == "CanApplyShamed")
			{
				return false;
			}
			if (E.ID == "ApplyShamed")
			{
				if (base.Duration > 0)
				{
					base.Duration = E.GetIntParameter("Duration");
					return false;
				}
			}
			else if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyStats();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyStats();
			}
		}
		return base.FireEvent(E);
	}
}

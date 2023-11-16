using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Salve_Tonic : ITonicEffect
{
	public float HealAmount;

	public Salve_Tonic()
	{
	}

	public Salve_Tonic(int Duration)
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{Y|Salve}} tonic";
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "Recovers 0.9 hit points per level (minimum 5) each turn.";
		}
		return "Recovers 0.6 hit points per level (minimum 3) each turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("You feel a soothing tingle in your chest as your wounds start to close.");
		}
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("The soothing tingle fades.");
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndAction");
		Object.RegisterEffectEvent(this, "Overdose");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndAction");
		Object.UnregisterEffectEvent(this, "Overdose");
		base.Unregister(Object);
	}

	public override void ApplyAllergy(GameObject target)
	{
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndAction")
		{
			if (base.Object.HasStat("Hitpoints") && base.Object.HasStat("Level"))
			{
				int num = base.Object.BaseStat("Level");
				if (base.Object.IsTrueKin())
				{
					HealAmount += (float)Math.Max(5.0, Math.Ceiling(0.9 * (double)(float)num));
				}
				else
				{
					HealAmount += (float)Math.Max(3.0, Math.Ceiling(0.6 * (double)(float)num));
				}
				if (HealAmount >= 1f)
				{
					base.Object.Heal((int)Math.Floor(HealAmount), Message: true, FloatText: true, RandomMinimum: true);
					HealAmount -= (float)Math.Floor(HealAmount);
				}
			}
		}
		else if (E.ID == "Overdose" && base.Duration > 0)
		{
			if (base.Object.HasPart("TonicAllergy"))
			{
				TonicAllergy.SalveOverdose(base.Object);
			}
			else
			{
				base.Duration = 0;
				if (base.Object.IsPlayer())
				{
					if (base.Object.GetLongProperty("Overdosing") == 1)
					{
						Popup.Show("Your mutant physiology reacts adversely to the tonic. The soothing tingle fades.");
					}
					else
					{
						Popup.Show("The tonics you ingested react adversely to each other. The soothing tingle fades.");
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 15 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "+";
			E.ColorString = "&Y";
		}
		return true;
	}
}

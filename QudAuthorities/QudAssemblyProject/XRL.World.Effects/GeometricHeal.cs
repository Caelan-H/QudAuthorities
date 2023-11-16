using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class GeometricHeal : Effect
{
	public double Amount;

	public int Ratio;

	public bool Initial = true;

	public GeometricHeal()
	{
		base.DisplayName = "healing";
	}

	public GeometricHeal(double Amount, int Ratio, int Duration = 3)
		: this()
	{
		this.Amount = Amount;
		this.Ratio = Ratio;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 83886084;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		double num = 1.0 + (double)Ratio / 100.0;
		double num2 = Amount * (1.0 - Math.Pow(num, base.Duration)) / (1.0 - num);
		return string.Format("Recovering {0} hit points over {1} {2}.", (int)num2, base.Duration, (base.Duration == 1) ? "turn" : "turns");
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetEffect("GeometricHeal") is GeometricHeal geometricHeal)
		{
			if (base.Duration >= geometricHeal.Duration)
			{
				geometricHeal.Duration = base.Duration;
				geometricHeal.Amount = Amount;
				geometricHeal.Initial = Initial;
				if (geometricHeal.Initial)
				{
					geometricHeal.Heal();
				}
			}
			else if (Initial)
			{
				Heal();
			}
			return false;
		}
		if (!Object.HasStat("Hitpoints"))
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyGeometricHeal") || !ApplyEffectEvent.Check(Object, "GeometricHeal", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyGeometricHeal"))
		{
			return false;
		}
		DidX("begin", "healing", null, null, Object);
		if (Initial)
		{
			Heal();
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndActionEvent E)
	{
		if (Initial)
		{
			Initial = false;
		}
		else
		{
			Heal();
		}
		return base.HandleEvent(E);
	}

	public void Heal()
	{
		base.Object.Heal((int)Amount, Message: false, FloatText: true);
		Amount += Amount * (double)Ratio / 100.0;
	}

	public override bool Render(RenderEvent E)
	{
		if (Amount > 0.0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "Z";
				E.ColorString = "&g";
			}
		}
		return true;
	}
}

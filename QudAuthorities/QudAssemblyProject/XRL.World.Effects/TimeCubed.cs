using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class TimeCubed : Effect
{
	public int Amount;

	public TimeCubed()
	{
		base.DisplayName = "time cubed";
		base.Duration = 1;
	}

	public TimeCubed(int Amount)
		: this()
	{
		this.Amount = Amount;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override int GetEffectType()
	{
		return 67112960;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (base.Object.Stat("Energy") < 2000)
		{
			base.Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			if (base.Object.Energy != null && base.Object.Energy.Value > 1000)
			{
				base.Object.Energy.BaseValue = 1000;
			}
			base.Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.Energy != null)
		{
			if (Amount == 0)
			{
				Amount = 20000 + 3000 * Stat.Random(1, 10);
			}
			Object.Energy.BaseValue += Amount;
		}
		if (Object.HasEffect("TimeCubed"))
		{
			return false;
		}
		return true;
	}
}

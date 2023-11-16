using System;

namespace XRL.World.Effects;

[Serializable]
public class MobilityImpaired : Effect
{
	public int Amount;

	public MobilityImpaired()
	{
		base.DisplayName = "{{B|mobility impaired}}";
		base.Duration = 1;
	}

	public MobilityImpaired(int _Amount)
		: this()
	{
		Amount = _Amount;
	}

	public override int GetEffectType()
	{
		return 33555456;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as MobilityImpaired).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		if (Amount > 0)
		{
			return "-" + Amount + " move speed due to missing or broken limbs";
		}
		return "Mobility impaired due to missing or broken limbs.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("MobilityImpaired"))
		{
			return false;
		}
		if (Object.pBrain != null && !Object.pBrain.Mobile)
		{
			return false;
		}
		if (Object.IsPlayer() || Visible())
		{
			Object.ParticleText("*mobility impaired*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		}
		return true;
	}
}

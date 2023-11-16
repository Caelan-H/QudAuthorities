using System;

namespace XRL.World.Parts;

[Serializable]
public class Metal : IPart
{
	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == TransparentToEMPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 25;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && !ParentObject.HasTag("Creature") && E.Damage.IsAcidDamage())
		{
			E.Damage.Amount /= 4;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeNonflammable();
		return base.HandleEvent(E);
	}
}

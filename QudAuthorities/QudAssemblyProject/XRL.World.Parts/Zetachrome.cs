using System;

namespace XRL.World.Parts;

[Serializable]
public class Zetachrome : IPart
{
	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != GetItemElementsEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == TransparentToEMPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction += 30;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && !ParentObject.HasTag("Creature"))
		{
			if (E.Damage.IsHeatDamage() || E.Damage.IsColdDamage())
			{
				E.Damage.Amount = 0;
			}
			else if (E.Damage.IsAcidDamage())
			{
				E.Damage.Amount /= 4;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("jewels", 1);
		E.Add("stars", 1);
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeImperviousToHeat();
		return base.HandleEvent(E);
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class FungusProperties : IPart
{
	public bool Rooted = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AnimateEvent.ID || !Rooted) && ID != ApplyEffectEvent.ID && ID != CanApplyEffectEvent.ID && ID != EffectAppliedEvent.ID && ID != EquipperEquippedEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetKineticResistanceEvent.ID && ID != IsRootedInPlaceEvent.ID && ID != LeftCellEvent.ID)
		{
			if (ID == ObjectCreatedEvent.ID)
			{
				return Rooted;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		Rooted = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (ParentObject.IsFlying)
		{
			Rooted = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquipperEquippedEvent E)
	{
		if (Rooted && E.Item?.GetPart("Armor") is Armor armor && armor.WornOn == "Roots")
		{
			Rooted = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetKineticResistanceEvent E)
	{
		if (Rooted)
		{
			E.LinearIncrease += 200;
			E.PercentageIncrease += 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRootedInPlaceEvent E)
	{
		if (Rooted)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Rooted = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ParentObject.IsPotentiallyMobile())
		{
			Rooted = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Rooted", Rooted);
		return base.HandleEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (E.Name == "CardiacArrest")
		{
			return false;
		}
		if (Rooted)
		{
			if (E.Name == "Prone")
			{
				return false;
			}
			if (E.Name == "Wading")
			{
				return false;
			}
			if (E.Name == "Swimming")
			{
				return false;
			}
		}
		return true;
	}
}

using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class FusionReactor : IPoweredPart
{
	public int ChargeRate = 1000;

	public int ExplodeChance = 50;

	public int ExplodeForce = 10000;

	public string Description = "fusion reactor";

	public bool CatastrophicDisable;

	public bool PartWasDisabled;

	[NonSerialized]
	private bool ForceExplode;

	public FusionReactor()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		FusionReactor fusionReactor = p as FusionReactor;
		if (fusionReactor.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (fusionReactor.ExplodeChance != ExplodeChance)
		{
			return false;
		}
		if (fusionReactor.ExplodeForce != ExplodeForce)
		{
			return false;
		}
		if (fusionReactor.Description != Description)
		{
			return false;
		}
		if (fusionReactor.CatastrophicDisable != CatastrophicDisable)
		{
			return false;
		}
		if (fusionReactor.PartWasDisabled != PartWasDisabled)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID)
		{
			return ID == QueryChargeProductionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ChargeRate > ChargeUse)
		{
			E.Amount += (ChargeRate - ChargeUse) * E.Multiple;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (!PartWasDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			PartWasDisabled = true;
			if (CatastrophicDisable && ExplodeChance.in100())
			{
				ForceExplode = true;
				ParentObject.Die(null, "catastrophic " + Description + " failure", "Your " + Description + " failed catastrophically.", ParentObject.Its + " " + Description + " failed catastrophically.", Accidental: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			PartWasDisabled = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ForceExplode || (ExplodeChance.in100() && (CatastrophicDisable || !IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))))
		{
			Explode(ParentObject.IsCombatObject() ? ParentObject : E.Killer, FromDeathRemoval: true);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ChargeRate > ChargeUse)
		{
			ParentObject.ChargeAvailable(ChargeRate - ChargeUse, 0L);
		}
	}

	public bool Explode(GameObject Actor = null, bool FromDeathRemoval = false)
	{
		if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " " + Description + " explodes!", 'R');
		}
		ParentObject.Explode(ExplodeForce, Actor, null, 1f, Neutron: false, FromDeathRemoval);
		return true;
	}
}

using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class EmitSmoke : IPart
{
	public int ChanceOnEndTurn;

	public int ChanceOnUserEnteredCell;

	public string IfActivePartOperational;

	public override bool SameAs(IPart p)
	{
		EmitSmoke emitSmoke = p as EmitSmoke;
		if (emitSmoke.ChanceOnEndTurn != ChanceOnEndTurn)
		{
			return false;
		}
		if (emitSmoke.ChanceOnUserEnteredCell != ChanceOnUserEnteredCell)
		{
			return false;
		}
		if (emitSmoke.IfActivePartOperational != IfActivePartOperational)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public bool ShouldEmitSmoke()
	{
		if (!string.IsNullOrEmpty(IfActivePartOperational))
		{
			if (!(ParentObject.GetPart(IfActivePartOperational) is IActivePart activePart))
			{
				return false;
			}
			if (activePart.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ChanceOnEndTurn > 0 && ShouldEmitSmoke() && (ChanceOnEndTurn >= 100 || Stat.RandomCosmetic(1, 100) < ChanceOnEndTurn))
			{
				ParentObject.Smoke();
			}
		}
		else if (E.ID == "EnteredCell")
		{
			if (ChanceOnUserEnteredCell > 0 && ShouldEmitSmoke() && (ChanceOnUserEnteredCell >= 100 || Stat.RandomCosmetic(1, 100) < ChanceOnUserEnteredCell))
			{
				ParentObject.Smoke();
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "EnteredCell");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "EnteredCell");
		}
		return base.FireEvent(E);
	}
}

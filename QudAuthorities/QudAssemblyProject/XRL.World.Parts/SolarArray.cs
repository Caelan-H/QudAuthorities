using System;

namespace XRL.World.Parts;

[Serializable]
public class SolarArray : IPoweredPart
{
	public int ChargeRate = 10;

	public SolarArray()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as SolarArray).ChargeRate != ChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID)
		{
			return ID == QueryChargeProductionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ChargeRate", ChargeRate);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += (ChargeRate - ChargeUse) * E.Multiple;
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone == null)
		{
			return true;
		}
		if (!anyBasisZone.IsWorldMap() && anyBasisZone.Z > 10)
		{
			return true;
		}
		if (!IsDay())
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "RadiationFluxInsufficient";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
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

	public override void TenTurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ChargeRate > ChargeUse)
		{
			ParentObject.ChargeAvailable(ChargeRate - ChargeUse, 0L, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ChargeRate > ChargeUse)
		{
			ParentObject.ChargeAvailable(ChargeRate - ChargeUse, 0L, 100);
		}
	}
}

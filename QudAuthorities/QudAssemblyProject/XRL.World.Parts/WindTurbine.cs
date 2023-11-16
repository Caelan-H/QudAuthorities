using System;

namespace XRL.World.Parts;

[Serializable]
public class WindTurbine : IPoweredPart
{
	public float ChargeRateFactor = 0.2f;

	public WindTurbine()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as WindTurbine).ChargeRateFactor != ChargeRateFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == QueryChargeProductionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				E.Amount += (chargeRate - ChargeUse) * E.Multiple;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (GetChargeRate() <= 0)
		{
			return true;
		}
		return false;
	}

	public int GetChargeRate()
	{
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone == null)
		{
			return 0;
		}
		return (int)((float)anyBasisZone.CurrentWindSpeed * ChargeRateFactor);
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "WindSpeedInsufficient";
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
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				ParentObject.ChargeAvailable(chargeRate - ChargeUse, 0L);
			}
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				ParentObject.ChargeAvailable(chargeRate - ChargeUse, 0L, 10);
			}
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				ParentObject.ChargeAvailable(chargeRate - ChargeUse, 0L, 100);
			}
		}
	}
}

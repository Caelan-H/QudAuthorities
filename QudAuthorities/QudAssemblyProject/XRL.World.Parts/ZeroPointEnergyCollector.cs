using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ZeroPointEnergyCollector : IPoweredPart
{
	public int ChargeRate = 10;

	public string World = "JoppaWorld";

	public string Plane = "*";

	public List<string> Worlds;

	public ZeroPointEnergyCollector()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsRealityDistortionBased = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		ZeroPointEnergyCollector zeroPointEnergyCollector = p as ZeroPointEnergyCollector;
		if (zeroPointEnergyCollector.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (zeroPointEnergyCollector.World != World)
		{
			return false;
		}
		if (zeroPointEnergyCollector.Plane != Plane)
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
		if (string.IsNullOrEmpty(World))
		{
			World = anyBasisZone.ZoneWorld;
			Worlds = World.CachedCommaExpansion();
		}
		else if (World != "*")
		{
			if (Worlds == null)
			{
				Worlds = World.CachedCommaExpansion();
			}
			if (!Worlds.Contains(anyBasisZone.ZoneWorld))
			{
				return true;
			}
		}
		if (Plane != "*" && WorldFactory.Factory.getWorld(anyBasisZone.ZoneWorld).Plane != Plane)
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "QuantumPhaseMismatch";
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

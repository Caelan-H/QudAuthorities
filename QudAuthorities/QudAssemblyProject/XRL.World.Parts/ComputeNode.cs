using System;

namespace XRL.World.Parts;

[Serializable]
public class ComputeNode : IPoweredPart
{
	public int Power = 20;

	public ComputeNode()
	{
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool WantTurnTick()
	{
		return ChargeUse > 0;
	}

	public override bool WantTenTurnTick()
	{
		return ChargeUse > 0;
	}

	public override bool WantHundredTurnTick()
	{
		return ChargeUse > 0;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAvailableComputePowerEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += GetEffectivePower();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Power != 0 && (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnImplantee))
		{
			E.Postfix.Append("\n{{rules|When equipped");
			if (ChargeUse > 0)
			{
				E.Postfix.Append(" and powered");
			}
			int effectivePower = GetEffectivePower();
			E.Postfix.Append(", provides ").Append(effectivePower).Append(' ')
				.Append((effectivePower == 1) ? "unit" : "units")
				.Append(" of compute power to the local lattice.");
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public int GetEffectivePower()
	{
		int num = Power;
		int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}
}

using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ThermalAmp : IPoweredPart
{
	public int HeatDamage;

	public int ColdDamage;

	public int ModifyHeat;

	public int ModifyCold;

	public ThermalAmp()
	{
		ChargeUse = 0;
		WorksOnWearer = true;
		IsPowerLoadSensitive = false;
		NameForStatus = "ThermalAmp";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int powerLoad = MyPowerLoadLevel();
		string statusSummary = GetStatusSummary();
		AppendRule(E.Postfix, GetPercentage(HeatDamage, powerLoad), "heat damage dealt", "R", "r", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ColdDamage, powerLoad), "cold damage dealt", "C", "c", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ModifyHeat, powerLoad), "to the intensity of your heating effects", "R", "r", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ModifyCold, powerLoad), "to the intensity of your cooling effects", "C", "c", statusSummary);
		return base.HandleEvent(E);
	}

	public void AppendRule(StringBuilder SB, int Value, string Effect, string Positive = "rules", string Negative = "rules", string Status = null)
	{
		if (Value != 0)
		{
			SB.Compound("{{", '\n').Append((Value > 0) ? Positive : Negative).Append('|')
				.AppendSigned(Value)
				.Append('%')
				.Compound(Effect)
				.Append("}}");
			AddStatusSummary(SB, Status);
		}
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "AttackerDealingDamage");
		E.Actor.RegisterPartEvent(this, "AttackerBeforeTemperatureChange");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AttackerDealingDamage");
		E.Actor.UnregisterPartEvent(this, "AttackerBeforeTemperatureChange");
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerDealingDamage")
		{
			int num = MyPowerLoadLevel();
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num) && E.GetParameter("Damage") is Damage damage)
			{
				if (HeatDamage != 0 && damage.IsHeatDamage())
				{
					damage.Amount *= 100 + GetPercentage(HeatDamage, num);
					damage.Amount /= 100;
				}
				else if (ColdDamage != 0 && damage.IsColdDamage())
				{
					damage.Amount *= 100 + GetPercentage(ColdDamage, num);
					damage.Amount /= 100;
				}
			}
		}
		else if (E.ID == "AttackerBeforeTemperatureChange")
		{
			int num2 = MyPowerLoadLevel();
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num2))
			{
				int intParameter = E.GetIntParameter("Amount");
				if (intParameter > 0 && ModifyHeat != 0)
				{
					intParameter *= 100 + GetPercentage(ModifyHeat, num2);
					intParameter /= 100;
					E.SetParameter("Amount", intParameter);
				}
				else if (intParameter < 0 && ModifyCold != 0)
				{
					intParameter *= 100 + GetPercentage(ModifyCold, num2);
					intParameter /= 100;
					E.SetParameter("Amount", intParameter);
				}
			}
		}
		return base.FireEvent(E);
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
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}

	public int GetPercentage(int Value, int PowerLoad)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return Value;
		}
		return Value * (100 + num) / 100;
	}
}

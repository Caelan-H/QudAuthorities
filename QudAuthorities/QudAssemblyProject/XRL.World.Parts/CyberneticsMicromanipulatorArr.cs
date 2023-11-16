using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMicromanipulatorArray : IPoweredPart
{
	public CyberneticsMicromanipulatorArray()
	{
		WorksOnImplantee = true;
		ChargeUse = 0;
		NameForStatus = "ServoMechanisms";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetEnergyCostEvent.ID && ID != GetTinkeringBonusEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "CombatPreventsRepair");
		E.Implantee.RegisterPartEvent(this, "CombatPreventsTinkering");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "CombatPreventsRepair");
		E.Implantee.UnregisterPartEvent(this, "CombatPreventsTinkering");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (WasReady() && !string.IsNullOrEmpty(E.Type) && E.Type.Contains("Skill Tinkering") && !E.Type.Contains("Build") && !E.Type.Contains("Mod") && !E.Type.Contains("Repair") && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.PercentageReduction = Math.Max(Math.Min(40 + GetAvailableComputePowerEvent.GetFor(ParentObject.Implantee) / 2, 60), E.PercentageReduction);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if ((E.Type == "Examine" || E.Type == "Repair" || E.Type == "ReverseEngineerTurns" || E.Type == "Hacking" || E.Type == "ItemModding") && E.Actor == ParentObject.Implantee)
		{
			E.Bonus++;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CombatPreventsTinkering")
		{
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "CombatPreventsRepair" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}

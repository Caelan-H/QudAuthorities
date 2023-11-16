using System;

namespace XRL.World.Parts;

[Serializable]
public class FlareCompensation : IPoweredPart
{
	public int Level = 6;

	public int LevelHardened;

	public bool ShowInShortDescription = true;

	public float ComputePowerFactor;

	public FlareCompensation()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		FlareCompensation flareCompensation = p as FlareCompensation;
		if (flareCompensation.Level != Level)
		{
			return false;
		}
		if (flareCompensation.LevelHardened != LevelHardened)
		{
			return false;
		}
		if (flareCompensation.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		if (flareCompensation.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
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
		ConsumeChargeIfOperational();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && (ID != GetShortDescriptionEvent.ID || !ShowInShortDescription) && ID != ImplantedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			E.Postfix.AppendRules("Offers protection against visual flash effects.", base.AddStatusSummary);
			if (ComputePowerFactor > 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
			}
			else if (ComputePowerFactor < 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice decreases this item's effectiveness.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "ModifyFlashbang");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "ModifyFlashbang");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "ModifyFlashbang");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "ModifyFlashbang");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ModifyFlashbang");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyFlashbang")
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(this, LevelHardened, ComputePowerFactor);
			int num2 = GetAvailableComputePowerEvent.AdjustUp(this, Level, ComputePowerFactor);
			if (num != 0 && IsReady(UseCharge: false, IgnoreCharge: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: true, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.SetParameter("Amount", Math.Max(E.GetIntParameter("Amount") - num, 0));
			}
			if (num2 != 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.SetParameter("Amount", Math.Max(E.GetIntParameter("Amount") - num2, 0));
			}
		}
		return base.FireEvent(E);
	}
}

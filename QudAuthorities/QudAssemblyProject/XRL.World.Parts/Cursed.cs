using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Cursed : IActivePart
{
	public bool RevealInDescription;

	public string DescriptionPostfix = "Cannot be removed once equipped.";

	public Cursed()
	{
		WorksOnEquipper = true;
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
		if (!base.WantEvent(ID, cascade) && ID != AllowHugeHandsEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return RevealInDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AllowHugeHandsEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (RevealInDescription)
		{
			E.Postfix.AppendRules(DescriptionPostfix);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingUnequipped");
		Object.RegisterPartEvent(this, "CanBeUnequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeUnequipped")
		{
			if (!E.HasFlag("Forced") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "BeginBeingUnequipped" && !E.HasFlag("Forced") && ParentObject.Equipped != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			string text = "You can't remove " + ParentObject.the + ParentObject.ShortDisplayName + "!";
			E.SetParameter("FailureMessage", text);
			if (!E.IsSilent() && !E.HasFlag("SemiForced") && ParentObject.Equipped.IsPlayer() && E.GetIntParameter("AutoEquipTry") <= 1)
			{
				Popup.Show(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}

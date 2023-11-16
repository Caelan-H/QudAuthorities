using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Smartgun : IPoweredPart
{
	public int Level = 1;

	public string EquipperPropertyEnables = "TechScannerEquipped";

	public string EquipperEventEnables = "HandleSmartData";

	public long UseTurn;

	[NonSerialized]
	public static Event eHandleSmartData = new ImmutableEvent("HandleSmartData");

	public Smartgun()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Smartgun smartgun = p as Smartgun;
		if (smartgun.Level != Level)
		{
			return false;
		}
		if (smartgun.EquipperPropertyEnables != EquipperPropertyEnables)
		{
			return false;
		}
		if (smartgun.EquipperEventEnables != EquipperEventEnables)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "ModifyAimVariance");
		Object.RegisterPartEvent(this, "ModifyMissileWeaponToHit");
		base.Register(Object);
	}

	public bool IsDataHandled()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(EquipperPropertyEnables) && equipped.GetIntProperty(EquipperPropertyEnables) > 0)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(EquipperEventEnables))
		{
			if (EquipperEventEnables == eHandleSmartData.ID)
			{
				if (!equipped.FireEvent(eHandleSmartData))
				{
					return true;
				}
			}
			else if (!equipped.FireEvent(EquipperEventEnables))
			{
				return true;
			}
		}
		return false;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return !IsDataHandled();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "DataConsumerNotFound";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyAimVariance")
		{
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (UseTurn < XRLCore.Core.Game.Turns)
				{
					UseTurn = XRLCore.Core.Game.Turns;
					ConsumeCharge();
				}
				E.SetParameter("Amount", E.GetIntParameter("Amount") - Level);
			}
		}
		else if (E.ID == "ModifyMissileWeaponToHit" && E.GetGameObjectParameter("Target") == E.GetGameObjectParameter("Defender") && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (UseTurn < XRLCore.Core.Game.Turns)
			{
				UseTurn = XRLCore.Core.Game.Turns;
				ConsumeCharge();
			}
			E.SetParameter("Amount", E.GetIntParameter("Amount") + Level);
		}
		return base.FireEvent(E);
	}
}

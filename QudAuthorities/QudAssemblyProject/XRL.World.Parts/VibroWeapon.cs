using System;

namespace XRL.World.Parts;

[Serializable]
public class VibroWeapon : IPoweredPart
{
	public int PenetrationBonus;

	public VibroWeapon()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as VibroWeapon).PenetrationBonus != PenetrationBonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GetWeaponPenModifier");
		Object.RegisterPartEvent(this, "IsAdaptivePenetrationActive");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsAdaptivePenetrationActive")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (PenetrationBonus != 0)
				{
					E.SetParameter("Bonus", E.GetIntParameter("Bonus") + PenetrationBonus);
				}
				return false;
			}
		}
		else if (E.ID == "GetWeaponPenModifier" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int intParameter = E.GetIntParameter("AV");
			E.SetParameter("Penetrations", intParameter + PenetrationBonus);
			E.SetParameter("MaxStrengthBonus", intParameter + PenetrationBonus);
		}
		return base.FireEvent(E);
	}
}

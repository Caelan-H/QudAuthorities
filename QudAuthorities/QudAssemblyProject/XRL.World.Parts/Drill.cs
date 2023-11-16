using System;

namespace XRL.World.Parts;

[Serializable]
public class Drill : IPoweredPart
{
	public int PenetrationBonus = 24;

	public int HitsRequired = 4;

	public Drill()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		Drill drill = p as Drill;
		if (drill.PenetrationBonus != PenetrationBonus)
		{
			return false;
		}
		if (drill.HitsRequired != HitsRequired)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		Object.RegisterPartEvent(this, "GetWeaponHitDice");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetWeaponHitDice")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && gameObjectParameter.IsDiggable() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.SetParameter("PenetrationBonus", PenetrationBonus);
			}
		}
		else if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2 != null && gameObjectParameter2.IsDiggable() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				(E.GetParameter("Damage") as Damage).Amount = (int)Math.Ceiling((float)gameObjectParameter2.BaseStat("Hitpoints") / (float)HitsRequired);
			}
		}
		return base.FireEvent(E);
	}
}

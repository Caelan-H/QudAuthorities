using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: damage and voltage are increased by the standard
///             power load bonus, i.e. 2 for the standard overload power load of 400.
///             </remarks>
[Serializable]
public class DischargeOnHit : IActivePart
{
	public string Voltage = "2d4";

	public string Damage = "1d4";

	public DischargeOnHit()
	{
		WorksOnSelf = true;
		NameForStatus = "DischargeGenerator";
	}

	public DischargeOnHit(string Voltage, string Damage)
		: this()
	{
		this.Voltage = Voltage;
		this.Damage = Damage;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		DischargeOnHit dischargeOnHit = p as DischargeOnHit;
		if (dischargeOnHit.Voltage != Voltage)
		{
			return false;
		}
		if (dischargeOnHit.Damage != Damage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AdjustWeaponScore");
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "ProjectileHit" || E.ID == "WeaponThrowHit")
		{
			int num = MyPowerLoadLevel();
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				int num2 = MyPowerLoadBonus(num);
				int voltage = Voltage.RollCached() + num2;
				string damage = Damage;
				if (num2 != 0)
				{
					damage = DieRoll.AdjustResult(damage, num2);
				}
				if (E.ID == "WeaponHit")
				{
					gameObjectParameter.Discharge(gameObjectParameter2.CurrentCell, voltage, Damage, gameObjectParameter);
				}
				else
				{
					gameObjectParameter2.Discharge(gameObjectParameter2.CurrentCell, voltage, Damage, gameObjectParameter);
				}
			}
		}
		else if (E.ID == "AdjustWeaponScore" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int intParameter = E.GetIntParameter("Score");
			int num3 = MyPowerLoadBonus();
			int num4 = Math.Max(Stat.RollMin(Voltage) / 2 + Stat.RollMax(Voltage) / 4 + num3 + (Stat.RollMin(Damage) + Stat.RollMax(Damage) / 2 + num3), 1);
			E.SetParameter("Score", intParameter + num4);
		}
		return base.FireEvent(E);
	}
}

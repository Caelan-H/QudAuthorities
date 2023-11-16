using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, chance to activate is increased by a
///             percentage equal to ((power load - 100) / 10), i.e. 30% for
///             the standard overload power load of 400, and save targets are
///             increased by the standard power load bonus, i.e. 2 for the standard
///             overload power load of 400.
///             </remarks>
[Serializable]
public class StunOnHit : IPoweredPart
{
	public string Duration = "1";

	public int Chance;

	public int SaveTarget = 12;

	public StunOnHit()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "StunMechanism";
	}

	public override bool SameAs(IPart p)
	{
		StunOnHit stunOnHit = p as StunOnHit;
		if (stunOnHit.Duration != Duration)
		{
			return false;
		}
		if (stunOnHit.Chance != Chance)
		{
			return false;
		}
		if (stunOnHit.SaveTarget != SaveTarget)
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
		Object.RegisterPartEvent(this, "AdjustWeaponScore");
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			int num = MyPowerLoadLevel();
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			int num2 = Chance;
			int num3 = IComponent<GameObject>.PowerLoadBonus(num, 100, 10);
			if (num3 != 0)
			{
				num2 = num2 * (100 + num3) / 100;
			}
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			num2 = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part StunOnHit Activation", num2, subject);
			if (num2.in100() && gameObjectParameter.PhaseMatches(gameObjectParameter2) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				int duration = Duration.RollCached();
				int saveTarget = SaveTarget + IComponent<GameObject>.PowerLoadBonus(num);
				gameObjectParameter2.ApplyEffect(new Stun(duration, saveTarget));
			}
		}
		else if (E.ID == "AdjustWeaponScore" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int @for = GetSpecialEffectChanceEvent.GetFor(E.GetGameObjectParameter("User"), ParentObject, "Part StunOnHit Activation", Chance);
			int intParameter = E.GetIntParameter("Score");
			int num4 = 8 * (Duration.RollMinCached() * 2 + Duration.RollMaxCached()) * @for * (IsPowerLoadSensitive ? (SaveTarget + MyPowerLoadBonus()) : SaveTarget) / 2000;
			E.SetParameter("Score", intParameter + num4);
		}
		return base.FireEvent(E);
	}
}

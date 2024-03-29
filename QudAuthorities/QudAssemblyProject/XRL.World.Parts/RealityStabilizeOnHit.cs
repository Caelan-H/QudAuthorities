using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RealityStabilizeOnHit : IPoweredPart
{
	public int Chance = 100;

	public string Strength = "100";

	public string Duration = "20-30";

	public bool Linked;

	public float ComputePowerFactor = 1f;

	public RealityStabilizeOnHit()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "NormalityProjection";
	}

	public override bool SameAs(IPart p)
	{
		RealityStabilizeOnHit realityStabilizeOnHit = p as RealityStabilizeOnHit;
		if (realityStabilizeOnHit.Chance != Chance)
		{
			return false;
		}
		if (realityStabilizeOnHit.Strength != Strength)
		{
			return false;
		}
		if (realityStabilizeOnHit.Duration != Duration)
		{
			return false;
		}
		if (realityStabilizeOnHit.Linked != Linked)
		{
			return false;
		}
		if (realityStabilizeOnHit.ComputePowerFactor != ComputePowerFactor)
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
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			GameObject projectile = gameObjectParameter3;
			GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part RealityStabilizeOnHit Activation", Chance, subject, projectile);
			if (Chance.in100())
			{
				CheckRealityStabilization(gameObjectParameter, gameObjectParameter2, E.ID == "ProjectileHit");
			}
		}
		return base.FireEvent(E);
	}

	public void CheckRealityStabilization(GameObject who, GameObject target, bool asProjectile = false)
	{
		GameObject gameObject = ((!asProjectile) ? ParentObject : ParentObject.GetPart<Projectile>()?.Launcher);
		int num = gameObject?.GetPowerLoadLevel() ?? 100;
		int num2 = 0;
		RealityStabilization realityStabilization = null;
		if (Linked)
		{
			realityStabilization = gameObject?.GetPart("RealityStabilization") as RealityStabilization;
			if (realityStabilization == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(Strength))
			{
				realityStabilization.FlushEffectiveStrengthCache();
				if (!realityStabilization.HasAnyWorksOn())
				{
					realityStabilization.LastStatus = ActivePartStatus.Operational;
				}
				realityStabilization.Strength = GetAvailableComputePowerEvent.AdjustUp(who, Strength.RollCached(), ComputePowerFactor) + IComponent<GameObject>.PowerLoadBonus(num, 100, 30);
			}
			num2 = Math.Max(realityStabilization.EffectiveStrength, 0);
		}
		else
		{
			num2 = Strength.RollCached();
		}
		if (num2 <= 0 || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num) || (realityStabilization != null && !realityStabilization.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num)))
		{
			return;
		}
		ConsumeCharge(null, num);
		realityStabilization?.ConsumeCharge(null, num);
		int num3 = GetAvailableComputePowerEvent.AdjustUp(who, Duration.RollCached() * (100 + IComponent<GameObject>.PowerLoadBonus(num, 100, 10)) / 100, ComputePowerFactor);
		RealityStabilized realityStabilized = target.GetEffectByClassName("RealityStabilized") as RealityStabilized;
		if (realityStabilized == null)
		{
			realityStabilized = new RealityStabilized();
			if (!target.ForceApplyEffect(realityStabilized))
			{
				return;
			}
		}
		if (realityStabilized.IndependentStrength < num2)
		{
			realityStabilized.IndependentStrength = num2;
		}
		if (realityStabilized.Duration < num3)
		{
			realityStabilized.Duration = num3;
		}
		if (!GameObject.validate(ref realityStabilized.Owner))
		{
			realityStabilized.Owner = who;
		}
		realityStabilized.Stabilize();
	}
}

using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class VampiricWeapon : IActivePart
{
	public int Chance = 100;

	public string Percent;

	public string Reduction;

	public string Maximum;

	public bool WorksInMelee = true;

	public bool WorksThrown = true;

	public bool WorksAsProjectile = true;

	public bool RealityDistortionBased;

	public bool RequiresLivingTarget;

	public VampiricWeapon()
	{
		WorksOnSelf = true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "WeaponDealDamage" || E.ID == "WeaponThrowHit" || E.ID == "ProjectileHit") && ((E.ID == "WeaponDealDamage" && WorksInMelee) || (E.ID == "WeaponThrowHit" && WorksThrown) || (E.ID == "ProjectileHit" && WorksAsProjectile)) && E.GetParameter("Damage") is Damage damage && damage.Amount > 0)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Projectile");
			if (!RequiresLivingTarget || gameObjectParameter2.IsAlive)
			{
				GameObject subject = gameObjectParameter2;
				GameObject projectile = gameObjectParameter4;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter3, "Part VampiricWeapon Activation", Chance, subject, projectile).in100())
				{
					bool flag = !RealityDistortionBased;
					if (!flag)
					{
						Cell cell = gameObjectParameter2.CurrentCell;
						Event @event = Event.New("InitiateRealityDistortionTransit");
						@event.SetParameter("Object", gameObjectParameter);
						@event.SetParameter("Device", E.GetGameObjectParameter("Launcher") ?? ParentObject);
						@event.SetParameter("Operator", gameObjectParameter);
						@event.SetParameter("Cell", cell);
						if (gameObjectParameter.FireEvent(@event, E) && cell.FireEvent(@event, E))
						{
							flag = true;
						}
					}
					if (flag && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
					{
						int num = damage.Amount;
						if (!string.IsNullOrEmpty(Percent))
						{
							num = num * Stat.Roll(Percent) / 100;
						}
						if (!string.IsNullOrEmpty(Reduction))
						{
							num -= Stat.Roll(Reduction);
						}
						if (!string.IsNullOrEmpty(Maximum))
						{
							num = Math.Min(num, Stat.Roll(Maximum));
						}
						if (num > 0)
						{
							gameObjectParameter.Heal(num, Message: true, FloatText: true);
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

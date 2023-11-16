using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LifeDrainOnHit : IPart
{
	public string Damage = "15-20";

	public int Chance = 100;

	public bool RealityDistortionBased = true;

	public LifeDrainOnHit()
	{
	}

	public LifeDrainOnHit(string Damage, int Chance)
		: this()
	{
		this.Damage = Damage;
		this.Chance = Chance;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject obj = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.validate(ref obj))
			{
				GameObject subject = obj;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part LifeDrainOnHit Activation", Chance, subject, projectile).in100())
				{
					obj.ApplyEffect(new LifeDrain(2, 10, Damage, gameObjectParameter, RealityDistortionBased));
					obj.GetAngryAt(gameObjectParameter, -100);
				}
			}
		}
		return base.FireEvent(E);
	}
}

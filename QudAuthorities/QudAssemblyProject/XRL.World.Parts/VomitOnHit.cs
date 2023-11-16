using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class VomitOnHit : IPart
{
	public int Hurls = 3;

	public int Chance = 100;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "DealingMissileDamage");
		Object.RegisterPartEvent(this, "WeaponMissileWeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "AttackerAfterDamage" || E.ID == "DealingMissileDamage" || E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject obj = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.validate(ref obj) && obj.HasPart("Stomach"))
			{
				GameObject subject = obj;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part VomitOnHit Activation", Chance, subject, projectile).in100())
				{
					bool flag = false;
					for (int num = Hurls; num > 0; num--)
					{
						LiquidVolume.getLiquid("putrid").Drank(null, 0, obj, new StringBuilder());
						if (!flag && obj.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("It's disgusting!");
							flag = true;
						}
						IComponent<GameObject>.XDidY(obj, "vomit", null, "!", null, null, obj);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PoisonOnHit : IPart
{
	public int Chance = 100;

	public string Strength = "15";

	public string DamageIncrement = "3d3";

	public string Duration = "6-9";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject obj = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Projectile");
			if (GameObject.validate(ref obj))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = obj;
				GameObject projectile = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part PoisonOnHit Activation", Chance, subject, projectile).in100())
				{
					int num = Strength.RollCached();
					if (!obj.MakeSave("Toughness", num, null, null, "Injected Damaging Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
					{
						obj.ApplyEffect(new Poisoned(Duration.RollCached(), DamageIncrement, num, ParentObject.Equipped));
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

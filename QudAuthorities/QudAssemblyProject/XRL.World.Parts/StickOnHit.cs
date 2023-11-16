using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StickOnHit : IPart
{
	public string Duration = "5";

	public int Chance;

	public int SaveTarget = 15;

	public string SaveVs = "Stuck Restraint";

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
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part StickOnHit Activation", Chance, subject, projectile).in100())
				{
					obj.ApplyEffect(new Stuck(Duration.RollCached(), SaveTarget, SaveVs));
				}
			}
		}
		return base.FireEvent(E);
	}
}

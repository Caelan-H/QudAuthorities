using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class MutateOnHit : IPart
{
	public int Chance = 100;

	public string ResultTable = "MutatingResults";

	public int IncubationTime = 100;

	public string SaveTarget = "20";

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
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part MutateOnHit Activation", Chance, subject, projectile).in100() && !obj.MakeSave("Toughness", SaveTarget.RollCached(), null, null, "Mutation", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
				{
					obj.ApplyEffect(new Mutating(IncubationTime, ResultTable));
				}
			}
		}
		return base.FireEvent(E);
	}
}

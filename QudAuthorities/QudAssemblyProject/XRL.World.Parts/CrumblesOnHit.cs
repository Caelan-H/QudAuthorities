using System;

namespace XRL.World.Parts;

[Serializable]
public class CrumblesOnHit : IPart
{
	public int Chance = 100;

	[NonSerialized]
	private bool Crumbling;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		Object.RegisterPartEvent(this, "WeaponAfterAttack");
		Object.RegisterPartEvent(this, "ThrownProjectileHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit")
		{
			if (Chance.in100())
			{
				Crumbling = true;
			}
		}
		else if (Crumbling && (E.ID == "WeaponAfterAttack" || E.ID == "ThrownProjectileHit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				DidX("crumble", "to dust", "!", null, null, gameObjectParameter);
			}
			ParentObject.Destroy(null, Silent: true);
		}
		return base.FireEvent(E);
	}
}

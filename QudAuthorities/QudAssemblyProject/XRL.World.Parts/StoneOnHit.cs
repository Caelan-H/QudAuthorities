using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StoneOnHit : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			E.GetGameObjectParameter("Defender").ApplyEffect(new BasiliskPoison(1, gameObjectParameter));
		}
		return base.FireEvent(E);
	}
}

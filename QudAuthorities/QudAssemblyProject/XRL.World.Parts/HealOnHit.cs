using System;

namespace XRL.World.Parts;

[Serializable]
public class HealOnHit : IPart
{
	public int Chance;

	public string Amount = "15-25";

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
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part HealOnHit Activation", Chance, subject).in100() && gameObjectParameter2.HasStat("Hitpoints") && gameObjectParameter2.GetIntProperty("Inorganic") == 0)
			{
				gameObjectParameter2.Heal(Amount.RollCached(), Message: true, FloatText: true);
			}
		}
		return base.FireEvent(E);
	}
}

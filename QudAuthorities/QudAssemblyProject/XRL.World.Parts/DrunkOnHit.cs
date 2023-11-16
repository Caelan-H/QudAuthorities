using System;

namespace XRL.World.Parts;

[Serializable]
public class DrunkOnHit : IPart
{
	public int Chance;

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
			GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part DrunkOnHit Activation", Chance, subject);
			if (Chance.in100())
			{
				for (int i = 0; i < 4; i++)
				{
					LiquidVolume.getLiquid("wine").Drank(null, 0, gameObjectParameter2, Event.NewStringBuilder());
				}
			}
		}
		return base.FireEvent(E);
	}
}

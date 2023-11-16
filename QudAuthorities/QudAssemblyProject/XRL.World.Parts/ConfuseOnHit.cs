using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ConfuseOnHit : IPart
{
	public int Chance = 100;

	public int Strength = 25;

	public string Duration = "10-15";

	public int Level = 10;

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
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part ConfuseOnHit Activation", Chance, subject).in100() && gameObjectParameter2 != null && !gameObjectParameter2.MakeSave("Willpower", Strength, null, null, "Confusion", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				gameObjectParameter2.ApplyEffect(new Confused(Duration.RollCached(), Level, Level + 2));
			}
		}
		return base.FireEvent(E);
	}
}

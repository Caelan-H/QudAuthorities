using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class MonochromePoisonOnDamage : IPart
{
	public int Chance = 100;

	public string Duration = "50-100";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "AttackerHit");
		Object.RegisterPartEvent(this, "WeaponAfterDamage");
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit" || E.ID == "WeaponHit")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject obj = E.GetGameObjectParameter("Defender");
				GameObject obj2 = E.GetGameObjectParameter("Weapon");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Projectile");
				if (GameObject.validate(ref obj) && GameObject.validate(ref obj2) && obj2.HasTag("NaturalGear"))
				{
					GameObject @object = obj2;
					GameObject subject = obj;
					GameObject projectile = gameObjectParameter2;
					if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, @object, "Part MonochromePoisonOnHit Activation", Chance, subject, projectile).in100())
					{
						E.SetParameter("DidSpecialEffect", 1);
						string text = E.GetStringParameter("Properties", "") ?? "";
						if (!text.Contains("Monochromed"))
						{
							E.SetParameter("Properties", (text == "") ? "Monochromed" : (text + ",Monochromed"));
						}
					}
				}
			}
		}
		else if (E.ID == "AttackerAfterDamage" || E.ID == "WeaponAfterDamage")
		{
			_ = ParentObject;
			if (ParentObject.Equipped != null)
			{
				_ = ParentObject.Equipped;
			}
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Defender");
			if ((E.GetStringParameter("Properties", "") ?? "").Contains("Monochromed") && gameObjectParameter3.ApplyEffect(new MonochromeOnset()))
			{
				if (gameObjectParameter3.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your vision blurs.");
				}
				E.SetParameter("DidSpecialEffect", 1);
			}
		}
		return base.FireEvent(E);
	}
}

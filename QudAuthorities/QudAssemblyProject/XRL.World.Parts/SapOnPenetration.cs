using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SapOnPenetration : IPart
{
	public int Chance = 100;

	public string Stat = "Hitpoints";

	public string Amount = "1-2";

	[NonSerialized]
	public static Dictionary<string, string> sapPresentation = new Dictionary<string, string> { { "Hitpoints", "life" } };

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "AttackerHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject obj = E.GetGameObjectParameter("Defender");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
				if (GameObject.validate(ref obj) && obj.HasStat(Stat))
				{
					GameObject subject = obj;
					GameObject projectile = gameObjectParameter3;
					if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part SapOnPenetration Activation", Chance, subject, projectile).in100() && (gameObjectParameter2 == null || gameObjectParameter2.HasTag("NaturalGear")))
					{
						E.SetParameter("DidSpecialEffect", 1);
						string text = E.GetStringParameter("Properties", "") ?? "";
						if (!text.Contains("DrainedStat"))
						{
							E.SetParameter("Properties", (text == "") ? "DrainedStat" : (text + ",DrainedStat"));
						}
					}
				}
			}
		}
		else if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Defender");
			if ((E.GetStringParameter("Properties") ?? "").Contains("DrainedStat"))
			{
				int num = XRL.Rules.Stat.Roll(Amount);
				string text2 = ((num != 1) ? "points" : "point");
				gameObjectParameter4.GetStat(Stat).BaseValue -= num;
				if (!sapPresentation.TryGetValue(Stat, out var value))
				{
					value = Stat;
				}
				DidXToY("permanently drain", gameObjectParameter4, value + " by " + num + " " + text2, "!", null, null, gameObjectParameter4, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
				E.SetFlag("DidSpecialEffect", State: true);
				gameObjectParameter4.GetAngryAt(ParentObject, -1000);
			}
		}
		return base.FireEvent(E);
	}
}

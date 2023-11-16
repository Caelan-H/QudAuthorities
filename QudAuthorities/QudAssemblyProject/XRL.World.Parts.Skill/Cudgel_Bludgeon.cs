using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Bludgeon : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterAttack");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterAttack")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject obj = E.GetGameObjectParameter("Defender");
			GameObject obj2 = E.GetGameObjectParameter("Weapon");
			if (GameObject.validate(ref obj) && GameObject.validate(ref obj2) && obj2.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.Skill == "Cudgel")
			{
				string text = E.GetStringParameter("Properties", "") ?? "";
				if (ParentObject.HasSkill("Cudgel_Conk") && text.Contains("Conking") && obj.HasEffect("Stun"))
				{
					obj.ApplyEffect(new Asleep(Stat.Random(30, 40)));
				}
				else
				{
					int num = 50;
					if (text.Contains("Conking"))
					{
						num = 100;
					}
					else if (text.Contains("Charging") && ParentObject.HasSkill("Cudgel_ChargingStrike"))
					{
						num = 100;
					}
					else if (ParentObject.HasEffect("SmashingUp"))
					{
						num = 100;
					}
					else if (ParentObject.HasIntProperty("ImprovedBludgeon"))
					{
						num += num * ParentObject.GetIntProperty("ImprovedBludgeon");
					}
					num = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, ParentObject, "Skill Bludgeon", num);
					if (num.in100() && obj.ApplyEffect(new Dazed(Stat.Random(3, 4), DontStunIfPlayer: true)) && obj.HasPart("Combat"))
					{
						IComponent<GameObject>.XDidY(obj, "reel", "from the force of " + (ParentObject.IsPlayer() ? "your" : Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName)) + " bludgeoning", null, null, null, obj);
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return true;
	}
}

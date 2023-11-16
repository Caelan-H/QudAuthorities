using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Expertise : BaseSkill
{
	public int HitBonus = 2;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerRollMeleeToHit");
		Object.RegisterPartEvent(this, "DealDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerRollMeleeToHit")
		{
			if (E.GetStringParameter("Skill") == "Cudgel" && HitBonus != 0)
			{
				E.SetParameter("Result", E.GetIntParameter("Result") + HitBonus);
			}
		}
		else if (E.ID == "DealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null)
			{
				MeleeWeapon part = gameObjectParameter.GetPart<MeleeWeapon>();
				if (part != null && part.Skill == "Cudgel" && ParentObject.HasSkill("Cudgel_Bonecrusher"))
				{
					(E.GetParameter("Damage") as Damage).Amount *= 2;
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

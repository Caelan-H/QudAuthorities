using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Expertise : BaseSkill
{
	public int HitBonus = 2;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerRollMeleeToHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerRollMeleeToHit" && E.GetStringParameter("Skill") == "Axe" && HitBonus != 0)
		{
			E.SetParameter("Result", E.GetIntParameter("Result") + HitBonus);
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

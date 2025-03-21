using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_PoisonTolerance : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Poison"))
		{
			damage.Amount = damage.Amount * 3 / 4;
			if (damage.Amount <= 0)
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

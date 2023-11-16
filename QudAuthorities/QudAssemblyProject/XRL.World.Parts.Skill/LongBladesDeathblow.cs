using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesDeathblow : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("En Garde!", "CommandDeathblow", "Skill", "Cooldown 100. For the next 10 rounds, Lunge and Swipe have no cooldown.", "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

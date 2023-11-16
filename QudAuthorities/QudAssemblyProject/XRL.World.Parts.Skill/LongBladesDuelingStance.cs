using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesDuelingStance : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dueling Stance", "CommandDuelingStance", "Stances", "+2/3 to hit while wielding a long blade in your primary hand.", "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

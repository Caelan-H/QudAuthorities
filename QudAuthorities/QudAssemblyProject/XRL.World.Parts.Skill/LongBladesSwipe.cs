using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesSwipe : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool AddSkill(GameObject GO)
	{
		string description = "Aggressive stance: You make an attack against all adjacent opponents.\n\nDefensive stance: You push all adjacent creatures back 1 space and attempt to trip the ones that are opponents (strength save; difficulty 30).\n\nDueling stance: You make an attack at an opponent and attempt to disarm them (strength save; difficulty 25 + your Agi modifier). The attack is guaranteed to hit and penetrate at least once.";
		ActivatedAbilityID = AddMyActivatedAbility("Swipe", "CommandSwipe", "Skill", description, "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

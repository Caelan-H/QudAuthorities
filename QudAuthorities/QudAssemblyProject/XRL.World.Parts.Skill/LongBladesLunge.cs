using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesLunge : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool AddSkill(GameObject GO)
	{
		string description = "Aggressive stance: You lunge through one empty space at an opponent and make an attack at +2 penetration. You must move through the empty space.\n\nDefensive stance: You make an attack at an opponent then lunge backward 2 spaces.\n\nDueling stance: You make an attack at an opponent at +1 penetration. The attack is guaranteed to hit and penetrate at least once.";
		ActivatedAbilityID = AddMyActivatedAbility("Lunge", "CommandLunge", "Skill", description, "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

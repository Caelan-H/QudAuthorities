using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Shield_ShieldWall : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandShieldWall");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandShieldWall")
		{
			if (!ParentObject.CanMoveExtremities("ShieldWall", ShowMessage: true))
			{
				return false;
			}
			ParentObject.ApplyEffect(new ShieldWall(3));
			CooldownMyActivatedAbility(ActivatedAbilityID, 30);
		}
		else if (E.ID == "AIGetOffensiveMutationList" && E.GetIntParameter("Distance") <= 1 && !ParentObject.IsFrozen() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.AddAICommand("CommandShieldWall");
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Shield Wall", "CommandShieldWall", "Skill", null, "\u0004");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

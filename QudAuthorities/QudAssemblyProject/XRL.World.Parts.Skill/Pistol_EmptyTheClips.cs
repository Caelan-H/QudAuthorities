using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol_EmptyTheClips : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandPistolEmptyTheClips");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
			if (activatedAbilityEntry != null)
			{
				activatedAbilityEntry.Enabled = !ParentObject.HasEffect("EmptyTheClips");
			}
		}
		else if (E.ID == "CommandPistolEmptyTheClips" && ParentObject.CheckFrozen())
		{
			ParentObject.ApplyEffect(new EmptyTheClips(21));
			CooldownMyActivatedAbility(ActivatedAbilityID, 200, null, "Agility");
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Empty the Clips", "CommandPistolEmptyTheClips", "Skill", null, "®");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

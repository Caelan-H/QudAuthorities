using System;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Dual_Wield_Fussilade : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandFussilade");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public bool PerformFlurry()
	{
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		if (ParentObject.HasEffect("Burrowed"))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot do that while burrowed.");
			}
			return false;
		}
		if (!ParentObject.CanMoveExtremities("Flurry", ShowMessage: true))
		{
			return false;
		}
		string text = PickDirectionS();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text, BuiltOnly: false);
		if (cellFromDirection == null)
		{
			return false;
		}
		GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject);
		if (combatTarget == null)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("There is nothing there you can attack.");
			}
			return false;
		}
		int num = 60;
		if (ParentObject.HasSkill("Dual_Wield_Two_Weapon_Fighting"))
		{
			num -= 10;
		}
		if (ParentObject.HasSkill("Dual_Wield_Ambidexterity"))
		{
			num -= 10;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, num, null, "Agility");
		DidX("launch", "into a flurry of attacks", "!", null, ParentObject);
		Event @event = Event.New("PerformMeleeAttack");
		@event.SetParameter("Attacker", ParentObject);
		@event.SetParameter("TargetCell", cellFromDirection);
		@event.SetParameter("Defender", combatTarget);
		@event.SetParameter("AlwaysOffhand", 100);
		@event.SetParameter("EnergyCost", 1000);
		return ParentObject.FireEvent(@event);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") == 1 && !ParentObject.IsFrozen() && !ParentObject.HasEffect("Burrowed") && ParentObject.CanMoveExtremities("Flurry") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandFussilade");
			}
		}
		else if (E.ID == "CommandFussilade" && !PerformFlurry())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Flurry", "CommandFussilade", "Skill", null, "ð", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

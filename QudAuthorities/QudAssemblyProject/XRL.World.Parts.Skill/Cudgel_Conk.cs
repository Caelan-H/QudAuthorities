using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Conk : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandConk");
		Object.RegisterPartEvent(this, "Conk");
		base.Register(Object);
	}

	public GameObject GetPrimaryCudgel()
	{
		return ParentObject.GetPrimaryWeaponOfType("Cudgel");
	}

	public bool IsPrimaryCudgelEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Cudgel");
	}

	public string GetConkLocation(GameObject obj)
	{
		Body body = obj.Body;
		if (body == null)
		{
			return null;
		}
		int partCount = body.GetPartCount("Head");
		if (partCount <= 0)
		{
			return null;
		}
		if (partCount == 1)
		{
			return "the " + body.GetFirstPart("Head").Name;
		}
		List<BodyPart> part = body.GetPart("Head");
		List<string> list = new List<string>();
		foreach (BodyPart item2 in part)
		{
			string item = Grammar.Pluralize(item2.VariantTypeModel().Name);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return "one of " + obj.its + " " + Grammar.MakeOrList(list);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (E.GetGameObjectParameter("Target") != null && intParameter <= 1 && IsPrimaryCudgelEquipped() && ParentObject.CanMoveExtremities() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandConk");
			}
		}
		else if (E.ID == "CommandConk" && !PerformConk())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool PerformConk()
	{
		if (!IsPrimaryCudgelEquipped())
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You must have a cudgel equipped in your primary hand to conk.");
			}
			return false;
		}
		if (!ParentObject.CanMoveExtremities("Conk", ShowMessage: true))
		{
			return false;
		}
		Cell cell = PickDirection();
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject);
		if (combatTarget == null)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("There's nothing there you can conk.");
			}
			return false;
		}
		try
		{
			string conkLocation = GetConkLocation(combatTarget);
			if (conkLocation == null)
			{
				if (ParentObject.IsPlayer())
				{
					if (combatTarget == ParentObject)
					{
						Popup.ShowFail("You do not have anything like a head to conk.");
					}
					else
					{
						Popup.ShowFail(combatTarget.The + combatTarget.ShortDisplayName + combatTarget.GetVerb("do") + " not have anything like a head to conk.");
					}
				}
				return false;
			}
			if (combatTarget == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to conk " + combatTarget.itself + " on " + conkLocation + "?") != 0)
			{
				return false;
			}
			DidXToY("attempt", "to conk", combatTarget, "on " + conkLocation);
			Event @event = Event.New("MeleeAttackWithWeapon");
			@event.SetParameter("Attacker", ParentObject);
			@event.SetParameter("Defender", combatTarget);
			@event.SetParameter("Weapon", GetPrimaryCudgel());
			@event.SetParameter("Properties", "Conking");
			ParentObject.FireEvent(@event);
			ParentObject.UseEnergy(1000, "Skill Cudgel Conk");
			CooldownMyActivatedAbility(ActivatedAbilityID, 10);
			return true;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Conk", x);
			return false;
		}
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Conk", "CommandConk", "Skill", "You make an attack with a cudgel at an adjacent opponent. If you hit, you automatically daze your opponent. If your opponent is already stunned, you instead knock them unconscious for 30-40 rounds (unconscious opponents wake up dazed when they take damage).", "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Shank : BaseSkill
{
	public bool Shanking;

	public Guid ActivatedAbilityID = Guid.Empty;

	public GameObject GetShortblade()
	{
		return ParentObject.GetWeaponOfType("ShortBlades", PreferPrimary: true);
	}

	public bool IsShortbladeEquipped()
	{
		return ParentObject.HasWeaponOfType("ShortBlades");
	}

	public bool IsPrimaryShortbladeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("ShortBlades");
	}

	public static int GetShankEffectCount(GameObject obj)
	{
		int num = 0;
		foreach (Effect effect in obj.Effects)
		{
			if (effect.IsOfType(33554432))
			{
				num++;
			}
		}
		return num;
	}

	public static bool Cast(GameObject ParentObject, ShortBlades_Shank skill = null, GameObject weapon = null)
	{
		if (skill == null)
		{
			skill = new ShortBlades_Shank();
			skill.ParentObject = ParentObject;
		}
		if (weapon == null)
		{
			weapon = skill.GetShortblade() ?? ParentObject.GetPrimaryWeapon();
			if (weapon == null)
			{
				return false;
			}
		}
		Cell cell = skill.PickDirection(ForAttack: true, "Shank whom?", ParentObject);
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject);
		if (combatTarget == null)
		{
			if (ParentObject.IsPlayer())
			{
				if (cell.HasObjectWithPart("Combat"))
				{
					Popup.Show("There's nothing there you can shank.");
				}
				else
				{
					Popup.Show("There's nothing there to shank.");
				}
			}
			return false;
		}
		try
		{
			skill.Shanking = true;
			if (combatTarget == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to shank " + combatTarget.itself + "?") != 0)
			{
				return false;
			}
			int num = GetShankEffectCount(combatTarget) * 2;
			if (IComponent<GameObject>.Visible(ParentObject))
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("attempt") + " to take advantage of " + combatTarget.poss("misfortune") + " and shank " + combatTarget.them + ".");
			}
			Event @event = Event.New("MeleeAttackWithWeapon");
			@event.SetParameter("Attacker", ParentObject);
			@event.SetParameter("Defender", combatTarget);
			@event.SetParameter("Weapon", weapon);
			@event.SetParameter("PenBonus", num);
			@event.SetParameter("PenCapBonus", num);
			if (num > 0)
			{
				combatTarget.Bloodsplatter();
			}
			for (int i = 0; i < num && i <= 12; i += 4)
			{
				combatTarget.BloodsplatterBurst(bSelfsplatter: true, Stat.Random(0, 359).toRadians(), 45);
			}
			if (num > 12)
			{
				combatTarget.DustPuff();
			}
			ParentObject.FireEvent(@event);
			int num2 = 1000;
			if (skill.IsPrimaryShortbladeEquipped() && ParentObject.HasSkill("ShortBlades_Expertise"))
			{
				num2 = (int)(0.75 * (double)num2);
			}
			ParentObject.UseEnergy(num2);
			if (skill.MyActivatedAbility(skill.ActivatedAbilityID) != null)
			{
				if (skill.IsPrimaryShortbladeEquipped())
				{
					skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, 10, null, "Agility");
				}
				else
				{
					skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, 20, null, "Agility");
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Shank", x);
		}
		finally
		{
			skill.Shanking = false;
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandShank");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null && IsShortbladeEquipped() && E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.CanMoveExtremities("Shank") && Stat.Random(0, 4) < GetShankEffectCount(gameObjectParameter))
			{
				E.AddAICommand("CommandShank");
			}
		}
		else if (E.ID == "CommandShank")
		{
			GameObject shortblade = GetShortblade();
			if (shortblade == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You must have a short blade equipped to shank.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities("Shank", ShowMessage: true))
			{
				return false;
			}
			return Cast(ParentObject, this, shortblade);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Shank", "CommandShank", "Skill", "You make a melee attack with a short blade in your primary hand (preferred) or offhand. If you hit, the attack gets +2 penetration for each negative status effect your opponent suffers from.", "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

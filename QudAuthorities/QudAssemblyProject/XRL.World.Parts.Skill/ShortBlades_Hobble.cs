using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Hobble : BaseSkill
{
	public bool Hobbling;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "CommandHobble");
		base.Register(Object);
	}

	public GameObject GetPrimaryShortblade()
	{
		return ParentObject.GetPrimaryWeaponOfType("ShortBlades");
	}

	public bool IsShortbladeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("ShortBlades");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 1 && ParentObject.CanMoveExtremities("Hobble") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandHobble");
			}
		}
		else if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2 != null && Hobbling)
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You find a weakness in " + Grammar.MakePossessive(gameObjectParameter3.the + gameObjectParameter3.ShortDisplayName) + " defenses.", 'g');
				}
				else if (gameObjectParameter3.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.ShortDisplayName + gameObjectParameter.GetVerb("find") + " a weakness in your defenses.", 'r');
				}
				gameObjectParameter3.ApplyEffect(new Hobbled(Stat.Random(16, 20)));
			}
		}
		else if (E.ID == "CommandHobble")
		{
			if (!IsShortbladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a short blade equipped in your primary hand to hobble.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities("Hobble", ShowMessage: true))
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
					Popup.ShowFail("There's nothing there to hobble.");
				}
				return false;
			}
			try
			{
				Hobbling = true;
				if (combatTarget == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to hobble " + combatTarget.itself + "?") != 0)
				{
					return false;
				}
				CooldownMyActivatedAbility(ActivatedAbilityID, 30, null, "Agility");
				DidXToY("attempt", "to hobble", combatTarget, null, null, null, null, combatTarget);
				Event @event = Event.New("MeleeAttackWithWeapon");
				@event.SetParameter("Attacker", ParentObject);
				@event.SetParameter("Defender", combatTarget);
				@event.SetParameter("Weapon", GetPrimaryShortblade());
				@event.SetParameter("Properties", "Autopen,Maxpen1");
				ParentObject.FireEvent(@event);
				int num = 1000;
				if (IsShortbladeEquipped() && ParentObject.HasSkill("ShortBlades_Expertise"))
				{
					num = (int)(0.75 * (double)num);
				}
				ParentObject.UseEnergy(num);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Hobble", x);
			}
			finally
			{
				Hobbling = false;
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Hobble", "CommandHobble", "Skill", "You make an attack with a dagger in your primary hand, looking for a weak spot in your opponent's armor. If you hit, you penetrate exactly once and hobble them (-50% movespeed for 16-20 rounds).", "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

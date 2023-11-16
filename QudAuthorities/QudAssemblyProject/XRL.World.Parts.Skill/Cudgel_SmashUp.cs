using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_SmashUp : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "CommandSmashUp");
		base.Register(Object);
	}

	public bool IsPrimaryCudgelEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Cudgel");
	}

	public GameObject GetPrimaryCudgel()
	{
		return ParentObject.GetPrimaryWeaponOfType("Cudgel");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (E.GetGameObjectParameter("Target") != null && IsPrimaryCudgelEquipped() && intParameter <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSmashUp");
			}
		}
		else if (E.ID == "CommandSmashUp")
		{
			if (!IsPrimaryCudgelEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a cudgel equipped in your primary hand to demolish things.");
				}
				return false;
			}
			if (ParentObject.GetPart("Cudgel_Slam") is Cudgel_Slam cudgel_Slam && cudgel_Slam.IsMyActivatedAbilityCoolingDown(cudgel_Slam.ActivatedAbilityID))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't Demolish until Slam is off cooldown.");
				}
				return false;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You prepare " + ParentObject.itself + " for demolition.");
				IComponent<GameObject>.ThePlayer.ParticleText("&R!!!");
			}
			int num = 5;
			if (ParentObject.HasIntProperty("ImprovedSmashUp"))
			{
				num += num * ParentObject.GetIntProperty("ImprovedSmashUp");
			}
			ParentObject.ApplyEffect(new Cudgel_SmashingUp(num));
			CooldownMyActivatedAbility(ActivatedAbilityID, 100);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Demolish", "CommandSmashUp", "Stances", "For the next 5 rounds, your chance to daze with cudgel attacks is 100% and Slam has no cooldown. To use Demolish, Slam must be off cooldown, and using Demolish puts Slam on cooldown.", "\u001e");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

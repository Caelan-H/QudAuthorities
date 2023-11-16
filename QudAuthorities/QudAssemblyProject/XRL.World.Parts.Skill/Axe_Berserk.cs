using System;
using XRL.Messages;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Berserk : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandAxeBerserk");
		base.Register(Object);
	}

	public bool IsPrimaryAxeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Axe");
	}

	public GameObject GetPrimaryAxe()
	{
		return ParentObject.GetPrimaryWeaponOfType("Axe");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsPrimaryAxeEquipped() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ConTarget(E) >= 1f)
			{
				E.AddAICommand("CommandAxeBerserk");
			}
		}
		else if (E.ID == "CommandAxeBerserk")
		{
			if (!IsPrimaryAxeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have an axe equipped in your primary hand to go berserk.");
				}
				return false;
			}
			Axe_Dismember axe_Dismember = ParentObject.GetPart("Axe_Dismember") as Axe_Dismember;
			if (axe_Dismember != null && axe_Dismember.IsMyActivatedAbilityCoolingDown(axe_Dismember.ActivatedAbilityID))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't go berserk until Dismember is off cooldown.");
				}
				return false;
			}
			if (ParentObject.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You work " + ParentObject.itself + " into a blood frenzy!");
				IComponent<GameObject>.ThePlayer.ParticleText("&R!!!");
			}
			int num = 5;
			if (ParentObject.HasIntProperty("ImprovedBerserk"))
			{
				num += num * ParentObject.GetIntProperty("ImprovedBerserk");
			}
			ParentObject.ApplyEffect(new Berserk(num));
			CooldownMyActivatedAbility(ActivatedAbilityID, 100);
			axe_Dismember?.CooldownMyActivatedAbility(axe_Dismember.ActivatedAbilityID, 30);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Berserk!", "CommandAxeBerserk", "Stances", "You enter a blood frenzy, and for 5 rounds your chance to dismember with axe attacks is 100%. To use Berserk, Dismember must be off cooldown, and using Berserk puts Dismember on cooldown.", "\u0001");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

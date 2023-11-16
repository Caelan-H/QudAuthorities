using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Rejoinder : BaseSkill
{
	public Guid ActivatedAbilityID;

	public bool Checked;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandToggleRejoinder");
		Object.RegisterPartEvent(this, "DefenderAfterAttackMissed");
		Object.RegisterPartEvent(this, "EndSegment");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Checked = false;
		}
		else if (E.ID == "DefenderAfterAttackMissed")
		{
			if (Checked)
			{
				return true;
			}
			Checked = true;
			if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
			{
				return true;
			}
			if (!60.in100())
			{
				return true;
			}
			if (!ParentObject.CanMoveExtremities("Attack") || !ParentObject.CanChangeBodyPosition("Attack"))
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObject = null;
			List<GameObject> list = Event.NewGameObjectList();
			foreach (BodyPart part in ParentObject.Body.GetParts())
			{
				if (part.Equipped?.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.Skill == "ShortBlades")
				{
					list.Add(part.Equipped);
				}
				else if (part.DefaultBehavior?.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon2 && meleeWeapon2.Skill == "ShortBlades")
				{
					list.Add(part.DefaultBehavior);
				}
			}
			gameObject = list.GetRandomElement();
			if (gameObject != null)
			{
				ParentObject.ParticleText("*rejoinder*", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You rejoinder with " + ParentObject.poss(gameObject) + ".", 'G');
				}
				else if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("rejoinder") + " with " + ParentObject.poss(gameObject) + ".", 'R');
				}
				Event @event = Event.New("MeleeAttackWithWeapon");
				@event.SetParameter("Attacker", ParentObject);
				@event.SetParameter("Defender", gameObjectParameter);
				@event.SetParameter("Weapon", gameObject);
				ParentObject.FireEvent(@event);
			}
		}
		else if (E.ID == "CommandToggleRejoinder")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	private void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rejoinder", "CommandToggleRejoinder", "Skill", null, "\u001b", null, Toggleable: true, DefaultToggleState: true);
	}

	public override bool AddSkill(GameObject GO)
	{
		AddAbility();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Shield_Slam : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandShieldSlam");
		Object.RegisterPartEvent(this, "ChargedTarget");
		base.Register(Object);
	}

	public bool Slam(GameObject TargetObject, GameObject shieldObj, Cell TargetCell = null, bool Free = false)
	{
		if (shieldObj == null || !GameObject.validate(ParentObject))
		{
			return false;
		}
		if (TargetCell == null)
		{
			TargetCell = TargetObject.CurrentCell;
			if (TargetCell == null)
			{
				return false;
			}
		}
		Event @event = Event.New("BeginAttack");
		@event.SetParameter("TargetObject", TargetObject);
		@event.SetParameter("TargetCell", TargetCell);
		if (!ParentObject.FireEvent(@event))
		{
			return false;
		}
		if (!Free)
		{
			ParentObject.UseEnergy(1000, "Combat Melee Skill Shield Slam");
		}
		if (ParentObject.IsPlayer())
		{
			ParentObject.Target = TargetObject;
		}
		if (TargetObject.FireEvent(Event.New("CanBeAngeredByBeingAttacked", "Attacker", ParentObject)))
		{
			TargetObject.GetAngryAt(ParentObject, -75);
		}
		if (TargetObject.MakeSave("Strength", 20, ParentObject, null, "ShieldSlam Slam", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, shieldObj))
		{
			if (IComponent<GameObject>.Visible(TargetObject))
			{
				TargetObject.ParticleText("*resisted*", IComponent<GameObject>.ConsequentialColorChar(TargetObject));
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(TargetObject.The + TargetObject.ShortDisplayName + TargetObject.GetVerb("resist") + " your shield slam.", 'r');
			}
			else if (TargetObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You resist " + Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " shield slam.", 'g');
			}
		}
		else
		{
			Event event2 = Event.New("ObjectAttacking");
			event2.SetParameter("Object", ParentObject);
			event2.SetParameter("TargetObject", TargetObject);
			event2.SetParameter("TargetCell", TargetCell);
			if (TargetCell.FireEvent(event2))
			{
				XRL.World.Parts.Shield shield = shieldObj.GetPart("Shield") as XRL.World.Parts.Shield;
				int num = ParentObject.StatMod("Strength");
				int num2 = Stat.Roll(num + "d2+" + shield.AV);
				string attributes;
				if (shieldObj.HasPart("ModSpiked"))
				{
					num2 += num;
					attributes = "Stabbing";
				}
				else
				{
					attributes = "Bludgeoning";
				}
				Event event3 = Event.New("AIMessage");
				event3.SetParameter("Message", "Attacked");
				event3.SetParameter("By", ParentObject);
				TargetObject.FireEvent(event3);
				TargetObject.ApplyEffect(new Prone());
				if (!TargetObject.IsNowhere())
				{
					TargetObject.TakeDamage(num2, "from %o shield slam!", attributes, null, null, null, ParentObject);
					if (!TargetObject.IsNowhere() && shieldObj.HasPart("ModSpiked"))
					{
						TargetObject.ApplyEffect(new Bleeding("1d2", 20 + shield.AV, ParentObject));
					}
				}
			}
		}
		return true;
	}

	public GameObject CheckShield()
	{
		GameObject result = null;
		int num = 0;
		foreach (BodyPart equippedPart in ParentObject.Body.GetEquippedParts())
		{
			XRL.World.Parts.Shield part = equippedPart.Equipped.GetPart<XRL.World.Parts.Shield>();
			if (part != null && part.AV > num)
			{
				result = equippedPart.Equipped;
				num = part.AV;
			}
		}
		return result;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null && E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.CanMoveExtremities("ShieldSlam") && CheckShield() != null && !gameObjectParameter.HasEffect("Prone") && ParentObject.PhaseAndFlightMatches(gameObjectParameter))
			{
				E.AddAICommand("CommandShieldSlam");
			}
		}
		else if (E.ID == "ChargedTarget")
		{
			GameObject gameObject = CheckShield();
			if (gameObject != null)
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				if (gameObjectParameter2 != null)
				{
					Slam(gameObjectParameter2, gameObject, null, Free: true);
				}
			}
		}
		else if (E.ID == "CommandShieldSlam")
		{
			GameObject gameObject2 = CheckShield();
			if (gameObject2 == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a shield equipped to use Shield Slam.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities("ShieldSlam", ShowMessage: true))
			{
				return false;
			}
			string text = PickDirectionS();
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text);
			if (cellFromDirection == null)
			{
				return false;
			}
			GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject);
			if (combatTarget == null || combatTarget == ParentObject)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("There's nothing there you can shield slam.");
				}
				return false;
			}
			if (!Slam(combatTarget, gameObject2, cellFromDirection))
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 40);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Shield Slam", "CommandShieldSlam", "Skill", null, "\u0011", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

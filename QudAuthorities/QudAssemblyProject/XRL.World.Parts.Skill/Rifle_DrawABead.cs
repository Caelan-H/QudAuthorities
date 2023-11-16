using System;
using XRL.Messages;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_DrawABead : BaseSkill
{
	public int MarkCooldown;

	public GameObject Mark;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SuspendingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		ClearMark();
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandMarkTarget");
		Object.RegisterPartEvent(this, "EndSegment");
		Object.RegisterPartEvent(this, "MarkMovedAway");
		Object.RegisterPartEvent(this, "MarkMovedTowards");
		base.Register(Object);
	}

	public void SetMark(GameObject Target)
	{
		ClearMark();
		if (Target.ApplyEffect(new RifleMark(ParentObject)))
		{
			Mark = Target;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, MarkCooldown, null, "Agility");
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You draw a bead on " + Target.t() + ". " + Target.T() + Target.GetVerb("are") + " marked.");
		}
	}

	public void ClearMark()
	{
		if (Mark != null && Mark.IsInvalid())
		{
			Mark = null;
		}
		if (Mark == null)
		{
			return;
		}
		if (Mark.HasEffect("RifleMark"))
		{
			foreach (Effect effect in Mark.GetEffects("RifleMark"))
			{
				if (((RifleMark)effect).Marker == ParentObject)
				{
					effect.Duration = 0;
					break;
				}
			}
			Mark.CleanEffects();
		}
		Mark = null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandMarkTarget")
		{
			if (ParentObject.HasEffect("Confused"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You cannot mark a target while you are confused.");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, null, Snap: true);
			if (cell == null)
			{
				return true;
			}
			GameObject gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true) ?? cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
			if (gameObject != null)
			{
				SetMark(gameObject);
			}
			ParentObject.UseEnergy(1000, "Physical Skill");
		}
		else if (E.ID == "EndSegment" || E.ID == "BeginTakeAction")
		{
			if (GameObject.validate(ref Mark) && (!ParentObject.HasLOSTo(Mark, IncludeSolid: true, UseTargetability: true) || !Mark.HasEffect("RifleMark")))
			{
				if (ParentObject.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("You lose sight of your mark.");
				}
				ClearMark();
			}
			if (E.ID == "EndSegment")
			{
				if (MarkCooldown > 0)
				{
					MarkCooldown--;
				}
				MyActivatedAbility(ActivatedAbilityID)?.SetCooldown(MarkCooldown);
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Mark Target", "CommandMarkTarget", "Skill", null, "Î");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

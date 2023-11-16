using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_HookAndDrag : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public GameObject HookedObject;

	public GlobalLocation LeftCell = new GlobalLocation();

	public GameObject HookingWeapon;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != BeginTakeActionEvent.ID && ID != EnteredCellEvent.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (GameObject.validate(ref HookedObject) && HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked e)
		{
			HookedObject.RemoveEffect(e);
		}
		HookingWeapon = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (HookedObject != null && HookedObject.IsInvalid())
		{
			HookedObject = null;
			HookingWeapon = null;
		}
		if (HookedObject != null && LeftCell.IsCell())
		{
			if (HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked hooked)
			{
				if (!HookedObject.PhaseMatches(ParentObject))
				{
					HookedObject.RemoveEffect(hooked);
					HookedObject = null;
					HookingWeapon = null;
				}
				else if (hooked.Duration > 0)
				{
					int num = ParentObject.DistanceTo(HookedObject);
					if (num == 2)
					{
						string directionFromCell = HookedObject.CurrentCell.GetDirectionFromCell(LeftCell.ResolveCell());
						Event @event = Event.New("MeleeAttackWithWeapon");
						@event.SetParameter("Attacker", ParentObject);
						@event.SetParameter("Defender", HookedObject);
						@event.SetParameter("Weapon", HookingWeapon);
						@event.SetParameter("Properties", "Autohit");
						ParentObject.FireEvent(@event);
						if (HookedObject.IsValid() && !HookedObject.IsInGraveyard())
						{
							HookedObject.Move(directionFromCell, Forced: false, System: false, IgnoreGravity: false, NoStack: false, HookingWeapon);
						}
					}
					else if (num > 2)
					{
						HookedObject.RemoveEffect(hooked);
						HookedObject = null;
						HookingWeapon = null;
					}
				}
			}
			else
			{
				HookedObject = null;
				HookingWeapon = null;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Validate(E.Cell);
		if (HookedObject != null)
		{
			LeftCell.SetCell(E.Cell);
		}
		else
		{
			LeftCell.Clear();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginMove");
		Object.RegisterPartEvent(this, "CommandAxeHookAndDrag");
		Object.RegisterPartEvent(this, "StopFighting");
		base.Register(Object);
	}

	private bool WeaponMatch(Effect FX)
	{
		if (FX is Hooked hooked)
		{
			return hooked.HookingWeapon == HookingWeapon;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			if (ParentObject != null && GameObject.validate(ref HookedObject) && HookedObject.DistanceTo(ParentObject) <= 1 && HookedObject.PhaseMatches(ParentObject))
			{
				if (HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked hooked)
				{
					if (E.GetParameter("DestinationCell") is Cell c && HookedObject.DistanceTo(c) == 2 && hooked.Duration > 0 && (!HookedObject.FireEvent("BeforeGrabbed") || HookedObject.MakeSave("Strength", 20, ParentObject, null, "HookAndDrag Move Grab Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, hooked.HookingWeapon)))
					{
						IComponent<GameObject>.XDidY(HookedObject, "stand", HookedObject.its + " ground", "!", null, null, ParentObject);
						ParentObject.UseEnergy(1000, "Movement Failure");
						return false;
					}
				}
				else
				{
					HookedObject = null;
					HookingWeapon = null;
				}
			}
			else
			{
				if (HookedObject != null)
				{
					if (HookedObject.IsValid() && HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked e)
					{
						HookedObject.RemoveEffect(e);
					}
					HookedObject = null;
				}
				HookingWeapon = null;
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") == 1 && !ParentObject.IsFrozen())
			{
				if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
				{
					E.AddAICommand("CommandAxeHookAndDrag");
				}
				if (HookedObject != null && !HookedObject.IsInGraveyard() && HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked hooked2 && hooked2.Duration > 0)
				{
					E.AddAICommand("MetaCommandMoveAway");
				}
			}
		}
		else if (E.ID == "CommandAxeHookAndDrag")
		{
			GameObject primaryWeaponOfType = ParentObject.GetPrimaryWeaponOfType("Axe", AcceptFirstHandForNonHandPrimary: true);
			if (primaryWeaponOfType == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have an axe equipped in your primary hand to use Hook and Drag.");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen())
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
				return false;
			}
			if (!combatTarget.FireEvent("BeforeGrabbed"))
			{
				return false;
			}
			if (!combatTarget.ApplyEffect(new Hooked(primaryWeaponOfType, 20, 9)))
			{
				return false;
			}
			DidXToYWithZ("hook", combatTarget, "with", primaryWeaponOfType, null, "!", null, null, combatTarget, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, ParentObject);
			HookedObject = combatTarget;
			HookingWeapon = primaryWeaponOfType;
			combatTarget.Bloodsplatter();
			CooldownMyActivatedAbility(ActivatedAbilityID, 50);
		}
		else if (E.ID == "StopFighting")
		{
			if (HookedObject != null && !HookedObject.IsValid())
			{
				HookedObject = null;
				HookingWeapon = null;
			}
			if (HookedObject != null)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (gameObjectParameter == null || gameObjectParameter == HookedObject)
				{
					if (HookedObject.GetEffect("Hooked", WeaponMatch) is Hooked e2)
					{
						HookedObject.RemoveEffect(e2);
					}
					HookedObject = null;
					HookingWeapon = null;
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Hook and Drag", "CommandAxeHookAndDrag", "Skill", "You grab an opponent's limb with the heel of your axe and pull them toward you. If successful, you pull your opponent with you as you move and make a free attack with your axe. Your opponent is forced to move with you but can attack you while moving. Your opponent gets a chance to resist the move (strength save; difficulty 20 + your strength modifier) and a chance to break free at the start of their turn (same save).\n\nThis effect lasts for 9 rounds or until you dismember the opponent.", "ô");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}

	public bool Validate(Cell C = null)
	{
		Hooked hooked = null;
		if (GameObject.validate(ref HookedObject))
		{
			hooked = HookedObject.GetEffect("Hooked", WeaponMatch) as Hooked;
			if (hooked != null)
			{
				if (C != null)
				{
					if (C.DistanceTo(HookedObject) <= 1)
					{
						goto IL_005c;
					}
				}
				else if (ParentObject.DistanceTo(HookedObject) <= 1)
				{
					goto IL_005c;
				}
			}
		}
		if (hooked != null)
		{
			HookedObject?.RemoveEffect(hooked);
		}
		HookedObject = null;
		HookingWeapon = null;
		return false;
		IL_005c:
		return true;
	}
}

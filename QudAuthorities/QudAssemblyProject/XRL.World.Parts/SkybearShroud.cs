using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SkybearShroud : IPart
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public bool bEquipRequired = true;

	public bool bWorn;

	public bool bBubbleOn;

	public int CooldownRemaining;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		bWorn = true;
		E.Actor.RegisterPartEvent(this, "ActivateSkyshroud");
		E.Actor.RegisterPartEvent(this, "AIGetOffensiveItemList");
		E.Actor.RegisterPartEvent(this, "AIGetMovementMutationList");
		E.Actor.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		ActivatedAbilityID = E.Actor.AddActivatedAbility("Activate Flume-Flier", "ActivateSkyshroud", "Items");
		if (CooldownRemaining != 0)
		{
			ActivatedAbilityEntry activatedAbility = E.Actor.GetActivatedAbility(ActivatedAbilityID);
			if (activatedAbility != null)
			{
				activatedAbility.Cooldown = CooldownRemaining;
			}
			CooldownRemaining = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		bWorn = false;
		E.Actor.RemoveEffect("Dashing");
		E.Actor.UnregisterPartEvent(this, "ActivateSkyshroud");
		E.Actor.UnregisterPartEvent(this, "AIGetOffensiveItemList");
		E.Actor.UnregisterPartEvent(this, "AIGetMovementMutationList");
		E.Actor.UnregisterPartEvent(this, "AIGetOffensiveMutationList");
		ActivatedAbilityEntry activatedAbility = E.Actor.GetActivatedAbility(ActivatedAbilityID);
		if (activatedAbility != null)
		{
			CooldownRemaining = activatedAbility.Cooldown;
		}
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			E.AddAction("Activate", "activate", "ActivateSkyshroud", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateSkyshroud")
		{
			ActivateSkyshroud();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetMovementMutationList" || E.ID == "AIGetOffensiveMutationList")
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("ActivateSkyshroud", 1, ParentObject, Inv: true);
			}
		}
		else if (E.ID == "ActivateSkyshroud" && !ActivateSkyshroud())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	private bool ActivateSkyshroud()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null)
		{
			return false;
		}
		if (!equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (!equipped.ApplyEffect(new Dashing(10)))
		{
			return false;
		}
		equipped.CooldownActivatedAbility(ActivatedAbilityID, 100);
		return true;
	}
}

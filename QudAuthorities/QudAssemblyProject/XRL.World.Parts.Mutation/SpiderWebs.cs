using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SpiderWebs : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public int SpinTimer;

	[FieldSaveVersion(249)]
	public bool Active;

	public SpiderWebs()
	{
		DisplayName = "Spider Webs";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			if (ParentObject.OnWorldMap())
			{
				SpinTimer = -1;
			}
			else
			{
				SpinTimer--;
			}
			if (SpinTimer < 0)
			{
				ToggleMyActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				GameObject gameObject = null;
				gameObject = ((!ParentObject.HasEffect("Phased")) ? GameObject.create("Web") : GameObject.create("PhaseWeb"));
				Sticky obj = gameObject.GetPart("Sticky") as Sticky;
				obj.SaveTarget = 15 + base.Level;
				obj.MaxWeight = 320 + 80 * base.Level;
				E.Cell.AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetMovementMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "ApplyStuck");
		Object.RegisterPartEvent(this, "CanApplyStuck");
		Object.RegisterPartEvent(this, "CommandSpinWeb");
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override string GetLevelText(int Level)
	{
		return "You bear two spinnerets with which you spin a sticky silk.\n";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck" || E.ID == "CanApplyStuck")
		{
			return false;
		}
		if (E.ID == "AIGetMovementMutationList" && Active)
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSpinWeb");
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSpinWeb");
			}
		}
		else if (E.ID == "CommandSpinWeb")
		{
			CooldownMyActivatedAbility(ActivatedAbilityID, 50);
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				SpinTimer = 4;
			}
			UseEnergy(1000, "Physical Mutation Spin Web");
		}
		else if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spin Webs", "CommandSpinWeb", "Physical Mutation", null, "#", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

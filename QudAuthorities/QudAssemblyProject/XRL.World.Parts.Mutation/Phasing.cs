using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Phasing : BaseMutation
{
	public Guid PhaseOutActivatedAbilityID = Guid.Empty;

	public Phasing()
	{
		DisplayName = "Phasing";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandPhaseIn");
		Object.RegisterPartEvent(this, "CommandPhaseOut");
		Object.RegisterPartEvent(this, "CommandTogglePhase");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You may phase through solid objects for brief periods of time.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Duration: {{rules|" + (6 + Level) + "}} rounds\n", "Cooldown: {{rules|", (103 - 3 * Level).ToString(), "}} rounds");
	}

	public void SyncAbilities()
	{
		ActivatedAbilityEntry activatedAbility = ParentObject.GetActivatedAbility(PhaseOutActivatedAbilityID);
		activatedAbility.ToggleState = ParentObject.HasEffect("Phased");
		activatedAbility.Visible = true;
	}

	public bool IsPhased()
	{
		return ParentObject?.HasEffect("Phased") ?? false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			SyncAbilities();
		}
		else
		{
			if (E.ID == "CommandTogglePhase")
			{
				return ParentObject.FireEvent(Event.New(IsPhased() ? "CommandPhaseIn" : "CommandPhaseOut"));
			}
			if (E.ID == "CommandPhaseOut")
			{
				if (ParentObject.OnWorldMap())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot do that on the world map.");
					}
					return false;
				}
				Event e = Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this);
				if (!ParentObject.FireEvent(e, E))
				{
					return false;
				}
				ParentObject.ApplyEffect(new Phased(6 + base.Level + 1));
				CooldownMyActivatedAbility(PhaseOutActivatedAbilityID, 103 - 3 * base.Level);
				SyncAbilities();
			}
			else if (E.ID == "CommandPhaseIn")
			{
				ParentObject.RemoveEffect("Phased");
				SyncAbilities();
			}
			else if (E.ID == "AIGetDefensiveMutationList")
			{
				if (IsMyActivatedAbilityAIUsable(PhaseOutActivatedAbilityID) && !ParentObject.HasEffect("Phased") && ParentObject.isDamaged(0.25) && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
				{
					E.AddAICommand("CommandPhaseOut");
				}
			}
			else if (E.ID == "AIGetOffensiveMutationList")
			{
				int intParameter = E.GetIntParameter("Distance");
				if (intParameter > 1 && intParameter < 5 + base.Level && IsMyActivatedAbilityAIUsable(PhaseOutActivatedAbilityID) && !ParentObject.HasEffect("Phased") && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
				{
					E.AddAICommand("CommandPhaseOut");
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		PhaseOutActivatedAbilityID = AddMyActivatedAbility("Phase", "CommandTogglePhase", "Physical Mutation", null, "°", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: true);
		SyncAbilities();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref PhaseOutActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ErosTeleportation : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public ErosTeleportation()
	{
		DisplayName = "Teleportation";
		Type = "Mental";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetMovementMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetRetreatMutationList");
		Object.RegisterPartEvent(this, "CommandTeleport");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You teleport to a nearby location near your leader.";
	}

	public override string GetLevelText(int Level)
	{
		int num = 125 - 10 * Level;
		if (num < 5)
		{
			num = 5;
		}
		return "Cooldown: " + num + " rounds";
	}

	public static bool Cast(ErosTeleportation mutation = null, string level = "5-6", Event E = null, Cell Destination = null)
	{
		if (mutation == null)
		{
			mutation = new ErosTeleportation();
			mutation.ParentObject = XRLCore.Core.Game.Player.Body;
			mutation.Level = Stat.Roll(level);
		}
		Cell cell = null;
		GameObject parentObject = mutation.ParentObject;
		if (!parentObject.IsRealityDistortionUsable())
		{
			RealityStabilized.ShowGenericInterdictMessage(parentObject);
			return false;
		}
		if (parentObject.PartyLeader != null)
		{
			Cell cell2 = parentObject.PartyLeader.CurrentCell;
			if (cell2 != null)
			{
				cell = cell2.GetEmptyAdjacentCells().GetRandomElement();
			}
		}
		if (cell == null)
		{
			return false;
		}
		if (parentObject.IsPlayer())
		{
			if (!cell.IsExplored())
			{
				Popup.ShowFail("You can only teleport to a place you have seen before!");
				return false;
			}
			if (!cell.IsEmptyOfSolid())
			{
				Popup.ShowFail("You may only teleport into an empty square!");
				return false;
			}
		}
		Event e = Event.New("InitiateRealityDistortionTransit", "Object", parentObject, "Mutation", mutation, "Cell", cell);
		if (!parentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
		{
			return false;
		}
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, Math.Max(125 - 10 * mutation.Level, 5));
		parentObject.ParticleBlip("&C\u000f");
		parentObject.TeleportTo(cell, 0);
		parentObject.TeleportSwirl();
		parentObject.ParticleBlip("&C\u000f");
		mutation.UseEnergy(1000, "Mental Mutation E-Ros Teleportation");
		IComponent<GameObject>.EmitMessage(parentObject, "E-Ros yells, {{W|'I'm coming, " + parentObject.PartyLeader.BaseDisplayName + "!'}}");
		parentObject.ParticleText("I'm coming, " + parentObject.PartyLeader.BaseDisplayName + "!", 'W');
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				int intParameter = E.GetIntParameter("Distance");
				if (gameObjectParameter != null && intParameter > 3 && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && ParentObject.PartyLeader != null && ParentObject.PartyLeader.Target != null && ParentObject.PartyLeader.DistanceTo(gameObjectParameter) == 1)
				{
					E.AddAICommand("CommandTeleport");
				}
			}
		}
		else if (E.ID == "AIGetMovementMutationList" || E.ID == "AIGetRetreatMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.GetParameter("TargetCell") is Cell cell && ParentObject.PartyLeader != null && ParentObject.PartyLeader.Target != null && ParentObject.PartyLeader.DistanceTo(cell) <= 1 && ParentObject.DistanceTo(cell) > 1 && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && cell.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
			{
				E.AddAICommand("CommandTeleport");
			}
		}
		else if (E.ID == "CommandTeleport" && !Cast(this, null, E, E.GetParameter("TargetCell") as Cell))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Teleport", "CommandTeleport", "Mental Mutation", null, "\u001d", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Teleportation : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Teleportation()
	{
		DisplayName = "Teleportation";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
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
		return "You teleport to a nearby location.";
	}

	public static int GetCooldown(int Level)
	{
		return Math.Max(103 - 3 * Level, 5);
	}

	public int GetRadius(int Level)
	{
		return Math.Max(13 - Level, 2);
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Teleport to a random location within a designated area.\n" + "Uncertainty radius: {{rules|" + GetRadius(Level) + "}}\n", "Cooldown: {{rules|", GetCooldown(Level).ToString(), "}} rounds");
	}

	public static bool Cast(Teleportation mutation = null, string level = "5-6", Event E = null, Cell Destination = null, GameObject subject = null, bool automatic = false)
	{
		if (mutation == null)
		{
			mutation = new Teleportation();
			mutation.ParentObject = subject ?? IComponent<GameObject>.ThePlayer;
			mutation.Level = level.RollCached();
		}
		Cell cell = null;
		GameObject parentObject = mutation.ParentObject;
		if (!parentObject.IsRealityDistortionUsable())
		{
			RealityStabilized.ShowGenericInterdictMessage(parentObject);
			return false;
		}
		if (Destination != null)
		{
			cell = Destination;
		}
		else
		{
			List<Cell> list;
			if (parentObject.IsPlayer())
			{
				if (parentObject.OnWorldMap())
				{
					if (!automatic)
					{
						Popup.ShowFail("You may not teleport on the world map.");
					}
					return false;
				}
				list = mutation.PickCircle(mutation.GetRadius(mutation.Level), 9999, bLocked: false, AllowVis.OnlyExplored);
			}
			else
			{
				GameObject gameObject = parentObject.Target ?? parentObject.PartyLeader;
				if (gameObject == null)
				{
					return false;
				}
				list = gameObject.CurrentCell.GetLocalAdjacentCells(mutation.GetRadius(mutation.Level));
			}
			if (list == null)
			{
				return false;
			}
			cell = list.GetRandomElement();
		}
		if (cell == null)
		{
			return false;
		}
		if (!cell.IsEmptyFor(parentObject))
		{
			cell = cell.GetConnectedSpawnLocation();
			if (cell == null)
			{
				if (parentObject.IsPlayer())
				{
					Popup.ShowFail("The teleport fails!");
				}
				return false;
			}
			if (parentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You are shunted to another location!");
			}
		}
		if (parentObject.InActiveZone())
		{
			parentObject.ParticleBlip("&C\u000f");
		}
		Event e = Event.New("InitiateRealityDistortionTransit", "Object", parentObject, "Mutation", mutation, "Cell", cell);
		if (!parentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
		{
			return false;
		}
		int cooldown = GetCooldown(mutation.Level);
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, cooldown);
		if (parentObject.InActiveZone())
		{
			parentObject.TeleportTo(cell, 0);
			parentObject.TeleportSwirl();
			parentObject.ParticleBlip("&C\u000f");
		}
		mutation.UseEnergy(1000, "Mental Mutation Teleportation");
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (gameObjectParameter != null && ParentObject.InActiveZone() && ParentObject.DistanceTo(gameObjectParameter) > GetRadius(base.Level) + 1 && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && gameObjectParameter.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
				{
					E.AddAICommand("CommandTeleport");
				}
			}
		}
		else if (E.ID == "AIGetMovementMutationList" || E.ID == "AIGetRetreatMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.GetParameter("TargetCell") is Cell cell && cell.IsEmptyOfSolid() && ParentObject.InActiveZone() && ParentObject.DistanceTo(cell) > GetRadius(base.Level) + 1 && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && cell.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
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

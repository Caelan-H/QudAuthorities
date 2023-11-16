using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsHighFidelityMatterRecompositer : IPart
{
	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		commandId = Guid.NewGuid().ToString();
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Recomposite", commandId, "Cybernetics", "You teleport to a designated explored space on the map.", "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor == ParentObject.Implantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && implantee.CurrentCell != null && !implantee.OnWorldMap())
			{
				Cell cell = null;
				if (implantee.IsPlayer())
				{
					cell = new Teleportation
					{
						ParentObject = implantee,
						Level = 10
					}.PickDestinationCell(999, AllowVis.OnlyExplored, Locked: false);
				}
				else
				{
					Brain part = implantee.GetPart<Brain>();
					if (part == null)
					{
						return false;
					}
					if (part.Target == null)
					{
						return false;
					}
					List<Cell> localAdjacentCells = part.Target.CurrentCell.GetLocalAdjacentCells();
					localAdjacentCells = new List<Cell>(Algorithms.RandomShuffle(localAdjacentCells, Stat.Rand));
					for (int i = 0; i < localAdjacentCells.Count; i++)
					{
						Cell cell2 = localAdjacentCells[i];
						if (cell2.IsEmpty())
						{
							cell = cell2;
							break;
						}
					}
				}
				if (cell == null)
				{
					return false;
				}
				if (implantee.IsPlayer())
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
				Event e = Event.New("InitiateRealityDistortionTransit", "Object", implantee, "Device", ParentObject, "Cell", cell);
				if (!implantee.FireEvent(e, E) || !cell.FireEvent(e, E))
				{
					return false;
				}
				implantee.TechTeleportSwirlOut();
				if (implantee.TeleportTo(cell, 0))
				{
					implantee.TechTeleportSwirlIn();
				}
				int turns = GetAvailableComputePowerEvent.AdjustDown(implantee, 50);
				implantee.CooldownActivatedAbility(ActivatedAbilityID, turns);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

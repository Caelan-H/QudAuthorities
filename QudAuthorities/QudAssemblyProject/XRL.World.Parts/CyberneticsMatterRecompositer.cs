using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMatterRecompositer : IPart
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
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Emergency Recomposite", commandId, "Cybernetics", "You teleport to a random, explored space on the map.", "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
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
				List<Cell> cells = implantee.CurrentCell.ParentZone.GetCells((Cell c) => c.Explored && c.IsEmpty());
				if (cells.Count <= 0)
				{
					if (implantee.IsPlayer())
					{
						Popup.ShowFail("There are no places to escape to safely!");
					}
					return false;
				}
				Cell randomElement = cells.GetRandomElement();
				Event e = Event.New("InitiateRealityDistortionTransit", "Object", implantee, "Device", ParentObject, "Cell", randomElement);
				if (!ParentObject.FireEvent(e, E) || !randomElement.FireEvent(e, E))
				{
					return false;
				}
				implantee.TechTeleportSwirlOut();
				if (implantee.TeleportTo(randomElement, 0))
				{
					implantee.TechTeleportSwirlIn();
				}
				int turns = GetAvailableComputePowerEvent.AdjustDown(implantee, 100);
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

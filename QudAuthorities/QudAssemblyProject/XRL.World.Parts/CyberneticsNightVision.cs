using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsNightVision : IPart
{
	public int Radius = 40;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != CommandEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.IsPlayer() && implantee.IsActivatedAbilityUsable(ActivatedAbilityID) && !IsBroken() && !IsRusted() && !IsEMPed())
		{
			Cell cell = implantee.CurrentCell;
			cell?.ParentZone?.AddLight(cell.X, cell.Y, Radius, LightLevel.Darkvision);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Night Vision", "CommandToggleCyberNightVision", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleCyberNightVision" && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPenetratingRadar : IPoweredPart
{
	public int Radius = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsPenetratingRadar()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "PhasedRadarArray";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != CommandEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.IsPlayer() && WasReady() && implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			Cell cell = implantee.CurrentCell;
			if (cell != null && !cell.OnWorldMap())
			{
				int r = GetAvailableComputePowerEvent.AdjustUp(implantee, Radius);
				cell.ParentZone?.AddLight(cell.X, cell.Y, r, LightLevel.Radar);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's range.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Penetrating Radar", "CommandTogglePenetratingRadar", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandTogglePenetratingRadar" && E.Actor == ParentObject.Implantee)
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

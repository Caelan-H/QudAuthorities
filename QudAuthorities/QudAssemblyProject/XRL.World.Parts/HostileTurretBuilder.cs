using System;

namespace XRL.World.Parts;

[Serializable]
public class HostileTurretBuilder : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		ParentObject.FireEventOnBodyparts(Event.New("PrepIntegratedHostToReceiveAmmo", "Host", ParentObject));
		ParentObject.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", ParentObject));
		CommandReloadEvent.Execute(ParentObject, FreeAction: true);
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (E.Context != "DeployTurret")
		{
			ParentObject.RegisterPartEvent(this, "EnteredCell");
		}
		else
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && cell.ParentZone.Built)
			{
				ParentObject.FireEventOnBodyparts(Event.New("PrepIntegratedHostToReceiveAmmo", "Host", ParentObject));
				ParentObject.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", ParentObject));
				CommandReloadEvent.Execute(ParentObject, FreeAction: true);
				ParentObject.RemovePart(this);
			}
		}
		return base.FireEvent(E);
	}
}

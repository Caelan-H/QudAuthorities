using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsBiologicalIndexer : IPart
{
	public string AffectedProperty = "BioScannerEquipped";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.ModIntProperty(AffectedProperty, 1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.ModIntProperty(AffectedProperty, -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}
}

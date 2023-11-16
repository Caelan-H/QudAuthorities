using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsEquipmentRack : IPart
{
	public string ManagerID => ParentObject.id + "::CyberneticsEquipmentRack";

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
		E.Part.ParentBody.GetBody().AddPartAt("Equipment Rack", 0, null, null, null, null, ManagerID, null, null, null, null, null, null, null, true, null, null, null, null, "Back", new string[5] { "Missile Weapon", "Hands", "Feet", "Roots", "Thrown Weapon" });
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		return base.HandleEvent(E);
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsGunRack : IPart
{
	public string ManagerID => ParentObject.id + "::CyberneticsGunRack";

	public override bool SameAs(IPart p)
	{
		return false;
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
		BodyPart body = E.Part.ParentBody.GetBody();
		BodyPart bodyPart = body.AddPartAt("Hardpoint", 2, null, null, null, null, Category: 6, Extrinsic: true, Manager: ManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Missile Weapon", OrInsertBefore: new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" });
		body.AddPartAt(bodyPart, "Hardpoint", 1, null, null, null, null, Category: 6, Extrinsic: true, Manager: ManagerID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Part.ParentBody.RemovePartsByManager(ManagerID, EvenIfDismembered: true);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class RootKnotInventory : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "ZoneActivated");
		Object.RegisterPartEvent(this, "ZoneFreezing");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneFreezing")
		{
			ParentObject.GetPart<Inventory>().Objects = new List<GameObject>();
			ParentObject.pBrain.PartyLeader = null;
		}
		if (E.ID == "EnteredCell" || E.ID == "ZoneActivated" || E.ID == "EndTurn")
		{
			Inventory part = ParentObject.GetPart<Inventory>();
			part.Objects = The.Game.RequireSystem(() => new RootKnotSystem()).inventory;
			if (part.Objects.Count == 0)
			{
				part.CheckEmptyState();
			}
			else
			{
				part.CheckNonEmptyState();
			}
			ParentObject.pBrain.PartyLeader = The.Player;
			ParentObject.SetIntProperty("ForceFeeling", 100);
		}
		return true;
	}
}

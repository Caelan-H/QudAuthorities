using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SupplyableIntegratedHost : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Supply", "supply", "Supply", null, 's');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Supply" && !AttemptSupply(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUseEarly");
		base.Register(Object);
	}

	public bool AttemptSupply(GameObject owner)
	{
		Event @event = Event.New("SupplyIntegratedHostWithAmmo");
		@event.SetParameter("Host", ParentObject);
		@event.SetParameter("Owner", owner);
		@event.SetFlag("TrackSupply", State: true);
		if (ParentObject.FireEventOnBodyparts(@event))
		{
			if (!@event.HasFlag("AnySupplyHandler"))
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("need") + " no supplies.");
			}
			else if (!@event.HasFlag("AnySupplies"))
			{
				Popup.Show("You have no supplies that " + ParentObject.the + ParentObject.ShortDisplayName + ParentObject.GetVerb("need") + ".");
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUseEarly")
		{
			if (ParentObject.HasPart("ConversationScript"))
			{
				return true;
			}
			if (AttemptSupply(E.GetGameObjectParameter("User")))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

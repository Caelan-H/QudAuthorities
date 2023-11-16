using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CherubimLock : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeContentsTaken");
		Object.RegisterPartEvent(this, "AfterContentsTaken");
		base.Register(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDestroyObjectEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneThawedEvent.ID)
		{
			return ID == AfterZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterZoneBuiltEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public void Obliterate()
	{
		if (The.Game.GetIntGameState("RobberChimesTriggered") == 1 && !ParentObject.HasIntProperty("Raised"))
		{
			ParentObject.Obliterate();
		}
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		Chime();
		return base.HandleEvent(E);
	}

	public void Chime()
	{
		if (The.Game.HasIntGameState("RobberChimesTriggered"))
		{
			return;
		}
		The.Game.SetIntGameState("RobberChimesTriggered", 1);
		Popup.Show("Robber chimes ring through the vault, and you hear the grating of stone on stone as the other imperial reliquaries slide into the unreachable recesses of the Tomb.");
		ParentObject.SetIntProperty("Raised", 1);
		ParentObject.GetPart<DoubleContainer>().GetSibling()?.SetIntProperty("Raised", 1);
		Event @event = Event.New("AIHelpBroadcast");
		@event.SetParameter("Faction", "Cherubim");
		@event.SetParameter("Target", The.Player);
		@event.SetParameter("Owned", ParentObject);
		foreach (Zone value in The.ZoneManager.CachedZones.Values)
		{
			foreach (GameObject item in value.YieldObjects())
			{
				if (IsLocked(item))
				{
					item.Obliterate();
				}
				else if (item.pBrain != null)
				{
					item.FireEvent(@event);
				}
			}
		}
	}

	private bool IsLocked(GameObject Object)
	{
		if (!Object.HasIntProperty("Raised"))
		{
			return Object.HasPart(typeof(CherubimLock));
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeContentsTaken" && ParentObject.CurrentCell.ParentZone.GetObjectsWithTagOrProperty("CherubimLock").Count > 0)
		{
			if (E.GetParameter<GameObject>("Taker") != null && E.GetParameter<GameObject>("Taker").IsPlayer())
			{
				Popup.Show("The protective force of the cherubim prevents you from taking anything from the reliquary.");
			}
			return false;
		}
		if (E.ID == "AfterContentsTaken")
		{
			if (E.GetParameter<GameObject>("Taker") != null && E.GetParameter<GameObject>("Taker").IsPlayer())
			{
				Chime();
			}
			return true;
		}
		return base.FireEvent(E);
	}
}

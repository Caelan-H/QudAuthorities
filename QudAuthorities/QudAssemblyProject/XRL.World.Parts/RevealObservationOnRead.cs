using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealObservationOnRead : IPart
{
	public string ObservationId;

	public RevealObservationOnRead()
	{
	}

	public RevealObservationOnRead(string ObservationId)
	{
		this.ObservationId = ObservationId;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RevealObservationOnRead).ObservationId != ObservationId)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.SetStringProperty("BookID", ParentObject.id);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != HasBeenReadEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && (string.IsNullOrEmpty(ObservationId) || JournalAPI.IsObservationRevealed(ObservationId)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		CheckObservation();
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (string.IsNullOrEmpty(ObservationId) || JournalAPI.IsObservationRevealed(ObservationId)) ? 1 : 100, 0, Override: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			CheckObservation();
			if (ObservationId != null)
			{
				JournalAPI.RevealObservation(ObservationId, onlyIfNotRevealed: true);
			}
		}
		return base.HandleEvent(E);
	}

	private void CheckObservation()
	{
		if (ObservationId == null && ParentObject.GetPart("AddObservation") is AddObservation addObservation)
		{
			ObservationId = addObservation.ID;
		}
	}
}

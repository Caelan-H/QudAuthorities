using System;
using Genkit;

namespace XRL.World;

public class IPartWithPrefabImposter : IPart
{
	[NonSerialized]
	public long ImposterID = -1L;

	public string prefabID;

	[NonSerialized]
	public string lastprefabID;

	public bool ImposterActive;

	public bool VisibleOnly = true;

	[FieldSaveVersion(223)]
	public int X;

	[FieldSaveVersion(223)]
	public int Y;

	public IPartWithPrefabImposter()
	{
		ImposterID = -1L;
		lastprefabID = null;
	}

	public void DestroyImposter()
	{
		if (ImposterID != -1)
		{
			ImposterBridge.DestroyImposter(ImposterID);
			ImposterID = -1L;
			lastprefabID = null;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetDebugInternalsEvent.ID && ID != LeftCellEvent.ID && ID != OnDestroyObjectEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == ZoneDeactivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ImposterID", ImposterID);
		E.AddEntry(this, "ImposterActive", ImposterActive);
		E.AddEntry(this, "VisibleOnly", VisibleOnly);
		E.AddEntry(this, "prefabID", prefabID);
		E.AddEntry(this, "lastprefabID", lastprefabID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Zone zone = ParentObject?.CurrentZone;
		if (zone != null && zone.IsActive())
		{
			ImposterActive = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		TeardownImposter();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		ImposterActive = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		TeardownImposter();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		TeardownImposter();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.SetIntProperty("HasImposter", 1);
		base.Register(Object);
	}

	private void TeardownImposter()
	{
		ImposterActive = false;
		DestroyImposter();
	}

	public override bool Render(RenderEvent E)
	{
		return true;
	}

	public override void UpdateImposter(QudScreenBufferExtra x)
	{
		if (!ImposterActive || prefabID == null)
		{
			lastprefabID = null;
			DestroyImposter();
			return;
		}
		if (ImposterID == -1)
		{
			ImposterID = ImposterManager.RegisterNewImposter();
			lastprefabID = null;
		}
		if (lastprefabID != prefabID)
		{
			x.setImposterPrefab(ImposterID, prefabID);
			lastprefabID = prefabID;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null || !currentZone.IsActive())
		{
			DestroyImposter();
			x.destroyImposter(ImposterID);
		}
		else if (!VisibleOnly || Visible())
		{
			x.showImposter(ImposterID);
			x.setImposterPosition(ImposterID, ParentObject.CurrentCell.Pos2D, new Point2D(X, Y));
		}
		else
		{
			x.hideImposter(ImposterID);
		}
	}
}

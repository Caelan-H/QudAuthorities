using System;

namespace XRL.World.Parts;

[Serializable]
public class ConnectWidget : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && ParentObject.CurrentZone != null)
		{
			ParentObject.CurrentZone.AddZoneConnection("-", ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, "Connection Widget", null);
		}
		return base.FireEvent(E);
	}
}

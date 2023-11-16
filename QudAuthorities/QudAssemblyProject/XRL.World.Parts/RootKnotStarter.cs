using System;

namespace XRL.World.Parts;

[Serializable]
public class RootKnotStarter : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			The.Game.RequireSystem(() => new RootKnotSystem()).NewZoneGenerated(ParentObject.pPhysics.CurrentCell.ParentZone);
			ParentObject.Destroy();
		}
		return true;
	}
}

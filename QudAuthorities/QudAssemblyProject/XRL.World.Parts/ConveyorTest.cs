using System;
using XRL.UI;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class ConveyorTest : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell cell = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 999, 40, 12, Locked: false, AllowVis.Any);
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 999, 40, 12, Locked: false, AllowVis.Any);
			ConveyorBelt conveyorBelt = new ConveyorBelt();
			conveyorBelt.x1 = cell.X;
			conveyorBelt.y1 = cell.Y;
			conveyorBelt.x2 = cell2.X;
			conveyorBelt.y2 = cell2.Y;
			conveyorBelt.BuildZone(ParentObject.pPhysics.CurrentCell.ParentZone);
		}
		return base.FireEvent(E);
	}
}

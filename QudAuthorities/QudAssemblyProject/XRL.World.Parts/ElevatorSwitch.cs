using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ElevatorSwitch : IPart
{
	public int TopLevel = 10;

	public int FloorLevel = 15;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "SwitchActivated");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SwitchActivated")
		{
			List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
			adjacentCells.Add(ParentObject.CurrentCell);
			bool flag = false;
			foreach (Cell item in adjacentCells)
			{
				foreach (GameObject item2 in item.GetObjectsWithIntProperty("ElevatorPlatform"))
				{
					flag = true;
					if (item2.CurrentCell == XRLCore.Core.Game.Player.Body.CurrentCell)
					{
						item2.CurrentCell.RemoveObject(XRLCore.Core.Game.Player.Body);
						Zone zoneAtLevel = item2.CurrentCell.ParentZone.GetZoneAtLevel(TopLevel);
						if (ParentObject.pPhysics.CurrentCell.ParentZone.Z == TopLevel)
						{
							zoneAtLevel = item2.pPhysics.CurrentCell.ParentZone.GetZoneAtLevel(FloorLevel);
						}
						int x = item2.CurrentCell.X;
						int y = item2.CurrentCell.Y;
						zoneAtLevel.GetCell(x, y).AddObject("Platform");
						zoneAtLevel.GetCell(x, y).AddObject(The.Player);
						The.ZoneManager.SetActiveZone(zoneAtLevel.ZoneID);
						The.ZoneManager.ProcessGoToPartyLeader();
						item2.Destroy();
						if (zoneAtLevel.Z < ParentObject.pPhysics.CurrentCell.ParentZone.Z)
						{
							Popup.Show("The chrome platform begins to hum as it ascends into the darkness.");
						}
						else
						{
							Popup.Show("The chrome platform begins to hum as it descends into the darkness.");
						}
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage("Nothing seems to happen when you hit the switch.");
					}
				}
			}
			if (!flag)
			{
				foreach (Cell item3 in adjacentCells)
				{
					foreach (GameObject item4 in item3.GetObjectsWithPart("StairsDown"))
					{
						if (!item4.CurrentCell.HasObject("Platform"))
						{
							item4.CurrentCell.AddObject("Platform");
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

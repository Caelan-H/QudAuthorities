using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class DoorSwitch : IPart
{
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
				foreach (GameObject item2 in item.LoopObjectsWithPart("Door"))
				{
					Door part = item2.GetPart<Door>();
					if (!part.bOpen)
					{
						part.Open();
						IComponent<GameObject>.AddPlayerMessage("The security door unlocks with a loud clank and swings open.");
						flag = true;
					}
					else
					{
						part.Close();
						IComponent<GameObject>.AddPlayerMessage("The security door swings closed and locks with a loud clank.");
						flag = true;
					}
				}
			}
			if (!flag)
			{
				IComponent<GameObject>.AddPlayerMessage("Nothing seems to happen when you hit the switch.");
			}
		}
		return base.FireEvent(E);
	}
}

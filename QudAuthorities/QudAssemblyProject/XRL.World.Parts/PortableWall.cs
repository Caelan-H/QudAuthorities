using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class PortableWall : IPart
{
	public string Blueprint = "Foamcrete";

	public int Size = 9;

	public string Message = "You open the box and expose the compressed foamcrete to air. It starts to expand.";

	public override bool SameAs(IPart p)
	{
		return false;
	}

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
		E.AddAction("Activate", "activate", "ActivatePortableWall", null, 'a', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivatePortableWall")
		{
			List<Cell> list = null;
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				list = PickFieldAdjacent(Size, E.Actor);
			}
			else
			{
				List<GameObject> list2 = E.Actor.CurrentCell.ParentZone.FastSquareVisibility(E.Actor.CurrentCell.X, E.Actor.CurrentCell.Y, Size, "ForceWallTarget", E.Actor);
				if (list2.Count > 0)
				{
					list = new List<Cell>(list2.Count);
					foreach (GameObject item in list2)
					{
						list.Add(item.CurrentCell);
						if (list.Count >= Size)
						{
							break;
						}
					}
				}
			}
			if (list != null && list.Count > 0)
			{
				int num = 0;
				foreach (Cell item2 in list)
				{
					string directionFromCell = E.Actor.CurrentCell.GetDirectionFromCell(item2);
					foreach (GameObject item3 in item2.GetObjectsWithPart("Physics"))
					{
						if (item3.pPhysics.Solid)
						{
							item3.pPhysics.Push(directionFromCell, 7500, 4);
						}
					}
					foreach (GameObject item4 in item2.GetObjectsWithPart("Combat"))
					{
						if (item4.pPhysics != null)
						{
							item4.pPhysics.Push(directionFromCell, 7500, 4);
						}
					}
					item2.AddObject(Blueprint);
					num++;
				}
				if (E.Actor != null && E.Actor.IsPlayer())
				{
					Popup.Show(GameText.VariableReplace(Message, E.Actor, null, ExplicitSubjectPlural: false, ParentObject));
				}
				ParentObject.Destroy();
			}
		}
		return base.HandleEvent(E);
	}
}

using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FixitSpray : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Item.IsBroken() || E.Item.IsRusted())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("The sprayer head won't move.");
				}
				return false;
			}
			List<GameObject> inventoryAndEquipment = E.Actor.GetInventoryAndEquipment();
			ProcessCellForTargets(E.Actor, E.Actor.CurrentCell, inventoryAndEquipment);
			foreach (Cell localAdjacentCell in E.Actor.CurrentCell.GetLocalAdjacentCells())
			{
				ProcessCellForTargets(E.Actor, localAdjacentCell, inventoryAndEquipment);
			}
			GameObject gameObject = PickItem.ShowPicker(inventoryAndEquipment, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
			if (gameObject == null)
			{
				return false;
			}
			int matterPhase = gameObject.GetMatterPhase();
			if (matterPhase >= 3 || !gameObject.PhaseMatches(E.Actor))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Some sticky goop passes through " + gameObject.t() + ".");
				}
			}
			else if (matterPhase == 2)
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Some sticky goop mixes in with " + gameObject.t() + ".");
				}
				gameObject.LiquidVolume?.MixWith(new LiquidVolume("gel", 1));
			}
			else
			{
				if (E.Actor.IsPlayer())
				{
					if (gameObject == E.Actor)
					{
						Popup.Show("You are covered in sticky goop!");
					}
					else
					{
						Popup.Show(gameObject.Does("are") + " covered in sticky goop!");
					}
					ParentObject.MakeUnderstood();
				}
				RepairedEvent.Send(E.Actor, gameObject, ParentObject);
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}

	private void ProcessCellForTargets(GameObject who, Cell C, List<GameObject> objs)
	{
		if (C == null)
		{
			return;
		}
		List<GameObject> list = (C.IsSolidFor(who) ? C.GetSolidObjectsFor(who) : C.Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject.pPhysics != null && gameObject.pPhysics.IsReal && IComponent<GameObject>.Visible(gameObject) && gameObject.pRender != null && gameObject.pRender.RenderLayer > 0 && !objs.Contains(gameObject) && gameObject != ParentObject)
			{
				objs.Add(gameObject);
			}
		}
	}
}

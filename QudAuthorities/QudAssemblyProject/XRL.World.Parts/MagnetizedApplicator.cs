using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class MagnetizedApplicator : IPart
{
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
			if (E.Item.IsBroken() || E.Item.IsRusted() || E.Item.IsEMPed())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(ParentObject.It + ParentObject.GetVerb("do") + " nothing.");
				}
				return false;
			}
			List<GameObject> objects = E.Actor.Inventory.GetObjects((GameObject o) => CanMagnetize(o, E.Actor));
			if (objects.Count == 0)
			{
				if (E.Actor.IsPlayer())
				{
					if (ParentObject.Understood())
					{
						Popup.ShowFail(ParentObject.It + ParentObject.GetVerb("do") + " nothing.");
					}
					else
					{
						Popup.ShowFail("You have no items that can be magnetized.");
					}
				}
				return false;
			}
			GameObject gameObject = PickItem.ShowPicker(objects, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			gameObject.SplitFromStack();
			if (E.Actor.IsPlayer())
			{
				ParentObject.MakeUnderstood();
			}
			string message = gameObject.The + gameObject.ShortDisplayName + gameObject.GetVerb("become") + " magnetized!";
			bool flag = gameObject.Understood();
			if (!TechModding.ApplyModification(gameObject, new ModMagnetized(), DoRegistration: true, E.Actor))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("Nothing happens.");
				}
				gameObject.CheckStack();
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(message);
			}
			if (flag && !gameObject.Understood())
			{
				gameObject.MakeUnderstood();
			}
			ParentObject.Destroy();
			gameObject.CheckStack();
		}
		return base.HandleEvent(E);
	}

	private bool CanMagnetize(GameObject obj, GameObject by)
	{
		return TechModding.ModificationApplicable("ModMagnetized", obj, by);
	}
}

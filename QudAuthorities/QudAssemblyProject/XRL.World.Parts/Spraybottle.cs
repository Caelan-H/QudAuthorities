using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Spraybottle : IPart
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
			if (E.Item.IsBroken() || E.Item.IsRusted())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("The sprayer head won't move.");
				}
				return false;
			}
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume.Volume <= 0)
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " empty.");
				}
				return false;
			}
			List<GameObject> equippedObjects = E.Actor.Body.GetEquippedObjects();
			equippedObjects.AddRange(E.Actor.Inventory.GetObjects());
			equippedObjects.Remove(ParentObject);
			GameObject gameObject = PickItem.ShowPicker(equippedObjects, "Fungal Infection", PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
			if (gameObject == null)
			{
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(gameObject.The + gameObject.ShortDisplayName + gameObject.Is + " covered in " + liquidVolume.GetLiquidName() + "!");
				ParentObject.MakeUnderstood();
			}
			gameObject.ApplyEffect(new LiquidCovered(liquidVolume, 1, Stat.Random(5, 10)));
		}
		return base.HandleEvent(E);
	}
}

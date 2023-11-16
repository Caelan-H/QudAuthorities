using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Polygel : IPart
{
	public const string REPLICATION_CONTEXT = "Polygel";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeReplicatedEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeReplicatedEvent E)
	{
		return false;
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
			Inventory inventory = E.Actor.Inventory;
			ParentObject.SplitFromStack();
			List<GameObject> list = Event.NewGameObjectList();
			inventory.GetObjects(list);
			list.Remove(ParentObject);
			GameObject gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.HasPart("Body") || !CanBeReplicatedEvent.Check(gameObject, E.Actor, "Polygel"))
			{
				Popup.ShowFail("A loud buzz is emitted. The unauthorized glyph flashes on the side of the applicator.");
				return false;
			}
			GameObject obj = null;
			bool flag = false;
			bool flag2 = false;
			if (ParentObject.GetIntProperty("NeverStack") == 0)
			{
				ParentObject.SetIntProperty("NeverStack", 1);
				flag = true;
			}
			try
			{
				gameObject.SplitStack(1, The.Player);
				obj = gameObject.DeepCopy(CopyEffects: true);
				if (obj.GetIntProperty("NeverStack") == 0)
				{
					obj.SetIntProperty("NeverStack", 1);
					flag2 = true;
				}
				obj.Inventory?.Clear();
				if (obj.HasPart("EnergyCellSocket"))
				{
					obj.GetPart<EnergyCellSocket>().Cell = null;
				}
				if (obj.HasPart("MagazineAmmoLoader"))
				{
					obj.GetPart<MagazineAmmoLoader>().Ammo = null;
				}
				Temporary.CarryOver(ParentObject, obj, CanRemove: true);
				Phase.carryOver(ParentObject, obj);
				if (!inventory.Objects.Contains(gameObject))
				{
					inventory.AddObject(gameObject);
				}
				if (!inventory.Objects.Contains(obj))
				{
					inventory.AddObject(obj);
				}
				try
				{
					if (E.Actor.IsPlayer() && !E.Item.Understood())
					{
						E.Item.MakeUnderstood();
						Popup.Show(ParentObject.Itis + " " + ParentObject.an() + "!");
					}
				}
				catch (Exception message)
				{
					MetricsManager.LogError(message);
				}
				try
				{
					ParentObject.Destroy();
				}
				catch (Exception message2)
				{
					MetricsManager.LogError(message2);
				}
				try
				{
					Popup.Show("The polygel morphs into another " + obj.ShortDisplayName + "!");
				}
				catch (Exception message3)
				{
					MetricsManager.LogError(message3);
				}
				try
				{
					WasReplicatedEvent.Send(gameObject, E.Actor, obj, "Polygel");
				}
				catch (Exception message4)
				{
					MetricsManager.LogError(message4);
				}
				try
				{
					ReplicaCreatedEvent.Send(obj, E.Actor, gameObject, "Polygel");
				}
				catch (Exception message5)
				{
					MetricsManager.LogError(message5);
				}
			}
			finally
			{
				if (flag && GameObject.validate(ParentObject))
				{
					ParentObject.RemoveIntProperty("NeverStack");
				}
				if (flag2 && GameObject.validate(ref obj))
				{
					obj.RemoveIntProperty("NeverStack");
				}
				obj?.CheckStack();
			}
		}
		return base.HandleEvent(E);
	}
}

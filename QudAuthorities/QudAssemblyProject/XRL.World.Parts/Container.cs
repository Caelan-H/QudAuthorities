using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Container : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && FireEvent(Event.New("Open", "Opener", E.Actor)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Open", "open", "Open", null, 'o');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Open")
		{
			ParentObject.FireEvent(Event.New("Open", "Opener", E.Actor), E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Open");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Open")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Opener");
			if (!ParentObject.IsValid() || !ParentObject.FireEvent("BeforeOpen"))
			{
				return true;
			}
			ParentObject.FireEvent("Opening");
			ParentObject.SetIntProperty("Autoexplored", 1);
			if (ParentObject.IsCreature)
			{
				if (gameObjectParameter.IsPlayer())
				{
					if (gameObjectParameter.PhaseMatches(ParentObject) && !ParentObject.HasPropertyOrTag("NoTrade") && !ParentObject.HasPropertyOrTag("FugueCopy") && gameObjectParameter.DistanceTo(ParentObject) <= 1)
					{
						if (ParentObject.IsPlayerLed())
						{
							TradeUI.ShowTradeScreen(ParentObject, 0f);
						}
						else
						{
							TradeUI.ShowTradeScreen(ParentObject);
						}
					}
					else
					{
						Popup.ShowFail("You cannot trade with " + ParentObject.t() + ".");
					}
				}
				return true;
			}
			if (!ParentObject.HasTagOrProperty("DontWarnOnOpen") && gameObjectParameter.IsPlayer() && !string.IsNullOrEmpty(ParentObject.Owner) && ParentObject.Equipped != IComponent<GameObject>.ThePlayer && ParentObject.InInventory != IComponent<GameObject>.ThePlayer)
			{
				if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to open it?") != 0)
				{
					return true;
				}
				ParentObject.pPhysics.BroadcastForHelp(gameObjectParameter);
			}
			if (gameObjectParameter.IsPlayer())
			{
				Inventory inventory = ParentObject.Inventory;
				if (inventory == null || inventory.GetObjectCount() == 0)
				{
					if (Popup.ShowYesNo("There's nothing in that. Would you like to store an item?") == DialogResult.Yes)
					{
						TradeUI.ShowTradeScreen(ParentObject, 0f, TradeUI.TradeScreenMode.Container);
						if (inventory.GetObjectCount() > 0)
						{
							gameObjectParameter.FireEvent(Event.New("PutSomethingIn", "Object", ParentObject));
						}
					}
				}
				else
				{
					List<GameObject> list = new List<GameObject>(inventory.GetObjectsDirect());
					bool RequestInterfaceExit = false;
					string title = "[ {{W|Opening " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "}} ]";
					bool notePlayerOwned = false;
					List<GameObject> objects = inventory.GetObjects();
					GameObject parentObject = ParentObject;
					Func<List<GameObject>> regenerate = inventory.GetObjects;
					PickItem.ShowPicker(objects, ref RequestInterfaceExit, null, PickItem.PickItemDialogStyle.GetItemDialog, gameObjectParameter, parentObject, null, title, PreserveOrder: false, regenerate, ShowContext: false, ShowIcons: true, notePlayerOwned);
					if (RequestInterfaceExit)
					{
						E.RequestInterfaceExit();
					}
					if (list.Count < inventory.GetObjectCountDirect() && inventory.GetObjectCount() > 0)
					{
						gameObjectParameter.FireEvent(Event.New("PutSomethingIn", "Object", ParentObject));
					}
					for (int i = 0; i < list.Count; i++)
					{
						GameObject gameObject = list[i];
						if (!inventory.Objects.Contains(gameObject))
						{
							if (ParentObject.IsOwned() && !gameObject.OwnedByPlayer)
							{
								ParentObject.pPhysics.BroadcastForHelp(gameObjectParameter);
							}
							ParentObject.FireEvent(Event.New("SomethingTakenOrUsed", "Object", gameObject));
							break;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

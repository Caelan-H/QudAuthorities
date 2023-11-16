using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class EnergyCellSocket : IPoweredPart
{
	public string SlotType = "EnergyCell";

	public GameObject Cell;

	public int ChanceSlotted = 50;

	public string SlottedType = "#Chem Cell,Chem Cell,@DynamicObjectsTable:EnergyCells:Tier{zonetier}";

	public int ChanceFullCell = 10;

	public int ChanceDestroyCellOnForcedUnequip;

	public string RemoveCellUnpoweredSound = "compartment_open";

	public string RemoveCellPoweredSound = "compartment_open_whine_down";

	public string SlotCellUnpoweredSound = "compartment_close";

	public string SlotCellPoweredSound = "compartment_close_whine_up";

	public EnergyCellSocket()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnSelf = true;
	}

	public override bool CanGenerateStacked()
	{
		if (ChanceSlotted != 0)
		{
			if (ChanceSlotted < 100)
			{
				return false;
			}
			if (SlottedType.Contains("#") || SlottedType.Contains("@") || SlottedType.Contains("*"))
			{
				return false;
			}
		}
		return base.CanGenerateStacked();
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		EnergyCellSocket energyCellSocket = new EnergyCellSocket();
		energyCellSocket.SlotType = SlotType;
		energyCellSocket.ChanceSlotted = ChanceSlotted;
		if (GameObject.validate(ref Cell))
		{
			energyCellSocket.Cell = MapInv?.Invoke(Cell) ?? Cell.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (energyCellSocket.Cell != null)
			{
				energyCellSocket.Cell.ForeachPartDescendedFrom(delegate(IEnergyCell p)
				{
					p.SlottedIn = Parent;
				});
			}
		}
		energyCellSocket.SlottedType = SlottedType;
		energyCellSocket.ChanceFullCell = ChanceFullCell;
		energyCellSocket.ChanceDestroyCellOnForcedUnequip = ChanceDestroyCellOnForcedUnequip;
		energyCellSocket.RemoveCellUnpoweredSound = RemoveCellUnpoweredSound;
		energyCellSocket.RemoveCellPoweredSound = RemoveCellPoweredSound;
		energyCellSocket.SlotCellUnpoweredSound = SlotCellUnpoweredSound;
		energyCellSocket.SlotCellPoweredSound = SlotCellPoweredSound;
		energyCellSocket.ParentObject = Parent;
		return energyCellSocket;
	}

	public override bool SameAs(IPart p)
	{
		EnergyCellSocket energyCellSocket = p as EnergyCellSocket;
		if (energyCellSocket.SlotType != SlotType)
		{
			return false;
		}
		if (energyCellSocket.Cell != null || Cell != null)
		{
			return false;
		}
		if (energyCellSocket.ChanceDestroyCellOnForcedUnequip != ChanceDestroyCellOnForcedUnequip)
		{
			return false;
		}
		if (energyCellSocket.RemoveCellUnpoweredSound != RemoveCellUnpoweredSound)
		{
			return false;
		}
		if (energyCellSocket.RemoveCellPoweredSound != RemoveCellPoweredSound)
		{
			return false;
		}
		if (energyCellSocket.SlotCellUnpoweredSound != SlotCellUnpoweredSound)
		{
			return false;
		}
		if (energyCellSocket.SlotCellPoweredSound != SlotCellPoweredSound)
		{
			return false;
		}
		return base.SameAs(p);
	}

	private bool CellWantsEvent(int ID, int cascade)
	{
		if (!GameObject.validate(ref Cell))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 4))
		{
			return false;
		}
		return Cell.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GetContentsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && (ID != ObjectCreatedEvent.ID || ChanceSlotted <= 0) && ID != StripContentsEvent.ID)
		{
			return CellWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(4) && GameObject.validate(ref Cell) && !Cell.HandleEvent(E))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Context != "Tinkering" && E.Understood())
		{
			if (!GameObject.validate(ref Cell))
			{
				E.AddTag("{{y|[{{K|no cell}}]}}", -5);
			}
			else
			{
				E.AddTag("{{y|[" + Cell.DisplayName + "]}}", -5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (GameObject.validate(ref Cell) && E.CascadeTo(4) && !Cell.HandleEvent(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (GameObject.validate(ref Cell))
		{
			E.AddAction("Replace Cell", "replace cell", "ReplaceSocketCell", "cell", 'c');
			GetSlottedInventoryActionsEvent.Send(Cell, E);
		}
		else
		{
			E.AddAction("Replace Cell", "install cell", "ReplaceSocketCell", null, 'c', FireOnActor: false, 15);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ReplaceSocketCell")
		{
			return AttemptReplaceCell(E.Actor, E, E.MinimumCharge);
		}
		if (E.Command == "EmptyForDisassemble" && GameObject.validate(ref Cell))
		{
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			if (ObjectContext != null)
			{
				ObjectContext.TakeObject(Cell, Silent: false, 0);
			}
			else if (CellContext != null)
			{
				CellContext.AddObject(Cell);
			}
			else
			{
				E.Actor.TakeObject(Cell, Silent: false, 0);
			}
			SetCell(null);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.validate(ref Cell))
		{
			E.Value += Cell.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.validate(ref Cell))
		{
			E.Weight += Cell.GetWeight();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		if (GameObject.validate(ref Cell) && (!E.KeepNatural || !Cell.IsNatural()))
		{
			GameObject cell = Cell;
			SetCell(null);
			cell.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		if (GameObject.validate(ref Cell))
		{
			E.Objects.Add(Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ChanceSlotted.in100())
		{
			GameObjectFactory.ProcessSpecification(SlottedType, LoadCell);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingUnequipped");
		Object.RegisterPartEvent(this, "InductionCharge");
		Object.RegisterPartEvent(this, "QueryCharge");
		Object.RegisterPartEvent(this, "QueryChargeStorage");
		Object.RegisterPartEvent(this, "QueryRechargeStorage");
		Object.RegisterPartEvent(this, "RechargeAvailable");
		Object.RegisterPartEvent(this, "TestCharge");
		Object.RegisterPartEvent(this, "UseCharge");
		base.Register(Object);
	}

	public override bool WantTurnTick()
	{
		if (GameObject.validate(ref Cell))
		{
			return Cell.WantTurnTick();
		}
		return false;
	}

	public override bool WantTenTurnTick()
	{
		if (GameObject.validate(ref Cell))
		{
			return Cell.WantTenTurnTick();
		}
		return false;
	}

	public override bool WantHundredTurnTick()
	{
		if (GameObject.validate(ref Cell))
		{
			return Cell.WantHundredTurnTick();
		}
		return false;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Cell) && Cell.WantTurnTick())
		{
			Cell.TurnTick(TurnNumber);
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Cell) && Cell.WantTenTurnTick())
		{
			Cell.TenTurnTick(TurnNumber);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Cell) && Cell.WantHundredTurnTick())
		{
			Cell.HundredTurnTick(TurnNumber);
		}
	}

	public void SetCell(GameObject obj)
	{
		if (obj == Cell)
		{
			return;
		}
		if (Cell != null)
		{
			Cell.ForeachPartDescendedFrom(delegate(IEnergyCell p)
			{
				p.SlottedIn = null;
			});
		}
		Cell = obj;
		obj?.ForeachPartDescendedFrom(delegate(IEnergyCell p)
		{
			p.SlottedIn = ParentObject;
		});
		FlushWantTurnTickCache();
	}

	private void LoadCell(GameObject obj)
	{
		if (obj == null)
		{
			Debug.LogError("Unknown cell type: " + SlottedType);
			return;
		}
		IEnergyCell partDescendedFrom = obj.GetPartDescendedFrom<IEnergyCell>();
		if (partDescendedFrom == null)
		{
			Debug.LogError("Cell type has no IEnergyCell part: " + obj.Blueprint);
			return;
		}
		if (Cell != null)
		{
			Debug.LogError("Multiple cells generated: " + Cell.Blueprint + ", " + obj.Blueprint);
			return;
		}
		SetCell(obj);
		if (ChanceFullCell.in100())
		{
			partDescendedFrom.MaximizeCharge();
		}
		else
		{
			partDescendedFrom.RandomizeCharge();
		}
	}

	public bool AttemptRemoveCell(GameObject Owner, InventoryActionEvent E)
	{
		GameObject cell = Cell;
		bool flag = cell.GetIntProperty("NeverStack") > 0;
		if (!flag)
		{
			cell.SetIntProperty("NeverStack", 1);
		}
		Event @event = Event.New("CommandTakeObject", "Object", cell);
		if (E.OverrideEnergyCost)
		{
			@event.SetParameter("EnergyCost", E.EnergyCostOverride);
		}
		Inventory inventory = Owner.Inventory;
		if (inventory == null || !inventory.FireEvent(@event))
		{
			if (Owner.IsPlayer())
			{
				Popup.ShowFail("You can't remove " + Cell.the + Cell.ShortDisplayName + "!");
			}
			return false;
		}
		IComponent<GameObject>.WDidXToYWithZ(Owner, "pop", cell, "out of", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, (ParentObject.Equipped == Owner || ParentObject.InInventory == Owner) ? Owner : null);
		if (!flag)
		{
			cell.RemoveIntProperty("NeverStack");
		}
		cell.RemoveIntProperty("StoredByPlayer");
		SetCell(null);
		cell.CheckStack();
		return true;
	}

	public bool AttemptReplaceCell(GameObject Actor, InventoryActionEvent E, int MinimumCharge = 0)
	{
		GameObject.validate(ref Cell);
		if (Actor == null)
		{
			Actor = ParentObject.Equipped ?? ParentObject.InInventory;
			if (Actor == null)
			{
				return false;
			}
		}
		bool flag = false;
		if (Cell != null && Cell.GetIntProperty("StoredByPlayer") < 1 && Actor.IsPlayer() && ParentObject.Owner != null && ParentObject.Equipped != Actor && ParentObject.InInventory != Actor)
		{
			if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to access " + ParentObject.its + " energy cell?") != 0)
			{
				return false;
			}
			flag = true;
		}
		Inventory inventory = Actor.Inventory;
		if (inventory == null)
		{
			return false;
		}
		List<string> OptionStrings = new List<string>(16);
		List<object> options = new List<object>(16);
		List<char> keymap = new List<char>(16);
		if (Cell != null)
		{
			OptionStrings.Add("remove cell");
			options.Add(null);
			keymap.Add('-');
			TinkerItem part = Cell.GetPart<TinkerItem>();
			if (part != null && part.CanBeDisassembled(Actor))
			{
				OptionStrings.Add("disassemble cell");
				options.Add(-1);
				keymap.Add('/');
			}
		}
		List<GameObject> objs = Event.NewGameObjectList();
		Dictionary<GameObject, int> charge = new Dictionary<GameObject, int>();
		Dictionary<GameObject, string> names = new Dictionary<GameObject, string>();
		inventory.ForeachObject(delegate(GameObject GO)
		{
			if (!Actor.IsPlayer() || GO.Understood())
			{
				GO.ForeachPartDescendedFrom(delegate(IEnergyCell P)
				{
					if (P.SlotType == SlotType)
					{
						objs.Add(GO);
						charge[GO] = P.GetCharge();
						names[GO] = GO.DisplayName;
						return false;
					}
					return true;
				});
			}
		});
		if (objs.Count > 1)
		{
			objs.Sort(delegate(GameObject a, GameObject b)
			{
				int num = charge[a].CompareTo(charge[b]);
				return (num != 0) ? (-num) : names[a].CompareTo(names[b]);
			});
		}
		char c = 'a';
		foreach (GameObject obj in objs)
		{
			obj.ForeachPartDescendedFrom(delegate(IEnergyCell P)
			{
				if (P.SlotType == SlotType)
				{
					OptionStrings.Add(names[obj]);
					options.Add(obj);
					if (c <= 'z')
					{
						List<char> list = keymap;
						char c2 = c;
						c = (char)(c2 + 1);
						list.Add(c2);
					}
					else
					{
						keymap.Add(' ');
					}
					return false;
				}
				return true;
			});
		}
		if (options.Count == 0)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You have no cells that fit!");
			}
			return false;
		}
		int choice = -1;
		if (Actor.IsPlayer())
		{
			choice = Popup.ShowOptionList("Select a cell for " + ParentObject.the + ParentObject.ShortDisplayName, OptionStrings.ToArray(), keymap.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
		}
		else
		{
			Cell?.ForeachPartDescendedFrom(delegate(IEnergyCell P)
			{
				MinimumCharge = Math.Max(MinimumCharge, P.GetCharge() + 1);
			});
			int maxCharge = -1;
			int x;
			for (x = 0; x < options.Count; x++)
			{
				if (!(options[x] is GameObject gameObject))
				{
					continue;
				}
				gameObject.ForeachPartDescendedFrom(delegate(IEnergyCell P)
				{
					int charge2 = P.GetCharge();
					if (charge2 > maxCharge && charge2 > 0 && charge2 >= MinimumCharge)
					{
						choice = x;
						maxCharge = charge2;
					}
				});
			}
			if (choice < 0)
			{
				return false;
			}
		}
		GameObject cell = Cell;
		bool flag2 = Cell != null && Cell.QueryCharge(LiveOnly: false, 0L) > 0 && !IsBroken() && !IsRusted() && !IsEMPed();
		bool flag3 = false;
		bool flag4 = false;
		if (choice >= 0)
		{
			if (options[choice] == null)
			{
				if (AttemptRemoveCell(Actor, E))
				{
					flag3 = true;
				}
			}
			else if (options[choice] as int? == -1)
			{
				GameObject cell2 = Cell;
				if (AttemptRemoveCell(Actor, E))
				{
					flag3 = true;
					InventoryActionEvent.Check(cell2, Actor, cell2, "Disassemble");
					FlushWeightCaches();
				}
			}
			else
			{
				bool flag5 = false;
				if (Cell == null)
				{
					Actor.UseEnergy(E.OverrideEnergyCost ? E.EnergyCostOverride : 1000, "Reload Energy Cell");
					flag5 = true;
				}
				else if (AttemptRemoveCell(Actor, E))
				{
					flag5 = true;
					flag3 = true;
				}
				if (flag5)
				{
					GameObject gameObject2 = options[choice] as GameObject;
					ParentObject.SplitFromStack();
					gameObject2 = gameObject2.RemoveOne();
					Event @event = Event.New("CommandRemoveObject");
					@event.SetParameter("Object", gameObject2);
					@event.SetFlag("ForEquip", State: true);
					@event.SetSilent(Silent: true);
					if (!inventory.FireEvent(@event))
					{
						if (Actor.IsPlayer())
						{
							Popup.ShowFail("You can't take the old cell out of your inventory!");
						}
						gameObject2.CheckStack();
					}
					else
					{
						flag4 = true;
						gameObject2.RemoveFromContext();
						SetCell(gameObject2);
						IComponent<GameObject>.WDidXToYWithZ(Actor, "slot", Cell, "into", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, (ParentObject.Equipped == Actor || ParentObject.InInventory == Actor) ? Actor : null);
					}
				}
				else if (Actor.IsPlayer())
				{
					Popup.ShowFail("You can't remove the old cell!");
				}
			}
		}
		bool flag6 = Cell != null && Cell.QueryCharge(LiveOnly: false, 0L) > 0 && !IsBroken() && !IsRusted() && !IsEMPed();
		if (flag3 || flag4)
		{
			string tag = ParentObject.GetTag("ReloadSound");
			if (!string.IsNullOrEmpty(tag))
			{
				PlayWorldSound(tag);
			}
			else if (flag4 && !string.IsNullOrEmpty(flag6 ? SlotCellPoweredSound : SlotCellUnpoweredSound))
			{
				PlayWorldSound(flag6 ? SlotCellPoweredSound : SlotCellUnpoweredSound);
			}
			else if (flag3 && !string.IsNullOrEmpty(flag2 ? RemoveCellPoweredSound : RemoveCellUnpoweredSound))
			{
				PlayWorldSound(flag2 ? RemoveCellPoweredSound : RemoveCellUnpoweredSound);
			}
			CellChangedEvent.Send(Actor, ParentObject, cell, Cell);
		}
		if (Cell != null && Actor.IsPlayer())
		{
			Cell.SetIntProperty("StoredByPlayer", 1);
		}
		if (flag3 && flag)
		{
			ParentObject.pPhysics.BroadcastForHelp(Actor);
		}
		ParentObject.CheckStack();
		return flag3 || flag4;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TestCharge" || E.ID == "QueryCharge" || E.ID == "RechargeAvailable" || E.ID == "InductionCharge" || E.ID == "QueryRechargeStorage" || E.ID == "QueryChargeStorage")
		{
			if (GameObject.validate(ref Cell) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && !Cell.FireEvent(E))
			{
				return false;
			}
		}
		else if (E.ID == "UseCharge")
		{
			if (GameObject.validate(ref Cell) && IsReady(E.GetIntParameter("Charge") > 0, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && !Cell.FireEvent(E))
			{
				return false;
			}
		}
		else if (E.ID == "BeginBeingUnequipped")
		{
			GameObject obj = Cell;
			if (ChanceDestroyCellOnForcedUnequip > 0 && GameObject.validate(ref obj) && E.HasFlag("Forced") && ChanceDestroyCellOnForcedUnequip.in100() && obj.FireEvent("CanForcedUnequipDestroy"))
			{
				SetCell(null);
				obj.Destroy();
			}
		}
		return base.FireEvent(E);
	}
}

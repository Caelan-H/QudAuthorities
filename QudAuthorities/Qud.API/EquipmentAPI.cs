using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace Qud.API;

public static class EquipmentAPI
{
	private static Dictionary<string, InventoryAction> CheckActionTable = new Dictionary<string, InventoryAction>(16);

	public static void TwiddleObject(GameObject GO, Action After = null, bool Distant = false, bool TelekineticOnly = false)
	{
		bool Done = false;
		TwiddleObject(The.Player, GO, ref Done, Distant, TelekineticOnly);
		After?.Invoke();
	}

	public static void TwiddleObject(GameObject GO, ref bool Done, Action After = null, bool Distant = false, bool TelekineticOnly = false)
	{
		TwiddleObject(The.Player, GO, ref Done, Distant, TelekineticOnly);
		After?.Invoke();
	}

	public static InventoryAction ShowInventoryActionMenu(Dictionary<string, InventoryAction> ActionTable, GameObject Owner = null, GameObject GO = null, bool Distant = false, bool TelekineticOnly = false, string Intro = null, IComparer<InventoryAction> Comparer = null)
	{
		List<InventoryAction> list = new List<InventoryAction>();
		foreach (KeyValuePair<string, InventoryAction> item in ActionTable)
		{
			InventoryAction value = item.Value;
			if ((!TelekineticOnly || value.WorksTelekinetically) && value.IsVisible(Owner, GO, Distant))
			{
				list.Add(value);
			}
		}
		list.Sort(Comparer ?? (Comparer = new InventoryAction.Comparer()));
		Dictionary<char, InventoryAction> dictionary = new Dictionary<char, InventoryAction>(16);
		List<InventoryAction> list2 = null;
		StringBuilder SB = null;
		foreach (InventoryAction item2 in list)
		{
			if (item2.Key == ' ')
			{
				continue;
			}
			if (dictionary.ContainsKey(item2.Key))
			{
				if (list2 == null)
				{
					list2 = new List<InventoryAction>();
				}
				list2.Add(item2);
			}
			else
			{
				dictionary.Add(item2.Key, item2);
				item2.Display = ApplyHotkey(item2.Display, item2.Key, item2.PreferToHighlight, ref SB);
			}
		}
		if (list2 != null)
		{
			if (SB == null)
			{
				SB = Event.NewStringBuilder();
			}
			foreach (InventoryAction item3 in list2)
			{
				char c = char.ToUpper(item3.Key);
				if (c != item3.Key && !dictionary.ContainsKey(c))
				{
					item3.Key = c;
					dictionary.Add(c, item3);
					item3.Display = ApplyHotkey(ColorUtility.StripFormatting(item3.Display), c, item3.PreferToHighlight, ref SB);
					continue;
				}
				string display = item3.Display;
				display = ColorUtility.StripFormatting(display);
				bool flag = false;
				SB.Clear();
				int i = 0;
				for (int length = display.Length; i < length; i++)
				{
					char c2 = display[i];
					if (!dictionary.ContainsKey(c2))
					{
						item3.Key = c2;
						dictionary.Add(c2, item3);
						SB.Append("{{hotkey|").Append(c2).Append("}}")
							.Append(display, i + 1, length - i - 1);
						flag = true;
						break;
					}
					SB.Append(c2);
				}
				if (!flag)
				{
					item3.Key = ' ';
				}
				item3.Display = SB.ToString();
			}
			list.Sort(Comparer);
		}
		List<string> list3 = new List<string>();
		List<char> list4 = new List<char>();
		foreach (InventoryAction item4 in list)
		{
			list3.Add(item4.Display);
			list4.Add(item4.Key);
		}
		int defaultSelected = 0;
		int num = int.MinValue;
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			if (list[j].Default > num)
			{
				defaultSelected = j;
				num = list[j].Default;
			}
		}
		The.Player.GetConfusion();
		int num2 = Popup.ShowOptionList("", list3.ToArray(), list4.ToArray(), 0, Intro ?? ((Confusion.CurrentConfusionLevel > 0) ? GO.DisplayName : null), 60, RespectOptionNewlines: true, AllowEscape: true, IntroIcon: (Confusion.CurrentConfusionLevel > 0) ? null : GO.RenderForUI(), defaultSelected: defaultSelected, SpacingText: "", onResult: null, context: (Confusion.CurrentConfusionLevel > 0) ? null : GO, Icons: null, Buttons: null, centerIntro: true);
		if (num2 < 0)
		{
			return null;
		}
		return list[num2];
	}

	public static void TwiddleObject(GameObject Owner, GameObject GO, ref bool Done, bool Distant = false, bool TelekineticOnly = false)
	{
		try
		{
			if (!GameObject.validate(GO) || GO.IsInGraveyard())
			{
				return;
			}
			GameManager.Instance.PushGameView("TwiddleObject");
			while (true)
			{
				Dictionary<string, InventoryAction> dictionary = new Dictionary<string, InventoryAction>(16);
				if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Owner.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(dictionary);
					GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
					GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
					Owner.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
				}
				GetInventoryActionsEvent.Send(Owner, GO, dictionary);
				GetInventoryActionsAlwaysEvent.Send(Owner, GO, dictionary);
				OwnerGetInventoryActionsEvent.Send(Owner, GO, dictionary);
				GameObject inInventory = GO.InInventory;
				GameObject equipped = GO.Equipped;
				Cell currentCell = GO.CurrentCell;
				InventoryAction inventoryAction = ShowInventoryActionMenu(dictionary, Owner, GO, Distant, TelekineticOnly);
				if (inventoryAction == null)
				{
					GO.CheckStack();
					GameManager.Instance.PopGameView(bHard: true);
					return;
				}
				if (!inventoryAction.IsUsable(Owner, GO, Distant, out var Telekinetic))
				{
					if (Telekinetic)
					{
						Popup.Show(GO.The + GO.ShortDisplayName + GO.Is + " out of your telekinetic range.");
					}
					else
					{
						Popup.Show("You cannot do that from here.");
					}
					continue;
				}
				IEvent @event = inventoryAction.Process(GO, Owner, Telekinetic);
				if (@event != null && @event.InterfaceExitRequested())
				{
					GameManager.Instance.PopGameView(bHard: true);
					Done = true;
					return;
				}
				if (GO.IsInGraveyard() || GO.IsInvalid())
				{
					GameManager.Instance.PopGameView(bHard: true);
					return;
				}
				if (currentCell != GO.CurrentCell)
				{
					GameManager.Instance.PopGameView(bHard: true);
					return;
				}
				if (inInventory != GO.InInventory)
				{
					GameManager.Instance.PopGameView(bHard: true);
					return;
				}
				if (equipped != GO.Equipped)
				{
					break;
				}
			}
			GameManager.Instance.PopGameView(bHard: true);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TwiddleObject", x);
			GameManager.Instance.PopGameView(bHard: true);
		}
	}

	private static string ApplyHotkey(string display, char key, string prefer, ref StringBuilder SB)
	{
		if (SB == null)
		{
			SB = Event.NewStringBuilder();
		}
		else
		{
			SB.Clear();
		}
		if (!display.Contains("{{") && !display.Contains("&"))
		{
			char c = char.ToLower(key);
			int num = -1;
			int num2 = -1;
			if (!string.IsNullOrEmpty(prefer))
			{
				num2 = display.IndexOf(prefer);
				if (num2 != -1)
				{
					num = prefer.IndexOf(key);
					if (num == -1 && c != key)
					{
						num = prefer.IndexOf(c);
					}
				}
			}
			if (num != -1)
			{
				int num3 = num + num2;
				display = SB.Append(display, 0, num3).Append("{{hotkey|").Append(key)
					.Append("}}")
					.Append(display, num3 + 1, display.Length - num3 - 1)
					.ToString();
			}
			else
			{
				int num4 = display.IndexOf(key);
				if (num4 != -1)
				{
					display = SB.Append(display, 0, num4).Append("{{hotkey|").Append(key)
						.Append("}}")
						.Append(display, num4 + 1, display.Length - num4 - 1)
						.ToString();
				}
				else if (c != key)
				{
					num4 = display.IndexOf(c);
					if (num4 != -1)
					{
						display = SB.Append(display, 0, num4).Append("{{hotkey|").Append(key)
							.Append("}}")
							.Append(display, num4 + 1, display.Length - num4 - 1)
							.ToString();
					}
				}
			}
		}
		return display;
	}

	private static bool GotUsableAction(GameObject GO, GameObject Actor, bool TelekineticOnly)
	{
		if (TelekineticOnly)
		{
			foreach (InventoryAction value in CheckActionTable.Values)
			{
				if (value.WorksTelekinetically && value.IsVisible(Actor, GO, Distant: true))
				{
					return true;
				}
			}
			return false;
		}
		return CheckActionTable.Count > 0;
	}

	public static bool CanBeTwiddled(GameObject GO, GameObject Actor, bool TelekineticOnly = false)
	{
		if (GO == null)
		{
			return false;
		}
		if (GO.HasTag("NoTwiddle"))
		{
			return false;
		}
		if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions") || GO.WantEvent(GetInventoryActionsEvent.ID, MinEvent.CascadeLevel) || GO.WantEvent(GetInventoryActionsAlwaysEvent.ID, MinEvent.CascadeLevel) || Actor.WantEvent(OwnerGetInventoryActionsEvent.ID, OwnerGetInventoryActionsEvent.CascadeLevel))
		{
			CheckActionTable.Clear();
			try
			{
				if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(CheckActionTable);
					if (GO.HasRegisteredEvent("GetInventoryActions"))
					{
						GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
					if (GO.HasRegisteredEvent("GetInventoryActionsAlways"))
					{
						GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
					if (Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
					{
						Actor.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
				}
				GetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
				GetInventoryActionsAlwaysEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
				OwnerGetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
			}
			finally
			{
				CheckActionTable.Clear();
			}
		}
		return false;
	}

	public static List<InventoryAction> GetInventoryActions(GameObject GO, GameObject Actor, bool TelekineticOnly = false, bool TelekineticRequireUsable = true)
	{
		if (GO.HasTag("NoInventoryActions"))
		{
			return null;
		}
		List<InventoryAction> list = null;
		if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions") || GO.WantEvent(GetInventoryActionsEvent.ID, MinEvent.CascadeLevel) || GO.WantEvent(GetInventoryActionsAlwaysEvent.ID, MinEvent.CascadeLevel) || Actor.WantEvent(OwnerGetInventoryActionsEvent.ID, OwnerGetInventoryActionsEvent.CascadeLevel))
		{
			CheckActionTable.Clear();
			if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
			{
				EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(CheckActionTable);
				if (GO.HasRegisteredEvent("GetInventoryActions"))
				{
					GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
				}
				if (GO.HasRegisteredEvent("GetInventoryActionsAlways"))
				{
					GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
				}
				if (Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					Actor.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
				}
			}
			GetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
			GetInventoryActionsAlwaysEvent.Send(Actor, GO, CheckActionTable);
			OwnerGetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
			if (CheckActionTable.Count > 0)
			{
				list = new List<InventoryAction>(CheckActionTable.Values);
				CheckActionTable.Clear();
				if (TelekineticOnly && list.Count > 0)
				{
					List<InventoryAction> list2 = new List<InventoryAction>();
					foreach (InventoryAction item in list)
					{
						if (!item.WorksTelekinetically)
						{
							continue;
						}
						if (TelekineticRequireUsable)
						{
							if (item.IsUsable(Actor, GO, Distant: true))
							{
								list2.Add(item);
							}
						}
						else if (item.IsVisible(Actor, GO, Distant: true))
						{
							list2.Add(item);
						}
					}
					list = list2;
				}
			}
		}
		return list;
	}

	public static bool UnequipObject(GameObject itemToUnequip)
	{
		GameObject equipped = itemToUnequip.Equipped;
		bool result = true;
		if (equipped != null)
		{
			result = InventoryActionEvent.Check(equipped, equipped, itemToUnequip, "Unequip");
		}
		if (itemToUnequip.Equipped == null && itemToUnequip.InInventory == null)
		{
			equipped.Inventory?.AddObject(itemToUnequip);
			result = true;
		}
		return result;
	}

	public static bool ForceUnequipObject(GameObject itemToUnequip, bool Silent = false)
	{
		GameObject equipped = itemToUnequip.Equipped;
		bool result = true;
		if (equipped != null)
		{
			Event @event = Event.New("CommandForceUnequipObject", "Object", itemToUnequip);
			@event.SetSilent(Silent);
			equipped.FireEvent(@event);
		}
		if (itemToUnequip.Equipped == null && itemToUnequip.InInventory == null)
		{
			equipped.Inventory?.AddObject(itemToUnequip, Silent);
			result = true;
		}
		return result;
	}

	public static void EquipObjectToPlayer(GameObject itemToEquip, BodyPart partToEquipOn)
	{
		EquipObject(The.Player, itemToEquip, partToEquipOn);
	}

	public static int GetPlayerCurrentCarryWeight()
	{
		return The.Player?.GetCarriedWeight() ?? 0;
	}

	public static int GetPlayerMaxCarryWeight()
	{
		return The.Player?.GetMaxCarriedWeight() ?? 0;
	}

	public static void EquipObject(GameObject equippingObject, GameObject itemToEquip, BodyPart partToEquipOn)
	{
		if (Inventory.IsItemSlotAppropriate(itemToEquip, partToEquipOn.Type))
		{
			if (partToEquipOn.Equipped != null)
			{
				equippingObject.FireEvent(Event.New("CommandUnequipObject", "BodyPart", partToEquipOn));
			}
			if (partToEquipOn.Equipped == null && itemToEquip.Equipped != null)
			{
				InventoryActionEvent.Check(itemToEquip.Equipped, partToEquipOn.ParentBody.ParentObject, itemToEquip, "Unequip");
			}
			if (partToEquipOn.Equipped == null && itemToEquip.Equipped == null)
			{
				equippingObject.FireEvent(Event.New("CommandEquipObject", "Object", itemToEquip, "BodyPart", partToEquipOn));
			}
			if (itemToEquip.Equipped == null && itemToEquip.InInventory == null && itemToEquip.CurrentCell == null)
			{
				equippingObject.Inventory.AddObject(itemToEquip);
			}
		}
	}

	public static void DropObject(GameObject objectToDrop)
	{
		if (objectToDrop.InInventory != null)
		{
			objectToDrop.InInventory.FireEvent(Event.New("CommandDropObject", "Object", objectToDrop));
		}
	}
}

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using Rewired;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("Inventory", false, true, false, "Charactersheet,Menu", "Inventory", false, 0, false)]
public class InventoryScreen : IScreen, IWantsTextConsoleInit
{
	private static Dictionary<char, GameObject> SelectionList = new Dictionary<char, GameObject>();

	private static Dictionary<char, CategorySelectionListEntry> CategorySelectionList = new Dictionary<char, CategorySelectionListEntry>();

	private static Dictionary<string, List<GameObject>> CategoryMap = new Dictionary<string, List<GameObject>>();

	public static SortGODisplayName displayNameSorter = new SortGODisplayName();

	public static SortGOCategory categorySorter = new SortGOCategory();

	public static SortGORenderLayer renderLayerSorter = new SortGORenderLayer();

	private static Dictionary<string, InventoryCategory> CategoryList = new Dictionary<string, InventoryCategory>();

	private static List<GameObject> SortList;

	private static List<string> Categories = new List<string>();

	private static int StartObject = 0;

	private static int CategorySort = 0;

	private static bool bMore = false;

	private static InventoryCategory forceCategorySelect = null;

	private static TextConsole TextConsole;

	private static ScreenBuffer Buffer;

	public static int itemsSkippedByFilter = 0;

	public static int nSelected = 0;

	public static int currentMaxWeight;

	public static string filterString = "";

	private static HotkeySpread hotkeySpread;

	public void Init(TextConsole console, ScreenBuffer buffer)
	{
		TextConsole = console;
		Buffer = buffer;
	}

	public static void ClearLists()
	{
		CategoryMap.Clear();
		SelectionList.Clear();
		CategorySelectionList.Clear();
		CategoryList.Clear();
		SortList.Clear();
	}

	public static void ResetNameCache(GameObject GO)
	{
		List<GameObject> objectsDirect = GO.Inventory.GetObjectsDirect();
		for (int i = 0; i < objectsDirect.Count; i++)
		{
			objectsDirect[i].ResetNameCache();
		}
	}

	public static void RebuildLists(GameObject GO)
	{
		hotkeySpread.restart();
		Inventory inventory = GO.Inventory;
		CategoryMap.Clear();
		SelectionList.Clear();
		itemsSkippedByFilter = 0;
		if (!Categories.CleanContains("Category"))
		{
			Categories.Add("Category");
		}
		List<GameObject> objectsDirect = inventory.GetObjectsDirect();
		for (int i = 0; i < objectsDirect.Count; i++)
		{
			GameObject gameObject = objectsDirect[i];
			if (gameObject.HasTag("HiddenInInventory"))
			{
				continue;
			}
			if (filterString != "" && !gameObject.GetCachedDisplayNameStripped().Contains(filterString, CompareOptions.IgnoreCase))
			{
				itemsSkippedByFilter++;
				continue;
			}
			gameObject.Seen();
			string inventoryCategory = gameObject.GetInventoryCategory();
			if (!CategoryList.ContainsKey(inventoryCategory))
			{
				CategoryList.Add(inventoryCategory, new InventoryCategory(inventoryCategory, Persist: true));
				Categories.Add(inventoryCategory);
			}
			if (!CategoryMap.ContainsKey(inventoryCategory))
			{
				CategoryMap.Add(inventoryCategory, new List<GameObject>());
			}
			CategoryMap[inventoryCategory].Add(gameObject);
		}
		List<string> list = new List<string>();
		foreach (string key2 in CategoryList.Keys)
		{
			if (!CategoryMap.ContainsKey(key2))
			{
				list.Add(key2);
			}
			else if (CategoryMap[key2].Count == 0)
			{
				list.Add(key2);
			}
		}
		foreach (string item in list)
		{
			if (CategoryList.ContainsKey(item))
			{
				CategoryList.Remove(item);
			}
			if (CategoryMap.ContainsKey(item))
			{
				CategoryMap.Remove(item);
			}
		}
		foreach (List<GameObject> value in CategoryMap.Values)
		{
			value.Sort(displayNameSorter);
		}
		while (CategorySort >= Categories.Count)
		{
			CategorySort--;
		}
		if (CategorySort == -1)
		{
			SortList = inventory.GetObjects();
			SortList.Sort(displayNameSorter);
		}
		else if (Categories[CategorySort] == "Category")
		{
			SortList = inventory.GetObjects();
			SortList.Sort(categorySorter);
		}
		else
		{
			if (CategoryMap.ContainsKey(Categories[CategorySort]))
			{
				SortList = CategoryMap[Categories[CategorySort]];
			}
			SortList.Sort(displayNameSorter);
		}
		int num = 0;
		bMore = false;
		if (CategorySort != -1 && Categories[CategorySort] == "Category")
		{
			CategorySelectionList.Clear();
			int num2 = 0;
			List<string> list2 = new List<string>();
			foreach (string key3 in CategoryList.Keys)
			{
				list2.Add(key3);
			}
			list2.Sort();
			for (int j = 0; j < list2.Count; j++)
			{
				string key = list2[j];
				InventoryCategory inventoryCategory2 = CategoryList[key];
				if (forceCategorySelect != null && inventoryCategory2 == forceCategorySelect)
				{
					if (num2 < StartObject)
					{
						StartObject = num2;
					}
					nSelected = num2 - StartObject;
					forceCategorySelect = null;
				}
				if (num2 >= StartObject && num2 <= 21 + StartObject)
				{
					CategorySelectionList.Add(hotkeySpread.ch(), new CategorySelectionListEntry(inventoryCategory2));
					hotkeySpread.next();
					num++;
				}
				if (inventoryCategory2.Expanded && CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item2 in CategoryMap[inventoryCategory2.Name])
					{
						num2++;
						num++;
						if (num2 >= StartObject && num2 <= 21 + StartObject)
						{
							CategorySelectionList.Add(hotkeySpread.ch(), new CategorySelectionListEntry(item2));
							hotkeySpread.next();
						}
						else if (num2 > 21 + StartObject)
						{
							bMore = true;
							break;
						}
					}
				}
				if (CategoryList.ContainsKey(key))
				{
					CategoryList[key].Weight = 0;
					CategoryList[key].Items = 0;
				}
				if (CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item3 in CategoryMap[inventoryCategory2.Name])
					{
						if (item3.pPhysics != null)
						{
							CategoryList[key].Weight += item3.pPhysics.Weight;
						}
						CategoryList[key].Items++;
					}
				}
				if (num2 > 21 + StartObject)
				{
					bMore = true;
					break;
				}
				num2++;
			}
		}
		else
		{
			if (inventory == null)
			{
				return;
			}
			int num3 = 0;
			foreach (GameObject sort in SortList)
			{
				if (num3 >= StartObject && num3 <= 21 + StartObject)
				{
					SelectionList.Add(hotkeySpread.ch(), sort);
					hotkeySpread.next();
				}
				num3++;
				if (num3 > 21 + StartObject)
				{
					bMore = true;
					break;
				}
			}
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Inventory");
		filterString = "";
		ResetNameCache(GO);
		filterString = "";
		Keys keys = Keys.None;
		bool Done = false;
		StartObject = 0;
		nSelected = 0;
		currentMaxWeight = GO.GetMaxCarriedWeight();
		hotkeySpread = HotkeySpread.get(new string[2] { "*default", "Menus" });
		string s = "< {{W|7}} Character | Equipment {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Character | Equipment {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		while (!Done)
		{
			while (true)
			{
				Event.ResetPool(resetMinEventPools: false);
				RebuildLists(GO);
				int num;
				while (true)
				{
					Buffer.Clear();
					Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					Buffer.Goto(35, 0);
					Buffer.Write("[ {{W|Inventory}} ]");
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						Buffer.Goto(60, 0);
						Buffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Back", mapGlyphs: false) + "}} to exit ");
					}
					else
					{
						Buffer.Goto(60, 0);
						Buffer.Write(" {{W|ESC}} or {{W|5}} to exit ");
					}
					Buffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
					Buffer.Write(s);
					if (StartObject > 0)
					{
						Buffer.Goto(1, 0);
						if (ControlManager.activeControllerType == ControllerType.Joystick)
						{
							Buffer.Write("<more...>");
						}
						else
						{
							Buffer.Write("<{{W|8}} to scroll up>");
						}
					}
					StringBuilder stringBuilder = Event.NewStringBuilder();
					stringBuilder.Append("Total weight: {{Y|").Append(GO.GetCarriedWeight()).Append(" {{y|/}}  ")
						.Append(currentMaxWeight)
						.Append(" lbs.}}");
					Buffer.Goto(79 - ColorUtility.LengthExceptFormatting(stringBuilder), 23);
					Buffer.Write(stringBuilder.ToString());
					dictionary.Clear();
					num = 0;
					InventoryCategory inventoryCategory = null;
					GameObject gameObject = null;
					int num2 = 1;
					int num3 = 1;
					foreach (char key in CategorySelectionList.Keys)
					{
						if (CategorySelectionList[key].Category != null)
						{
							Buffer.Goto(num3, num2 + num);
							string text = "";
							if (num == nSelected)
							{
								text = "{{Y|>}}";
								inventoryCategory = CategorySelectionList[key].Category;
							}
							else
							{
								text = " ";
							}
							StringBuilder stringBuilder2 = Event.NewStringBuilder();
							StringBuilder stringBuilder3 = Event.NewStringBuilder();
							char value = ((num == nSelected) ? 'Y' : 'K');
							stringBuilder3.Append("{{").Append(value).Append('|');
							if (Options.ShowNumberOfItems)
							{
								stringBuilder3.Append(", ").Append(CategorySelectionList[key].Category.Items).Append((CategorySelectionList[key].Category.Items == 1) ? " item" : " items");
							}
							stringBuilder3.Append("}}");
							stringBuilder2.Append(" {{").Append((num == nSelected) ? 'Y' : 'y').Append("|[")
								.Append(CategorySelectionList[key].Category.Weight)
								.Append("#]}}");
							if (CategorySelectionList[key].Category.Expanded)
							{
								if (num == nSelected)
								{
									StringBuilder stringBuilder4 = Event.NewStringBuilder();
									stringBuilder4.Append(text).Append(key).Append(") [-] {{K|[{{Y|")
										.Append(CategorySelectionList[key].Category.Name)
										.Append(stringBuilder3)
										.Append("}}]}}");
									Buffer.Write(stringBuilder4.ToString());
								}
								else
								{
									StringBuilder stringBuilder5 = Event.NewStringBuilder();
									stringBuilder5.Append(text).Append(key).Append(") [-] {{K|[")
										.Append(CategorySelectionList[key].Category.Name)
										.Append(stringBuilder3)
										.Append("]}}");
									Buffer.Write(stringBuilder5.ToString());
								}
							}
							else if (num == nSelected)
							{
								StringBuilder stringBuilder6 = Event.NewStringBuilder();
								stringBuilder6.Append(text).Append(key).Append(") [+] {{K|[{{Y|")
									.Append(CategorySelectionList[key].Category.Name)
									.Append(stringBuilder3)
									.Append("}}]}}");
								Buffer.Write(stringBuilder6.ToString());
							}
							else
							{
								StringBuilder stringBuilder7 = Event.NewStringBuilder();
								stringBuilder7.Append(text).Append(key).Append(") [+] {{K|[")
									.Append(CategorySelectionList[key].Category.Name)
									.Append(stringBuilder3)
									.Append("]}}");
								Buffer.Write(stringBuilder7.ToString());
							}
							Buffer.Goto(79 - ColorUtility.LengthExceptFormatting(stringBuilder2), num2 + num);
							Buffer.Write(stringBuilder2);
							dictionary.Add(key, num);
							num++;
							continue;
						}
						GameObject @object = CategorySelectionList[key].Object;
						string text2 = "";
						if (num == nSelected)
						{
							text2 = "{{Y|>}}  ";
							gameObject = @object;
						}
						else
						{
							text2 = "   ";
						}
						Buffer.Goto(num3, num2 + num);
						Buffer.Write(text2 + key + ")");
						Buffer.Goto(num3 + 6, num2 + num);
						if (@object.pRender != null)
						{
							if (XRLCore.player.GetConfusion() <= 0)
							{
								Buffer.Write(@object.RenderForUI());
							}
							else
							{
								Buffer.Write(" ");
							}
							Buffer.Goto(num3 + 8, num2 + num);
						}
						Buffer.Write(@object.DisplayName);
						int weight = @object.Weight;
						StringBuilder stringBuilder8 = Event.NewStringBuilder();
						stringBuilder8.Append(" {{").Append((num == nSelected) ? 'Y' : 'K').Append("|")
							.Append(weight)
							.Append("#}}")
							.Append('³');
						Buffer.Goto(80 - ColorUtility.LengthExceptFormatting(stringBuilder8), num2 + num);
						Buffer.Write(stringBuilder8);
						dictionary.Add(key, num);
						num++;
					}
					if (num == 0 && StartObject != 0)
					{
						break;
					}
					if (nSelected < num)
					{
						if (bMore)
						{
							Buffer.Goto(num3, 24);
							if (ControlManager.activeControllerType == ControllerType.Joystick)
							{
								Buffer.Write("<...more>");
							}
							else
							{
								Buffer.Write("<{{W|2}} to scroll down>");
							}
						}
						if (itemsSkippedByFilter > 0)
						{
							Buffer.Goto(3, 23);
							Buffer.Write(itemsSkippedByFilter + " items hidden by filter");
						}
						if (ControlManager.activeControllerType == ControllerType.Keyboard)
						{
							Buffer.Goto(20, 24);
							Buffer.Write("[{{W|?}} view quick keys]");
						}
						TextConsole.DrawBuffer(Buffer);
						if (!XRLCore.Core.Game.Running)
						{
							GameManager.Instance.PopGameView();
							return ScreenReturn.Exit;
						}
						IEvent GeneratedEvent = null;
						keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
						ScreenBuffer.ClearImposterSuppression();
						char c = ("" + (char)ConsoleLib.Console.Keyboard.Char + " ").ToLower()[0];
						if (keys == Keys.Enter)
						{
							keys = Keys.Space;
						}
						if (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							Done = true;
						}
						if (keys == Keys.OemQuestion || c == '?')
						{
							Popup.Show("Inventory quick keys\r\n\r\n&WCtrl+D&y - Drop\r\n&WCtrl+A&y - Eat\r\n&WCtrl+E&y - Auto-equip\r\n&WCtrl+R&y - Drink\r\n&WCtrl+P&y - Apply\n&WCtrl+F or ,&y - Filter");
						}
						else if (keys == (Keys)131137)
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Eat", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, (GameObject)null, (Cell)null, (Cell)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == (Keys)131140)
						{
							if (gameObject != null)
							{
								Event @event = Event.New("CommandDropObject", "Object", gameObject);
								GeneratedEvent = @event;
								GO.FireEvent(@event);
								ResetNameCache(GO);
							}
						}
						else if (keys == (Keys)131142 || c == ',')
						{
							filterString = Popup.AskString("Enter text to filter inventory by item name.", filterString);
							ClearLists();
						}
						else if (keys == (Keys)131154)
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Drink", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, (GameObject)null, (Cell)null, (Cell)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == (Keys)131152)
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Apply", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, (GameObject)null, (Cell)null, (Cell)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
						{
							Done = true;
						}
						else if (keys == Keys.Escape || keys == Keys.NumPad5 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
						{
							Done = true;
						}
						else if (keys == Keys.NumPad8)
						{
							if (nSelected > 0)
							{
								nSelected--;
								continue;
							}
							if (StartObject > 0)
							{
								StartObject--;
							}
						}
						else if (keys == Keys.NumPad2)
						{
							if (nSelected < num - 1)
							{
								nSelected++;
								continue;
							}
							if (bMore)
							{
								StartObject++;
							}
						}
						else if (keys == Keys.Next || keys == Keys.Next || ConsoleLib.Console.Keyboard.RawCode == Keys.Next || ConsoleLib.Console.Keyboard.RawCode == Keys.Next)
						{
							if (nSelected < num - 1)
							{
								nSelected = num - 1;
							}
							else if (bMore)
							{
								StartObject += 21;
							}
						}
						else if (keys == Keys.Prior || keys == Keys.Back || ConsoleLib.Console.Keyboard.RawCode == Keys.Prior || ConsoleLib.Console.Keyboard.RawCode == Keys.Back)
						{
							if (nSelected > 0)
							{
								nSelected = 0;
							}
							else
							{
								StartObject -= 21;
								if (StartObject < 0)
								{
									StartObject = 0;
								}
							}
						}
						else
						{
							switch (keys)
							{
							case Keys.Subtract:
							case Keys.OemMinus:
								foreach (InventoryCategory value2 in CategoryList.Values)
								{
									value2.Expanded = false;
								}
								break;
							case Keys.Add:
							case Keys.Oemplus:
								foreach (InventoryCategory value3 in CategoryList.Values)
								{
									value3.Expanded = true;
								}
								break;
							default:
								if (gameObject != null)
								{
									if ((ConsoleLib.Console.Keyboard.vkCode == Keys.Right || keys == Keys.NumPad6 || keys == (Keys)131141) && Options.GetOption("OptionPressingRightInInventoryEquips", "Yes") == "Yes" && GO.AutoEquip(gameObject))
									{
										ResetNameCache(GO);
									}
									if (ConsoleLib.Console.Keyboard.vkCode == Keys.Left || keys == Keys.NumPad4 || keys == (Keys)131141)
									{
										foreach (KeyValuePair<string, List<GameObject>> item in CategoryMap)
										{
											if (item.Value.Contains(gameObject))
											{
												CategoryList[item.Key].Expanded = false;
												forceCategorySelect = CategoryList[item.Key];
												break;
											}
										}
									}
									if (keys == Keys.Space)
									{
										EquipmentAPI.TwiddleObject(GO, gameObject, ref Done);
										ResetNameCache(GO);
									}
									if (keys == Keys.Tab)
									{
										InventoryActionEvent.Check(gameObject, GO, gameObject, "Look");
										ResetNameCache(GO);
									}
								}
								if (inventoryCategory != null)
								{
									if (keys == Keys.NumPad4)
									{
										inventoryCategory.Expanded = false;
									}
									if (keys == Keys.NumPad6)
									{
										inventoryCategory.Expanded = true;
									}
									if (keys == Keys.Space)
									{
										inventoryCategory.Expanded = !inventoryCategory.Expanded;
									}
								}
								if (keys < Keys.A || keys > Keys.Z || !CategorySelectionList.ContainsKey(c))
								{
									break;
								}
								if (nSelected == dictionary[c] && (!CategorySelectionList.ContainsKey(c) || CategorySelectionList[c].Category == null))
								{
									EquipmentAPI.TwiddleObject(GO, gameObject, ref Done);
									ResetNameCache(GO);
									break;
								}
								nSelected = dictionary[c];
								if (CategorySelectionList.ContainsKey(c) && CategorySelectionList[c].Category != null)
								{
									CategorySelectionList[c].Category.Expanded = !CategorySelectionList[c].Category.Expanded;
								}
								break;
							}
						}
						if (GeneratedEvent != null && !Done && GeneratedEvent.InterfaceExitRequested())
						{
							Done = true;
						}
						goto end_IL_00be;
					}
					goto IL_0778;
				}
				StartObject = 0;
				continue;
				IL_0778:
				nSelected = num - 1;
				continue;
				end_IL_00be:
				break;
			}
		}
		ClearLists();
		switch (keys)
		{
		case Keys.NumPad7:
			GameManager.Instance.PopGameView();
			return ScreenReturn.Previous;
		case Keys.NumPad9:
			GameManager.Instance.PopGameView();
			return ScreenReturn.Next;
		default:
			GameManager.Instance.PopGameView();
			return ScreenReturn.Exit;
		}
	}
}

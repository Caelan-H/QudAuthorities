using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using Rewired;
using XRL.Core;
using XRL.Language;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class EquipmentScreen : IScreen
{
	private enum ScreenTab
	{
		Equipment,
		Cybernetics
	}

	public static List<GameObject> EquipmentList = new List<GameObject>(16);

	private static StringBuilder SB = new StringBuilder();

	public static void ShowBodypartEquipUI(GameObject GO, BodyPart SelectedBodyPart)
	{
		Inventory inventory = GO.Inventory;
		if (inventory != null)
		{
			GameObject gameObject = null;
			EquipmentList.Clear();
			inventory.GetEquipmentListForSlot(EquipmentList, SelectedBodyPart.Type);
			if (EquipmentList.Count > 0)
			{
				string categoryPriority = null;
				if (SelectedBodyPart.Type == "Hand")
				{
					categoryPriority = "Melee Weapon,Shield,Light Source";
				}
				if (SelectedBodyPart.Type == "Thrown Weapon")
				{
					categoryPriority = "Thrown Weapons,Grenades";
				}
				gameObject = PickItem.ShowPicker(EquipmentList, categoryPriority, PickItem.PickItemDialogStyle.SelectItemDialog, GO, null, null, null, PreserveOrder: true);
			}
			else
			{
				Popup.Show("You don't have anything to use in that slot.");
			}
			if (gameObject != null)
			{
				GO.FireEvent(Event.New("CommandEquipObject", "Object", gameObject, "BodyPart", SelectedBodyPart));
			}
		}
		else
		{
			Popup.Show("You have no inventory!");
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Equipment");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Keys keys = Keys.None;
		int num = 0;
		int num2 = 0;
		ScreenTab screenTab = ScreenTab.Equipment;
		bool Done = false;
		Body body = GO.Body;
		while (true)
		{
			bool flag = false;
			List<BodyPart> list = new List<BodyPart>();
			List<GameObject> list2 = new List<GameObject>();
			List<GameObject> list3 = new List<GameObject>();
			List<GameObject> list4 = new List<GameObject>();
			Dictionary<char, int> dictionary = new Dictionary<char, int>();
			foreach (BodyPart item in body.LoopParts())
			{
				if (screenTab == ScreenTab.Equipment)
				{
					if (item.Equipped != null)
					{
						list.Add(item);
						list3.Add(item.Equipped);
						list4.Add(item.Equipped);
						list2.Add(null);
					}
					else if (item.DefaultBehavior != null)
					{
						list.Add(item);
						list3.Add(item.DefaultBehavior);
						list4.Add(null);
						list2.Add(null);
					}
					else
					{
						list.Add(item);
						list3.Add(null);
						list4.Add(null);
						list2.Add(null);
					}
				}
				if (item.Cybernetics != null)
				{
					if (screenTab == ScreenTab.Cybernetics)
					{
						list.Add(item);
						list3.Add(null);
						list4.Add(item.Cybernetics);
						list2.Add(item.Cybernetics);
					}
					flag = true;
				}
			}
			string s = "< {{W|7}} Inventory | Factions {{W|9}} >";
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Inventory | Factions {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
			}
			bool flag2 = XRLCore.Core.Game.Player.Body.AreHostilesNearby();
			while (true)
			{
				List<GameObject> list5;
				if (!Done)
				{
					Event.ResetPool(resetMinEventPools: false);
					list5 = Event.NewGameObjectList();
					if (!XRLCore.Core.Game.Running)
					{
						GameManager.Instance.PopGameView();
						return ScreenReturn.Exit;
					}
					scrapBuffer.Clear();
					scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					if (screenTab == ScreenTab.Equipment)
					{
						scrapBuffer.Goto(35, 0);
						scrapBuffer.Write("[ {{W|Equipment}} ]");
					}
					else
					{
						scrapBuffer.Goto(35, 0);
						scrapBuffer.Write("[ {{W|Cybernetics}} ]");
					}
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						scrapBuffer.Goto(60, 0);
						scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Back", mapGlyphs: false) + "}} to exit ");
					}
					else
					{
						scrapBuffer.Goto(60, 0);
						scrapBuffer.Write(" {{W|ESC}} or {{W|5}} to exit ");
					}
					scrapBuffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
					scrapBuffer.Write(s);
					scrapBuffer.Goto(25, 24);
					if (!flag2)
					{
						BodyPart bodyPart = list[num];
						if (bodyPart == null || !bodyPart.Abstract)
						{
							BodyPart bodyPart2 = list[num];
							if (bodyPart2 == null || !bodyPart2.Extrinsic)
							{
								if (ControlManager.activeControllerType == ControllerType.Joystick)
								{
									scrapBuffer.Goto(20, 24);
									scrapBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("Set Primary Limb", mapGlyphs: false) + " - Set primary limb}}]");
								}
								else
								{
									scrapBuffer.Write("[{{W|Tab}} - Set primary limb]");
								}
								goto IL_0385;
							}
						}
					}
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						scrapBuffer.Goto(20, 24);
						scrapBuffer.Write("[{{K|" + ControlManager.getCommandInputDescription("Set Primary Limb", mapGlyphs: false) + " - Set primary limb}}]");
					}
					else
					{
						scrapBuffer.Write("[{{K|Tab - Set primary limb}}]");
					}
					goto IL_0385;
				}
				GameManager.Instance.PopGameView();
				return keys switch
				{
					Keys.NumPad7 => ScreenReturn.Previous, 
					Keys.NumPad9 => ScreenReturn.Next, 
					_ => ScreenReturn.Exit, 
				};
				IL_0385:
				int num3 = 22;
				int num4 = 2;
				if (flag)
				{
					scrapBuffer.Goto(3, num4);
					if (screenTab == ScreenTab.Cybernetics)
					{
						scrapBuffer.Write("{{K|Equipment}} {{Y|Cybernetics}}");
					}
					else
					{
						scrapBuffer.Write("{{Y|Equipment}} {{K|Cybernetics}}");
					}
					num3 -= 2;
					num4 += 2;
				}
				if (list == null)
				{
					continue;
				}
				dictionary.Clear();
				for (int i = num2; i < list.Count && i - num2 < num3; i++)
				{
					if (num == i)
					{
						scrapBuffer.Goto(27, num4 + i - num2);
						scrapBuffer.Write("{{K|>}}");
					}
					scrapBuffer.Goto(1, num4 + i - num2);
					string text = "";
					text = ((num != i) ? " {{w|" : "{{Y|>}}{{W|");
					SB.Clear();
					if (list2[i] == null)
					{
						if (Options.IndentBodyParts)
						{
							for (int num5 = body.GetPartDepth(list[i]); num5 >= 1; num5--)
							{
								SB.Append(' ');
							}
						}
						SB.Append(list[i].GetCardinalDescription());
					}
					else
					{
						SB.Append(Grammar.MakeTitleCase(list[i].GetCardinalName()));
					}
					if (list[i].Primary)
					{
						SB.Append(" {{G|*}}");
					}
					char c = (char)(97 + i);
					if (c > 'z')
					{
						c = ' ';
					}
					else
					{
						dictionary.Add(c, i);
					}
					if (list2[i] != null)
					{
						if (num == i)
						{
							scrapBuffer.Write(text + c + "}}) " + SB.ToString());
							scrapBuffer.Goto(28, num4 + i - num2);
							scrapBuffer.Write(list2[i].RenderForUI());
							scrapBuffer.Goto(30, num4 + i - num2);
							scrapBuffer.Write(list2[i].DisplayName);
							continue;
						}
						scrapBuffer.Write(text + "}}{{K|" + c + ") " + SB.ToString() + "}}");
						scrapBuffer.Goto(28, num4 + i - num2);
						scrapBuffer.Write(list2[i].RenderForUI(), null, null, "&K", "&K", 'K');
						scrapBuffer.Goto(30, num4 + i - num2);
						scrapBuffer.Write("{{K|" + list2[i].DisplayNameStripped + "}}");
						continue;
					}
					scrapBuffer.Write(text + c + "}}) " + SB.ToString());
					if (list[i].Equipped == null)
					{
						if (list[i].DefaultBehavior == null)
						{
							scrapBuffer.Goto(28, num4 + i - num2);
							if (num == i)
							{
								scrapBuffer.Write("{{Y|-}}");
							}
							else
							{
								scrapBuffer.Write("{{K|-}}");
							}
						}
						else
						{
							scrapBuffer.Goto(28, num4 + i - num2);
							scrapBuffer.Write(list[i].DefaultBehavior.RenderForUI(), null, null, "&K", "&K", 'K');
							scrapBuffer.Write(" ");
							scrapBuffer.Goto(30, num4 + i - num2);
							scrapBuffer.Write("{{K|" + list[i].DefaultBehavior.DisplayName + "}}");
						}
					}
					else if (list[i].Equipped != null && list5.CleanContains(list[i].Equipped))
					{
						scrapBuffer.Goto(28, num4 + i - num2);
						scrapBuffer.Write(list[i].Equipped.RenderForUI(), null, null, "&K", "&K", 'K');
						scrapBuffer.Write(" ");
						scrapBuffer.Goto(30, num4 + i - num2);
						scrapBuffer.Write("{{K|" + list[i].Equipped.DisplayNameStripped + "}}");
					}
					else if (list[i].Equipped != null && list[i].Equipped.HasTag("RenderImplantGreyInEquipment") && !list5.CleanContains(list[i].Equipped))
					{
						if (list[i].Equipped.IsImplant)
						{
							if ((list[i].Equipped.GetPart("CyberneticsBaseItem") as CyberneticsBaseItem).ImplantedOn != null)
							{
								scrapBuffer.Goto(28, num4 + i - num2);
								scrapBuffer.Write(list[i].Equipped.RenderForUI(), null, null, "&K", "&K", 'K');
								scrapBuffer.Write(" ");
								scrapBuffer.Goto(30, num4 + i - num2);
								scrapBuffer.Write("{{K|" + list[i].Equipped.DisplayNameStripped + "}}");
							}
							else
							{
								list5.Add(list[i].Equipped);
								scrapBuffer.Goto(28, num4 + i - num2);
								scrapBuffer.Write(list[i].Equipped.RenderForUI());
								scrapBuffer.Write(" ");
								scrapBuffer.Goto(30, num4 + i - num2);
								scrapBuffer.Write(list[i].Equipped.DisplayName);
							}
						}
					}
					else
					{
						list5.Add(list[i].Equipped);
						scrapBuffer.Goto(28, num4 + i - num2);
						scrapBuffer.Write(list[i].Equipped.RenderForUI());
						scrapBuffer.Goto(30, num4 + i - num2);
						scrapBuffer.Write(list[i].Equipped.DisplayName);
					}
				}
				if (num2 + num3 < list.Count)
				{
					scrapBuffer.Goto(2, 24);
					scrapBuffer.Write("<more...>");
				}
				if (num2 > 0)
				{
					scrapBuffer.Goto(2, 0);
					scrapBuffer.Write("<more...>");
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer);
				keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
				ScreenBuffer.ClearImposterSuppression();
				char key = (char.ToLower((char)ConsoleLib.Console.Keyboard.Char) + " ").ToLower()[0];
				if (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "RightClick")
				{
					Done = true;
				}
				if (keys == Keys.Escape || keys == Keys.NumPad5)
				{
					Done = true;
				}
				else if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
				{
					Done = true;
				}
				else if (keys == Keys.Prior)
				{
					if (num == num2)
					{
						num2 = Math.Max(num2 - num3, 0);
					}
					num = num2;
				}
				else if (keys == Keys.Next)
				{
					if (num == num2 + num3 - 1 && list.Count > num2 + num3)
					{
						num2 = Math.Min(num2 + num3, list.Count - num3);
					}
					num = Math.Min(num2 + num3 - 1, list.Count - 1);
				}
				else if (keys == Keys.NumPad4 || keys == Keys.NumPad6)
				{
					if (flag)
					{
						screenTab = ((screenTab != ScreenTab.Cybernetics) ? ScreenTab.Cybernetics : ScreenTab.Equipment);
						num = 0;
						num2 = 0;
						break;
					}
				}
				else if (keys != Keys.F2)
				{
					if (keys == Keys.NumPad8)
					{
						if (num - num2 <= 0 && num2 > 0)
						{
							num2--;
						}
						if (num > 0)
						{
							num--;
						}
					}
					else if (keys == Keys.NumPad2 && num < list.Count - 1)
					{
						if (num - num2 == num3 - 1)
						{
							num2++;
						}
						num++;
					}
					else if ((ConsoleLib.Console.Keyboard.vkCode == Keys.Left || keys == Keys.NumPad4) && Options.GetOption("OptionPressingRightInInventoryEquips", "Yes") == "Yes" && list3[num] != null)
					{
						BodyPart value = list[num];
						GO.FireEvent(Event.New("CommandUnequipObject", "BodyPart", value));
						break;
					}
				}
				if (keys == Keys.Enter)
				{
					keys = Keys.Space;
				}
				if (keys == Keys.Tab || (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "Passthrough:Set Primary Limb"))
				{
					list[num].SetAsPreferredDefault();
				}
				if (keys == (Keys)131104)
				{
					if (list3[num] != null)
					{
						InventoryActionEvent.Check(list3[num], The.Player, list3[num], "Look");
						break;
					}
				}
				else if ((keys == Keys.Space || (keys >= Keys.A && keys <= Keys.Z)) && (dictionary.ContainsKey(key) || keys == Keys.Space))
				{
					int num6 = num;
					num6 = ((keys != Keys.Space) ? dictionary[key] : num);
					if (list4[num6] != null)
					{
						EquipmentAPI.TwiddleObject(GO, list4[num6], ref Done);
					}
					else
					{
						ShowBodypartEquipUI(GO, list[num6]);
					}
					break;
				}
			}
		}
	}
}

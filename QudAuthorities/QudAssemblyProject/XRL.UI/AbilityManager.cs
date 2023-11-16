using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Rewired;
using UnityEngine;
using XRL.Messages;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[HasGameBasedStaticCache]
public class AbilityManager
{
	[GameBasedStaticCache]
	public static Dictionary<int, ActivatedAbilityEntry> keyToAbility;

	[GameBasedStaticCache]
	private static Dictionary<int, string> keyToCommand;

	[GameBasedStaticCache]
	public static Dictionary<string, int> commandToKey;

	public static void UpdateFavorites()
	{
		ActivatedAbilities activatedAbilities = The.Player?.ActivatedAbilities;
		if (activatedAbilities == null || activatedAbilities.AbilityLists == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<ActivatedAbilityEntry>> abilityList in activatedAbilities.AbilityLists)
		{
			foreach (ActivatedAbilityEntry item in abilityList.Value)
			{
				if (item == null || string.IsNullOrEmpty(item.Command) || commandToKey.ContainsKey(item.Command))
				{
					continue;
				}
				List<int> favorites = HotkeyFavorites.GetFavorites(item.Command);
				if (favorites == null)
				{
					continue;
				}
				foreach (int item2 in favorites)
				{
					if (!keyToCommand.ContainsKey(item2))
					{
						keyToAbility.Add(item2, item);
						keyToCommand.Add(item2, item.Command);
						commandToKey.Add(item.Command, item2);
						break;
					}
				}
			}
		}
	}

	public static string MapKeyToCommand(int key)
	{
		if (keyToCommand.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public static void Save(SerializationWriter Writer)
	{
		Writer.Write(keyToAbility);
		Writer.Write(keyToCommand);
		Writer.Write(commandToKey);
	}

	public static void Load(SerializationReader Reader)
	{
		keyToAbility = Reader.ReadDictionary<int, ActivatedAbilityEntry>();
		keyToCommand = Reader.ReadDictionary<int, string>();
		commandToKey = Reader.ReadDictionary<string, int>();
	}

	private static void BuildNodes(List<AbilityNode> NodeList, XRL.World.GameObject GO)
	{
		NodeList.Clear();
		ActivatedAbilities activatedAbilities = GO.ActivatedAbilities;
		if (activatedAbilities == null || activatedAbilities.AbilityLists == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<ActivatedAbilityEntry>> abilityList in activatedAbilities.AbilityLists)
		{
			AbilityNode abilityNode = new AbilityNode(null, abilityList.Key);
			NodeList.Add(abilityNode);
			foreach (ActivatedAbilityEntry item in abilityList.Value)
			{
				NodeList.Add(new AbilityNode(item, "", abilityNode));
			}
		}
	}

	public static string Show(XRL.World.GameObject GO)
	{
		GameManager.Instance.PushGameView("AbilityManager");
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<AbilityNode> list = new List<AbilityNode>();
		int Index = 0;
		int Index2 = 0;
		BuildNodes(list, GO);
		List<UnityEngine.KeyCode> hotkeySpread = ControlManager.GetHotkeySpread(new string[3] { "*default", "Default", "Menus" });
		while (!flag)
		{
			Dictionary<char, string> dictionary;
			Dictionary<char, ActivatedAbilityEntry> dictionary2;
			int num3;
			int i;
			while (true)
			{
				XRL.World.Event.ResetPool();
				int num = 0;
				dictionary = new Dictionary<char, string>();
				dictionary2 = new Dictionary<char, ActivatedAbilityEntry>();
				scrapBuffer.Clear();
				scrapBuffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Goto(13, 0);
				scrapBuffer.Write("[ {{W|Manage Abilities}} ]");
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
				int num2 = 2;
				num3 = 0;
				ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index);
				if (Index2 < Index)
				{
					Index = Index2;
				}
				i = Index;
				bool flag4 = The.Player.IsRealityDistortionUsable();
				while (num2 <= 21 && i <= list.Count)
				{
					for (; i < list.Count && list[i].ParentNode != null && !list[i].ParentNode.Expand; i++)
					{
					}
					if (i < list.Count)
					{
						if (list[i].Category != "")
						{
							scrapBuffer.Goto(4, num2);
							if (list[i].Expand)
							{
								scrapBuffer.Write("[-] ");
							}
							else
							{
								scrapBuffer.Write("[+] ");
							}
							scrapBuffer.Write("{{W|" + list[i].Category + "}}");
						}
						else
						{
							scrapBuffer.Goto(4, num2);
							StringBuilder stringBuilder = new StringBuilder();
							ActivatedAbilityEntry ability = list[i].Ability;
							dictionary[getHotkeyChar(num)] = ability.Command;
							dictionary2[getHotkeyChar(num)] = ability;
							if (!ability.Enabled)
							{
								stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [disabled]}}");
							}
							else if (ability.Cooldown <= 0)
							{
								if (ability.IsRealityDistortionBased && !flag4)
								{
									stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [astrally tethered]}}");
								}
								else
								{
									stringBuilder.Append("  " + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [{{W|attack}}]" : ""));
								}
							}
							else if (ability.IsRealityDistortionBased && !flag4)
							{
								stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownTurns + "}} turn cooldown, astrally tethered]");
							}
							else
							{
								stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownTurns + "}} turn cooldown]");
							}
							if (ability.Toggleable)
							{
								if (ability.ToggleState)
								{
									stringBuilder.Append(" {{K|[{{g|Toggled on}}]}}");
								}
								else
								{
									stringBuilder.Append(" {{K|[{{y|Toggled off}}]}}");
								}
							}
							if (!string.IsNullOrEmpty(ability.Command) && commandToKey.ContainsKey(ability.Command))
							{
								stringBuilder.Append(" {{Y|<{{w|" + ConsoleLib.Console.Keyboard.MetaToString(commandToKey[ability.Command]) + "}}>}}");
							}
							stringBuilder.Append("\n");
							num++;
							scrapBuffer.Write(stringBuilder.ToString());
						}
						num3 = i;
					}
					if (i == Index2)
					{
						scrapBuffer.Goto(2, num2);
						scrapBuffer.Write("{{Y|>}}");
					}
					num2++;
					i++;
				}
				if (list.Count == 0)
				{
					Popup.Show("You have no abilities to manage!");
					GameManager.Instance.PopGameView();
					return "";
				}
				if (flag3)
				{
					Index2 = num3;
					flag3 = false;
					continue;
				}
				if (list[Index2].Ability != null)
				{
					TextBlock textBlock = new TextBlock(list[Index2].Ability.Description, 30, 20);
					for (int j = 0; j < textBlock.Lines.Count; j++)
					{
						scrapBuffer.Goto(45, j + 2);
						scrapBuffer.Write(textBlock.Lines[j]);
					}
				}
				scrapBuffer.Goto(2, 24);
				if (list[Index2].Category != "")
				{
					scrapBuffer.Write(" [{{W|Space}} or {{W|<Letter>}}-Use Ability {{W|Z or Enter}}-Map key] ");
				}
				else
				{
					scrapBuffer.Write(" [{{W|Space}} or {{W|<Letter>}}-Use Ability {{W|Z or Enter}}-Map key] ");
				}
				if (Index != 0)
				{
					scrapBuffer.Goto(4, 1);
					scrapBuffer.Write("{{W|<More...>}}");
				}
				if (i < list.Count)
				{
					scrapBuffer.Goto(4, 22);
					scrapBuffer.Write("{{W|<More...>}}");
				}
				if (Index2 > num3 && Index < list.Count)
				{
					Index++;
					continue;
				}
				if (!flag2)
				{
					break;
				}
				Index2 = Index;
				flag2 = false;
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Keys keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
			char key = ("" + (char)ConsoleLib.Console.Keyboard.Char + " ").ToLower()[0];
			if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
			{
				flag = true;
			}
			if (keys == Keys.NumPad2 && Index2 < list.Count - 1)
			{
				ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index2, 1);
			}
			if (keys == Keys.NumPad8 && Index2 > 0)
			{
				ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2, -1);
			}
			if (keys == Keys.NumPad4)
			{
				if (list[Index2].Category != "")
				{
					list[Index2].Expand = false;
				}
				else
				{
					list[Index2].ParentNode.Expand = false;
					ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2);
				}
			}
			if (keys == Keys.NumPad6)
			{
				list[Index2].Expand = true;
			}
			if (keys == Keys.Next)
			{
				if (num3 == Index2 && i < list.Count)
				{
					ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index2, 1);
					Index = Math.Min(Index2, list.Count - 20);
					flag3 = true;
				}
				else
				{
					Index2 = num3;
				}
			}
			if (keys == Keys.Prior)
			{
				if (Index == Index2 && Index > 0)
				{
					ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2, -1);
					Index = Math.Max(Index2 - 21, 0);
					flag2 = true;
				}
				else
				{
					Index2 = Index;
				}
			}
			if (keys == Keys.OemMinus || keys == Keys.Subtract)
			{
				list.ForEach(delegate(AbilityNode n)
				{
					n.Expand = false;
				});
				ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2);
			}
			if (keys == Keys.Add || keys == Keys.Oemplus)
			{
				list.ForEach(delegate(AbilityNode n)
				{
					n.Expand = true;
				});
			}
			_ = 191;
			ActivatedAbilityEntry activatedAbilityEntry = null;
			if ((ConsoleLib.Console.Keyboard.RawCode == Keys.Delete || ConsoleLib.Console.Keyboard.RawCode == Keys.Back) && list[Index2].Ability != null && commandToKey.ContainsKey(list[Index2].Ability.Command))
			{
				keyToCommand.Remove(commandToKey[list[Index2].Ability.Command]);
				keyToAbility.Remove(commandToKey[list[Index2].Ability.Command]);
				commandToKey.Remove(list[Index2].Ability.Command);
			}
			int metaKey;
			string text;
			if ((ConsoleLib.Console.Keyboard.RawCode == Keys.Z || ConsoleLib.Console.Keyboard.RawCode == Keys.Enter || ConsoleLib.Console.Keyboard.RawCode == Keys.Enter) && list[Index2].Ability != null)
			{
				ControlManager.ResetInput();
				Popup.ShowChoice("Press the key combination you would like to bind to {{C|" + list[Index2].Ability.DisplayName + "}}");
				metaKey = ConsoleLib.Console.Keyboard.MetaKey;
				text = "{{W|" + ConsoleLib.Console.Keyboard.MetaToString(metaKey) + "}}";
				if (ConsoleLib.Console.Keyboard.vkCode != Keys.Escape)
				{
					if (ControlManager.PrereleaseInput)
					{
						if (!ControlManager.isKeyMapped((UnityEngine.KeyCode)metaKey, new string[4] { "Adventure", "AltAdventure", "Menus", "Default" }, out var conflict))
						{
							goto IL_0a78;
						}
						Popup.Show(text + " is already bound to {{C|" + conflict.action.descriptiveName + "}}.");
					}
					else if (!LegacyKeyMapping.IsKeyMapped(metaKey) || Popup.ShowYesNo(text + " is already bound to {{C|" + LegacyKeyMapping.CommandsByID[LegacyKeyMapping.CurrentMap.GetCommandMappedTo(metaKey)].DisplayText + "}}. Do you want to bind it to {{C|" + list[Index2].Ability.DisplayName + "}} instead? [This temporary binding will only apply for the current game]") != DialogResult.No)
					{
						goto IL_0a78;
					}
				}
			}
			goto IL_0c18;
			IL_0a78:
			if (keyToCommand.ContainsKey(metaKey))
			{
				if (Popup.ShowYesNo(text + " is already bound to the ability {{C|" + keyToAbility[metaKey].DisplayName + "}}. Do you want to bind it to {{C|" + list[Index2].Ability.DisplayName + "}} instead? [This temporary binding will only apply for the current game]") == DialogResult.No)
				{
					goto IL_0c18;
				}
				commandToKey.Remove(keyToCommand[metaKey]);
				keyToCommand.Remove(metaKey);
				keyToAbility.Remove(metaKey);
			}
			if (commandToKey.ContainsKey(list[Index2].Ability.Command))
			{
				keyToCommand.Remove(commandToKey[list[Index2].Ability.Command]);
				keyToAbility.Remove(commandToKey[list[Index2].Ability.Command]);
				commandToKey.Remove(list[Index2].Ability.Command);
			}
			keyToCommand.Add(metaKey, list[Index2].Ability.Command);
			keyToAbility.Add(metaKey, list[Index2].Ability);
			commandToKey.Add(list[Index2].Ability.Command, metaKey);
			HotkeyFavorites.AddFavorite(list[Index2].Ability.Command, metaKey);
			goto IL_0c18;
			IL_0c18:
			if (keys >= Keys.A && keys <= Keys.Z && dictionary.ContainsKey(key))
			{
				activatedAbilityEntry = dictionary2[key];
			}
			if (ConsoleLib.Console.Keyboard.RawCode == Keys.Space && list[Index2].Ability != null)
			{
				activatedAbilityEntry = list[Index2].Ability;
			}
			if (activatedAbilityEntry != null)
			{
				if (activatedAbilityEntry.Enabled)
				{
					if (activatedAbilityEntry.Cooldown <= 0)
					{
						Popup._TextConsole.DrawBuffer(TextConsole.ScrapBuffer2);
						GameManager.Instance.PopGameView();
						return activatedAbilityEntry.Command;
					}
					if (Options.GetOption("OptionAbilityCooldownWarningAsMessage").ToUpper() == "YES")
					{
						MessageQueue.AddPlayerMessage("You must wait {{C|" + activatedAbilityEntry.CooldownDescription + "}} to use that ability again.");
					}
					else
					{
						Popup.Show("You must wait {{C|" + activatedAbilityEntry.CooldownDescription + "}} to use that ability again.");
					}
				}
				else
				{
					Popup.Show(activatedAbilityEntry.DisabledMessage ?? "That ability is not enabled.");
				}
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				flag = true;
			}
			if (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "RightClick")
			{
				flag = true;
			}
		}
		GameManager.Instance.PopGameView();
		return null;
		char getHotkeyChar(int n)
		{
			if (hotkeySpread.Count <= n)
			{
				return '\0';
			}
			return ConsoleLib.Console.Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]);
		}
		string getHotkeyDisplay(int n)
		{
			if (hotkeySpread.Count <= n)
			{
				return " ";
			}
			return ConsoleLib.Console.Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]).ToString() ?? "";
		}
	}
}

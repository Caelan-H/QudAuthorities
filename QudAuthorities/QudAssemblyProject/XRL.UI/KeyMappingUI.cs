using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using XRL.World;

namespace XRL.UI;

[UIView("KeymappingLegacy", false, true, false, "Menu,Keybind", "Keymapping", false, 0, false)]
public class KeyMappingUI : IWantsTextConsoleInit
{
	private class KeyNode : ConsoleTreeNode<KeyNode>
	{
		public GameCommand Command;

		public KeyNode(GameCommand Command, bool Expand, KeyNode ParentNode)
			: base("", Expand, ParentNode)
		{
			this.Command = Command;
		}

		public KeyNode(string Category, bool Expand, KeyNode ParentNode)
			: base(Category, Expand, ParentNode)
		{
		}
	}

	private static ScreenBuffer Buffer;

	private static TextConsole TextConsole;

	public void Init(TextConsole console, ScreenBuffer buffer)
	{
		Buffer = buffer;
		TextConsole = console;
	}

	private static void BuildNodes(List<KeyNode> NodeList)
	{
		NodeList.Clear();
		foreach (string item in LegacyKeyMapping.CategoriesInOrder)
		{
			KeyNode keyNode = new KeyNode(item, Expand: true, null);
			NodeList.Add(keyNode);
			foreach (GameCommand item2 in LegacyKeyMapping.CommandsByCategory[item])
			{
				NodeList.Add(new KeyNode(item2, Expand: true, keyNode));
			}
		}
	}

	public static ScreenReturn Show()
	{
		List<KeyNode> list = new List<KeyNode>();
		BuildNodes(list);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Keys keys = Keys.None;
		int Index = 0;
		int Index2 = 0;
		if (Options.ModernUI || GameManager.Instance.PrereleaseInput)
		{
			GameManager.Instance.PushGameView("Keybinds");
			Event.ResetPool();
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			Buffer.Goto(13, 0);
			Buffer.Write("[ {{w|Control Mapping}} ]");
			Buffer.Goto(3, 10);
			Buffer.Write("Loading...");
			Popup._TextConsole.DrawBuffer(Buffer);
			ControlManager.ResetInput();
			_ = SingletonWindowBase<KeybindsScreen>.instance.KeybindsMenu().Result;
			GameManager.Instance.PopGameView();
			return ScreenReturn.Exit;
		}
		GameManager.Instance.PushGameView("Keymapping");
		while (!flag)
		{
			int num2;
			int i;
			while (true)
			{
				Event.ResetPool();
				Buffer.Clear();
				Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				Buffer.Goto(13, 0);
				Buffer.Write("[ {{w|Key Mapping}} ]");
				Buffer.Goto(50, 0);
				Buffer.Write(" {{W|ESC}} - Exit ");
				int num = 2;
				num2 = 0;
				ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index);
				if (Index2 < Index)
				{
					Index = Index2;
				}
				i = Index;
				while (num <= 21 && i <= list.Count)
				{
					for (; i < list.Count && list[i].ParentNode != null && !list[i].ParentNode.Expand; i++)
					{
					}
					if (i < list.Count)
					{
						if (list[i].Command == null)
						{
							Buffer.Goto(4, num);
							if (list[i].Expand)
							{
								Buffer.Write("[-] ");
							}
							else
							{
								Buffer.Write("[+] ");
							}
							Buffer.Write("{{C|" + list[i].Category + "}}");
						}
						else
						{
							Buffer.Goto(4, num);
							if (i == Index2)
							{
								Buffer.Write("  {{Y|" + list[i].Command.DisplayText + "}}");
							}
							else
							{
								Buffer.Write("  " + list[i].Command.DisplayText);
							}
							string text = ((i == Index2) ? "W" : "w");
							Buffer.Goto(35, num);
							if (LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[i].Command.ID).ContainsKey(list[i].Command.ID))
							{
								Buffer.Write("{{" + text + "|" + Keyboard.MetaToString(LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[i].Command.ID)[list[i].Command.ID]) + "}}");
							}
							else
							{
								Buffer.Write("{{K|<none>}}");
							}
							Buffer.Goto(60, num);
							if (LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[i].Command.ID).ContainsKey(list[i].Command.ID))
							{
								Buffer.Write("{{" + text + "|" + Keyboard.MetaToString(LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[i].Command.ID)[list[i].Command.ID]) + "}}");
							}
							else
							{
								Buffer.Write("{{K|<none>}}");
							}
						}
						num2 = i;
					}
					if (i == Index2)
					{
						Buffer.Goto(2, num);
						Buffer.Write("{{Y|>}}");
					}
					num++;
					i++;
				}
				if (flag4)
				{
					Index2 = num2;
					flag4 = false;
					continue;
				}
				Buffer.Goto(2, 24);
				Buffer.Write(" [{{W|Space}}-Assign] ");
				Buffer.Goto(17, 24);
				Buffer.Write(" [{{W|Del}}-Clear] ");
				Buffer.Goto(32, 24);
				Buffer.Write(" [{{W|F1}}-Load defaults] ");
				Buffer.Goto(51, 24);
				Buffer.Write(" [{{W|F2}}-Load laptop defaults] ");
				if (Index != 0)
				{
					Buffer.Goto(4, 1);
					Buffer.Write("{{W|<More...>}}");
				}
				if (i < list.Count)
				{
					Buffer.Goto(4, 22);
					Buffer.Write("{{W|<More...>}}");
				}
				if (Index2 > num2 && Index < list.Count)
				{
					Index++;
					continue;
				}
				if (!flag3)
				{
					break;
				}
				Index2 = Index;
				flag3 = false;
			}
			TextConsole.DrawBuffer(Buffer);
			keys = Keyboard.getvk(MapDirectionToArrows: false);
			if (keys == Keys.NumPad2)
			{
				ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index2, 1);
			}
			if (keys == Keys.NumPad8)
			{
				ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2, -1);
			}
			if (keys == Keys.Next)
			{
				if (num2 == Index2 && i < list.Count)
				{
					ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index2, 1);
					Index = Math.Min(Index2, list.Count - 20);
					flag4 = true;
				}
				else
				{
					Index2 = num2;
				}
			}
			if (keys == Keys.Prior)
			{
				if (Index == Index2 && Index > 0)
				{
					ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2, -1);
					Index = Math.Max(Index2 - 21, 0);
					flag3 = true;
				}
				else
				{
					Index2 = Index;
				}
			}
			if (keys == Keys.OemMinus || keys == Keys.Subtract)
			{
				list.ForEach(delegate(KeyNode n)
				{
					n.Expand = false;
				});
				ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2);
			}
			if (keys == Keys.Oemplus || keys == Keys.Add)
			{
				list.ForEach(delegate(KeyNode n)
				{
					n.Expand = true;
				});
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
					ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2);
				}
			}
			if (keys == Keys.NumPad6)
			{
				list[Index2].Expand = true;
			}
			if (keys == Keys.Delete && list[Index2].Command != null && list[Index2].Command.ID != "CmdSystemMenu" && Popup.ShowYesNo("Are you sure you want to clear the bindings for {{C|" + list[Index2].Command.DisplayText + "}}?") == DialogResult.Yes)
			{
				if (LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[Index2].Command.ID).ContainsKey(list[Index2].Command.ID))
				{
					LegacyKeyMapping.CurrentMap.getPrimaryKeyToCommand(list[Index2].Command.ID).Remove(LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[Index2].Command.ID)[list[Index2].Command.ID]);
					LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[Index2].Command.ID).Remove(list[Index2].Command.ID);
				}
				if (LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[Index2].Command.ID).ContainsKey(list[Index2].Command.ID))
				{
					LegacyKeyMapping.CurrentMap.getSecondaryKeyToCommand(list[Index2].Command.ID).Remove(LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[Index2].Command.ID)[list[Index2].Command.ID]);
					LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[Index2].Command.ID).Remove(list[Index2].Command.ID);
				}
				flag2 = true;
			}
			if (Keyboard.RawCode == Keys.F1 && Popup.ShowYesNo("Are you sure you want to override your keymap with the default?") == DialogResult.Yes)
			{
				LegacyKeyMapping.CurrentMap = LegacyKeyMapping.LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
				LegacyKeyMapping.CurrentMap.ApplyDefaults();
				flag2 = true;
			}
			if (Keyboard.RawCode == Keys.F2 && Popup.ShowYesNo("Are you sure you want to override your keymap with the default?") == DialogResult.Yes)
			{
				LegacyKeyMapping.CurrentMap = LegacyKeyMapping.LoadLegacyKeymap(DataManager.FilePath("DefaultLaptopKeymap.xml"));
				LegacyKeyMapping.CurrentMap.ApplyDefaults();
				flag2 = true;
			}
			if ((Keyboard.RawCode == Keys.Space || keys == Keys.Enter) && list[Index2].Command != null && list[Index2].Command.ID != "CmdSystemMenu")
			{
				Popup.ShowChoice("Press the key combination you would like to bind to {{C|" + list[Index2].Command.DisplayText + "}}");
				int metaKey = Keyboard.MetaKey;
				if (Keyboard.vkCode == Keys.Escape)
				{
					continue;
				}
				string text2 = "{{W|" + Keyboard.MetaToString(metaKey) + "}}";
				Dictionary<int, string> primaryKeyToCommand = LegacyKeyMapping.CurrentMap.getPrimaryKeyToCommand(list[Index2].Command.ID);
				Dictionary<string, int> primaryCommandToKey = LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(list[Index2].Command.ID);
				if (primaryKeyToCommand.ContainsKey(metaKey) && LegacyKeyMapping.CommandsByID.ContainsKey(primaryKeyToCommand[metaKey]))
				{
					if (Popup.ShowYesNo(text2 + " is already bound to {{C|" + LegacyKeyMapping.CommandsByID[primaryKeyToCommand[metaKey]].DisplayText + "}}. Do you want to bind it to {{C|" + list[Index2].Command.DisplayText + "}} instead?") == DialogResult.No)
					{
						continue;
					}
					primaryCommandToKey.Remove(primaryKeyToCommand[metaKey]);
					primaryKeyToCommand.Remove(metaKey);
				}
				Dictionary<int, string> secondaryKeyToCommand = LegacyKeyMapping.CurrentMap.getSecondaryKeyToCommand(list[Index2].Command.ID);
				Dictionary<string, int> secondaryCommandToKey = LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(list[Index2].Command.ID);
				if (secondaryKeyToCommand.ContainsKey(metaKey) && LegacyKeyMapping.CommandsByID.ContainsKey(secondaryKeyToCommand[metaKey]))
				{
					if (Popup.ShowYesNo(text2 + " is already bound to {{C|" + LegacyKeyMapping.CommandsByID[secondaryKeyToCommand[metaKey]].DisplayText + "}}. Do you want to bind it to {{C|" + list[Index2].Command.DisplayText + "}} instead?") == DialogResult.No)
					{
						continue;
					}
					secondaryCommandToKey.Remove(secondaryKeyToCommand[metaKey]);
					secondaryKeyToCommand.Remove(metaKey);
				}
				flag2 = true;
				if (!primaryCommandToKey.ContainsKey(list[Index2].Command.ID))
				{
					primaryCommandToKey.Add(list[Index2].Command.ID, metaKey);
					primaryKeyToCommand.Add(metaKey, list[Index2].Command.ID);
				}
				else if (!secondaryCommandToKey.ContainsKey(list[Index2].Command.ID))
				{
					secondaryCommandToKey.Add(list[Index2].Command.ID, metaKey);
					secondaryKeyToCommand.Add(metaKey, list[Index2].Command.ID);
				}
				else
				{
					secondaryKeyToCommand.Remove(secondaryCommandToKey[list[Index2].Command.ID]);
					secondaryCommandToKey.Remove(list[Index2].Command.ID);
					secondaryCommandToKey.Add(list[Index2].Command.ID, primaryCommandToKey[list[Index2].Command.ID]);
					secondaryKeyToCommand.Add(primaryCommandToKey[list[Index2].Command.ID], list[Index2].Command.ID);
					primaryKeyToCommand.Remove(primaryCommandToKey[list[Index2].Command.ID]);
					primaryCommandToKey.Remove(list[Index2].Command.ID);
					primaryCommandToKey.Add(list[Index2].Command.ID, metaKey);
					primaryKeyToCommand.Add(metaKey, list[Index2].Command.ID);
				}
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				if (flag2 && Popup.ShowYesNo("Would you like to save your changes?") == DialogResult.Yes)
				{
					LegacyKeyMapping.SaveKeymap(LegacyKeyMapping.CurrentMap, DataManager.SavePath(Environment.UserName + ".Keymap.json"));
				}
				flag = true;
			}
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}

using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Rewired;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.UI;

public class SkillsAndPowersScreen : IScreen
{
	public static List<SPNode> Nodes;

	private static bool HasAnyPower(GameObject GO, SkillEntry Skill)
	{
		foreach (PowerEntry value in Skill.Powers.Values)
		{
			if (GO.HasPart(value.Class))
			{
				return true;
			}
		}
		return false;
	}

	private static int SkillNodePos(SkillEntry Skill)
	{
		if (Nodes != null)
		{
			int i = 0;
			for (int count = Nodes.Count; i < count; i++)
			{
				if (Nodes[i].Skill == Skill)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static int PowerNodePos(PowerEntry Power)
	{
		if (Nodes != null)
		{
			int i = 0;
			for (int count = Nodes.Count; i < count; i++)
			{
				if (Nodes[i].Power == Power)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static SPNode OldSkillNode(List<SPNode> OldNodes, SkillEntry Skill)
	{
		if (OldNodes != null)
		{
			int i = 0;
			for (int count = OldNodes.Count; i < count; i++)
			{
				if (OldNodes[i].Skill == Skill)
				{
					return OldNodes[i];
				}
			}
		}
		return null;
	}

	private static SPNode OldPowerNode(List<SPNode> OldNodes, PowerEntry Power)
	{
		if (OldNodes != null)
		{
			int i = 0;
			for (int count = OldNodes.Count; i < count; i++)
			{
				if (OldNodes[i].Power == Power)
				{
					return OldNodes[i];
				}
			}
		}
		return null;
	}

	private static void AddSkillNodes(GameObject GO, SkillEntry Skill, List<SPNode> OldNodes, bool Expand)
	{
		SPNode sPNode = null;
		int num = SkillNodePos(Skill);
		if (num == -1)
		{
			num = Nodes.Count;
			sPNode = OldSkillNode(OldNodes, Skill) ?? new SPNode(Skill, null, Expand, null);
			Nodes.Add(sPNode);
		}
		else
		{
			sPNode = Nodes[num];
		}
		int num2 = num;
		foreach (PowerEntry value in Skill.Powers.Values)
		{
			if (Skill.Initiatory == true && !GO.HasSkill(value.Class))
			{
				continue;
			}
			int num3 = PowerNodePos(value);
			if (num3 == -1)
			{
				SPNode sPNode2 = OldPowerNode(OldNodes, value);
				SPNode item = sPNode2 ?? new SPNode(null, value, Expand: true, sPNode);
				if (num2 >= Nodes.Count - 1)
				{
					num3 = Nodes.Count;
					Nodes.Add(item);
				}
				else
				{
					num3 = num2 + 1;
					Nodes.Insert(num3, item);
				}
				if (sPNode2 == null && !sPNode.Expand && Expand)
				{
					sPNode.Expand = true;
				}
			}
			num2 = num3;
		}
	}

	private static void BuildNodes(GameObject GO, bool Rebuild = false)
	{
		List<SPNode> nodes = Nodes;
		if (Rebuild || Nodes == null)
		{
			Nodes = new List<SPNode>();
		}
		foreach (SkillEntry value in SkillFactory.Factory.SkillList.Values)
		{
			if ((value.Initiatory != true || GO.HasSkill(value.Class)) && (GO.HasSkill(value.Class) || HasAnyPower(GO, value)))
			{
				AddSkillNodes(GO, value, nodes, Expand: true);
			}
		}
		foreach (SkillEntry value2 in SkillFactory.Factory.SkillList.Values)
		{
			if ((value2.Initiatory != true || GO.HasSkill(value2.Class)) && !GO.HasSkill(value2.Class) && !HasAnyPower(GO, value2))
			{
				AddSkillNodes(GO, value2, nodes, Expand: false);
			}
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("SkillsAndPowers");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		GameObject body = XRLCore.Core.Game.Player.Body;
		bool flag = false;
		Keys keys = Keys.None;
		int num = 0;
		int num2 = 0;
		BuildNodes(GO, Rebuild: true);
		int num3 = 3;
		foreach (SPNode node in Nodes)
		{
			string text = null;
			if (node.Skill != null)
			{
				text = node.Skill.Description;
			}
			else if (node.Power != null)
			{
				text = node.Power.Description;
			}
			if (text != null)
			{
				TextBlock textBlock = new TextBlock(text.Replace("\r\n", "\n"), 73, 12);
				if (textBlock.Lines.Count > num3)
				{
					num3 = textBlock.Lines.Count;
				}
			}
		}
		string s = "< {{W|7}} Tinkering | Character {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Tinkering | Character {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			while (true)
			{
				IL_013e:
				Event.ResetPool();
				scrapBuffer.Clear();
				scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Goto(13, 0);
				scrapBuffer.Write("[ {{W|Buy Skills}} - {{C|" + GO.Stat("SP") + "}}sp remaining ]");
				scrapBuffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
				scrapBuffer.Write(s);
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
				if (num2 < num)
				{
					num = num2;
				}
				int num4 = 2;
				int i = num;
				int num5 = 0;
				while (num4 <= 22 - num3 && i <= Nodes.Count)
				{
					for (; i < Nodes.Count && !Nodes[i].Visible; i++)
					{
					}
					if (i < Nodes.Count)
					{
						if (Nodes[i].Skill != null)
						{
							scrapBuffer.Goto(4, num4);
							if (Nodes[i].Expand)
							{
								scrapBuffer.Write("[-] ");
							}
							else
							{
								scrapBuffer.Write("[+] ");
							}
							bool flag2 = HasAnyPower(GO, Nodes[i].Skill);
							if (body.HasPart(Nodes[i].Skill.Class))
							{
								scrapBuffer.Write("{{W|" + Nodes[i].Skill.Name + "}}");
							}
							else if (flag2)
							{
								if (Nodes[i].Skill.Cost <= body.Stat("SP"))
								{
									scrapBuffer.Write("[{{C|" + Nodes[i].Skill.Cost + "}}sp] {{W|" + Nodes[i].Skill.Name + "}}");
								}
								else
								{
									scrapBuffer.Write("[{{R|" + Nodes[i].Skill.Cost + "}}sp] {{W|" + Nodes[i].Skill.Name + "}}");
								}
							}
							else if (Nodes[i].Skill.Cost <= body.Stat("SP"))
							{
								scrapBuffer.Write("[{{C|" + Nodes[i].Skill.Cost + "}}sp] {{w|" + Nodes[i].Skill.Name + "}}");
							}
							else
							{
								scrapBuffer.Write("[{{R|" + Nodes[i].Skill.Cost + "}}sp] {{w|" + Nodes[i].Skill.Name + "}}");
							}
						}
						else
						{
							scrapBuffer.Goto(4, num4);
							string text2 = "";
							if (body.HasPart(Nodes[i].Power.Class))
							{
								scrapBuffer.Write(" - {{G|" + Nodes[i].Power.Name + "}}");
							}
							else
							{
								if (Nodes[i].Power.Prereq != null)
								{
									foreach (string item in Nodes[i].Power.Prereq.CachedCommaExpansion())
									{
										string text3 = item;
										bool flag3 = false;
										if (SkillFactory.Factory.PowersByClass.TryGetValue(item, out var value))
										{
											text3 = value.Name;
											flag3 = body.HasSkill(item);
										}
										else if (MutationFactory.HasMutation(item))
										{
											text3 = MutationFactory.GetMutationEntryByName(item).DisplayName;
											flag3 = body.HasPart(item);
										}
										text2 = ((!flag3) ? (text2 + ", {{R|" + text3 + "}}") : (text2 + ", {{G|" + text3 + "}}"));
									}
								}
								if (Nodes[i].Power.Exclusion != null)
								{
									foreach (string item2 in Nodes[i].Power.Exclusion.CachedCommaExpansion())
									{
										string text4 = item2;
										bool flag4 = false;
										if (SkillFactory.Factory.PowersByClass.TryGetValue(item2, out var value2))
										{
											text4 = value2.Name;
											flag4 = !body.HasSkill(item2);
										}
										else if (MutationFactory.HasMutation(item2))
										{
											text4 = MutationFactory.GetMutationEntryByName(item2).DisplayName;
											flag4 = !body.HasPart(item2);
										}
										text2 = ((!flag4) ? (text2 + ", Ex: {{R|" + text4 + "}}") : (text2 + ", Ex: {{g|" + text4 + "}}"));
									}
								}
								scrapBuffer.Write(Nodes[i].Power.Render(GO));
								scrapBuffer.Write(text2);
							}
						}
						num5 = i;
					}
					if (i == num2)
					{
						scrapBuffer.Goto(2, num4);
						scrapBuffer.Write("{{Y|>}}");
					}
					num4++;
					i++;
				}
				if (num2 > num5 && num < Nodes.Count)
				{
					for (int j = num + 1; j < Nodes.Count; j++)
					{
						if (Nodes[j].Visible)
						{
							num = j;
							break;
						}
					}
					continue;
				}
				int num6 = 0;
				int num7 = 0;
				for (int k = 0; k < Nodes.Count; k++)
				{
					if (Nodes[k].Visible)
					{
						num6++;
						num7 = k;
					}
				}
				scrapBuffer.Goto(2, 24);
				if (Nodes[num2].Power != null && XRLCore.Core.Game.Player.Body.HasPart(Nodes[num2].Power.Class))
				{
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						scrapBuffer.Write(" [{{K|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
					}
					else
					{
						scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{K|Space}}-Buy] ");
					}
				}
				else if (Nodes[num2].Skill != null && XRLCore.Core.Game.Player.Body.HasPart(Nodes[num2].Skill.Class))
				{
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						scrapBuffer.Write(" [{{K|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
					}
					else
					{
						scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{K|Space}}-Buy] ");
					}
				}
				else if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.Write(" [{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
				}
				else
				{
					scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{W|Space}}-Buy] ");
				}
				string text5 = "";
				string text6 = "";
				if (Nodes[num2].Skill != null)
				{
					text5 = Nodes[num2].Skill.Name;
					text6 = Nodes[num2].Skill.Description;
				}
				else
				{
					text5 = Nodes[num2].Power.Name;
					text6 = Nodes[num2].Power.Description;
				}
				if (num != 0)
				{
					scrapBuffer.Goto(4, 1);
					scrapBuffer.Write("{{W|<More...>}}");
				}
				if (num5 != num7)
				{
					scrapBuffer.Goto(4, 19);
					scrapBuffer.Write("{{W|<More...>}}");
				}
				TextBlock textBlock2 = new TextBlock(text6.Replace("\r\n", "\n"), 73, 12);
				int num8 = 24 - num3;
				scrapBuffer.SingleBox(0, num8 - 1, 79, num8 - 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Fill(1, num8, 78, 23, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
				scrapBuffer.Goto(4, num8 - 1);
				scrapBuffer.Goto(0, num8 - 1);
				scrapBuffer.Write(195);
				scrapBuffer.Goto(79, num8 - 1);
				scrapBuffer.Write(180);
				scrapBuffer.Goto(4, num8 - 1);
				scrapBuffer.Write("{{W|" + text5 + "}}");
				for (int l = 0; l < textBlock2.Lines.Count; l++)
				{
					scrapBuffer.Goto(2, num8 + l);
					scrapBuffer.Write(textBlock2.Lines[l]);
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer);
				keys = ConsoleLib.Console.Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
				{
					flag = true;
				}
				if (keys == Keys.NumPad2)
				{
					for (int m = num2 + 1; m < Nodes.Count; m++)
					{
						if (Nodes[m].Visible)
						{
							num2 = m;
							break;
						}
					}
				}
				if (keys == Keys.NumPad8)
				{
					for (int num9 = num2 - 1; num9 >= 0; num9--)
					{
						if (Nodes[num9].Visible)
						{
							num2 = num9;
							break;
						}
					}
				}
				if (keys == Keys.Prior)
				{
					if (num2 == num)
					{
						int n = 0;
						for (int num10 = 21 - num3; n < num10; n++)
						{
							if (num2 <= 0)
							{
								break;
							}
							for (int num11 = num2 - 1; num11 >= 0; num11--)
							{
								if (Nodes[num11].Visible)
								{
									num2 = num11;
									break;
								}
							}
						}
					}
					else
					{
						num2 = num;
					}
				}
				if (keys == Keys.Next)
				{
					if (num2 == num5)
					{
						int num12 = 0;
						for (int num13 = 21 - num3; num12 < num13; num12++)
						{
							if (num2 >= Nodes.Count)
							{
								break;
							}
							for (int num14 = num2 + 1; num14 < Nodes.Count; num14++)
							{
								if (Nodes[num14].Visible)
								{
									num2 = num14;
									break;
								}
							}
						}
					}
					else
					{
						num2 = num5;
					}
				}
				if (keys == Keys.NumPad4)
				{
					if (Nodes[num2].Skill != null)
					{
						Nodes[num2].Expand = false;
					}
					else
					{
						Nodes[num2].ParentNode.Expand = false;
						for (int num15 = num2 - 1; num15 >= 0; num15--)
						{
							if (Nodes[num15].Visible)
							{
								num2 = num15;
								break;
							}
						}
					}
				}
				if (keys == Keys.OemMinus || keys == Keys.Subtract)
				{
					foreach (SPNode node2 in Nodes)
					{
						node2.Expand = false;
					}
					for (int num16 = num2 - 1; num16 >= 0; num16--)
					{
						if (Nodes[num16].Visible)
						{
							num2 = num16;
							break;
						}
					}
				}
				if (keys == Keys.Oemplus || keys == Keys.Add)
				{
					foreach (SPNode node3 in Nodes)
					{
						node3.Expand = true;
					}
				}
				if (keys == Keys.NumPad6)
				{
					Nodes[num2].Expand = true;
				}
				if (keys == Keys.OemQuestion)
				{
					if (Nodes[num2].Skill != null)
					{
						Popup.Show(Nodes[num2].Skill.Description);
					}
					else
					{
						Popup.Show(Nodes[num2].Power.Description);
					}
				}
				if (keys != Keys.Space && keys != Keys.Enter)
				{
					break;
				}
				SkillEntry skillEntry = null;
				PowerEntry powerEntry = null;
				string name;
				string @class;
				string text7;
				int cost;
				if (Nodes[num2].Power == null)
				{
					name = Nodes[num2].Skill.Name;
					@class = Nodes[num2].Skill.Class;
					text7 = "skill";
					cost = Nodes[num2].Skill.Cost;
					skillEntry = Nodes[num2].Skill;
				}
				else if (!GO.HasSkill(Nodes[num2].ParentNode.Skill.Class))
				{
					if (Popup.ShowYesNoCancel("You do not have the skill associated with that power. Would you like to purchase the required skill?") != 0)
					{
						continue;
					}
					name = Nodes[num2].ParentNode.Skill.Name;
					@class = Nodes[num2].ParentNode.Skill.Class;
					text7 = "skill";
					cost = Nodes[num2].ParentNode.Skill.Cost;
					skillEntry = Nodes[num2].ParentNode.Skill;
				}
				else
				{
					name = Nodes[num2].Power.Name;
					@class = Nodes[num2].Power.Class;
					text7 = "power";
					cost = Nodes[num2].Power.Cost;
					powerEntry = Nodes[num2].Power;
				}
				if (body.HasPart(@class))
				{
					Popup.Show("You already have that " + text7 + ".");
					break;
				}
				if (body.Stat("SP") < cost)
				{
					Popup.Show("You don't have enough skill points to buy that " + text7 + "!");
					break;
				}
				if (powerEntry != null && !powerEntry.MeetsRequirements(body))
				{
					powerEntry.ShowRequirementsFailurePopup(body);
					break;
				}
				if (skillEntry != null && !skillEntry.MeetsRequirements(body))
				{
					skillEntry.ShowRequirementsFailurePopup(body);
					break;
				}
				string text8 = "XRL.World.Parts.Skill." + @class;
				Type type = ModManager.ResolveType(text8);
				if (type == null)
				{
					Popup.Show("No implementation for " + text8);
					break;
				}
				if (text7 != "power")
				{
					foreach (PowerEntry value3 in skillEntry.Powers.Values)
					{
						if (value3.Cost == 0 && !value3.MeetsAttributeMinimum(body))
						{
							value3.ShowAttributeFailurePopup(body);
							goto IL_013e;
						}
					}
				}
				if (Popup.ShowYesNo("Are you sure you want to buy " + name + " for {{C|" + cost + "}} sp?") != 0)
				{
					break;
				}
				BaseSkill newSkill = Activator.CreateInstance(type) as BaseSkill;
				body.GetPart<Skills>().AddSkill(newSkill);
				body.GetStat("SP").Penalty += cost;
				if (Nodes[num2].Skill != null)
				{
					Nodes[num2].Expand = true;
				}
				SkillEntry skill = Nodes[num2].Skill;
				if (!string.IsNullOrEmpty(name))
				{
					MetricsManager.LogEvent("Gameplay:Skill:Purchase:" + name);
				}
				BuildNodes(GO);
				if (skill == null || Nodes[num2].Skill == skill)
				{
					break;
				}
				for (int num17 = 0; num17 < Nodes.Count; num17++)
				{
					if (Nodes[num17].Skill == skill)
					{
						num2 = num17;
						if (num2 < num)
						{
							num = num2;
						}
						break;
					}
				}
				break;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5 || keys == Keys.NumPad9 || keys == Keys.NumPad7)
			{
				flag = true;
			}
		}
		GameManager.Instance.PopGameView();
		return keys switch
		{
			Keys.NumPad7 => ScreenReturn.Previous, 
			Keys.NumPad9 => ScreenReturn.Next, 
			_ => ScreenReturn.Exit, 
		};
	}
}

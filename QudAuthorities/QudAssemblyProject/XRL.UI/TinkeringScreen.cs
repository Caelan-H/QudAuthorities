using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Rewired;
using XRL.Core;
using XRL.Language;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.UI;

public class TinkeringScreen : IScreen
{
	public ScreenReturn Show(GameObject GO)
	{
		return Show(GO, null, null);
	}

	public ScreenReturn Show(GameObject GO, GameObject ForModdingOf = null, IEvent FromEvent = null)
	{
		GameManager.Instance.PushGameView("Tinkering");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Keys keys = Keys.None;
		bool flag = false;
		List<GameObject> list = null;
		string text = "Build";
		int num = 0;
		int num2 = 0;
		if (ForModdingOf != null)
		{
			list = new List<GameObject> { ForModdingOf };
			text = "Mod";
		}
		GameObject player = The.Player;
		BitLocker bitLocker = ((!(player.GetPart("Tinkering") is Tinkering)) ? (player.GetPart("BitLocker") as BitLocker) : player.RequirePart<BitLocker>());
		List<TinkerData> list2 = new List<TinkerData>(64);
		List<TinkerData> ModRecipes = new List<TinkerData>(64);
		List<GameObject> ModdableItems = null;
		Dictionary<GameObject, List<TinkerData>> ItemMods = null;
		Dictionary<GameObject, bool> ItemExpanded = null;
		List<TinkerData> list3 = null;
		int TotalModLines = 0;
		int num3 = 15;
		int num4 = 0;
		foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
		{
			if (knownRecipe.Type == "Build")
			{
				list2.Add(knownRecipe);
			}
			else if (knownRecipe.Type == "Mod")
			{
				ModRecipes.Add(knownRecipe);
			}
		}
		list2.Sort((TinkerData a, TinkerData b) => ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName));
		ModRecipes.Sort((TinkerData a, TinkerData b) => ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName));
		string s = "< {{W|7}} Journal | Skills {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Journal | Skills {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		BitCost bitCost = new BitCost();
		BitCost bitCost2 = new BitCost();
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			bool flag2 = GO.AreHostilesNearby();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(35, 0);
			scrapBuffer.Write("[ {{W|Tinkering}} ]");
			if (flag2)
			{
				scrapBuffer.WriteAt(10, 0, " {{R|hostiles nearby}} ");
			}
			if (bitLocker != null)
			{
				scrapBuffer.SingleBox(51, 0, 79, 16, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			if (list == null)
			{
				if (text == "Build")
				{
					scrapBuffer.Goto(2, 1);
					scrapBuffer.Write("{{Y|>}} {{W|Build}}    {{w|Mod}}");
				}
				else
				{
					scrapBuffer.Goto(2, 1);
					scrapBuffer.Write("  {{w|Build}}  {{Y|>}} {{W|Mod}}");
				}
			}
			TinkerData tinkerData = null;
			GameObject obj2 = null;
			bitCost.Clear();
			if (text == "Mod")
			{
				bool flag3 = false;
				if (!player.HasSkill("Tinkering"))
				{
					scrapBuffer.Goto(4, 4);
					scrapBuffer.Write("You don't have the Tinkering skill.");
				}
				else if (ModRecipes.Count == 0)
				{
					scrapBuffer.Goto(4, 4);
					scrapBuffer.Write("You don't have any modification schematics.");
				}
				else
				{
					if (ModdableItems == null)
					{
						TotalModLines = 0;
						ModdableItems = new List<GameObject>(128);
						ItemExpanded = new Dictionary<GameObject, bool>(128);
						ItemMods = new Dictionary<GameObject, List<TinkerData>>(64);
						list3 = new List<TinkerData>(ModRecipes);
						Action<GameObject> action = delegate(GameObject obj)
						{
							string text7 = TechModding.ModKey(obj);
							if (text7 != null && obj.Understood())
							{
								for (int num13 = 0; num13 < ModRecipes.Count; num13++)
								{
									if (ModRecipes[num13].CanMod(text7) && TechModding.ModificationApplicable(ModRecipes[num13].PartName, obj, XRLCore.Core.Game.Player.Body))
									{
										if (!ItemMods.ContainsKey(obj))
										{
											ModdableItems.Add(obj);
											ItemMods.Add(obj, new List<TinkerData>(8));
											ItemExpanded.Add(obj, value: false);
											TotalModLines++;
										}
										ItemMods[obj].Add(ModRecipes[num13]);
										TotalModLines++;
									}
								}
							}
						};
						if (list == null)
						{
							player.Inventory.ForeachObject(action);
							player.Body.ForeachEquippedObject(action);
							if (list3.Count > 0)
							{
								GameObject gameObject = GameObject.createSample("DummyTinkerScreenObject");
								ModdableItems.Add(gameObject);
								ItemMods.Add(gameObject, new List<TinkerData>());
								ItemExpanded.Add(gameObject, value: false);
								TotalModLines++;
								for (int i = 0; i < list3.Count; i++)
								{
									ItemMods[gameObject].Add(list3[i]);
									TotalModLines++;
								}
							}
						}
						else
						{
							foreach (GameObject item in list)
							{
								action(item);
							}
						}
					}
					if (ModdableItems.Count == 0)
					{
						scrapBuffer.Goto(4, 4);
						if (list != null)
						{
							flag = true;
							break;
						}
						scrapBuffer.Write("You don't have any moddable items.");
					}
					else
					{
						int num5 = 3;
						int num6 = 0;
						for (int j = 0; j < ModdableItems.Count; j++)
						{
							if (num5 >= num3)
							{
								break;
							}
							if (num6 >= num2)
							{
								scrapBuffer.Goto(4, num5);
								scrapBuffer.Write(StringFormat.ClipLine(ModdableItems[j].DisplayName, 46));
								if (num6 == num)
								{
									scrapBuffer.Goto(2, num5);
									scrapBuffer.Write("{{Y|>}}");
									flag3 = true;
								}
								num5++;
							}
							num6++;
							for (int k = 0; k < ItemMods[ModdableItems[j]].Count; k++)
							{
								if (num5 >= num3)
								{
									break;
								}
								if (num6 >= num2)
								{
									int key = Tier.Constrain(ItemMods[ModdableItems[j]][k].Tier);
									int key2 = Tier.Constrain(ModdableItems[j].GetIntProperty("nMods") - ModdableItems[j].GetIntProperty("NoCostMods") + ModdableItems[j].GetTier());
									bitCost2.Clear();
									bitCost2.Increment(BitType.TierBits[key]);
									bitCost2.Increment(BitType.TierBits[key2]);
									ModifyBitCostEvent.Process(player, bitCost2, "Mod");
									if (num6 == num)
									{
										scrapBuffer.Goto(2, num5);
										scrapBuffer.Write("{{Y|>}}");
										bitCost2.CopyTo(bitCost);
										tinkerData = ItemMods[ModdableItems[j]][k];
										obj2 = ModdableItems[j];
										flag3 = true;
									}
									scrapBuffer.Goto(4, num5);
									scrapBuffer.Write("  ");
									scrapBuffer.Write(ItemMods[ModdableItems[j]][k].DisplayName);
									scrapBuffer.Write(" <");
									scrapBuffer.Write(bitCost2.ToString());
									scrapBuffer.Write(">");
									num5++;
								}
								num6++;
							}
						}
					}
				}
				if (!flag3 && num > 0)
				{
					num--;
					continue;
				}
				scrapBuffer.SingleBox(0, 16, 80, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				if (tinkerData != null)
				{
					string s2 = StringFormat.ClipText("{{rules|" + TechModding.GetModificationDescription(tinkerData.Blueprint, obj2) + "}}", 76, KeepNewlines: true);
					scrapBuffer.Goto(2, 18);
					scrapBuffer.WriteBlockWithNewlines(s2);
				}
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.WriteAt(3, 24, " {{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}} Mod Item ");
				}
				else
				{
					scrapBuffer.WriteAt(3, 24, " {{W|Space}} Mod Item ");
					if (ModRecipes.Count > 0)
					{
						scrapBuffer.WriteAt(21, 24, " {{W|L}} List Mods ");
						scrapBuffer.WriteAt(36, 24, " {{W|ESC}}/{{W|5}} Exit ");
					}
					else
					{
						scrapBuffer.WriteAt(21, 24, " {{W|ESC}}/{{W|5}} Exit ");
					}
				}
			}
			else if (text == "Build")
			{
				if (list2.Count > 0)
				{
					tinkerData = list2[num];
				}
				if (tinkerData != null)
				{
					bitCost.Import(TinkerItem.GetBitCostFor(tinkerData.Blueprint));
					ModifyBitCostEvent.Process(player, bitCost, "Build");
				}
				if (!player.HasSkill("Tinkering"))
				{
					scrapBuffer.Goto(4, 4);
					scrapBuffer.Write("You don't have the Tinkering skill.");
				}
				else if (list2.Count == 0)
				{
					scrapBuffer.Goto(4, 4);
					scrapBuffer.Write("You don't have any item schematics.");
				}
				else
				{
					for (int l = num2; l < list2.Count && l - num2 < 12; l++)
					{
						scrapBuffer.Goto(4, 3 + (l - num2));
						if (string.IsNullOrEmpty(list2[l].DisplayName))
						{
							scrapBuffer.Write(list2[l].Blueprint);
						}
						else
						{
							scrapBuffer.Write(list2[l].DisplayName);
						}
						string text2;
						if (l == num)
						{
							text2 = bitCost.ToString();
						}
						else
						{
							bitCost2.Clear();
							bitCost2.Import(TinkerItem.GetBitCostFor(list2[l].Blueprint));
							ModifyBitCostEvent.Process(player, bitCost2, "Build");
							text2 = bitCost2.ToString();
						}
						scrapBuffer.Goto(50 - ColorUtility.LengthExceptFormatting(text2), 3 + (l - num2));
						if (l == num)
						{
							scrapBuffer.Write("{{^K|" + text2 + "}}");
						}
						else
						{
							scrapBuffer.Write(text2);
						}
						if (l == num)
						{
							scrapBuffer.Goto(2, 3 + (l - num2));
							scrapBuffer.Write("{{Y|>}}");
						}
					}
					scrapBuffer.SingleBox(0, 16, 80, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					if (tinkerData != null)
					{
						scrapBuffer.Goto(2, 16);
						scrapBuffer.Write(list2[num].LongDisplayName);
						scrapBuffer.Goto(2, 17);
						scrapBuffer.WriteBlockWithNewlines(list2[num].Description, 8, num4, drawIndicators: true);
					}
				}
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.WriteAt(3, 24, " {{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}} Build ");
				}
				else
				{
					scrapBuffer.WriteAt(3, 24, " {{W|Space}} Build ");
					scrapBuffer.WriteAt(18, 24, " {{W|+}}/{{W|-}} Scroll ");
					scrapBuffer.WriteAt(32, 24, " {{W|ESC}}/{{W|5}} Exit ");
				}
			}
			if (bitLocker != null)
			{
				scrapBuffer.WriteAt(53, 0, " Bit Locker ");
				int num5 = 1;
				foreach (char item2 in BitType.BitOrder)
				{
					BitType bitType = BitType.BitMap[item2];
					string s3 = "{{" + bitType.Color + "|" + (Options.AlphanumericBits ? BitType.CharTranslateBit(bitType.Color) : '\a') + " " + bitType.Description + "}}";
					int bitCount = bitLocker.GetBitCount(item2);
					string s4 = ((bitCount == 0) ? "{{K|0}}" : ((bitCount >= 1000000) ? ("{{C|" + bitCount / 1000000 + "M}}") : ((bitCount < 10000) ? ("{{C|" + bitCount + "}}") : ("{{C|" + bitCount / 1000 + "K}}"))));
					scrapBuffer.Goto(52, num5);
					scrapBuffer.Write(s3);
					scrapBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(s4), num5);
					scrapBuffer.Write(s4);
					if (tinkerData != null)
					{
						scrapBuffer.Goto(52, num5);
						if (bitCost.TryGetValue(item2, out var value))
						{
							if (bitCount >= value)
							{
								scrapBuffer.Write("{{G|û}}");
							}
							else
							{
								scrapBuffer.Write("{{R|X}}");
							}
						}
						else
						{
							scrapBuffer.Write("{{K|-}}");
						}
					}
					num5++;
				}
				if (!string.IsNullOrEmpty(tinkerData?.Ingredient))
				{
					bool flag4 = false;
					string[] array = tinkerData.Ingredient.Split(',');
					int num7 = 14;
					if (array.Length > 1)
					{
						num7--;
					}
					num5 = num7;
					string[] array2 = array;
					foreach (string blueprint in array2)
					{
						if (GO.Inventory.FindObjectByBlueprint(blueprint) != null)
						{
							scrapBuffer.Goto(52, num5);
							num5++;
							scrapBuffer.Write("{{G|û}} ");
							scrapBuffer.Write(TinkeringHelpers.TinkeredItemShortDisplayName(blueprint));
							flag4 = true;
							break;
						}
					}
					if (!flag4)
					{
						array2 = array;
						foreach (string blueprint2 in array2)
						{
							if (num5 != num7)
							{
								scrapBuffer.Goto(52, num5);
								scrapBuffer.Write("-or-");
								num5++;
							}
							scrapBuffer.Goto(52, num5);
							num5++;
							scrapBuffer.Write("{{R|X}} ");
							scrapBuffer.Write(TinkeringHelpers.TinkeredItemShortDisplayName(blueprint2));
						}
					}
				}
			}
			if (list == null)
			{
				scrapBuffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
				scrapBuffer.Write(s);
			}
			scrapBuffer.WriteAt(51, 0, "Â");
			scrapBuffer.WriteAt(0, 16, "Ã");
			scrapBuffer.WriteAt(79, 16, "\u00b4");
			scrapBuffer.WriteAt(51, 16, "Á");
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (list == null && (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next)))
			{
				flag = true;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				flag = true;
			}
			if (list == null && (keys == Keys.NumPad4 || keys == Keys.NumPad6))
			{
				num4 = 0;
				num2 = 0;
				num = 0;
				text = ((!(text == "Build")) ? "Build" : "Mod");
			}
			if (keys == Keys.L && text == "Mod" && ModRecipes.Count > 0)
			{
				string text3 = "";
				foreach (TinkerData item3 in ModRecipes)
				{
					if (text3 != "")
					{
						text3 += "\n";
					}
					text3 += item3.DisplayName;
				}
				Popup.Show(text3, CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
			if ((keys == Keys.Oemplus || keys == Keys.Add) && tinkerData != null && num4 + 6 < tinkerData.DescriptionLineCount)
			{
				num4++;
			}
			if ((keys == Keys.OemMinus || keys == Keys.Subtract) && num4 > 0)
			{
				num4--;
			}
			if (keys == Keys.NumPad8)
			{
				num4 = 0;
				if (num == num2)
				{
					if (num2 > 0)
					{
						num2--;
						num--;
					}
				}
				else if (num > 0)
				{
					num--;
				}
			}
			if (keys == Keys.NumPad2)
			{
				num4 = 0;
				int num8 = list2.Count - 1;
				if (text == "Mod")
				{
					num8 = TotalModLines - 1;
				}
				if (num < num8)
				{
					num++;
				}
				if (num - num2 >= num3 - 3)
				{
					num2++;
				}
			}
			if (keys == Keys.Prior)
			{
				num4 = 0;
				num = ((num != num2) ? num2 : (num2 = Math.Max(num2 - num3 + 3, 0)));
			}
			if (keys == Keys.Next)
			{
				num4 = 0;
				int val = ((text == "Mod") ? (TotalModLines - 1) : (list2.Count - 1));
				int num9 = num2 + num3 - 4;
				if (num == num9)
				{
					num = Math.Min(num + num3 - 3, val);
					num2 = Math.Max(num - num3 + 4, 0);
				}
				else
				{
					num = Math.Min(num9, val);
				}
			}
			if (text == "Mod" && (keys == Keys.Space || keys == Keys.Enter))
			{
				if (tinkerData == null || obj2 == null || !(obj2.Blueprint != "DummyTinkerScreenObject"))
				{
					goto IL_15f8;
				}
				GameObject player2 = The.Player;
				Inventory inventory = player2.Inventory;
				BodyPart bodyPart = null;
				GameObject gameObject2 = null;
				GameObject gameObject3 = null;
				if (!string.IsNullOrEmpty(tinkerData.Ingredient))
				{
					List<string> list4 = tinkerData.Ingredient.CachedCommaExpansion();
					foreach (string item4 in list4)
					{
						gameObject2 = inventory.FindObjectByBlueprint(item4, XRL.World.Parts.Temporary.IsNotTemporary);
						if (gameObject2 != null)
						{
							break;
						}
						if (gameObject3 == null)
						{
							gameObject3 = inventory.FindObjectByBlueprint(item4);
						}
					}
					if (gameObject2 == null)
					{
						if (gameObject3 != null)
						{
							Popup.ShowFail((gameObject3.HasProperName ? "" : "Your ") + gameObject3.ShortDisplayName + gameObject3.Is + " too unstable to craft with.");
						}
						else
						{
							string text4 = "";
							foreach (string item5 in list4)
							{
								if (text4 != "")
								{
									text4 += " or ";
								}
								text4 += TinkeringHelpers.TinkeredItemShortDisplayName(item5);
							}
							Popup.ShowFail("You don't have the required ingredient: " + text4 + "!");
						}
						goto IL_1623;
					}
				}
				bool flag5 = bitLocker.HasBits(bitCost);
				string sifrahItemModding = Options.SifrahItemModding;
				if (!flag5 && sifrahItemModding == "Never")
				{
					Popup.ShowFail("You don't have the required <" + bitCost.ToString() + "> bits! You have:\n\n " + bitLocker.GetBitsString());
				}
				else if (flag2 && GO.FireEvent("CombatPreventsTinkering"))
				{
					if (GO.IsPlayer())
					{
						Popup.ShowFail("You can't tinker with hostiles nearby!");
					}
				}
				else if (GO.CheckFrozen())
				{
					int num10 = 0;
					bool flag6 = false;
					if (sifrahItemModding == "Always")
					{
						flag6 = true;
					}
					else if (sifrahItemModding != "Never")
					{
						DialogResult dialogResult = Popup.ShowYesNoCancel("Do you want to play a game of Sifrah to mod " + obj2.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "? You can potentially improve the mod's performance and add capabilities to the item, and the cost of playing Sifrah will replace the normal modding cost." + (flag5 ? "" : (" You do not have the required <" + bitCost.ToString() + " bits to perform the mod normally.")));
						if (dialogResult == DialogResult.Yes)
						{
							flag6 = true;
						}
						else if (dialogResult == DialogResult.Cancel || (!flag5 && dialogResult == DialogResult.No))
						{
							goto IL_1623;
						}
					}
					if (flag6)
					{
						ItemModdingSifrah itemModdingSifrah = new ItemModdingSifrah(obj2, bitCost.GetHighestTier(), obj2.GetIntProperty("nMods"), The.Player.Stat("Intelligence"));
						itemModdingSifrah.Play(obj2);
						if (itemModdingSifrah.InterfaceExitRequested)
						{
							flag = true;
							break;
						}
						if (!itemModdingSifrah.ApplyMod)
						{
							goto IL_1623;
						}
						num10 = itemModdingSifrah.Performance;
					}
					bool flag7 = obj2.GetIntProperty("NeverStack") > 0;
					bool flag8 = false;
					try
					{
						if (obj2.Equipped != player2)
						{
							goto IL_142a;
						}
						bodyPart = player2.FindEquippedObject(obj2);
						if (bodyPart == null)
						{
							MetricsManager.LogError("could not find equipping part for " + obj2.Blueprint + " " + obj2.DebugName + " tracked as equipped on player");
						}
						else
						{
							if (!flag7 && !flag8)
							{
								obj2.SetIntProperty("NeverStack", 1);
								flag8 = true;
							}
							if (player2.FireEvent(Event.New("CommandUnequipObject", "BodyPart", bodyPart, "EnergyCost", 0)))
							{
								goto IL_142a;
							}
							Popup.ShowFail("You can't unequip " + obj2.the + obj2.ShortDisplayName + ".");
						}
						goto end_IL_1368;
						IL_142a:
						if (string.IsNullOrEmpty(tinkerData.Ingredient))
						{
							goto IL_1470;
						}
						gameObject2.SplitStack(1, player);
						if (inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject2)))
						{
							goto IL_1470;
						}
						Popup.ShowFail("You cannot use the ingredient!");
						goto end_IL_1368;
						IL_1470:
						if (!flag6)
						{
							bitLocker.UseBits(bitCost);
						}
						GameObject gameObject4 = obj2.SplitStack(1, player);
						if (gameObject4 != null)
						{
							list?.Add(gameObject4);
						}
						int num11 = obj2.GetTier();
						if (num10 != 0)
						{
							if (num10 > 0)
							{
								for (int n = 0; n < num10; n++)
								{
									if (num11 >= 8 || 10.in100())
									{
										RelicGenerator.ApplyBasicBestowal(obj2);
									}
									else
									{
										num11++;
									}
								}
							}
							else
							{
								num11 += num10;
							}
							num11 = Tier.Constrain(num11);
						}
						string text5 = obj2.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true);
						if (TechModding.ApplyModification(obj2, tinkerData.PartName, out var ModPart, num11, DoRegistration: true, The.Player))
						{
							obj2.MakeUnderstood();
							Popup.Show("You mod " + text5 + " to be {{C|" + (ModPart.GetModificationDisplayName() ?? tinkerData.DisplayName) + "}}.");
							ModdableItems = null;
						}
						if (obj2.Equipped == null && obj2.InInventory == null)
						{
							player2.ReceiveObject(obj2);
						}
						goto IL_15f8;
						end_IL_1368:;
					}
					catch (Exception x)
					{
						MetricsManager.LogError("Exception applying mod", x);
						goto IL_15f8;
					}
					finally
					{
						if (GameObject.validate(ref obj2))
						{
							if (bodyPart != null && bodyPart.Equipped == null)
							{
								player2.FireEvent(Event.New("CommandEquipObject", "Object", obj2, "BodyPart", bodyPart, "EnergyCost", 0));
							}
							if (flag8)
							{
								obj2.RemoveIntProperty("NeverStack");
							}
						}
					}
				}
			}
			goto IL_1623;
			IL_15f8:
			GO.UseEnergy(1000, "Skill Tinkering Mod");
			if (flag2)
			{
				flag = true;
				FromEvent?.RequestInterfaceExit();
			}
			goto IL_1623;
			IL_1623:
			if (!(text == "Build") || (keys != Keys.Space && keys != Keys.Enter) || tinkerData == null)
			{
				continue;
			}
			Inventory inventory2 = GO.Inventory;
			GameObject gameObject5 = null;
			bool flag9 = true;
			if (!string.IsNullOrEmpty(tinkerData.Ingredient))
			{
				string[] array3 = tinkerData.Ingredient.Split(',');
				string[] array2 = array3;
				foreach (string blueprint3 in array2)
				{
					gameObject5 = inventory2.FindObjectByBlueprint(blueprint3);
					if (gameObject5 != null)
					{
						break;
					}
				}
				if (gameObject5 == null)
				{
					string text6 = "";
					array2 = array3;
					foreach (string blueprint4 in array2)
					{
						if (text6 != "")
						{
							text6 += " or ";
						}
						text6 += TinkeringHelpers.TinkeredItemShortDisplayName(blueprint4);
					}
					Popup.ShowFail("You don't have the required ingredient: " + text6 + "!");
					flag9 = false;
				}
			}
			if (!flag9)
			{
				continue;
			}
			if (!bitLocker.HasBits(bitCost))
			{
				Popup.ShowFail("You don't have the required <" + bitCost.ToString() + "> bits! You have:\n\n" + bitLocker.GetBitsString());
			}
			else if (flag2 && GO.FireEvent("CombatPreventsTinkering"))
			{
				if (GO.IsPlayer())
				{
					Popup.ShowFail("You can't tinker with hostiles nearby!");
				}
			}
			else
			{
				if (!GO.CheckFrozen())
				{
					continue;
				}
				GameObject obj3 = GameObject.createSample(tinkerData.Blueprint);
				try
				{
					obj3.MakeUnderstood();
					bool Interrupt = false;
					int @for = GetTinkeringBonusEvent.GetFor(GO, obj3, "BonusMod", 0, 0, ref Interrupt);
					if (!Interrupt)
					{
						if (string.IsNullOrEmpty(tinkerData.Ingredient))
						{
							goto IL_181b;
						}
						gameObject5.SplitStack(1, player);
						if (inventory2.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject5)))
						{
							goto IL_181b;
						}
						Popup.ShowFail("You cannot use the ingredient!");
						gameObject5.CheckStack();
					}
					goto end_IL_17a5;
					IL_181b:
					bitLocker.UseBits(bitCost);
					TinkerItem part = obj3.GetPart<TinkerItem>();
					GameObject gameObject6 = null;
					for (int num12 = 0; num12 < Math.Max(part.NumberMade, 1); num12++)
					{
						gameObject6 = GameObject.create(tinkerData.Blueprint, 0, @for.in100() ? 1 : 0, "Tinkering");
						TinkeringHelpers.ProcessTinkeredItem(gameObject6);
						inventory2.AddObject(gameObject6);
					}
					if (part.NumberMade > 1)
					{
						Popup.Show("You tinker up " + Grammar.Cardinal(part.NumberMade) + " " + Grammar.Pluralize(obj3.ShortDisplayName) + "!");
					}
					else
					{
						Popup.Show("You tinker up " + gameObject6.a + gameObject6.ShortDisplayName + "!");
					}
					GO.UseEnergy(1000, "Skill Tinkering Build");
					if (flag2)
					{
						flag = true;
						FromEvent?.RequestInterfaceExit();
					}
					end_IL_17a5:;
				}
				finally
				{
					if (GameObject.validate(ref obj3))
					{
						obj3.Obliterate();
					}
				}
			}
		}
		GameManager.Instance.PopGameView();
		if (list == null)
		{
			switch (keys)
			{
			case Keys.NumPad7:
				return ScreenReturn.Previous;
			case Keys.NumPad9:
				return ScreenReturn.Next;
			}
		}
		return ScreenReturn.Exit;
	}
}

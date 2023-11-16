using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using Rewired;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.UI;

public class StatusScreen : IScreen
{
	public const int MAX_MUTATION_SHOW = 12;

	private static readonly Dictionary<char, string> RankBoostSigilColorCodes = new Dictionary<char, string>
	{
		{ '\0', "C" },
		{ '-', "R" },
		{ '+', "G" }
	};

	private static void WriteStat(ScreenBuffer SB, GameObject GO, string Stat, bool bSelected, int x, int y)
	{
		SB.Goto(x, y);
		if (bSelected)
		{
			SB.Write("> {{W|" + Stat + "}}");
		}
		else
		{
			SB.Write("  " + Stat);
		}
		SB.Goto(x + 15, y);
		Statistic statistic = GO.Statistics[Stat];
		string text = "C";
		if (statistic.Value > statistic.BaseValue)
		{
			text = "G";
		}
		else if (statistic.Value < statistic.BaseValue)
		{
			text = "r";
		}
		SB.Write("{{" + text + "|" + statistic.Value + "}}");
		if (statistic.Modifier > 0)
		{
			SB.Write(" {{c|(+" + statistic.Modifier + ")}}");
		}
		else
		{
			SB.Write(" {{c|(" + statistic.Modifier + ")}}");
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Status");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<BaseMutation> mutationList = GetMutationList(GO);
		string text = GO.genotypeEntry?.DisplayName;
		string text2 = GO.subtypeEntry?.DisplayName;
		Keys keys = Keys.None;
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string s = "< {{W|7}} Skills | Inventory {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Skills | Inventory {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(30, 0);
			scrapBuffer.Write("[ {{W|Character Sheet}} ]");
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
			if (TreatAsMutant(GO))
			{
				string @for = GetMutationTermEvent.GetFor(GO);
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.WriteAt(4, 24, " [{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}] Raise");
				}
				else
				{
					scrapBuffer.WriteAt(4, 24, " [{{W|space}}] Raise selected statistic or " + @for + " ");
				}
				scrapBuffer.Goto(3, 22);
				if (num == 0 && num2 == 7)
				{
					scrapBuffer.Write("{{Y|>}} ");
				}
				else
				{
					scrapBuffer.Write("  ");
				}
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.Write("Buy a new random " + @for + " for 4 MP");
				}
				else
				{
					scrapBuffer.Write("{{W|M}} - Buy a new random " + @for + " for 4 MP");
				}
				if (PronounSet.EnableSelection)
				{
					scrapBuffer.Goto(3, 23);
					if (num == 0 && num2 == 8)
					{
						scrapBuffer.Write("{{Y|>}} ");
					}
					else
					{
						scrapBuffer.Write("  ");
					}
					scrapBuffer.Write("{{W|P}} - Change pronoun set");
				}
			}
			else
			{
				scrapBuffer.Goto(4, 24);
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.Write(" [{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}] Raise");
				}
				else
				{
					scrapBuffer.Write(" [{{W|space}}] Raise selected statistic ");
				}
				if (PronounSet.EnableSelection)
				{
					scrapBuffer.Goto(3, 22);
					if (num == 0 && num2 == 7)
					{
						scrapBuffer.Write("{{Y|>}} ");
					}
					else
					{
						scrapBuffer.Write("  ");
					}
					scrapBuffer.Write("{{W|P}} - Change pronoun set");
				}
			}
			WriteStat(scrapBuffer, GO, "Strength", num == 0 && num2 == 0, 3, 2);
			WriteStat(scrapBuffer, GO, "Agility", num == 0 && num2 == 1, 3, 3);
			WriteStat(scrapBuffer, GO, "Toughness", num == 0 && num2 == 2, 3, 4);
			WriteStat(scrapBuffer, GO, "Intelligence", num == 0 && num2 == 3, 3, 5);
			WriteStat(scrapBuffer, GO, "Willpower", num == 0 && num2 == 4, 3, 6);
			WriteStat(scrapBuffer, GO, "Ego", num == 0 && num2 == 5, 3, 7);
			scrapBuffer.Goto(3, 9);
			Gender gender = GO.GetGender();
			if (gender != null && gender.Name != "nonspecific")
			{
				scrapBuffer.Write(Grammar.MakeTitleCase(gender.Name) + " ");
			}
			if (!text.IsNullOrEmpty())
			{
				scrapBuffer.Write(text + " ");
			}
			if (!text2.IsNullOrEmpty())
			{
				scrapBuffer.Write(text2 + " ");
			}
			scrapBuffer.Goto(3, 11);
			if (GO.Statistics.ContainsKey("AcidResistance"))
			{
				if (GO.Statistics["AcidResistance"].Value > 0)
				{
					scrapBuffer.Write("Acid Resist: {{G|" + GO.Statistics["AcidResistance"].Value + "}}");
				}
				else if (GO.Statistics["AcidResistance"].Value < 0)
				{
					scrapBuffer.Write("Acid Resist: {{R|" + GO.Statistics["AcidResistance"].Value + "}}");
				}
				else
				{
					scrapBuffer.Write("Acid Resist: " + GO.Statistics["AcidResistance"].Value);
				}
			}
			scrapBuffer.Goto(3, 12);
			if (GO.Statistics.ContainsKey("ColdResistance"))
			{
				if (GO.Statistics["ColdResistance"].Value > 0)
				{
					scrapBuffer.Write("Cold Resist: {{G|" + GO.Statistics["ColdResistance"].Value + "}}");
				}
				else if (GO.Statistics["ColdResistance"].Value < 0)
				{
					scrapBuffer.Write("Cold Resist: {{R|" + GO.Statistics["ColdResistance"].Value + "}}");
				}
				else
				{
					scrapBuffer.Write("Cold Resist: " + GO.Statistics["ColdResistance"].Value);
				}
			}
			scrapBuffer.Goto(21, 11);
			if (GO.Statistics.ContainsKey("ElectricResistance"))
			{
				if (GO.Statistics["ElectricResistance"].Value > 0)
				{
					scrapBuffer.Write("Electrical Resist: {{G|" + GO.Statistics["ElectricResistance"].Value + "}}");
				}
				else if (GO.Statistics["ElectricResistance"].Value < 0)
				{
					scrapBuffer.Write("Electrical Resist: {{R|" + GO.Statistics["ElectricResistance"].Value + "}}");
				}
				else
				{
					scrapBuffer.Write("Electrical Resist: " + GO.Statistics["ElectricResistance"].Value);
				}
			}
			scrapBuffer.Goto(21, 12);
			if (GO.Statistics.ContainsKey("HeatResistance"))
			{
				if (GO.Statistics["HeatResistance"].Value > 0)
				{
					scrapBuffer.Write("Heat Resist: {{G|" + GO.Statistics["HeatResistance"].Value + "}}");
				}
				else if (GO.Statistics["HeatResistance"].Value < 0)
				{
					scrapBuffer.Write("Heat Resist: {{R|" + GO.Statistics["HeatResistance"].Value + "}}");
				}
				else
				{
					scrapBuffer.Write("Heat Resist: " + GO.Statistics["HeatResistance"].Value);
				}
			}
			scrapBuffer.Goto(3, 14);
			if (num == 0 && num2 == 6)
			{
				scrapBuffer.Write("{{Y|>}} ");
			}
			else
			{
				scrapBuffer.Write("  ");
			}
			scrapBuffer.Write("active {{W|e}}ffects [{{C|" + XRLCore.Core.Game.Player.Body.Effects.Count((Effect e) => e != null && !string.IsNullOrEmpty(e.GetDescription())) + "}}]");
			int num4 = 0;
			int num5 = 2;
			bool flag2 = mutationList != null && mutationList.Count > 0;
			int psychicGlimmer = GO.GetPsychicGlimmer();
			if (flag2)
			{
				GetMutationTermEvent.GetFor(The.Player, out var Term, out var Color);
				string text3 = ColorUtility.CapitalizeExceptFormatting(Grammar.Pluralize(Term));
				scrapBuffer.WriteAt(45, 2, "[ {{" + Color + "|" + text3 + "}} ]");
				num5 = 4;
				if (num3 > 0)
				{
					scrapBuffer.Goto(45, 3);
					scrapBuffer.Write("{{W|<more...>}}");
				}
				int num6 = 4;
				int i;
				for (i = num3; i < mutationList.Count; i++)
				{
					if (num6 >= 16)
					{
						break;
					}
					BaseMutation baseMutation = mutationList[i];
					int level = baseMutation.Level;
					num5++;
					scrapBuffer.Goto(45, num6);
					if (num == 1 && num2 == i)
					{
						scrapBuffer.Write("{{Y|>}} ");
					}
					else
					{
						scrapBuffer.Write("  ");
					}
					if (!baseMutation.ShouldShowLevel())
					{
						scrapBuffer.Write(baseMutation.DisplayName);
					}
					else
					{
						string value = "C";
						if (level > baseMutation.BaseLevel)
						{
							value = ((level <= baseMutation.GetMutationCap()) ? "G" : "M");
						}
						else if (level < baseMutation.BaseLevel)
						{
							value = "R";
						}
						scrapBuffer.Write(Event.NewStringBuilder().Append(baseMutation.DisplayName).Append(" ({{")
							.Append(value)
							.Append('|')
							.Append(level)
							.Append("}})")
							.ToString());
					}
					num4++;
					num6++;
				}
				if (i < mutationList.Count)
				{
					scrapBuffer.Goto(45, num6);
					scrapBuffer.Write("{{W|<more...>}}");
				}
				if (PsychicGlimmer.Perceptible(psychicGlimmer))
				{
					if (num == 1 && num2 == mutationList.Count)
					{
						scrapBuffer.Goto(45, num6 + 1);
						scrapBuffer.Write(">");
					}
					scrapBuffer.Goto(47, num6 + 1);
					scrapBuffer.Write("Psychic Glimmer ({{R|" + psychicGlimmer + "}})");
					num5++;
				}
				num5 += 2;
			}
			if (!flag2 && PsychicGlimmer.Perceptible(psychicGlimmer))
			{
				scrapBuffer.Goto(45, num5);
				scrapBuffer.Write("[ {{M|Esoterica}} ]");
				if (num == 1 && num2 == num4)
				{
					scrapBuffer.Goto(45, num5 + 2);
					scrapBuffer.Write(">");
				}
				scrapBuffer.Goto(47, num5 + 2);
				scrapBuffer.Write("Psychic Glimmer ({{R|" + psychicGlimmer + "}})");
			}
			string text4 = "";
			text4 = ((XRLCore.Core.Game.Player.Body.GetIntProperty("Analgesia") <= 0) ? ("{{C|" + GO.hitpoints + "}} / {{C|" + GO.baseHitpoints + "}}") : Strings.WoundLevel(XRLCore.Core.Game.Player.Body));
			scrapBuffer.WriteAt(3, 16, "HP: " + text4);
			if (GO.HasStat("Level"))
			{
				scrapBuffer.WriteAt(3, 17, "Level: {{C|" + GO.Stat("Level") + "}}");
			}
			if (GO.HasStat("XP") && GO.HasStat("Level"))
			{
				scrapBuffer.WriteAt(3, 18, "XP: {{C|" + GO.Stat("XP") + "}} {{K|({{c|" + (Leveler.GetXPForLevel(GO.Stat("Level") + 1) - GO.Stat("XP")) + "}} till next level)}}");
			}
			string text5 = "G";
			string text6 = "G";
			string text7 = "G";
			if (GO.Stat("SP") == 0)
			{
				text5 = "K";
			}
			if (GO.Stat("AP") == 0)
			{
				text6 = "K";
			}
			if (GO.Stat("MP") == 0)
			{
				text7 = "K";
			}
			if (GO.HasStat("SP") && GO.HasStat("AP"))
			{
				if (TreatAsMutant(GO))
				{
					scrapBuffer.WriteAt(3, 20, "Skill points: {{" + text5 + "|" + GO.Stat("SP") + "}}, Attribute points: {{" + text6 + "|" + GO.Stat("AP") + "}}, Mutation points: {{" + text7 + "|" + GO.Stat("MP") + "}}");
				}
				else
				{
					scrapBuffer.WriteAt(3, 20, "Skill points: {{" + text5 + "|" + GO.Stat("SP") + "}}, Attribute points: {{" + text6 + "|" + GO.Stat("AP") + "}}");
				}
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
			switch (num)
			{
			case 3:
				if (keys == Keys.NumPad4)
				{
					num2 = 0;
					num = 0;
				}
				if (keys == Keys.NumPad8)
				{
					num = 1;
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					Popup.Show("TODOJASON GLIMMER=" + GO.GetPsychicGlimmer());
				}
				break;
			case 1:
				if (keys == Keys.NumPad4)
				{
					num = 0;
					num2 = 0;
				}
				if (keys == Keys.NumPad8)
				{
					if (num2 > 0)
					{
						num2--;
					}
					if (num3 > num2)
					{
						num3 = num2;
					}
				}
				if (keys == Keys.NumPad2)
				{
					if (num2 < mutationList.Count - 1)
					{
						num2++;
						if (num3 + 12 <= num2)
						{
							num3 = num2 - 12 + 1;
						}
					}
					else if (num2 == mutationList.Count - 1 && PsychicGlimmer.Perceptible(psychicGlimmer))
					{
						num2++;
					}
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					if (num2 >= mutationList.Count)
					{
						Popup.Show(PsychicHunterSystem.GetPsychicGlimmerDescription(psychicGlimmer), CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
					}
					else
					{
						ShowMutationPopup(GO, mutationList[num2]);
					}
				}
				break;
			}
			if (num == 0)
			{
				if (keys == Keys.NumPad6 && (mutationList.Count > 0 || PsychicGlimmer.Perceptible(psychicGlimmer)))
				{
					num = 1;
					num2 = 0;
				}
				if (keys == Keys.NumPad8 && num2 > 0)
				{
					num2--;
				}
				if (keys == Keys.NumPad2)
				{
					int num7 = 6;
					if (TreatAsMutant(GO))
					{
						num7++;
					}
					if (PronounSet.EnableSelection)
					{
						num7++;
					}
					if (num2 < num7)
					{
						num2++;
					}
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					string text8 = num2 switch
					{
						0 => "Strength", 
						1 => "Agility", 
						2 => "Toughness", 
						3 => "Intelligence", 
						4 => "Willpower", 
						5 => "Ego", 
						_ => "", 
					};
					if (text8 != "")
					{
						Statistic statistic = GO.Statistics[text8];
						int baseValue = statistic.BaseValue;
						int value2 = statistic.Value;
						string text9 = ((value2 == baseValue) ? ("Your " + text8 + " is {{C|" + baseValue + "}}") : ((value2 <= baseValue) ? ("Your base " + text8 + " is {{C|" + baseValue + "}}, modified to {{R|" + value2 + "}}") : ("Your base " + text8 + " is {{C|" + baseValue + "}}, modified to {{G|" + value2 + "}}")));
						text9 += ".\n\n";
						if (statistic.BaseValue >= 100)
						{
							Popup.Show(text9 + "You may not raise an attribute above 100.");
						}
						else if (GO.Stat("AP") >= 1)
						{
							if (Popup.ShowYesNo(text9 + "It will cost {{C|1}} attribute point to increase " + text8 + " by 1.\nDo you wish to increase this attribute?") == DialogResult.Yes)
							{
								GO.GetStat("AP").Penalty++;
								statistic.BaseValue++;
								Popup.Show("You have increased your " + text8 + " to {{C|" + statistic.Value + "}}!");
								MetricsManager.LogEvent("Gameplay:Statistic:Purchase:" + text8);
							}
						}
						else
						{
							Popup.Show(text9 + "You have no attribute points to raise this attribute.");
						}
					}
				}
			}
			if (keys == Keys.E || ((keys == Keys.Space || keys == Keys.Enter) && num == 0 && num2 == 6))
			{
				The.Player.ShowActiveEffects();
			}
			if ((keys == Keys.P || ((keys == Keys.Space || keys == Keys.Enter) && num == 0 && num2 == (GO.IsTrueKin() ? 7 : 8))) && PronounSet.EnableSelection)
			{
				PronounAndGenderSets.ShowChangePronounSet(The.Player);
			}
			GO.Stat("MP");
			if ((keys == Keys.M || ((keys == Keys.Space || keys == Keys.Enter) && num == 0 && TreatAsMutant(GO) && num2 == 7)) && MutationsAPI.BuyRandomMutation(GO))
			{
				mutationList = GetMutationList(GO);
			}
			switch (keys)
			{
			case Keys.NumPad7:
			case Keys.NumPad9:
				flag = true;
				break;
			case Keys.Escape:
			case Keys.NumPad5:
				flag = true;
				break;
			}
		}
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

	public static bool TreatAsMutant(GameObject GO)
	{
		if (!GO.IsMutant())
		{
			return GO.Stat("MP") > 0;
		}
		return true;
	}

	public static List<BaseMutation> GetMutationList(GameObject GO)
	{
		return GO.GetPartsDescendedFrom((BaseMutation m) => m.Level > 0);
	}

	public static void ShowMutationPopup(GameObject GO, BaseMutation Mutation)
	{
		bool num = TreatAsMutant(GO) && Mutation.BaseLevel > 0;
		string @for = GetMutationTermEvent.GetFor(GO, Mutation);
		bool flag = !Mutation.CanLevel() || Mutation.BaseLevel >= Mutation.GetMaxLevel();
		StringBuilder stringBuilder = Event.NewStringBuilder(Mutation.GetDescription());
		if (!num || flag)
		{
			stringBuilder.Compound(Mutation.GetLevelText(Mutation.Level), "\n\n");
		}
		else
		{
			stringBuilder.Compound("{{w|This rank}}:\n", "\n\n").Append(Mutation.GetLevelText(Mutation.Level)).Append("\n\n{{w|Next rank}}:\n")
				.Append(Mutation.GetLevelText(Mutation.Level + 1));
		}
		stringBuilder.Append('\n', 2);
		AppendRankBoost(stringBuilder, GO, Mutation);
		if (num)
		{
			if (!flag && Mutation.Level < Mutation.GetMutationCap())
			{
				if (GO.Stat("MP") >= 1)
				{
					stringBuilder.Append("It will cost {{C|1}} mutation point to increase ").Append(Mutation.DisplayName).Append("'s rank by 1.\nDo you wish to increase this ")
						.Append(Grammar.MakePossessive(@for))
						.Append(" rank?");
					if (Popup.ShowYesNo(stringBuilder.ToString()) != 0)
					{
						return;
					}
					GO.UseMP(1);
					GO.GetPart<Mutations>().LevelMutation(Mutation, Mutation.BaseLevel + 1);
					stringBuilder.Clear().Append("You have increased ").Append(Grammar.MakePossessive(Mutation.DisplayName))
						.Append(" base rank to {{C|")
						.Append(Mutation.BaseLevel)
						.Append("}}!")
						.Append('\n', 2);
					AppendRankBoost(stringBuilder, GO, Mutation);
				}
				else
				{
					stringBuilder.Append("{{C|You do not have enough mutation points to increase that " + Grammar.MakePossessive(@for) + " rank.}}");
				}
			}
			else if (Mutation.CanLevel())
			{
				stringBuilder.Append("{{C|You may not advance this " + Grammar.MakePossessive(@for) + " rank yet.}}");
			}
		}
		Popup.Show(stringBuilder.ToString().TrimEnd(), CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
	}

	private static void AppendRankBoost(StringBuilder SB, GameObject GO, BaseMutation Mutation)
	{
		foreach (BaseMutation.LevelCalculation levelCalculation in Mutation.GetLevelCalculations())
		{
			if (!string.IsNullOrEmpty(levelCalculation.reason))
			{
				SB.AppendMarkupNode(RankBoostSigilColorCodes[levelCalculation.sigil], levelCalculation.reason);
				SB.AppendLine();
			}
		}
		if (!Mutation.GetLevelCalculations().IsNullOrEmpty())
		{
			SB.AppendLine();
		}
	}

	public static bool BuyRandomMutation(GameObject GO)
	{
		Mutations part = GO.GetPart<Mutations>();
		List<MutationEntry> Selections = part.GetMutatePool();
		GO.WithSeededRandom((Random rng) => Selections.ShuffleInPlace(rng), "RandomMutationBuy");
		int TargetSelectionCount = GlobalConfig.GetIntSetting("RandomBuyMutationCount", 3);
		TargetSelectionCount = GetRandomBuyMutationCountEvent.GetFor(GO, TargetSelectionCount);
		if (Selections.Count > TargetSelectionCount)
		{
			List<MutationEntry> list = new List<MutationEntry>(Selections.Where((MutationEntry e) => e.Cost >= 2));
			if (list.Count < TargetSelectionCount)
			{
				int filler = TargetSelectionCount - list.Count;
				list.AddRange(Selections.Where((MutationEntry e) => e.Cost < 2 && filler-- > 0));
			}
			Selections = list;
		}
		if (Selections.Count > TargetSelectionCount)
		{
			Selections.RemoveRange(TargetSelectionCount, Selections.Count - TargetSelectionCount);
		}
		List<int> extraLimb = new List<int>();
		if (GO.IsChimera())
		{
			int @for = GetRandomBuyChimericBodyPartRollsEvent.GetFor(GO, 1);
			for (int i = 0; i < @for; i++)
			{
				GO.WithSeededRandom(delegate(Random rng)
				{
					extraLimb.Add(rng.Next(0, TargetSelectionCount));
				}, "RandomMutationBuy");
			}
		}
		string for2 = GetMutationTermEvent.GetFor(GO);
		if (Selections.Count > 0)
		{
			string[] array = new string[Selections.Count];
			List<BaseMutation> list2 = new List<BaseMutation>(Selections.Count);
			for (int j = 0; j < Selections.Count; j++)
			{
				list2.Add(Selections[j].CreateInstance());
				array[j] = "{{W|" + list2[j].DisplayName + "}}" + (extraLimb.Contains(j) ? "{{G| + grow a new body part}}" : "") + " {{y|- " + list2[j].GetDescription() + "}}\n" + list2[j].GetLevelText(1);
			}
			int num;
			for (num = -1; num < 0; num = Popup.ShowOptionList("", array, null, 1, "Choose " + Grammar.A(for2) + ".", 78))
			{
			}
			if (!GO.HasEffect("Dominated"))
			{
				JournalAPI.AddAccomplishment("Your genome destabilized and you gained the " + list2[num].DisplayName + " " + for2 + ".", HistoricStringExpander.ExpandString("<spice.commonPhrases.oneStarryNight.!random.capitalize>, =name= manifested a latent power inside " + The.Player.GetPronounProvider().Objective + " and joined the divine ranks of " + list2[num].GetBearerDescription() + "."), "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
			if (list2[num].GetVariants() != null)
			{
				int num2 = Popup.ShowOptionList("Choose a variant for this " + for2 + ".", list2[num].GetVariants().ToArray());
				if (num2 > 0)
				{
					list2[num].SetVariant(num2);
				}
			}
			int num3 = part.AddMutation(list2[num], 1);
			string displayName = list2[num].DisplayName;
			if (num3 < part.MutationList.Count)
			{
				displayName = part.MutationList[num3].DisplayName;
			}
			Popup.Show("You gain " + displayName + "!");
			if (extraLimb.Contains(num))
			{
				part.AddChimericBodyPart();
			}
			MetricsManager.LogEvent("Gameplay:Mutation:Purchase:" + list2[num].DisplayName);
			return true;
		}
		Popup.Show("You have all available " + Grammar.Pluralize(for2) + ".");
		return false;
	}
}

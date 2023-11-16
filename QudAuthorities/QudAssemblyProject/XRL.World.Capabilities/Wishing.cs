using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Qud.API;
using Sheeter;
using UnityEngine;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.QuestManagers;
using XRL.World.Skills.Cooking;
using XRL.World.Tinkering;
using XRL.World.ZoneBuilders;

namespace XRL.World.Capabilities;

public static class Wishing
{
	public static void HandleWish(GameObject who, string Wish)
	{
		if (WishManager.HandleWish(Wish))
		{
			return;
		}
		XRLGame game = The.Game;
		if (Wish.StartsWith("convnode:"))
		{
			GameObject gameObject = null;
			string[] array = Wish.Split(':');
			OldConversationUI.HaveConversation(Speaker: (array.Length < 4) ? GameObjectFactory.Factory.CreateObject("BaseHumanoid") : GameObjectFactory.Factory.CreateObject(array[3]), ConversationID: array[1], TradeEnabled: true, bCheckObjectTalking: true, startNode: array[2]);
			return;
		}
		if (Wish.StartsWith("conv:"))
		{
			GameObject gameObject2 = null;
			string[] array2 = Wish.Split(':');
			OldConversationUI.HaveConversation(Speaker: (array2.Length < 3) ? GameObjectFactory.Factory.CreateObject("BaseHumanoid") : GameObjectFactory.Factory.CreateObject(array2[2]), ConversationID: array2[1]);
			return;
		}
		if (Wish.StartsWith("startquest:"))
		{
			The.Game.StartQuest(Wish.Split(':')[1]);
			return;
		}
		if (Wish.StartsWith("completequest:"))
		{
			The.Game.CompleteQuest(Wish.Split(':')[1]);
			return;
		}
		if (Wish.StartsWith("questdebug"))
		{
			while (true)
			{
				List<string> list = new List<string>();
				List<Quest> list2 = new List<Quest>();
				foreach (Quest value3 in The.Game.Quests.Values)
				{
					if (!The.Game.FinishedQuests.ContainsKey(value3.ID))
					{
						list2.Add(value3);
						list.Add(value3.Name);
					}
				}
				int num = Popup.ShowOptionList("<Quest Debug>", list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num < 0)
				{
					break;
				}
				while (true)
				{
					list = new List<string> { "info", "complete", "steps" };
					int num2 = Popup.ShowOptionList("Debug: " + list2[num].Name, list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
					if (num2 == 0)
					{
						Popup.Show(list2[num].ToString());
						break;
					}
					if (num2 == 1)
					{
						game.CompleteQuest(list2[num].ID);
						break;
					}
					if (num2 != 2)
					{
						break;
					}
					list = new List<string>();
					List<QuestStep> list3 = new List<QuestStep>();
					foreach (KeyValuePair<string, QuestStep> item in list2[num].StepsByID)
					{
						list3.Add(item.Value);
						list.Add(item.Key);
					}
					int num3 = Popup.ShowOptionList("Pick step from " + list2[num].Name, list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
					if (num3 < 0)
					{
						continue;
					}
					int num4;
					while (true)
					{
						list = new List<string> { "info", "finish" };
						num4 = Popup.ShowOptionList("Debug step " + list3[num3], list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						if (num4 != 0)
						{
							break;
						}
						Popup.Show(list3[num3].ToString());
					}
					if (num4 == 1)
					{
						game.FinishQuestStep(list2[num].ID, list3[num3].ID);
					}
				}
			}
			return;
		}
		if (Wish.StartsWith("finishallquests"))
		{
			foreach (KeyValuePair<string, Quest> quest in The.Game.Quests)
			{
				quest.Value.Finish();
			}
			return;
		}
		if (Wish.StartsWith("finishqueststep:"))
		{
			The.Game.FinishQuestStep(Wish.Split(':')[1], Wish.Split(':')[2]);
			return;
		}
		if (Wish.StartsWith("geno:"))
		{
			Zone parentZone = who.CurrentCell.ParentZone;
			string cmp = Wish.Split(':')[1];
			for (int i = 0; i < parentZone.Height; i++)
			{
				for (int j = 0; j < parentZone.Width; j++)
				{
					foreach (GameObject item2 in parentZone.GetCell(j, i).GetObjectsWithPart("Physics"))
					{
						if (item2.Blueprint.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
						else if (item2.DisplayNameOnlyStripped.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
						else if (item2.DisplayNameStripped.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
					}
				}
			}
			return;
		}
		if (Wish.StartsWith("deathgeno:"))
		{
			Zone parentZone2 = who.CurrentCell.ParentZone;
			string cmp2 = Wish.Split(':')[1];
			for (int k = 0; k < parentZone2.Height; k++)
			{
				for (int l = 0; l < parentZone2.Width; l++)
				{
					foreach (GameObject item3 in parentZone2.GetCell(l, k).GetObjectsWithPart("Physics"))
					{
						if (item3.Blueprint.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
						else if (item3.DisplayNameOnlyStripped.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
						else if (item3.DisplayNameStripped.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
					}
				}
			}
			return;
		}
		if (Wish.StartsWith("allaggroxp"))
		{
			int num5 = 0;
			Zone parentZone3 = who.CurrentCell.ParentZone;
			for (int m = 0; m < parentZone3.Height; m++)
			{
				for (int n = 0; n < parentZone3.Width; n++)
				{
					foreach (GameObject item4 in parentZone3.GetCell(n, m).GetObjectsWithPart("Brain"))
					{
						if (item4.pBrain.IsHostileTowards(The.Player))
						{
							num5 += item4.AwardXPTo(The.Player, ForKill: true, null, MockAward: true);
						}
					}
				}
			}
			MessageQueue.AddPlayerMessage("Total xp: " + num5);
			return;
		}
		if (Wish.StartsWith("testhero:"))
		{
			GameObject gameObject3 = GameObjectFactory.Factory.CreateObject(Wish.Split(':')[1]);
			HeroMaker.MakeHero(gameObject3);
			who.pPhysics.CurrentCell.GetCellFromDirection("E").AddObject(gameObject3).MakeActive();
			return;
		}
		if (Wish.StartsWith("animatedhero"))
		{
			GameObject anAnimatedObject = EncountersAPI.GetAnAnimatedObject();
			HeroMaker.MakeHero(anAnimatedObject);
			who.pPhysics.CurrentCell.GetCellFromDirection("E").AddObject(anAnimatedObject).MakeActive();
			return;
		}
		if (Wish.StartsWith("showstringproperty:"))
		{
			string text = Wish.Split(':')[1];
			if (who.HasStringProperty(text))
			{
				Popup.Show(who.GetStringProperty(text));
			}
			else
			{
				Popup.Show("no string property '" + text + "' found");
			}
			return;
		}
		if (Wish.StartsWith("setintproperty "))
		{
			string[] array3 = Wish.Split(' ');
			who.SetIntProperty(array3[1], Convert.ToInt32(array3[2]));
			return;
		}
		if (Wish.StartsWith("pushgameview "))
		{
			string newView = Wish.Split(' ')[1];
			GameManager.Instance.PushGameView(newView);
			return;
		}
		if (Wish.StartsWith("showintproperty:"))
		{
			string text2 = Wish.Split(':')[1];
			if (who.HasIntProperty(text2))
			{
				Popup.Show(who.GetIntProperty(text2).ToString());
			}
			else
			{
				Popup.Show("no int property '" + text2 + "' found");
			}
			return;
		}
		switch (Wish)
		{
		case "fire":
		{
			Cell cell2 = who.pPhysics.PickDirection();
			if (cell2 != null)
			{
				GameObject gameObject4 = cell2.GetCombatTarget(who, IgnoreFlight: true) ?? cell2.GetCombatTarget(who, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (gameObject4 != null && gameObject4.pPhysics != null)
				{
					gameObject4.pPhysics.Temperature = gameObject4.pPhysics.FlameTemperature + 200;
				}
			}
			return;
		}
		case "zap":
		{
			Cell cell = who.pPhysics.PickDirection();
			if (cell != null)
			{
				who.Discharge(cell, 3, 20, who);
			}
			return;
		}
		case "gates":
		{
			List<List<string>> list4 = The.Game.GetObjectGameState("JoppaWorldTeleportGate2Rings") as List<List<string>>;
			List<List<string>> list5 = The.Game.GetObjectGameState("JoppaWorldTeleportGate3Rings") as List<List<string>>;
			List<List<string>> list6 = The.Game.GetObjectGameState("JoppaWorldTeleportGate4Rings") as List<List<string>>;
			List<string> list7 = The.Game.GetObjectGameState("JoppaWorldTeleportGateSecants") as List<string>;
			List<string> list8 = The.Game.GetObjectGameState("JoppaWorldTeleportGateZones") as List<string>;
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			if (list4 != null && list4.Count > 0)
			{
				stringBuilder2.Compound("2-rings:\n", "\n");
				foreach (List<string> item5 in list4)
				{
					stringBuilder2.Append('\n').Append(The.ZoneManager.GetZoneProperty(item5[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item5[0])
						.Append(The.ZoneManager.HasZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item5[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item5[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item5[1])
						.Append(The.ZoneManager.HasZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item5[1]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder2.Compound("No 2-rings found!\n", '\n');
			}
			if (list5 != null && list5.Count > 0)
			{
				stringBuilder2.Compound("3-rings:\n", "\n");
				foreach (List<string> item6 in list5)
				{
					stringBuilder2.Append('\n').Append(The.ZoneManager.GetZoneProperty(item6[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item6[0])
						.Append(The.ZoneManager.HasZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item6[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item6[1])
						.Append(The.ZoneManager.HasZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[1]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item6[2], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item6[2])
						.Append(The.ZoneManager.HasZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[2]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder2.Compound("No 3-rings found!\n", '\n');
			}
			if (list6 != null && list6.Count > 0)
			{
				stringBuilder2.Compound("4-rings:\n", "\n");
				foreach (List<string> item7 in list6)
				{
					stringBuilder2.Append('\n').Append(The.ZoneManager.GetZoneProperty(item7[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item7[0])
						.Append(The.ZoneManager.HasZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[1])
						.Append(The.ZoneManager.HasZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[1]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[2], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[2])
						.Append(The.ZoneManager.HasZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[2]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[3], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[3])
						.Append(The.ZoneManager.HasZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[3]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder2.Compound("No 4-rings found!\n", '\n');
			}
			if (list7 != null && list7.Count > 0)
			{
				stringBuilder2.Compound("Secants:\n", "\n");
				foreach (string item8 in list7)
				{
					stringBuilder2.Append('\n').Append(item8).Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item8))
						.Append('\n');
					string text3 = The.ZoneManager.GetZoneProperty(item8, "TeleportGateDestinationZone") as string;
					if (!string.IsNullOrEmpty(text3))
					{
						stringBuilder2.Append("to ").Append(text3).Append(' ')
							.Append(The.ZoneManager.GetZoneDisplayName(text3))
							.Append('\n');
					}
					else
					{
						stringBuilder2.Append("no destination set\n");
					}
				}
			}
			else
			{
				stringBuilder2.Compound("No secants found!\n", '\n');
			}
			if (list8 != null && list8.Count > 0)
			{
				stringBuilder2.Compound(list8.Count, "\n").Append(" total gate zones in the above:\n\n");
				foreach (string item9 in list8)
				{
					stringBuilder2.Append(item9).Append(' ').Append(The.ZoneManager.GetZoneDisplayName(item9))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder2.Compound("No gate zones found!\n", '\n');
			}
			Popup.Show(stringBuilder2.ToString());
			return;
		}
		case "sparks":
			who.Sparksplatter();
			return;
		case "groundliquid":
		{
			Cell currentCell = game.Player.Body.CurrentCell;
			if (currentCell != null)
			{
				Popup.Show("[" + (currentCell.GroundLiquid ?? "null") + "]");
			}
			return;
		}
		case "testmarkup":
			MessageQueue.AddPlayerMessage(Markup.Transform("{{blue|blue blue blue blue blue blue blue blue blue blue blue {{rainbow|rainbow rainbow {{random|random1}} {{random|random2}} rainbow rainbow rainbow}} blue blue}} gray gray"));
			return;
		case "showcooldownminima":
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			int num6 = 20;
			int num7 = 10;
			while (num6 <= 600)
			{
				int num8 = ActivatedAbilities.MinimumValueForCooldown(num6);
				stringBuilder.Append(num6).Append(": ").Append(num8)
					.Append(" (")
					.Append((int)Math.Round((double)num8 * 100.0 / (double)num6, MidpointRounding.AwayFromZero))
					.Append("%)\n");
				num6 += num7;
				num7 += 10;
			}
			Popup.Show(stringBuilder.ToString());
			return;
		}
		}
		if (Wish.StartsWith("find:"))
		{
			string text4 = Wish.Split(':')[1];
			StringBuilder stringBuilder3 = Event.NewStringBuilder();
			List<GameObject> objects = who.pPhysics.CurrentCell.ParentZone.GetObjects(text4);
			if (objects.Count == 0)
			{
				stringBuilder3.Append("no ").Append(text4).Append(" found in zone");
			}
			else
			{
				foreach (GameObject item10 in objects)
				{
					stringBuilder3.Append(item10.pPhysics.CurrentCell.X).Append(' ').Append(item10.pPhysics.CurrentCell.Y)
						.Append('\n');
				}
			}
			Popup.Show(stringBuilder3.ToString());
			return;
		}
		if (Wish == "testcardinal")
		{
			StringBuilder stringBuilder4 = Event.NewStringBuilder();
			for (int num9 = 0; num9 <= 150; num9++)
			{
				stringBuilder4.Append(num9).Append(": ").Append(Grammar.Cardinal(num9))
					.Append('\n');
			}
			Popup.Show(stringBuilder4.ToString());
			return;
		}
		if (Wish == "testordinal")
		{
			StringBuilder stringBuilder5 = Event.NewStringBuilder();
			for (int num10 = 0; num10 <= 150; num10++)
			{
				stringBuilder5.Append(num10).Append(": ").Append(Grammar.Ordinal(num10))
					.Append('\n');
			}
			Popup.Show(stringBuilder5.ToString());
			return;
		}
		if (Wish == "testpets")
		{
			bool flag = false;
			foreach (GameObjectBlueprint item11 in GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant")))
			{
				GameObject gameObject5 = GameObjectFactory.Factory.CreateObject(item11.Name);
				if (gameObject5.DisplayName.StartsWith("["))
				{
					Popup.Show(gameObject5.Blueprint + ": " + gameObject5.DisplayName);
					flag = true;
				}
			}
			if (!flag)
			{
				Popup.Show("No problems found.");
			}
			return;
		}
		if (Wish == "testobjects")
		{
			bool flag2 = false;
			foreach (GameObjectBlueprint item12 in GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters")))
			{
				GameObject gameObject6 = GameObjectFactory.Factory.CreateObject(item12.Name);
				if (gameObject6.DisplayName.StartsWith("["))
				{
					Popup.Show(gameObject6.Blueprint + ": " + gameObject6.DisplayName);
					flag2 = true;
				}
			}
			if (!flag2)
			{
				Popup.Show("No problems found.");
			}
			return;
		}
		if (Wish == "showgenders")
		{
			List<Gender> all = Gender.GetAll();
			StringBuilder stringBuilder6 = Event.NewStringBuilder();
			for (int num11 = 0; num11 < all.Count; num11++)
			{
				stringBuilder6.Length = 0;
				stringBuilder6.Append(num11 + 1).Append('/').Append(all.Count)
					.Append("\n\n");
				all[num11].GetSummary(stringBuilder6);
				Popup.Show(stringBuilder6.ToString());
			}
			return;
		}
		if (Wish == "showmygender")
		{
			Popup.Show(who.GetGender().GetSummary());
			return;
		}
		if (Wish == "showpronounsets")
		{
			List<PronounSet> all2 = PronounSet.GetAll();
			StringBuilder stringBuilder7 = Event.NewStringBuilder();
			for (int num12 = 0; num12 < all2.Count; num12++)
			{
				stringBuilder7.Length = 0;
				stringBuilder7.Append(num12 + 1).Append('/').Append(all2.Count)
					.Append("\n\n");
				all2[num12].GetSummary(stringBuilder7);
				Popup.Show(stringBuilder7.ToString());
			}
			return;
		}
		if (Wish == "powergrid")
		{
			new PowerGrid().BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "powergriddebug")
		{
			PowerGrid powerGrid = new PowerGrid();
			powerGrid.ShowPathfinding = true;
			powerGrid.ShowPathWeights = true;
			powerGrid.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "powergridruin")
		{
			PowerGrid powerGrid2 = new PowerGrid();
			powerGrid2.DamageChance = "20-40";
			powerGrid2.DamageIsBreakageChance = "30-80";
			powerGrid2.MissingConsumers = "2-15";
			powerGrid2.MissingProducers = "2-5";
			powerGrid2.Noise = ((Stat.Random(0, 1) == 0) ? Stat.Random(0, 10) : Stat.Random(0, 80));
			powerGrid2.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulics")
		{
			new Hydraulics().BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "1hp")
		{
			Statistic stat = The.Player.GetStat("Hitpoints");
			stat.Penalty += stat.Value - 1;
			return;
		}
		if (Wish == "hydraulicsmetal")
		{
			Hydraulics hydraulics = new Hydraulics();
			hydraulics.ConduitBlueprint = "MetalHydraulicPipe";
			hydraulics.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsplastic")
		{
			Hydraulics hydraulics2 = new Hydraulics();
			hydraulics2.ConduitBlueprint = "PlasticHydraulicPipe";
			hydraulics2.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsglass")
		{
			Hydraulics hydraulics3 = new Hydraulics();
			hydraulics3.ConduitBlueprint = "GlassHydraulicPipe";
			hydraulics3.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsruin")
		{
			Hydraulics hydraulics4 = new Hydraulics();
			hydraulics4.DamageChance = "10-20";
			hydraulics4.DamageIsBreakageChance = "30-80";
			hydraulics4.MissingConsumers = "1-8";
			hydraulics4.MissingProducers = "1-3";
			hydraulics4.Noise = ((Stat.Random(0, 1) == 0) ? Stat.Random(0, 10) : Stat.Random(0, 80));
			hydraulics4.BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "mechpower")
		{
			new MechanicalPower().BuildZone(who.pPhysics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "showrandomfaction")
		{
			Popup.Show(Factions.GetRandomFaction().Name);
			return;
		}
		if (Wish == "showrandomoldfaction")
		{
			Popup.Show(Factions.GetRandomOldFaction().Name);
			return;
		}
		if (Wish == "showrandomfactionexceptbeasts")
		{
			Popup.Show(Factions.GetRandomFaction("Beasts").Name);
			return;
		}
		if (Wish == "showrandomfactionshortname")
		{
			Popup.Show(Factions.GetRandomFaction((Faction f) => f.Name.Length <= 8).Name);
			return;
		}
		if (Wish == "teststringbuilder")
		{
			Action<StringBuilder, string> obj = delegate(StringBuilder sb, string s)
			{
				if (!sb.Contains(s))
				{
					Popup.Show("'" + sb.ToString() + "' should contain '" + s + "' but doesn't");
				}
			};
			Action<StringBuilder, string> action = delegate(StringBuilder sb, string s)
			{
				if (sb.Contains(s))
				{
					Popup.Show("'" + sb.ToString() + "' shouldn't contain '" + s + "' but does");
				}
			};
			StringBuilder arg = new StringBuilder("abcde");
			obj(arg, "a");
			obj(arg, "b");
			obj(arg, "c");
			obj(arg, "d");
			obj(arg, "e");
			action(arg, "f");
			obj(arg, "ab");
			obj(arg, "bc");
			obj(arg, "cd");
			obj(arg, "de");
			action(arg, "aa");
			action(arg, "ac");
			action(arg, "ad");
			action(arg, "ae");
			action(arg, "af");
			action(arg, "ea");
			action(arg, "ba");
			action(arg, "bb");
			action(arg, "bd");
			action(arg, "be");
			action(arg, "bf");
			action(arg, "ca");
			action(arg, "cb");
			action(arg, "cc");
			action(arg, "ce");
			action(arg, "cf");
			action(arg, "da");
			action(arg, "db");
			action(arg, "dc");
			action(arg, "dd");
			action(arg, "df");
			action(arg, "eb");
			action(arg, "ec");
			action(arg, "ed");
			action(arg, "ee");
			action(arg, "ef");
			obj(arg, "abc");
			obj(arg, "bcd");
			obj(arg, "cde");
			action(arg, "def");
			obj(arg, "abcd");
			obj(arg, "bcde");
			action(arg, "cdef");
			obj(arg, "abcde");
			action(arg, "abcdef");
			StringBuilder arg2 = new StringBuilder("abcdee");
			obj(arg2, "ab");
			obj(arg2, "bc");
			obj(arg2, "cd");
			obj(arg2, "de");
			obj(arg2, "ee");
			action(arg2, "ef");
			obj(arg2, "abc");
			obj(arg2, "bcd");
			obj(arg2, "dee");
			action(arg2, "deee");
			action(arg2, "def");
			obj(arg2, "abcd");
			obj(arg2, "bcde");
			obj(arg2, "cdee");
			action(arg2, "cdef");
			obj(arg2, "abcde");
			obj(arg2, "bcdee");
			action(arg2, "bcdef");
			action(arg2, "bcdeee");
			action(arg2, "cdeee");
			Popup.Show("Done.");
			return;
		}
		if (Wish == "testzoneparse")
		{
			string zoneID = who.CurrentZone.ZoneID;
			string World;
			int ParasangX;
			int ParasangY;
			int ZoneX;
			int ZoneY;
			int ZoneZ;
			bool value = ZoneID.Parse(zoneID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
			string text5 = ZoneID.Assemble(World, ParasangX, ParasangY, ZoneX, ZoneY, ZoneZ);
			StringBuilder stringBuilder8 = Event.NewStringBuilder();
			stringBuilder8.Append("ZoneID: ").Append(zoneID).Append('\n')
				.Append("Parse result: ")
				.Append(value)
				.Append('\n')
				.Append("Components: ")
				.Append(World)
				.Append(' ')
				.Append(ParasangX)
				.Append(' ')
				.Append(ParasangY)
				.Append(' ')
				.Append(ZoneX)
				.Append(' ')
				.Append(ZoneY)
				.Append(' ')
				.Append(ZoneZ)
				.Append('\n')
				.Append("Match on reassemble: ")
				.Append(text5 == zoneID);
			Popup.Show(stringBuilder8.ToString());
			return;
		}
		if (Wish == "topevents")
		{
			Event.ShowTopEvents();
			return;
		}
		if (Wish == "testrig")
		{
			Mutations part = who.GetPart<Mutations>();
			part.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Clairvoyance)), 12);
			part.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Teleportation)), 12);
			AddSkill("Survival");
			AddSkill("Survival_Trailblazer");
			return;
		}
		if (Wish == "testhero")
		{
			GameObject gameObject7 = GameObjectFactory.Factory.CreateObject("Scrapbot");
			HeroMaker.MakeHero(gameObject7);
			who.pPhysics.CurrentCell.GetCellFromDirection("E").AddObject(gameObject7);
			return;
		}
		if (Wish == "reload")
		{
			GameManager.Instance.uiQueue.awaitTask(delegate
			{
				ModManager.ResetModSensitiveStaticCaches();
				The.Core.LoadEverything();
				MessageQueue.AddPlayerMessage("Configuration hotloaded...");
				UnityEngine.Debug.Log("Hot Loading Books...\n");
				BookUI.InitBooks();
			});
			MessageQueue.AddPlayerMessage("Hotload complete.");
			return;
		}
		if (Wish == "xy")
		{
			Popup.Show(who.CurrentCell.X + ", " + who.CurrentCell.Y);
			return;
		}
		if (Wish == "rebuild" || Wish == "flushandrebuild")
		{
			Cell currentCell2 = who.CurrentCell;
			_ = currentCell2.X;
			_ = currentCell2.Y;
			List<GameObject> objects2 = who.CurrentZone.GetObjects((GameObject o) => o.IsPlayerLed());
			Dictionary<GameObject, Location2D> dictionary = new Dictionary<GameObject, Location2D>();
			dictionary[who] = who.CurrentCell.location;
			who.CurrentCell.RemoveObject(who);
			foreach (GameObject item13 in objects2)
			{
				try
				{
					dictionary[item13] = item13.CurrentCell.location;
					item13.CurrentCell.RemoveObject(item13);
				}
				catch (Exception)
				{
				}
			}
			string zoneWorld = The.ZoneManager.ActiveZone.GetZoneWorld();
			The.ZoneManager.ActiveZone.GetZonewX();
			The.ZoneManager.ActiveZone.GetZonewY();
			The.ZoneManager.GetZone(zoneWorld);
			The.ZoneManager.SetActiveZone(zoneWorld);
			Zone parentZone4 = currentCell2.ParentZone;
			The.ZoneManager.SuspendZone(parentZone4);
			The.ZoneManager.DeleteZone(parentZone4);
			if (Wish == "flushandrebuild")
			{
				The.ZoneManager.CachedZones.Clear();
				The.ZoneManager.ActiveZone = null;
			}
			Zone zone = The.ZoneManager.GetZone(parentZone4.ZoneID);
			The.ZoneManager.SetActiveZone(zone.ZoneID);
			try
			{
				zone.GetCell(dictionary[who]).AddObject(who);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			catch (Exception)
			{
			}
			{
				foreach (GameObject item14 in objects2)
				{
					try
					{
						zone.GetCell(dictionary[item14]).AddObject(item14);
					}
					catch (Exception)
					{
					}
				}
				return;
			}
		}
		if (Wish == "popuptest")
		{
			Popup.Show(BookUI.Books["Skybear"][0].FullText);
			return;
		}
		if (Wish == "nanoterm")
		{
			Cell currentCell3 = who.pPhysics.CurrentCell;
			currentCell3.GetCellFromDirection("E").AddObject("Nanowall1W");
			currentCell3.GetCellFromDirection("E").GetCellFromDirection("E").GetCellFromDirection("E")
				.AddObject("Nanowall1E");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("E").AddObject("Nanowall2W");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("E").GetCellFromDirection("E")
				.AddObject("CyberneticsFabTerminal");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("E").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("Nanowall2E");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.AddObject("Nanowall3W");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("ArmNook");
			currentCell3.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("Nanowall3E");
			return;
		}
		if (Wish == "hungry")
		{
			Stomach obj2 = who.GetPart("Stomach") as Stomach;
			obj2.CookingCounter = obj2.CalculateCookingIncrement();
			return;
		}
		if (Wish == "famished")
		{
			Stomach obj3 = who.GetPart("Stomach") as Stomach;
			obj3.CookingCounter = obj3.CalculateCookingIncrement() * 2;
			return;
		}
		if (Wish == "what")
		{
			_ = who.pPhysics.CurrentCell;
			{
				foreach (GameObject @object in who.pPhysics.CurrentCell.Objects)
				{
					MessageQueue.AddPlayerMessage(@object.Blueprint);
				}
				return;
			}
		}
		if (Wish == "where")
		{
			Cell currentCell4 = who.pPhysics.CurrentCell;
			MessageQueue.AddPlayerMessage(currentCell4.X + "," + currentCell4.Y + " in " + currentCell4.ParentZone.ZoneID);
			return;
		}
		if (Wish == "bordertest")
		{
			for (int num13 = 0; num13 < 10000; num13++)
			{
				Popup._ScreenBuffer.ThickSingleBox(Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), ConsoleLib.Console.ColorUtility.MakeColor(ConsoleLib.Console.ColorUtility.Bright(TextColor.Black), TextColor.Black));
			}
			return;
		}
		if (Wish == "curefungus")
		{
			foreach (BodyPart part3 in who.Body.GetParts())
			{
				if (part3.Equipped != null && part3.Equipped.HasTag("FungalInfection"))
				{
					part3.Equipped.Destroy();
				}
			}
			return;
		}
		if (Wish == "cureironshank")
		{
			if (who.HasEffect("IronshankOnset"))
			{
				who.GetEffect("IronshankOnset").Duration = 0;
			}
			if (who.HasEffect("Ironshank"))
			{
				who.GetEffect("Ironshank").Duration = 0;
			}
			return;
		}
		if (Wish == "cureglotrot")
		{
			if (who.HasEffect("GlotrotOnset"))
			{
				who.GetEffect("GlotrotOnset").Duration = 0;
			}
			if (who.HasEffect("Glotrot"))
			{
				who.GetEffect("Glotrot").Duration = 0;
			}
			return;
		}
		if (Wish == "glotrotonset")
		{
			who.ApplyEffect(new GlotrotOnset());
			return;
		}
		switch (Wish)
		{
		case "glotrot":
			who.ApplyEffect(new Glotrot());
			return;
		case "glotrotfinal":
			who.ApplyEffect(new Glotrot());
			(who.GetEffect("Glotrot") as Glotrot).Stage = 3;
			return;
		case "ironshankonset":
			who.ApplyEffect(new IronshankOnset());
			return;
		case "ironshank":
			who.ApplyEffect(new Ironshank());
			return;
		case "monochromeonset":
			who.ApplyEffect(new MonochromeOnset());
			return;
		case "monochrome":
			who.ApplyEffect(new Monochrome());
			return;
		case "glotrotonset":
			who.ApplyEffect(new GlotrotOnset());
			return;
		case "mazetest":
		{
			Keys num16 = Popup.ShowBlock("1) random maze\n2) recursive backtrack maze");
			if (num16 == Keys.D1)
			{
				RandomMaze.Generate(80, 25, Stat.Random(0, 2147483646)).Test(bWait: true);
			}
			if (num16 == Keys.D2)
			{
				RecursiveBacktrackerMaze.Generate(80, 25, bShow: true, Stat.Random(0, 2147483646)).Test(bWait: true);
			}
			return;
		}
		case "tunneltest":
			do
			{
				TunnelMaker tunnelMaker = new TunnelMaker(5, 3, Stat.Random(0, 2).ToString(), Stat.Random(0, 2).ToString(), "NES");
				tunnelMaker.CreateTunnel();
				ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
				for (int num14 = 0; num14 < tunnelMaker.Width; num14++)
				{
					for (int num15 = 0; num15 < tunnelMaker.Height; num15++)
					{
						scrapBuffer.Goto(num14, num15);
						if (tunnelMaker.Map[num14, num15] == "")
						{
							scrapBuffer.Write(".");
						}
						if (tunnelMaker.Map[num14, num15].Contains("N") && tunnelMaker.Map[num14, num15].Contains("S"))
						{
							scrapBuffer.Write(186);
						}
						if (tunnelMaker.Map[num14, num15].Contains("E") && tunnelMaker.Map[num14, num15].Contains("W"))
						{
							scrapBuffer.Write(205);
						}
						if (tunnelMaker.Map[num14, num15].Contains("E") && tunnelMaker.Map[num14, num15].Contains("N"))
						{
							scrapBuffer.Write(200);
						}
						if (tunnelMaker.Map[num14, num15].Contains("W") && tunnelMaker.Map[num14, num15].Contains("N"))
						{
							scrapBuffer.Write(188);
						}
						if (tunnelMaker.Map[num14, num15].Contains("E") && tunnelMaker.Map[num14, num15].Contains("S"))
						{
							scrapBuffer.Write(201);
						}
						if (tunnelMaker.Map[num14, num15].Contains("W") && tunnelMaker.Map[num14, num15].Contains("S"))
						{
							scrapBuffer.Write(187);
						}
						if (tunnelMaker.Map[num14, num15] == "N")
						{
							scrapBuffer.Write(208);
						}
						if (tunnelMaker.Map[num14, num15] == "S")
						{
							scrapBuffer.Write(210);
						}
						if (tunnelMaker.Map[num14, num15] == "E")
						{
							scrapBuffer.Write(198);
						}
						if (tunnelMaker.Map[num14, num15] == "W")
						{
							scrapBuffer.Write(181);
						}
					}
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer);
			}
			while (Keyboard.getch() != 120);
			return;
		}
		if (Wish.StartsWith("rebuildbody:"))
		{
			string text6 = Wish.Split(':')[1];
			if (!who.Body.Rebuild(text6))
			{
				Popup.Show("Failed to rebuild body as " + text6);
			}
			return;
		}
		if (Wish == "bodyparttypes")
		{
			StringBuilder stringBuilder9 = Event.NewStringBuilder();
			who.Body.TypeDump(stringBuilder9);
			Popup.Show(stringBuilder9.ToString());
			return;
		}
		if (Wish.StartsWith("xpmul:"))
		{
			The.Core.XPMul = (float)Convert.ToDouble(Wish.Split(':')[1]);
			return;
		}
		if (Wish.StartsWith("xp:"))
		{
			Popup.bSuppressPopups = true;
			who.AwardXP(Convert.ToInt32(Wish.Split(':')[1]));
			Popup.bSuppressPopups = false;
			return;
		}
		if (Wish.StartsWith("xpverbose:"))
		{
			who.AwardXP(Convert.ToInt32(Wish.Split(':')[1]));
			return;
		}
		switch (Wish)
		{
		case "cleaneffects":
			if (who.Effects == null)
			{
				break;
			}
			{
				foreach (Effect effect in who.Effects)
				{
					effect.Duration = 0;
				}
				break;
			}
		case "clean":
			if (who.Effects != null)
			{
				foreach (Effect effect2 in who.Effects)
				{
					effect2.Duration = 0;
				}
			}
			who.Statistics["Strength"].Penalty = 0;
			who.Statistics["Agility"].Penalty = 0;
			who.Statistics["Intelligence"].Penalty = 0;
			who.Statistics["Toughness"].Penalty = 0;
			who.Statistics["Willpower"].Penalty = 0;
			who.Statistics["Ego"].Penalty = 0;
			who.Statistics["Speed"].Penalty = 0;
			break;
		case "websplat":
		{
			for (int num60 = 0; num60 < 8; num60++)
			{
				Cryobarrio1.Websplat(Stat.Random(0, 79), Stat.Random(0, 24), who.pPhysics.CurrentCell.ParentZone, "PhaseWeb");
			}
			break;
		}
		case "sultantomb1":
			The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.6");
			who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(39, 14));
			The.ZoneManager.ProcessGoToPartyLeader();
			break;
		case "sultantomb6":
			The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.0.1");
			who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(39, 14));
			The.ZoneManager.ProcessGoToPartyLeader();
			break;
		default:
			if (!Wish.StartsWith("stage9"))
			{
				switch (Wish)
				{
				case "stage10":
				case "stage11":
				case "tombbeta":
				case "tombbetastart":
				case "tombbetaend":
				case "tombbetainside":
				case "reefjump":
					break;
				case "reequip":
					who.pBrain.PerformReequip(IsPlayer: true);
					return;
				case "garbagetest":
				{
					for (int num59 = 0; num59 < 1000000; num59++)
					{
						MessageQueue.AddPlayerMessage("garbage");
					}
					return;
				}
				case "playeronly":
					The.Game.ActionManager.ActionQueue.Clear();
					The.Game.ActionManager.ActionQueue.Enqueue(who);
					The.Game.ActionManager.ActionQueue.Enqueue(null);
					MessageQueue.AddPlayerMessage("Removed everyone but the player from the action queue.");
					return;
				default:
				{
					if (Wish.StartsWith("confusion"))
					{
						string[] array4 = Wish.Split(':');
						int duration = ((array4.Length >= 2) ? int.Parse(array4[1]) : 10);
						int num17 = ((array4.Length >= 3) ? int.Parse(array4[2]) : 5);
						who.ApplyEffect(new XRL.World.Effects.Confused(duration, num17, num17 + 2));
						return;
					}
					if (Wish.StartsWith("roll:"))
					{
						Popup.Show(Stat.Roll(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("rollmin:"))
					{
						Popup.Show(Stat.RollMin(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("rollmax:"))
					{
						Popup.Show(Stat.RollMax(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("rollcached:"))
					{
						Popup.Show(Stat.RollCached(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("rollmincached:"))
					{
						Popup.Show(Stat.RollMinCached(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("rollmaxcached:"))
					{
						Popup.Show(Stat.RollMaxCached(Wish.Split(':')[1]).ToString());
						return;
					}
					if (Wish.StartsWith("godown:"))
					{
						int n2 = int.Parse(Wish.Split(':')[1]);
						Zone zoneFromDirection = who.CurrentZone.GetZoneFromDirection("D", n2);
						Point2D pos2D = who.CurrentCell.Pos2D;
						who.pPhysics.CurrentCell.RemoveObject(who);
						zoneFromDirection.GetCell(pos2D).AddObject(who);
						The.ZoneManager.SetActiveZone(zoneFromDirection.ZoneID);
						The.ZoneManager.ProcessGoToPartyLeader();
						return;
					}
					if (Wish.StartsWith("hindrenawardtest"))
					{
						string[] array5 = Wish.Split(',');
						new Neelahind().giveAward("", array5[1], array5[2], array5[3]);
						return;
					}
					switch (Wish)
					{
					case "sherlock":
						game.CompleteQuest("Find Eskhind");
						game.StartQuest("Kith and Kin");
						JournalAPI.Observations.Where((JournalObservation o) => o.Has("hindrenclue") && o.Has("free")).ToList().Shuffle()
							.Take(5)
							.ToList()
							.ForEach(delegate(JournalObservation o)
							{
								JournalAPI.RevealObservation(o);
							});
						JournalAPI.Observations.Where((JournalObservation o) => o.Has("hindrenclue") && o.attributes.Any((string p) => p.StartsWith("motive:"))).ToList().Shuffle()
							.Take(5)
							.ToList()
							.ForEach(delegate(JournalObservation o)
							{
								JournalAPI.RevealObservation(o);
							});
						HindrenMysteryGamestate.instance.foundClue();
						if (The.ActiveZone.FindObject("Neelahind") == null)
						{
							who.GetCurrentCell().GetCellFromDirection("NW").AddObject("Neelahind");
						}
						return;
					case "revealobservations":
						Popup.bSuppressPopups = true;
						JournalAPI.Observations.ForEach(delegate(JournalObservation o)
						{
							JournalAPI.RevealObservation(o);
						});
						Popup.bSuppressPopups = false;
						return;
					case "revealmapnotes":
						Popup.bSuppressPopups = true;
						JournalAPI.MapNotes.ForEach(delegate(JournalMapNote o)
						{
							JournalAPI.RevealMapNote(o);
						});
						Popup.bSuppressPopups = false;
						return;
					case "calm":
						The.Core.Calm = !The.Core.Calm;
						MessageQueue.AddPlayerMessage("Calm now " + The.Core.Calm);
						return;
					case "minime":
						EvilTwin.CreateEvilTwin(who, "Mini");
						return;
					case "blink":
						who.TeleportTo(who.pPhysics.PickDestinationCell(999, AllowVis.OnlyExplored, Locked: false), 0);
						return;
					case "license":
						MessageQueue.AddPlayerMessage("License tier now " + who.ModIntProperty("CyberneticsLicenses", 20));
						return;
					}
					if (Wish.StartsWith("license:"))
					{
						int num18 = int.Parse(Wish.Split(':')[1]);
						if (num18 != 0)
						{
							MessageQueue.AddPlayerMessage("License tier now " + who.ModIntProperty("CyberneticsLicenses", num18));
						}
						return;
					}
					if (Wish == "impl")
					{
						who.ImplosionSplat();
						return;
					}
					if (Wish.StartsWith("impl:"))
					{
						int num19 = int.Parse(Wish.Split(':')[1]);
						if (num19 != 0)
						{
							who.ImplosionSplat(num19);
						}
						return;
					}
					if (Wish == "gainmp")
					{
						Popup.Show(who.GainMP(1).ToString());
						return;
					}
					if (Wish.StartsWith("gainmp:"))
					{
						int num20 = int.Parse(Wish.Split(':')[1]);
						if (num20 != 0)
						{
							Popup.Show(who.GainMP(num20).ToString());
						}
						return;
					}
					if (Wish == "bits")
					{
						who.RequirePart<BitLocker>().AddAllBits(20);
						return;
					}
					if (Wish.StartsWith("bits:"))
					{
						int num21 = int.Parse(Wish.Split(':')[1]);
						if (num21 > 0)
						{
							who.RequirePart<BitLocker>().AddAllBits(num21);
						}
						return;
					}
					switch (Wish)
					{
					case "smartass":
						Popup.bSuppressPopups = true;
						try
						{
							AddSkill("Tinkering");
							AddSkill("Tinkering_GadgetInspector");
							AddSkill("Tinkering_Repair");
							AddSkill("Tinkering_ReverseEngineer");
							AddSkill("Tinkering_Scavenger");
							AddSkill("Tinkering_Disassemble");
							AddSkill("Tinkering_LayMine");
							AddSkill("Tinkering_DeployTurret");
							AddSkill("Tinkering_Tinker1");
							AddSkill("Tinkering_Tinker2");
							AddSkill("Tinkering_Tinker3");
							foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
							{
								if (!TinkerData.KnownRecipes.CleanContains(tinkerRecipe))
								{
									TinkerData.KnownRecipes.Add(tinkerRecipe);
								}
							}
							return;
						}
						finally
						{
							Popup.bSuppressPopups = false;
						}
					case "cloacasurprise":
						CookingDomainSpecial_UnitSlogTransform.ApplyTo(who);
						return;
					case "findduplicaterecipes":
					{
						Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
						foreach (TinkerData tinkerRecipe2 in TinkerData.TinkerRecipes)
						{
							string blueprint = tinkerRecipe2.Blueprint;
							string key = (blueprint.StartsWith("[") ? blueprint : GameObjectFactory.Factory.CreateSampleObject(blueprint).pRender.DisplayName);
							if (dictionary2.ContainsKey(key))
							{
								dictionary2[key]++;
							}
							else
							{
								dictionary2.Add(key, 1);
							}
						}
						StringBuilder stringBuilder10 = Event.NewStringBuilder();
						bool flag3 = false;
						foreach (string key2 in dictionary2.Keys)
						{
							if (dictionary2[key2] > 1)
							{
								stringBuilder10.Append(key2).Append(" (").Append(dictionary2[key2])
									.Append(")\n");
								flag3 = true;
							}
						}
						Popup.Show(flag3 ? stringBuilder10.ToString() : "no duplicate recipes found");
						return;
					}
					case "cluber":
						AddSkill("Cudgel");
						AddSkill("Cudgel_Expertise");
						AddSkill("Cudgel_Backswing");
						AddSkill("Cudgel_Bludgeon");
						AddSkill("Cudgel_ChargingStrike");
						AddSkill("Cudgel_Conk");
						AddSkill("Cudgel_Slam");
						AddSkill("Cudgel_SmashUp");
						return;
					case "fencer":
						AddSkill("LongBlades");
						AddSkill("LongBladesDuelingStance");
						AddSkill("LongBladesImprovedAggressiveStance");
						AddSkill("LongBladesImprovedDefensiveStance");
						AddSkill("LongBladesImprovedDuelistStance");
						AddSkill("LongBladesDuelingStance");
						AddSkill("LongBladesLunge");
						AddSkill("LongBladesProficiency");
						AddSkill("LongBladesSwipe");
						AddSkill("LongBladesDeathblow");
						return;
					case "axer":
						AddSkill("Axe");
						AddSkill("Axe_Expertise");
						AddSkill("Axe_Cleave");
						AddSkill("Axe_ChargingStrike");
						AddSkill("Axe_Dismember");
						AddSkill("Axe_HookAndDrag");
						AddSkill("Axe_Decapitate");
						AddSkill("Axe_Berserk");
						return;
					case "sblader":
						AddSkill("ShortBlades");
						AddSkill("ShortBlades_Expertise");
						AddSkill("ShortBlades_Hobble");
						AddSkill("ShortBlades_Jab");
						AddSkill("ShortBlades_Bloodletter");
						AddSkill("ShortBlades_Shank");
						AddSkill("ShortBlades_PointedCircle");
						AddSkill("ShortBlades_Rejoinder");
						return;
					case "wandermode":
					{
						foreach (Faction item15 in Factions.loop())
						{
							if (The.Game.PlayerReputation.get(item15) < 0)
							{
								The.Game.PlayerReputation.set(item15, 0);
							}
						}
						return;
					}
					case "skillpoints":
						who.Statistics["SP"].BaseValue = 20000;
						return;
					case "traveler":
					{
						foreach (Faction item16 in Factions.loop())
						{
							game.PlayerReputation.set(item16, 0);
						}
						return;
					}
					case "togglementalshields":
						MentalShield.Disabled = !MentalShield.Disabled;
						return;
					case "trip":
						who.ApplyEffect(new Prone());
						return;
					case "pro":
						who.Statistics["Strength"].BaseValue = 40;
						who.Statistics["Intelligence"].BaseValue = 40;
						who.Statistics["Ego"].BaseValue = 40;
						who.Statistics["Agility"].BaseValue = 40;
						who.Statistics["Toughness"].BaseValue = 40;
						who.Statistics["Willpower"].BaseValue = 40;
						return;
					case "where?":
						MessageQueue.AddPlayerMessage(who.CurrentZone.ZoneID);
						XRLCore.SetClipboard(who.CurrentZone.ZoneID);
						return;
					}
					if (Wish.StartsWith("factionrep") || Wish.StartsWith("reputation"))
					{
						string[] array6 = Wish.Split(' ');
						if (array6.Length > 3 || Wish.Contains(":"))
						{
							array6 = Wish.Split(':');
						}
						game.PlayerReputation.modify(array6[1], Convert.ToInt32(array6[2]));
						return;
					}
					if (Wish == "memtest")
					{
						return;
					}
					if (Wish == "leadslugs")
					{
						who.TakeObject("Lead Slug", 10000, Silent: false, 0);
						return;
					}
					if (Wish.StartsWith("regionalize"))
					{
						new RegionPopulator().BuildZone(who.pPhysics.CurrentCell.ParentZone);
						return;
					}
					if (Wish.StartsWith("smartitem:"))
					{
						WishResult wishResult = WishSearcher.SearchForBlueprint(Wish);
						GameObject gameObject8 = GameObjectFactory.Factory.CreateObject(wishResult.Result);
						gameObject8.AddPart(new SmartItem());
						who.pPhysics.CurrentCell.AddObject(gameObject8);
						return;
					}
					if (Wish.StartsWith("seed:"))
					{
						int value2 = Convert.ToInt32(Wish.Split(':')[1]);
						game.SetIntGameState("WorldSeed", value2);
						return;
					}
					switch (Wish)
					{
					case "glowcrust":
						FungalSporeInfection.ApplyFungalInfection(who, "LuminousInfection");
						return;
					case "testmadness":
						who.pPhysics.CurrentCell.ParentZone.FindClosestObjectWithPart(who, "Brain", ExploredOnly: true, IncludeSelf: false).pBrain.PushGoal(new PaxKlanqMadness());
						return;
					case "randomitems":
					{
						for (int num22 = 0; num22 < 20; num22++)
						{
							who.GetCurrentCell().AddObject(EncountersAPI.GetAnItem());
						}
						return;
					}
					}
					if (Wish.StartsWith("sultantest:"))
					{
						string[] collection = Wish.Split(':')[1].Split(',');
						Zone zone2 = The.ZoneManager.GetZone("JoppaWorld");
						ZoneManager zoneManager = The.ZoneManager;
						History sultanHistory = The.Game.sultanHistory;
						Cell cell3 = zone2.GetCell(who.GetCurrentCell().Pos2D);
						SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
						HistoricEntity newEntity = sultanHistory.GetNewEntity(sultanHistory.currentYear);
						newEntity.ApplyEvent(new InitializeRegion(5));
						HistoricEntitySnapshot currentSnapshot = newEntity.GetCurrentSnapshot();
						foreach (string item17 in new List<string>(currentSnapshot.properties.Keys))
						{
							if (item17 != "name" && item17 != "newName" && item17 != "period")
							{
								currentSnapshot.properties.Remove(item17);
							}
						}
						currentSnapshot.listProperties.Clear();
						currentSnapshot.listProperties.Add("testAttributes", new List<string>());
						currentSnapshot.listProperties["testAttributes"].AddRange(collection);
						sultanDungeonArgs.UpdateFromEntity(currentSnapshot);
						string property = currentSnapshot.GetProperty("name");
						The.Game.SetObjectGameState("sultanDungeonArgs_" + property, sultanDungeonArgs);
						Vector2i vector2i = new Vector2i(cell3.X, cell3.Y);
						string text7 = Grammar.MakeTitleCase(property);
						int num23 = 10;
						for (int num24 = 0; num24 < num23; num24++)
						{
							HistoricEntity newEntity2 = sultanHistory.GetNewEntity(sultanHistory.currentYear);
							newEntity2.ApplyEvent(new InitializeLocation(property, 5));
							string property2 = newEntity2.GetCurrentSnapshot().GetProperty("name");
							string zoneID2 = "JoppaWorld." + vector2i.x + "." + vector2i.y + ".1.1." + (num24 + 10);
							if (num24 == 0)
							{
								zoneManager.SetZoneName(zoneID2, text7, null, null, null, null, Proper: true);
							}
							else
							{
								zoneManager.SetZoneName(zoneID2, "liminal floor", text7);
							}
							string text8 = "";
							if (num24 < num23 - 1)
							{
								text8 += "D";
							}
							if (num24 > 0)
							{
								text8 += "U";
							}
							zoneManager.ClearZoneBuilders(zoneID2);
							zoneManager.SetZoneProperty(zoneID2, "SkipTerrainBuilders", true);
							zoneManager.AddZoneMidBuilder(zoneID2, new ZoneBuilderBlueprint("SultanDungeon", "locationName", property2, "regionName", property, "stairs", text8));
						}
						return;
					}
					switch (Wish)
					{
					case "sultanreveal":
					{
						Popup.bSuppressPopups = true;
						ItemNaming.Suppress = true;
						Zone zone3 = The.ZoneManager.GetZone("JoppaWorld");
						for (int num27 = 0; num27 < 80; num27++)
						{
							for (int num28 = 0; num28 < 25; num28++)
							{
								zone3.GetCell(num27, num28).GetFirstObjectWithPart("TerrainTravel").FireEvent("SultanReveal");
							}
						}
						Popup.bSuppressPopups = false;
						ItemNaming.Suppress = false;
						return;
					}
					case "villagereveal":
					{
						Popup.bSuppressPopups = true;
						ItemNaming.Suppress = true;
						Zone zone4 = The.ZoneManager.GetZone("JoppaWorld");
						for (int num29 = 0; num29 < 80; num29++)
						{
							for (int num30 = 0; num30 < 25; num30++)
							{
								zone4.GetCell(num29, num30).GetFirstObjectWithPart("TerrainTravel").FireEvent("VillageReveal");
							}
						}
						Popup.bSuppressPopups = false;
						ItemNaming.Suppress = false;
						return;
					}
					case "zonebuilders":
					{
						List<ZoneBuilderBlueprint> buildersFor = The.ZoneManager.GetBuildersFor(The.ZoneManager.ActiveZone);
						StringBuilder stringBuilder11 = new StringBuilder();
						foreach (ZoneBuilderBlueprint item18 in buildersFor)
						{
							stringBuilder11.Append(item18.Class);
							if (item18.Parameters != null && item18.Parameters.Count > 0)
							{
								stringBuilder11.Append(" [");
								foreach (KeyValuePair<string, object> parameter in item18.Parameters)
								{
									if (stringBuilder11[stringBuilder11.Length - 1] != '[')
									{
										stringBuilder11.Append(", ");
									}
									stringBuilder11.Append(parameter.Key).Append(": ");
									stringBuilder11.Append(parameter.Value.ToString());
								}
								stringBuilder11.Append(']');
							}
							stringBuilder11.Append('\n');
						}
						Popup.Show(stringBuilder11.ToString());
						return;
					}
					case "zoneconnections":
					{
						Zone activeZone = The.ActiveZone;
						ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer1();
						scrapBuffer2.RenderBase();
						foreach (ZoneConnection item19 in activeZone.EnumerateConnections())
						{
							if (item19 is CachedZoneConnection cachedZoneConnection)
							{
								scrapBuffer2.WriteAt(item19.X, item19.Y, "{{R|" + cachedZoneConnection.TargetDirection + "}}");
							}
							else
							{
								scrapBuffer2.WriteAt(item19.X, item19.Y, "{{M|X}}");
							}
						}
						scrapBuffer2.Draw();
						Keyboard.getch();
						return;
					}
					case "freezezones":
						foreach (Zone value4 in The.ZoneManager.CachedZones.Values)
						{
							if (!value4.IsActive())
							{
								value4.LastActive = 0L;
							}
						}
						The.ZoneManager.Tick(bAllowFreeze: true);
						return;
					case "clearfrozen":
						The.ZoneManager.ClearFrozen();
						return;
					case "sultanhistory":
					{
						foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
						{
							sultanNote.Reveal(silent: true);
						}
						return;
					}
					case "sultanquests":
					{
						HistoricEntityList entitiesWherePropertyEquals = The.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
						for (int num31 = 0; num31 < entitiesWherePropertyEquals.entities.Count; num31++)
						{
							for (int num32 = 0; num32 < entitiesWherePropertyEquals.entities[num31].events.Count; num32++)
							{
								entitiesWherePropertyEquals.entities[num31].events[num32].Reveal();
							}
						}
						return;
					}
					case "reveal1sultanhistory":
					{
						string text9 = null;
						{
							foreach (JournalSultanNote sultanNote2 in JournalAPI.GetSultanNotes())
							{
								if (text9 == null)
								{
									text9 = sultanNote2.sultan;
								}
								if (sultanNote2.sultan == text9)
								{
									sultanNote2.Reveal();
								}
							}
							return;
						}
					}
					case "glass":
					{
						History sultanHistory2 = The.Game.sultanHistory;
						HistoricEntityList entitiesWithProperty = sultanHistory2.GetEntitiesWithProperty("itemType");
						for (int num25 = 0; num25 < entitiesWithProperty.entities.Count; num25++)
						{
							HistoricEntitySnapshot currentSnapshot2 = entitiesWithProperty.entities[num25].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot2.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot2, Stat.Random(1, 4)));
						}
						List<string> list9 = new List<string>();
						list9.Add("glass");
						HistoricEntitySnapshot currentSnapshot3 = sultanHistory2.GetEntitiesWherePropertyEquals("type", "region").GetRandomElement().GetCurrentSnapshot();
						List<string> list10 = new List<string>();
						list10.Add(list9.GetRandomElement());
						string[] supportedTypes = RelicGenerator.supportedTypes;
						foreach (string type in supportedTypes)
						{
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(type, Stat.Random(1, 4), currentSnapshot3, list10, new Dictionary<string, string>(), new Dictionary<string, List<string>>()));
						}
						return;
					}
					}
					if (Wish.StartsWith("randomrelic:"))
					{
						string[] array7 = Wish.Split(':');
						for (int num33 = 0; num33 < Convert.ToInt32(array7[1]); num33++)
						{
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(Stat.Random(1, 4), randomName: true));
						}
						return;
					}
					switch (Wish)
					{
					case "EarlRelic":
					{
						List<IBaseJournalEntry> list14 = (from c in ((IEnumerable<JournalAccomplishment>)JournalAPI.Accomplishments).Select((Func<JournalAccomplishment, IBaseJournalEntry>)((JournalAccomplishment c) => c))
							where c.revealed && c.text.StartsWith("You ")
							select c).ToList();
						string playerNameAndAppositive = ((list14.Count <= 0) ? (" with " + who.DisplayName) : (" with " + who.DisplayName + ", who " + list14.GetRandomElement().text.Substring(4).TrimEnd('.')));
						who.GetCurrentCell().AddObject(RelicGenerator.GenerateSpindleNegotiationRelic("Armor", "Frogs", "Fish", playerNameAndAppositive));
						return;
					}
					case "sultanrelics":
					{
						HistoricEntityList entitiesWithProperty2 = The.Game.sultanHistory.GetEntitiesWithProperty("itemType");
						for (int num34 = 0; num34 < entitiesWithProperty2.entities.Count; num34++)
						{
							HistoricEntitySnapshot currentSnapshot4 = entitiesWithProperty2.entities[num34].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot4.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot4));
						}
						return;
					}
					case "relic":
					{
						History sultanHistory3 = The.Game.sultanHistory;
						HistoricEntityList entitiesWithProperty3 = sultanHistory3.GetEntitiesWithProperty("itemType");
						for (int num35 = 0; num35 < entitiesWithProperty3.entities.Count; num35++)
						{
							HistoricEntitySnapshot currentSnapshot5 = entitiesWithProperty3.entities[num35].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot5.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot5));
						}
						List<string> list12 = new List<string>();
						list12.Add("chance");
						list12.Add("might");
						list12.Add("scholarship");
						list12.Add("time");
						list12.Add("ice");
						list12.Add("stars");
						list12.Add("salt");
						list12.Add("jewels");
						list12.Add("glass");
						list12.Add("circuitry");
						list12.Add("travel");
						HistoricEntitySnapshot currentSnapshot6 = sultanHistory3.GetEntitiesWherePropertyEquals("type", "region").GetRandomElement().GetCurrentSnapshot();
						List<string> list13 = new List<string>();
						list13.Add(list12.GetRandomElement());
						string[] supportedTypes = RelicGenerator.supportedTypes;
						foreach (string type2 in supportedTypes)
						{
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(type2, Stat.Random(1, 8), currentSnapshot6, list13, new Dictionary<string, string>(), new Dictionary<string, List<string>>()));
						}
						return;
					}
					case "shownamingchances":
					{
						StringBuilder stringBuilder12 = Event.NewStringBuilder();
						Dictionary<GameObject, int> chances = ItemNaming.GetNamingChances(game.Player.Body);
						List<GameObject> list11 = Event.NewGameObjectList();
						list11.AddRange(chances.Keys);
						list11.Sort((GameObject a, GameObject b) => chances[b].CompareTo(chances[a]));
						foreach (GameObject item20 in list11)
						{
							stringBuilder12.Append(chances[item20]).Append(": ").Append(item20.DisplayName)
								.Append("&y\n");
						}
						Popup.Show(stringBuilder12.ToString());
						return;
					}
					case "whatami":
						MessageQueue.AddPlayerMessage("I am a: " + who.Blueprint);
						return;
					case "cool":
						The.Core.cool = !The.Core.cool;
						MessageQueue.AddPlayerMessage("Coolmode now " + The.Core.cool);
						return;
					}
					if (Wish.StartsWith("stat:"))
					{
						string[] array8 = Wish.Split(':');
						who.Statistics[array8[1]].BaseValue += Convert.ToInt32(array8[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array8[2]) + " to " + array8[1] + "'s BaseValue");
						return;
					}
					if (Wish.StartsWith("statbonus:"))
					{
						string[] array9 = Wish.Split(':');
						who.Statistics[array9[1]].Bonus += Convert.ToInt32(array9[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array9[2]) + " to " + array9[1] + "'s Bonus");
						return;
					}
					if (Wish.StartsWith("statpenality:") || Wish.StartsWith("statpenalty:"))
					{
						string[] array10 = Wish.Split(':');
						who.Statistics[array10[1]].Penalty += Convert.ToInt32(array10[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array10[2]) + " to " + array10[1] + "'s Penalty");
						return;
					}
					switch (Wish)
					{
					case "testearl":
					{
						string[] array11 = new string[5];
						List<string> list15 = new List<string>();
						list15.Add("Dogs");
						list15.Add("Cannibals");
						list15.Add("Dromad");
						list15.Add("Girsh");
						List<string> list16 = new List<string>();
						for (int num39 = 0; num39 < list15.Count; num39++)
						{
							list16.Add(Faction.getFormattedName(list15[num39]));
						}
						array11[0] = "Share the burden across all allies. [-&C50&y reputation with each attending faction]";
						array11[1] = "Share the burden between two allies. [-&C100&y reputation with two attending factions of your choice]";
						array11[2] = "Spare one faction of all obligation by betraying a second faction and selling their secrets to Asphodel. [-&C800&y with the betrayed faction, +&C200&y reputation with the spared faction + a faction heirloom]";
						array11[3] = "Invoke the Chaos Spiel. [????????, +&C300&y reputation with &Chighly &Centropic &Cbeings&y]";
						array11[4] = "Take time to weigh the options.";
						char[] hotkeys = new char[5] { 'a', 'b', 'c', 'd', 'e' };
						string text10 = "";
						for (int num40 = 0; num40 < list15.Count; num40++)
						{
							text10 = text10 + list15[num40] + ", ";
						}
						Popup.ShowOptionList("", array11, hotkeys, 1, "The First Council of Omonporch has begun. Choose how to appease Asphodel.", 75);
						return;
					}
					case "bookfuck":
					{
						for (int num41 = 0; num41 < 10; num41++)
						{
							GameObject gameObject10 = GameObjectFactory.Factory.CreateObject("StandaloneMarkovBook");
							StringBuilder stringBuilder14 = new StringBuilder();
							gameObject10.GetPart<MarkovBook>().GeneratePages();
							stringBuilder14.Append("{\\rtf1\\ansi\r\n\r\n    \\pgbrdrt\r\n    \\brdrart1\r\n    \\pgbrdrb\r\n    \\brdrart1\r\n    \\pgbrdrl\r\n    \\brdrart1\r\n    \\pgbrdrr\r\n    \\brdrart1\r\n\r\n    {\\fonttbl\\f0\\froman Georgia;}\\f0\\pard");
							stringBuilder14.Append("{\\pard\\qc\\fs36 ");
							stringBuilder14.AppendLine(gameObject10.DisplayNameOnlyStripped);
							stringBuilder14.Append(" \\par}");
							stringBuilder14.AppendLine();
							stringBuilder14.AppendLine("\\par");
							foreach (BookPage page in gameObject10.GetPart<MarkovBook>().Pages)
							{
								stringBuilder14.AppendLine(page.FullText.Replace("\n", "").Replace("\r", ""));
								stringBuilder14.AppendLine("\\par\\fs24");
							}
							stringBuilder14.Append("{\\footer\\pard\\qc\\fs18 Using Predictive Text to Generate Lore in {\\pard\\qc\\fs18\\i Caves of Qud} \\par}");
							stringBuilder14.Append("}");
							File.WriteAllText(DataManager.SavePath("book_" + num41 + ".rtf"), stringBuilder14.ToString());
						}
						return;
					}
					case "bookfuckonefile":
					{
						StringBuilder stringBuilder15 = new StringBuilder();
						stringBuilder15.Append("{\\rtf1\\ansi\r\n\r\n    \\pgbrdrt\r\n    \\brdrart1\r\n    \\pgbrdrb\r\n    \\brdrart1\r\n    \\pgbrdrl\r\n    \\brdrart1\r\n    \\pgbrdrr\r\n    \\brdrart1\r\n\r\n    {\\fonttbl\\f0\\froman Georgia;}\\f0\\pard");
						for (int num42 = 0; num42 < 300; num42++)
						{
							GameObject gameObject11 = GameObjectFactory.Factory.CreateObject("StandaloneMarkovBook");
							gameObject11.GetPart<MarkovBook>().GeneratePages();
							stringBuilder15.Append("{\\pard\\qc\\fs36 ");
							stringBuilder15.AppendLine(gameObject11.DisplayNameOnlyStripped);
							stringBuilder15.Append(" \\par}");
							stringBuilder15.AppendLine();
							stringBuilder15.AppendLine("\\par");
							foreach (BookPage page2 in gameObject11.GetPart<MarkovBook>().Pages)
							{
								stringBuilder15.AppendLine(page2.FullText.Replace("\n", "").Replace("\r", ""));
								stringBuilder15.AppendLine("\\par\\fs24");
							}
							stringBuilder15.Append("{\\footer\\pard\\qc\\fs18 Using Predictive Text to Generate Lore in {\\pard\\qc\\fs18\\i Caves of Qud} \\par}");
							stringBuilder15.Append("\\pard \\insrsid \\page \\par");
						}
						stringBuilder15.Append("}");
						File.WriteAllText(DataManager.SavePath("bigbook.rtf"), stringBuilder15.ToString());
						return;
					}
					case "clearfactionmembership":
						who.pBrain.FactionMembership.Clear();
						MessageQueue.AddPlayerMessage("Cleared faction membership");
						return;
					case "allbox":
					{
						GameObject gameObject9 = GameObjectFactory.Factory.CreateObject("Chest");
						Inventory inventory = gameObject9.Inventory;
						foreach (string key3 in GameObjectFactory.Factory.Blueprints.Keys)
						{
							if (GameObjectFactory.Factory.Blueprints[key3].InheritsFrom("Item"))
							{
								inventory.AddObject(GameObjectFactory.Factory.CreateObject(key3));
							}
						}
						who.GetCurrentCell().GetCellFromDirection("N").AddObject(gameObject9);
						return;
					}
					case "fungone":
						who.RemoveEffect("FungalSporeInfection");
						who.RemoveEffect("SporeCloudPoison");
						return;
					case "heapshot":
						GameManager.Instance.uiQueue.queueTask(delegate
						{
							UnityHeapDump.Create();
						});
						return;
					case "test437":
					{
						StringBuilder stringBuilder13 = new StringBuilder();
						char c2 = '\0';
						for (int num37 = 0; num37 < 16; num37++)
						{
							for (int num38 = 0; num38 < 16; num38++)
							{
								stringBuilder13.Append(c2);
								c2 = (char)(c2 + 1);
							}
							stringBuilder13.AppendLine();
						}
						Popup.Show(stringBuilder13.ToString());
						return;
					}
					case "spend":
						who.RandomlySpendPoints();
						return;
					case "playerlevelmob":
						who.GetCurrentCell().GetCellFromDirection("NW").AddObject(EncountersAPI.GetCreatureAroundPlayerLevel());
						return;
					case "auditblueprints":
					{
						foreach (GameObjectBlueprint value5 in GameObjectFactory.Factory.Blueprints.Values)
						{
							if (value5.HasTag("BaseObject") && value5.HasTag("ExcludeFromDynamicEncounters") && value5.GetTag("ExcludeFromDynamicEncounters") == "*noinherit")
							{
								Popup.Show(value5.Name + " has BaseObject and ExcludeFromDynamicEncounters with *noinherit");
							}
							string text11 = value5.DisplayName();
							if (text11 != null && (text11.StartsWith("[") || text11.EndsWith("]")) && !value5.HasTag("BaseObject") && !value5.HasTag("ExcludeFromDynamicEncounters"))
							{
								Popup.Show(value5.Name + " has display name " + text11 + " and neither BaseObject nor ExcludeFromDynamicEncounters");
							}
						}
						return;
					}
					case "auditrenderlayers":
					{
						foreach (GameObjectBlueprint value6 in GameObjectFactory.Factory.Blueprints.Values)
						{
							string partParameter = value6.GetPartParameter("Render", "RenderLayer");
							if (!value6.GetPartParameter("Physics", "Solid").EqualsNoCase("true") || value6.InheritsFrom("Terrain"))
							{
								continue;
							}
							int num36 = -1;
							try
							{
								num36 = Convert.ToInt32(partParameter);
							}
							catch
							{
							}
							if (num36 < 6)
							{
								if (num36 == -1)
								{
									Popup.Show(value6.Name + " is solid and has unparseable RenderLayer '" + partParameter + "'");
								}
								else
								{
									Popup.Show(value6.Name + " is solid and has RenderLayer " + num36);
								}
							}
						}
						return;
					}
					case "findfarmers":
					{
						string text12 = "";
						foreach (GameObjectBlueprint value7 in GameObjectFactory.Factory.Blueprints.Values)
						{
							if (!value7.Tags.ContainsKey("BaseObject"))
							{
								GamePartBlueprint part2 = value7.GetPart("Render");
								if (part2 != null && part2.Parameters.ContainsKey("Tile") && part2.Parameters["Tile"] == "Assets_Content_Textures_Creatures_sw_farmer.bmp")
								{
									text12 = text12 + value7.Name + "\n";
								}
							}
						}
						Popup.Show(text12);
						return;
					}
					}
					if (Wish.StartsWith("dismember:"))
					{
						string text13 = Wish.Split(':')[1];
						Body body = who.Body;
						BodyPart bodyPart = body.GetPartByName(text13) ?? body.GetPartByDescription(text13) ?? body.GetPartByName(text13.ToLower()) ?? body.GetPartByDescription(Grammar.MakeTitleCase(text13));
						if (bodyPart == null)
						{
							Popup.Show("Could not find body part by name or description: " + text13);
						}
						else if (!bodyPart.IsSeverable())
						{
							if (bodyPart.Integral)
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " an integral part of your body and " + (bodyPart.Plural ? "are" : "is") + " not dismemberable.");
							}
							else if (bodyPart.DependsOn != null || bodyPart.RequiresType != null)
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " not directly dismemberable, instead being lost when other body parts are dismembered.");
							}
							else
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " not dismemberable.");
							}
						}
						else
						{
							who.Body.Dismember(bodyPart);
						}
						return;
					}
					if (Wish == "clearach!!!")
					{
						if (Popup.ShowYesNo("Are you sure you want to RESET ALL YOUR ACHIEVEMENTS?") == DialogResult.Yes)
						{
							AchievementManager.Reset();
						}
						return;
					}
					if (Wish.StartsWith("regeneratedefaultequipment"))
					{
						who?.Body?.RegenerateDefaultEquipment();
						return;
					}
					if (Wish.StartsWith("cooktestunits:"))
					{
						GameObject gameObject12 = who;
						ProceduralCookingEffect proceduralCookingEffect = ProceduralCookingEffect.CreateJustUnits(new List<string>(Wish.Split(':')[1].Split(',')));
						proceduralCookingEffect.Init(gameObject12);
						gameObject12.ApplyEffect(proceduralCookingEffect);
						return;
					}
					if (Wish.StartsWith("cooktestfull:"))
					{
						GameObject gameObject13 = who;
						string[] array12 = Wish.Split(':')[1].Split(',');
						ProceduralCookingEffectWithTrigger proceduralCookingEffectWithTrigger = ProceduralCookingEffect.CreateBaseAndTriggeredAction(array12[0], array12[0], array12[0]);
						proceduralCookingEffectWithTrigger.Init(gameObject13);
						gameObject13.ApplyEffect(proceduralCookingEffectWithTrigger);
						return;
					}
					if (Wish == "purgeobjectcache!")
					{
						The.ZoneManager.CachedObjects.Clear();
						MessageQueue.AddPlayerMessage("Purged object cache.");
						return;
					}
					if (Wish.StartsWith("unequip:"))
					{
						string requiredPart = Wish.Split(':')[1];
						GameObject gameObject14 = who;
						GameObject equipped = gameObject14.Body.GetPartByName(requiredPart).Equipped;
						gameObject14.Body.GetPartByName(requiredPart)._Equipped = null;
						gameObject14.Inventory.AddObject(equipped);
						return;
					}
					switch (Wish)
					{
					case "objtest":
					{
						GameObject gameObject15 = null;
						Stopwatch stopwatch = Stopwatch.StartNew();
						for (int num43 = 0; num43 < 100; num43++)
						{
							gameObject15 = GameObjectFactory.Factory.CreateObject("Doru");
						}
						stopwatch.Stop();
						Stopwatch stopwatch2 = Stopwatch.StartNew();
						for (int num44 = 0; num44 < 100; num44++)
						{
							gameObject15 = gameObject15.DeepCopy();
						}
						stopwatch2.Stop();
						UnityEngine.Debug.Log("MS create: " + stopwatch.Elapsed.ToString());
						UnityEngine.Debug.Log("MS clone: " + stopwatch2.Elapsed.ToString());
						return;
					}
					case "memcheck":
						MessageQueue.AddPlayerMessage("Before Total Memory: " + GC.GetTotalMemory(forceFullCollection: false));
						MessageQueue.AddPlayerMessage("After Total Memory: " + GC.GetTotalMemory(forceFullCollection: true));
						return;
					case "makevillage":
					{
						Cell currentCell5 = who.pPhysics.CurrentCell;
						Zone parentZone5 = who.pPhysics.CurrentCell.ParentZone;
						who.pPhysics.CurrentCell.RemoveObject(who);
						try
						{
							Village village = new Village();
							History sultanHistory4 = The.Game.sultanHistory;
							sultanHistory4.currentYear = 1000 + Stat.Random(400, 900);
							HistoricEntity newEntity3 = sultanHistory4.GetNewEntity(sultanHistory4.currentYear);
							newEntity3.ApplyEvent(new InitializeVillage(new string[11]
							{
								"DesertCanyon", "Saltdunes", "Saltmarsh", "Hills", "Water", "BananaGrove", "Mountains", "Flowerfields", "Jungle", "Ruins",
								"Ruins"
							}.GetRandomElement()));
							village.villageEntity = newEntity3;
							village.BuildZone(parentZone5);
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogException(exception);
						}
						currentCell5.AddObject(who);
						return;
					}
					case "villageprops":
						MessageQueue.AddPlayerMessage("Listing villageEntity if one exists...");
						{
							foreach (ZoneBuilderBlueprint item21 in The.ZoneManager.GetBuildersFor(who.GetCurrentCell().ParentZone))
							{
								if (item21.Class == "Village")
								{
									HistoricEntitySnapshot currentSnapshot7 = (item21.Parameters["villageEntity"] as HistoricEntity).GetCurrentSnapshot();
									MessageQueue.AddPlayerMessage(currentSnapshot7.ToString());
									UnityEngine.Debug.Log(currentSnapshot7.ToString());
								}
							}
							return;
						}
					}
					if (Wish.StartsWith("listtags:"))
					{
						string text14 = Wish.Split(':')[1];
						MessageQueue.AddPlayerMessage("Listing the tags of " + text14 + "...");
						{
							foreach (KeyValuePair<string, string> tag in GameObjectFactory.Factory.Blueprints[text14].Tags)
							{
								MessageQueue.AddPlayerMessage(tag.Key + " = " + tag.Value);
							}
							return;
						}
					}
					if (Wish.StartsWith("goto:"))
					{
						string zoneID3 = Wish.Split(':')[1];
						Zone zone5 = The.ZoneManager.GetZone(zoneID3);
						Point2D pos2D2 = who.pPhysics.CurrentCell.Pos2D;
						who.pPhysics.CurrentCell.RemoveObject(who.pPhysics.ParentObject);
						zone5.GetCell(pos2D2).AddObject(who);
						The.ZoneManager.SetActiveZone(zone5.ZoneID);
						The.ZoneManager.ProcessGoToPartyLeader();
						return;
					}
					if (Wish.StartsWith("revealsecret:"))
					{
						JournalMapNote mapNote = JournalAPI.GetMapNote(Wish.Split(':')[1]);
						if (mapNote != null && !mapNote.revealed)
						{
							JournalAPI.RevealMapNote(mapNote);
						}
						return;
					}
					if (Wish == "sheeter")
					{
						global::Sheeter.Sheeter.MonsterSheeter();
						return;
					}
					if (Wish == "factionsheeter")
					{
						global::Sheeter.Sheeter.FactionSheeter();
						return;
					}
					if (Wish.StartsWith("removepart:"))
					{
						string text15 = Wish.Split(':')[1];
						who.RemovePart(text15);
						MessageQueue.AddPlayerMessage("Removed part " + text15 + " from player body.");
						return;
					}
					if (Wish.StartsWith("spawn:"))
					{
						string[] array13 = Wish.Split(':');
						for (int num45 = 0; num45 < 16; num45++)
						{
							The.Game.ActionManager.AddActiveObject(who.CurrentCell.ParentZone.GetCells((Cell c) => !c.Explored).ShuffleInPlace().GetRandomElement()
								.AddObject(array13[1]));
						}
						return;
					}
					if (Wish.StartsWith("othowander1"))
					{
						OthoWander1.begin();
						return;
					}
					switch (Wish)
					{
					case "reshephgospel":
						JournalAPI.GetNotesForResheph().ForEach(delegate(JournalSultanNote g)
						{
							g.Reveal();
						});
						return;
					case "markofdeath":
						The.Game.Player.Body.ToggleMarkOfDeath();
						MessageQueue.AddPlayerMessage("Mark of death now " + The.Game.Player.Body.HasMarkOfDeath());
						return;
					case "markofdeath?":
						MessageQueue.AddPlayerMessage("Mark of death is: " + The.Game.GetStringGameState("MarkOfDeath"));
						if (The.Game.Player.Body.HasMarkOfDeath())
						{
							MessageQueue.AddPlayerMessage("HAS mark of death");
						}
						else
						{
							MessageQueue.AddPlayerMessage("DOES NOT HAVE mark of death");
						}
						return;
					case "goclam":
					{
						Cell randomElement = The.Game.RequireSystem(() => new ClamSystem()).GetClamZone().GetCells()
							.GetRandomElement();
						The.Player.SetLongProperty("ClamTeleportTurn", The.Game.Turns);
						The.Player.TeleportTo(randomElement, 0);
						return;
					}
					case "hydropon":
						The.Player.ZoneTeleport(The.Game.GetStringGameState("HydroponZoneID"));
						return;
					case "thinworld":
					{
						GameObject body3 = The.Game.Player.Body;
						body3.GetCurrentCell().GetCellFromDirection("NW").AddObject("SultanSarcophagusWPeriod6");
						ThinWorld.TransitToThinWorld(body3.GetCurrentCell().GetCellFromDirection("N").AddObject("SultanSarcophagusEPeriod6"));
						return;
					}
					case "thinworldx":
					{
						GameObject body2 = The.Game.Player.Body;
						body2.GetCurrentCell().GetCellFromDirection("NW").AddObject("SultanSarcophagusWPeriod6");
						ThinWorld.TransitToThinWorld(body2.GetCurrentCell().GetCellFromDirection("N").AddObject("SultanSarcophagusEPeriod6"), express: true);
						return;
					}
					case "returntoqud":
						ThinWorld.ReturnToQud();
						return;
					case "somethinggoeswrong":
						ThinWorld.SomethingGoesWrong(The.Game.Player.Body);
						return;
					case "sultanmuralwalltest":
					{
						foreach (Cell cell9 in The.Game.Player.Body.pPhysics.CurrentCell.ParentZone.GetCells())
						{
							if (cell9.Y % 2 == 0)
							{
								cell9.ClearWalls();
								cell9.AddObject("SultanMuralWall");
							}
						}
						return;
					}
					case "sultanmuralwalltest2":
					{
						Zone parentZone6 = The.Game.Player.Body.pPhysics.CurrentCell.ParentZone;
						for (int num47 = 0; num47 < 12; num47++)
						{
							Cell cell4 = parentZone6.GetCell(num47, 0);
							cell4.ClearWalls();
							cell4.AddObject("SultanMuralWall");
						}
						return;
					}
					case "zonetier":
						MessageQueue.AddPlayerMessage(The.Game.Player.Body.CurrentCell.ParentZone.NewTier.ToString());
						return;
					case "exception":
					{
						int num46 = 0;
						_ = 10 / num46;
						return;
					}
					case "buildscriptmods":
						ModManager.BuildScriptMods();
						return;
					case "iamconfused":
						XRLCore.player.ApplyEffect(new XRL.World.Effects.Confused(80, 10, 0));
						return;
					case "clearprefs":
						GameManager.Instance.uiQueue.queueTask(delegate
						{
							PlayerPrefs.DeleteAll();
						});
						MessageQueue.AddPlayerMessage("Player prefs cleared");
						return;
					}
					if (Wish.StartsWith("placeobjecttest:"))
					{
						string[] array14 = Wish.Split(':');
						if (GameObjectFactory.Factory.CreateObject(array14[1]) != null)
						{
							for (int num48 = 0; num48 < 200; num48++)
							{
								ZoneBuilderSandbox.PlaceObject(array14[1], The.Game.Player.Body.CurrentZone);
							}
						}
						return;
					}
					switch (Wish)
					{
					case "allloved":
						foreach (string factionName in Factions.getFactionNames())
						{
							The.Game.PlayerReputation.set(factionName, 2000);
						}
						MessageQueue.AddPlayerMessage("Factionrep set to all loved.");
						return;
					case "allhated":
						foreach (string factionName2 in Factions.getFactionNames())
						{
							The.Game.PlayerReputation.set(factionName2, -2000);
						}
						MessageQueue.AddPlayerMessage("Factionrep set to all hated.");
						return;
					case "cloacasurprise":
						new CloacaSurprise().ApplyEffectsTo(The.Player);
						return;
					case "cryotube":
					{
						PossibleCryotube possibleCryotube = new PossibleCryotube();
						possibleCryotube.Chance = 10000;
						possibleCryotube.BuildZone(The.Player.CurrentZone);
						return;
					}
					}
					if (Wish.StartsWith("redrock:"))
					{
						string zoneID4 = "JoppaWorld.11.20.1.1." + Wish.Split(':')[1];
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID4).GetCell(40, 24));
						return;
					}
					switch (Wish)
					{
					case "ensurevoids":
						ZoneBuilderSandbox.EnsureAllVoidsConnected(The.Player.CurrentZone);
						return;
					case "delaytest":
						The.Player.PlayWorldSound("hiss_high", 0.5f, 0f, combat: false, 4f);
						CombatJuice.playWorldSound(The.Player, "hiss_low", 0.5f, 0f, 0f, 3f);
						return;
					case "idkfa":
						The.Core.IDKFA = !The.Core.IDKFA;
						MessageQueue.AddPlayerMessage("Godmode now " + The.Core.IDKFA);
						return;
					case "clearnavcache":
						The.ZoneManager.ActiveZone.ClearNavigationCaches();
						return;
					case "eviltwin":
						EvilTwin.CreateEvilTwin(The.Game.Player.Body, "Evil");
						return;
					case "roadtest":
					{
						Zone currentZone2 = The.Game.Player.Body.CurrentZone;
						currentZone2.AddZoneConnection("-", 0, 14, "Road", null);
						currentZone2.AddZoneConnection("-", 79, 14, "Road", null);
						new RoadBuilder().BuildZone(currentZone2);
						return;
					}
					case "rivertest":
					{
						Zone currentZone = The.Game.Player.Body.CurrentZone;
						currentZone.AddZoneConnection("-", 40, 0, "RiverMouth", null);
						currentZone.AddZoneConnection("-", 40, 24, "RiverMouth", null);
						new RiverBuilder().BuildZone(currentZone);
						return;
					}
					case "testendmessage":
						PaxKlanqIPresume.UnderConstructionMessage();
						return;
					case "crossintobright":
						ThinWorld.CrossIntoBrightsheol();
						return;
					case "eventtest":
					{
						Stopwatch stopwatch3 = new Stopwatch();
						stopwatch3.Reset();
						stopwatch3.Start();
						EndTurnEvent e = new EndTurnEvent();
						for (int num49 = 0; num49 < 100000; num49++)
						{
							The.Game.Player.Body.HandleEvent(e);
						}
						UnityEngine.Debug.Log("1m events in " + stopwatch3.Elapsed.ToString());
						return;
					}
					case "shove":
					{
						string direction = PickDirection.ShowPicker();
						The.Game.Player.Body.GetCurrentCell().GetCellFromDirection(direction).GetObjectsWithPart("Physics")[0].Move(direction, Forced: true);
						return;
					}
					case "weather":
					{
						Zone parentZone7 = The.Game.Player.Body.CurrentCell.ParentZone;
						StringBuilder stringBuilder16 = Event.NewStringBuilder();
						stringBuilder16.Append("HasWeather: ").Append(parentZone7.HasWeather).Append('\n')
							.Append("WindSpeed: ")
							.Append(parentZone7.WindSpeed)
							.Append('\n')
							.Append("WindDirections: ")
							.Append(parentZone7.WindDirections)
							.Append('\n')
							.Append("WindDuration: ")
							.Append(parentZone7.WindDuration)
							.Append('\n')
							.Append("CurrentWindSpeed: ")
							.Append(parentZone7.CurrentWindSpeed)
							.Append('\n')
							.Append("CurrentWindDirection: ")
							.Append(parentZone7.CurrentWindDirection)
							.Append('\n')
							.Append("NextWindChange: +")
							.Append(parentZone7.NextWindChange - The.Game.TimeTicks)
							.Append('\n');
						Popup.Show(stringBuilder16.ToString());
						return;
					}
					case "fungalvision":
						FungalVisionary.VisionLevel = 1;
						return;
					case "masterchef":
					{
						Popup.bSuppressPopups = true;
						who.TakeObject("Fermented Yuckwheat Stem", 10, Silent: false, 0);
						who.TakeObject("Voider Gland Paste", 10, Silent: false, 0);
						who.TakeObjectsFromPopulation("Ingredients_MidTiers", 20, null, Silent: false, 0);
						List<string> list17 = new List<string>
						{
							"Phase Silk", "Starapple Preserves", "Cured Dawnglider Tail", "Pickled Mushrooms", "Fermented Yondercane", "Spark Tick Plasma", "Spine Fruit Jam", "Vinewafer Sheaf", "Goat Jerky", "Beetle Jerky",
							"Pickles", "Fire Ant Gaster Paste", "Voider Gland Paste", "Congealed Shade Oil", "CiderPool"
						};
						for (int num50 = 0; num50 < 6; num50++)
						{
							string randomElement2 = list17.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list17.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list17.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGamestate.LearnRecipe(CookingRecipe.FromIngredients(new List<GameObject>
							{
								GameObjectFactory.Factory.CreateObject(randomElement2),
								GameObjectFactory.Factory.CreateObject(randomElement3),
								GameObjectFactory.Factory.CreateObject(randomElement4)
							}, null, who.DisplayNameOnlyStripped));
						}
						for (int num51 = 0; num51 < 3; num51++)
						{
							string randomElement2 = list17.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list17.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list17.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGamestate.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement2, randomElement3 }, null, who.DisplayNameOnlyStripped));
						}
						for (int num52 = 0; num52 < 1; num52++)
						{
							string randomElement2 = list17.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list17.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list17.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGamestate.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement2 }, null, who.DisplayNameOnlyStripped));
						}
						CookingGamestate.LearnRecipe(new HotandSpiny());
						AddSkill("CookingAndGathering");
						AddSkill("CookingAndGathering_Harvestry");
						AddSkill("CookingAndGathering_Butchery");
						AddSkill("CookingAndGathering_Spicer");
						AddSkill("CookingAndGathering_CarbideChef");
						MessageQueue.AddPlayerMessage("Added cooking knowledge.");
						Popup.bSuppressPopups = false;
						return;
					}
					}
					if (Wish.StartsWith("gamestate:"))
					{
						string[] array15 = Wish.Split(':');
						if (array15.Length == 2)
						{
							MessageQueue.AddPlayerMessage(array15[1] + "=" + The.Game.GetStringGameState(array15[1]));
						}
						else if (array15[2] == "null")
						{
							The.Game.StringGameState.Set(array15[1], null);
						}
						else
						{
							The.Game.StringGameState.Set(array15[1], array15[2]);
						}
						return;
					}
					switch (Wish)
					{
					case "cyber":
					{
						who.GetCurrentCell().GetCellFromDirection("N").AddObject("CyberneticsTerminal2");
						GameObject gameObject16 = who.GetCurrentCell().GetCellFromDirection("NW").AddObject("CyberneticsStationRack");
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("DermalPlating"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("DermalInsulation"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("BiologicalIndexer"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("TechnologicalIndexer"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("NightVision"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("HyperElasticAnkleTendons"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("ParabolicMuscularSubroutine"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CherubicVisage"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("TranslucentSkin"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("StabilizerArmLocks"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("RapidReleaseFingerFlexors"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CarbideHandBones"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("Pentaceps"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("MotorizedTreads"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("InflatableAxons"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("NocturnalApex"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("ElectromagneticSensor"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("MatterRecompositer"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("AirCurrentMicrosensor"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CustomVisage"));
						gameObject16.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("GunRack"));
						foreach (GameObject item22 in gameObject16.Inventory.GetObjectsDirect())
						{
							item22.MakeUnderstood();
						}
						who.TakeObject("CyberneticsCreditWedge", 12, Silent: false, 0);
						return;
					}
					case "filljournal":
					{
						for (int num53 = 0; num53 < 100; num53++)
						{
							JournalAPI.AddAccomplishment("Afkfasdf ajsd fa fs adkf as dfas dfk asdlf a f asdf a" + Guid.NewGuid().ToString(), null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
						}
						return;
					}
					case "wavetilegen":
						GameManager.Instance.uiQueue.queueTask(delegate
						{
							Texture2D texture2D = new Texture2D(80, 25, TextureFormat.ARGB32, mipChain: false)
							{
								filterMode = UnityEngine.FilterMode.Point
							};
							for (int num61 = 0; num61 < 80; num61++)
							{
								for (int num62 = 0; num62 < 25; num62++)
								{
									Cell cell8 = who.pPhysics.CurrentCell.ParentZone.GetCell(num61, num62);
									if (cell8.HasObjectWithTag("Wall"))
									{
										texture2D.SetPixel(num61, num62, new Color32(0, 0, 0, byte.MaxValue));
									}
									else if (cell8.HasObjectWithPart("Combat"))
									{
										texture2D.SetPixel(num61, num62, new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
									}
									else if (cell8.HasObjectWithPart("LiquidVolume"))
									{
										texture2D.SetPixel(num61, num62, new Color32(0, 0, byte.MaxValue, byte.MaxValue));
									}
									else if (cell8.HasObjectWithPart("PlantProperties"))
									{
										texture2D.SetPixel(num61, num62, new Color32(0, byte.MaxValue, 0, byte.MaxValue));
									}
									else if (cell8.HasObjectWithPart("Door"))
									{
										texture2D.SetPixel(num61, num62, new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
									}
									else if (cell8.HasObjectWithPart("Furniture"))
									{
										texture2D.SetPixel(num61, num62, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
									}
									else
									{
										texture2D.SetPixel(num61, num62, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
									}
								}
							}
							File.WriteAllBytes(DataManager.SavePath("tilegen.png"), texture2D.EncodeToPNG());
						});
						return;
					case "slow":
						who.Statistics["Speed"].BaseValue = 25;
						return;
					case "fast":
						who.Statistics["Speed"].BaseValue = 500;
						return;
					}
					if (Wish.StartsWith("testpop"))
					{
						string[] array16 = Wish.Split(' ');
						List<PopulationResult> list18 = ((array16.Length > 2) ? PopulationManager.Generate(array16[1], array16[2], array16[3]) : PopulationManager.Generate(array16[1]));
						MessageQueue.AddPlayerMessage("-- generating " + array16[1] + " ---");
						{
							foreach (PopulationResult item23 in list18)
							{
								MessageQueue.AddPlayerMessage(item23.Blueprint + " x" + item23.Number + ((!string.IsNullOrEmpty(item23.Hint)) ? (" hint:" + item23.Hint) : ""));
							}
							return;
						}
					}
					switch (Wish)
					{
					case "maxmod":
						The.Core.CheatMaxMod = !The.Core.CheatMaxMod;
						return;
					case "night":
						The.Game.TimeTicks += (10000 - Calendar.CurrentDaySegment) / 10;
						return;
					case "day":
						The.Game.TimeTicks += (3250 - Calendar.CurrentDaySegment) / 10;
						return;
					case "daze":
						game.Player.Body.ApplyEffect(new Dazed(Stat.Random(2, 8)));
						return;
					case "stun":
						game.Player.Body.ApplyEffect(new Stun(Stat.Random(2, 8), 30));
						return;
					case "dude":
					{
						GameObject oneCreatureFromZone = The.ZoneManager.GetOneCreatureFromZone(The.ZoneManager.ActiveZone.ZoneID);
						if (oneCreatureFromZone != null)
						{
							Popup.Show(oneCreatureFromZone.DisplayName);
						}
						return;
					}
					}
					if (Wish.StartsWith("effect:"))
					{
						string text16 = Wish.Split(':')[1];
						who.ApplyEffect(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text16)) as Effect);
						return;
					}
					switch (Wish)
					{
					case "lost":
						who.ApplyEffect(new Lost(1));
						return;
					case "notlost":
						who.RemoveEffect("Lost");
						return;
					case "clone":
					{
						string text21 = PickDirection.ShowPicker();
						if (text21 != null)
						{
							Cell cellFromDirection5 = who.pPhysics.CurrentCell.GetCellFromDirection(text21);
							if (cellFromDirection5 != null)
							{
								GameObject gO = who.DeepCopy();
								cellFromDirection5.AddObject(gO);
							}
						}
						return;
					}
					case "copy":
					{
						string text22 = PickDirection.ShowPicker();
						if (text22 != null)
						{
							Cell cellFromDirection6 = who.pPhysics.CurrentCell.GetCellFromDirection(text22);
							if (cellFromDirection6 != null)
							{
								GameObject combatTarget = cellFromDirection6.GetCombatTarget();
								combatTarget.pPhysics.CurrentCell.GetEmptyAdjacentCells().GetRandomElement().AddObject(combatTarget.DeepCopy());
							}
						}
						return;
					}
					case "RandomNoTakeObject":
					{
						GameObject anObject = EncountersAPI.GetAnObject((GameObjectBlueprint o) => !o.HasTag("Item"));
						who.GetCurrentCell().GetFirstEmptyAdjacentCell().AddObject(anObject);
						return;
					}
					case "expand":
						Popup.Show(HistoricStringExpander.ExpandString("<spice.quests.questContext.itemNameMutation.!random>"));
						return;
					case "explodingpalm":
					{
						string text18 = PickDirection.ShowPicker();
						if (text18 == null)
						{
							return;
						}
						Cell cellFromDirection2 = who.pPhysics.CurrentCell.GetCellFromDirection(text18);
						if (cellFromDirection2 != null)
						{
							GameObject firstObjectWithPart = cellFromDirection2.GetFirstObjectWithPart("Combat");
							for (int num54 = 0; num54 < 6; num54++)
							{
								Axe_Dismember.Dismember(who, firstObjectWithPart);
							}
							Axe_Decapitate.Decapitate(who, firstObjectWithPart);
						}
						return;
					}
					case "swap":
					{
						string text20 = PickDirection.ShowPicker();
						if (text20 == null)
						{
							return;
						}
						Cell cellFromDirection4 = (who.GetPart("Physics") as XRL.World.Parts.Physics).CurrentCell.GetCellFromDirection(text20);
						if (cellFromDirection4 != null)
						{
							List<GameObject> objectsWithPart2 = cellFromDirection4.GetObjectsWithPart("Combat");
							if (objectsWithPart2.Count > 0)
							{
								game.Player.Body = objectsWithPart2[0];
							}
						}
						return;
					}
					case "svardymstorm":
						The.Game.GetSystem<SvardymSystem>().BeginStorm();
						return;
					case "beguile":
					{
						string text19 = PickDirection.ShowPicker();
						if (text19 == null)
						{
							return;
						}
						Cell cellFromDirection3 = (who.GetPart("Physics") as XRL.World.Parts.Physics).CurrentCell.GetCellFromDirection(text19);
						if (cellFromDirection3 != null)
						{
							List<GameObject> objectsWithPart = cellFromDirection3.GetObjectsWithPart("Brain");
							if (objectsWithPart.Count > 0)
							{
								objectsWithPart[0].pBrain.Goals.Clear();
								objectsWithPart[0].pBrain.PartyLeader = game.Player.Body;
								MessageQueue.AddPlayerMessage(objectsWithPart[0].DisplayName + " is now your follower.");
							}
						}
						return;
					}
					case "blueprint":
					{
						string text17 = PickDirection.ShowPicker();
						if (text17 == null)
						{
							return;
						}
						Cell cellFromDirection = game.Player.Body.CurrentCell.GetCellFromDirection(text17);
						if (cellFromDirection != null)
						{
							GameObject firstObject = cellFromDirection.GetFirstObject();
							if (firstObject != null)
							{
								Popup.Show(firstObject.Blueprint);
							}
						}
						return;
					}
					case "supermutant":
					{
						Type[] types = Assembly.GetExecutingAssembly().GetTypes();
						foreach (Type type3 in types)
						{
							if (!type3.FullName.Contains("Parts.Mutation"))
							{
								continue;
							}
							Mutations mutations = who.GetPart("Mutations") as Mutations;
							try
							{
								if (Activator.CreateInstance(type3) is BaseMutation baseMutation && baseMutation.Name != "FearAura" && baseMutation.GetMutationEntry() != null)
								{
									mutations.AddMutation(baseMutation, 1);
								}
							}
							catch
							{
							}
						}
						return;
					}
					case "decapitateme":
						Axe_Decapitate.Decapitate(who, who);
						return;
					case "optionspop":
						Popup.ShowOptionList("Title only - short", new string[3] { "Option 1", "Option 2", "Option 3" }, null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - short - keymap", new string[3] { "Option 1", "Option 2", "Option 3" }, new char[3] { 'a', 'b', 'c' }, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - newlines - no respect", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - newlines - no respect - keymap", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, new char[3] { 'a', 'b', 'c' }, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - no respect", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, 0, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - no respect - spacing", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, 1, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: false, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - respect - spacing", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, 1, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: true, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - respect - spacing - scrolling", new string[16]
						{
							"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines",
							"Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines"
						}, null, 1, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: true, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - respect - scrolling", new string[28]
						{
							"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines",
							"Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines",
							"Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines"
						}, null, 0, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: true, AllowEscape: true);
						Popup.ShowOptionList("Title - intro - newlines - no respect - scrolling", new string[28]
						{
							"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines",
							"Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines",
							"Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines"
						}, null, 0, "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", 60, RespectOptionNewlines: false, AllowEscape: true);
						return;
					}
					if (Wish.StartsWith("checkforpart:"))
					{
						string text23 = Wish.Split(':')[1];
						Popup.Show(who.HasPart(text23) ? ("Player has " + text23 + " part") : ("No " + text23 + " part on player"));
						return;
					}
					if (Wish.StartsWith("item:"))
					{
						string[] array17 = Wish.Split(':');
						string result = WishSearcher.SearchForBlueprint(array17[1]).Result;
						int n3 = 1;
						if (array17.Length > 2)
						{
							n3 = Convert.ToInt32(array17[2]);
						}
						Cell cell5 = who.GetCurrentCell();
						if (!array17.Contains("here"))
						{
							cell5 = cell5.GetFirstEmptyAdjacentCell();
						}
						cell5.AddObject(result, n3);
						return;
					}
					if (Wish.StartsWith("understandpartial:"))
					{
						string[] array18 = Wish.Split(':');
						GameObject gameObject17 = GameObjectFactory.create(WishSearcher.SearchForBlueprint(array18[1]).Result);
						gameObject17.SetIntProperty("PartiallyUnderstood", 1);
						Cell cell6 = who.GetCurrentCell();
						if (!array18.Contains("here"))
						{
							cell6 = cell6.GetFirstEmptyAdjacentCell();
						}
						cell6.AddObject(gameObject17);
						return;
					}
					if (Wish.StartsWith("factionheirloom:"))
					{
						who.CurrentCell.AddObject(Factions.get(Wish.Split(':')[1]).GetHeirloom());
						return;
					}
					if (Wish.StartsWith("setstringgamestate:"))
					{
						string[] array19 = Wish.Split(':');
						The.Game.SetStringGameState(array19[1], array19[2]);
						return;
					}
					if (Wish.StartsWith("getstringgamestate:"))
					{
						string[] array20 = Wish.Split(':');
						string stringGameState = The.Game.GetStringGameState(array20[1]);
						if (stringGameState == null)
						{
							Popup.Show(array20[1] + ": null");
						}
						else
						{
							Popup.Show(array20[1] + ": \"" + stringGameState + "\"");
						}
						return;
					}
					if (Wish == "highreg")
					{
						Popup.Show("Registration statistics are only available in the Unity editor.");
						return;
					}
					if (Wish.StartsWith("hasblueprintbeenseen:"))
					{
						string[] array21 = Wish.Split(':');
						bool flag4 = The.Game.HasBlueprintBeenSeen(array21[1]);
						Popup.Show(array21[1] + ": " + (flag4 ? "yes" : "no"));
						return;
					}
					if (Wish.StartsWith("pluralize:"))
					{
						Popup.Show(Grammar.Pluralize(Wish.Split(':')[1]));
						return;
					}
					if (Wish.StartsWith("a:"))
					{
						Popup.Show(Grammar.A(Wish.Split(':')[1]));
						return;
					}
					if (Wish.StartsWith("opinion:"))
					{
						Popup.Show(Brain.GetOpinion(Wish.Split(':')[1], The.Player).ToString());
						return;
					}
					if (Wish.StartsWith("wordrel:"))
					{
						string Input = Wish.Split(':')[1];
						List<string> relatedWords = WordDataManager.GetRelatedWords(ref Input);
						if (relatedWords == null)
						{
							Popup.Show(Input + ": no results", CopyScrap: true, Capitalize: false, DimBackground: true, LogMessage: false);
						}
						else
						{
							Popup.Show(Input + ": " + string.Join(", ", relatedWords.ToArray()), CopyScrap: true, Capitalize: false, DimBackground: true, LogMessage: false);
						}
						return;
					}
					switch (Wish)
					{
					case "highpools":
						Popup.Show(MinEvent.GetTopPoolCountReport());
						return;
					case "enablemarkup":
						Markup.Enabled = true;
						return;
					case "disablemarkup":
						Markup.Enabled = false;
						return;
					}
					if (!(Wish != ""))
					{
						return;
					}
					if (Wish == "objdump")
					{
						TextWriter textWriter = new StreamWriter(DataManager.SavePath("ObjectDump.txt"));
						foreach (string key4 in GameObjectFactory.Factory.Blueprints.Keys)
						{
							string text24 = "?";
							GameObject gameObject18 = GameObjectFactory.Factory.CreateObject(key4, -200);
							if (gameObject18.HasPart("Description"))
							{
								text24 = gameObject18.GetPart<Description>()._Short;
							}
							gameObject18.MakeUnderstood();
							string displayNameOnly = gameObject18.DisplayNameOnly;
							textWriter.WriteLine(ConsoleLib.Console.ColorUtility.StripFormatting(displayNameOnly.Substring(0, displayNameOnly.Length - 2).Replace("[", "").Replace("]", "")) + "," + text24.Replace(',', ';'));
						}
						textWriter.Close();
						textWriter.Dispose();
					}
					WishResult wishResult2 = WishSearcher.SearchForWish(Wish);
					if (wishResult2.Type == WishResultType.Quest)
					{
						The.Game.StartQuest(wishResult2.Result);
						return;
					}
					if (wishResult2.Type == WishResultType.Mutation)
					{
						Type type4 = ModManager.ResolveType(wishResult2.Result);
						(who.GetPart("Mutations") as Mutations).AddMutation((BaseMutation)Activator.CreateInstance(type4), 1);
						return;
					}
					if (wishResult2.Type == WishResultType.Blueprint)
					{
						bool flag5 = false;
						int num55 = 1;
						if (char.ToUpper(Wish[Wish.Length - 1]) == 'S')
						{
							num55 = Stat.RandomCosmetic(2, 4);
						}
						for (int num56 = 0; num56 < num55; num56++)
						{
							foreach (Cell adjacentCell in who.CurrentCell.GetAdjacentCells())
							{
								if (adjacentCell.IsEmpty())
								{
									GameObject gameObject19 = GameObject.create(wishResult2.Result);
									adjacentCell.AddObject(gameObject19);
									gameObject19.MakeActive();
									flag5 = true;
									break;
								}
							}
						}
						if (!flag5)
						{
							Popup.Show("No adjacent empty squares to create your wish!");
						}
						return;
					}
					if (wishResult2.Type != WishResultType.Zone)
					{
						return;
					}
					The.ZoneManager.SetActiveZone(wishResult2.Result);
					Cell cell7 = null;
					for (int num57 = 23; num57 >= 0; num57--)
					{
						for (int num58 = 40; num58 >= 0; num58--)
						{
							if (The.ZoneManager.ActiveZone.GetCell(num58, num57).IsReachable() && The.ZoneManager.ActiveZone.GetCell(num58, num57).IsEmpty())
							{
								cell7 = The.ZoneManager.ActiveZone.GetCell(num58, num57);
								break;
							}
							if (The.ZoneManager.ActiveZone.GetCell(num58, num57).IsReachable() && The.ZoneManager.ActiveZone.GetCell(num58, num57).IsEmpty())
							{
								cell7 = The.ZoneManager.ActiveZone.GetCell(num58, num57);
								break;
							}
							if (The.ZoneManager.ActiveZone.GetCell(40 - num58, num57).IsReachable() && The.ZoneManager.ActiveZone.GetCell(40 - num58, num57).IsEmpty())
							{
								cell7 = The.ZoneManager.ActiveZone.GetCell(40 - num58, num57);
								break;
							}
						}
						if (cell7 != null)
						{
							break;
						}
					}
					who.SystemMoveTo(cell7);
					The.ZoneManager.ProcessGoToPartyLeader();
					return;
				}
				case "maket2":
					return;
				case "worldbmp":
					return;
				}
			}
			goto case "stage2";
		case "stage2":
		case "stage3a":
		case "stage3":
		case "stage4":
		case "stage5":
		case "stage6":
		case "stage8":
		{
			Popup.bSuppressPopups = true;
			ItemNaming.Suppress = true;
			who.AwardXP(15000);
			game.CompleteQuest("Fetch Argyve a Knickknack");
			game.CompleteQuest("Fetch Argyve Another Knickknack");
			game.CompleteQuest("Weirdwire Conduit... Eureka!");
			game.StartQuest("A Canticle for Barathrum");
			XRL.World.Parts.Physics physics = who.GetPart("Physics") as XRL.World.Parts.Physics;
			if (Wish == "stage2")
			{
				The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.1.10");
				who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(0, 0));
				The.ZoneManager.ProcessGoToPartyLeader();
				who.TakeObjectsFromEncounterTable("Junk 1", Stat.Random(1, 4), Silent: false, 0);
				who.TakeObjectsFromEncounterTable("Meds 1", Stat.Random(1, 4), Silent: false, 0);
			}
			if (Wish == "stage3")
			{
				who.TakeObject("Steel Plate Mail", Silent: false, 0);
				who.TakeObject("Steel Buckler", Silent: false, 0);
				who.TakeObject("Steel Boots", Silent: false, 0);
				who.TakeObject("Steel Gauntlets", Silent: false, 0);
				if (who.HasSkill("Cudgel"))
				{
					who.TakeObject("Cudgel3", Silent: false, 0);
				}
				if (who.HasSkill("ShortBlades"))
				{
					who.TakeObject("Dagger3", Silent: false, 0);
				}
				if (who.HasSkill("LongBlades"))
				{
					who.TakeObject("Long Sword3", Silent: false, 0);
				}
				if (who.HasSkill("Axe"))
				{
					who.TakeObject("Battle Axe3", Silent: false, 0);
				}
				who.TakeObjectsFromPopulation("Junk 1", Stat.Random(1, 4), null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Junk 2", Stat.Random(1, 3), null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Meds 1", Stat.Random(1, 3), null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Meds 2", Stat.Random(1, 2), null, Silent: false, 0);
			}
			if (Wish == "stage4")
			{
				who.TakeObject("Carbide Plate Armor", Silent: false, 0);
				who.TakeObject("Steel Buckler", Silent: false, 0);
				who.TakeObject("Chain Coif", Silent: false, 0);
				who.TakeObject("Steel Boots", Silent: false, 0);
				who.TakeObject("Steel Gauntlets", Silent: false, 0);
				who.TakeObject("MasterworkCarbine", Silent: false, 0);
				who.TakeObject("UbernostrumTonic", Silent: false, 0);
				who.TakeObject("Sowers_Seed", 12, Silent: false, 0);
				who.TakeObject("Fixit Spray", Silent: false, 0);
				who.TakeObject("SalveTonic", 6, Silent: false, 0);
				who.TakeObject("Floating Glowsphere", Silent: false, 0);
				who.TakeObject("Ironweave Cloak", Silent: false, 0);
				if (who.HasSkill("Cudgel"))
				{
					who.TakeObject("Cudgel4", Silent: false, 0);
				}
				if (who.HasSkill("ShortBlades"))
				{
					who.TakeObject("Dagger4", Silent: false, 0);
				}
				if (who.HasSkill("LongBlades"))
				{
					who.TakeObject("Long Sword4", Silent: false, 0);
				}
				if (who.HasSkill("Axe"))
				{
					who.TakeObject("Battle Axe4", Silent: false, 0);
				}
				who.TakeObjectsFromPopulation("Junk 3", 2, null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Junk 4", 2, null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Meds 3", 3, null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Meds 4", 2, null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Artifact 3", 3, null, Silent: false, 0);
				who.TakeObjectsFromPopulation("Artifact 4", 2, null, Silent: false, 0);
			}
			who.TakeObject("Droid Scrambler", Silent: false, 0);
			who.TakeObject("Joppa Recoiler", Silent: false, 0);
			who.TakeObject("Borderlands Revolver", Silent: false, 0);
			who.TakeObject("Lead Slug", 500, Silent: false, 0);
			if (Wish == "stage3a")
			{
				who.AwardXP(15000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.StartQuest("Decoding the Signal");
				game.FinishedQuestStep("Decoding the Signal~Decode the Signal");
				The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
				The.ZoneManager.ActiveZone.GetCell(33, 16).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
				GritGateScripts.OpenRank1Doors();
			}
			if (Wish == "stage3")
			{
				who.AwardXP(15000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
				The.ZoneManager.ActiveZone.GetCell(33, 16).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
				GritGateScripts.OpenRank1Doors();
			}
			if (Wish == "stage4")
			{
				who.AwardXP(75000);
				The.ZoneManager.SetActiveZone("JoppaWorld");
				The.ZoneManager.ActiveZone.GetCell(25, 3).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			if (Wish == "stage5")
			{
				who.AwardXP(210000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("Decoding the Signal");
				game.StartQuest("The Earl of Omonporch");
				game.FinishQuestStep("The Earl of Omonporch", "Travel to Omonporch");
				game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
				The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
				The.ZoneManager.ActiveZone.GetCell(31, 21).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
				GritGateScripts.PromoteToJourneyfriend();
			}
			if (Wish == "stage6")
			{
				who.AwardXP(210000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("The Assessment");
				game.StartQuest("Pax Klanq, I Presume?");
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				The.ZoneManager.ActiveZone.GetCell(48, 19).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			if (Wish == "stage8" || Wish == "tombbetastart")
			{
				game.Player.Body.TakePopulation("TombSupply");
				who.AwardXP(240000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("The Assessment");
				game.CompleteQuest("Pax Klanq, I Presume?");
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				GritGateScripts.OpenRank2Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.14");
				The.ZoneManager.ActiveZone.GetCell(18, 8).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			if (Wish == "stage9")
			{
				game.Player.Body.TakePopulation("TombSupply");
				who.AwardXP(240000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("Pax Klanq, I Presume?");
				game.StartQuest("Tomb of the Eaters");
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				GritGateScripts.OpenRank2Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				The.ZoneManager.ActiveZone.GetCell(53, 4).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			switch (Wish)
			{
			case "stage9b":
			case "stage9c":
			case "stage9d":
			case "stage9e":
			case "stage9f":
			case "stage9u":
			case "stage10":
				game.Player.Body.TakePopulation("TombSupply");
				who.AwardXP(240000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("Pax Klanq, I Presume?");
				game.StartQuest("Tomb of the Eaters");
				game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
				game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
				game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
				The.Game.Player.Body.ToggleMarkOfDeath();
				MessageQueue.AddPlayerMessage("Mark of death now " + The.Game.Player.Body.HasMarkOfDeath());
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				GritGateScripts.OpenRank2Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				if (Wish == "stage9b")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.9");
				}
				if (Wish == "stage9c")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.2.2.9");
				}
				if (Wish == "stage9d")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.2.9");
				}
				if (Wish == "stage9e")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.8");
				}
				if (Wish == "stage9f")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.2.0.0");
				}
				if (Wish == "stage9u")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.12");
				}
				The.ZoneManager.ActiveZone.GetCell(45, 24).AddObject(physics.ParentObject);
				The.ZoneManager.ProcessGoToPartyLeader();
				break;
			}
			switch (Wish)
			{
			case "tombbeta":
			case "tombbetainside":
			case "tombbetaend":
				game.Player.Body.TakePopulation("TombSupply");
				who.AwardXP(240000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("Pax Klanq, I Presume?");
				game.StartQuest("Tomb of the Eaters");
				game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
				game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
				game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
				The.Game.Player.Body.ToggleMarkOfDeath();
				MessageQueue.AddPlayerMessage("Mark of death now " + The.Game.Player.Body.HasMarkOfDeath());
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				GritGateScripts.OpenRank2Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				if (Wish == "tombbetaend")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.0.0");
					The.ZoneManager.ActiveZone.GetCell(45, 12).AddObject(physics.ParentObject);
				}
				else
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.53.4.0.0.11");
					The.ZoneManager.ActiveZone.GetCell(45, 0).AddObject(physics.ParentObject);
				}
				The.ZoneManager.ProcessGoToPartyLeader();
				break;
			}
			if (Wish == "stage11" || Wish == "reefjump")
			{
				game.Player.Body.TakePopulation("ReefBetaSupply");
				who.AwardXP(350000);
				game.CompleteQuest("A Canticle for Barathrum");
				game.CompleteQuest("Decoding the Signal");
				game.CompleteQuest("More Than a Willing Spirit");
				game.CompleteQuest("The Earl of Omonporch");
				game.SetIntGameState("ForcePostEarlSpawn", 1);
				game.CompleteQuest("A Call to Arms");
				game.CompleteQuest("Pax Klanq, I Presume?");
				game.StartQuest("Tomb of the Eaters");
				game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
				game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
				game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
				game.FinishQuestStep("Tomb of the Eaters", "Ascend the Tomb and Cross into Brightsheol");
				game.FinishQuestStep("Tomb of the Eaters", "Disable the Spindle's Magnetic Field");
				game.FinishQuestStep("Tomb of the Eaters", "Return to Grit Gate");
				game.Player.Body.TakeObject("BarathrumKey", Silent: false, 0);
				GritGateScripts.PromoteToJourneyfriend();
				GritGateScripts.OpenRank1Doors();
				GritGateScripts.OpenRank2Doors();
				The.ZoneManager.SetActiveZone("JoppaWorld");
				GritGateScripts.PromoteToJourneyfriend();
				ThinWorld.ReturnBody(The.Player);
				game.SetBooleanGameState("Recame", Value: true);
			}
			foreach (GameObject item24 in who.GetInventoryAndEquipment())
			{
				item24.MakeUnderstood();
			}
			if (who.pBrain != null)
			{
				who.pBrain.PerformEquip(IsPlayer: true);
			}
			Popup.bSuppressPopups = false;
			ItemNaming.Suppress = false;
			break;
		}
		}
	}

	private static void AddSkill(string Class)
	{
		object obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + Class));
		The.Game.Player.Body.GetPart<XRL.World.Parts.Skills>().AddSkill(obj as BaseSkill);
	}
}

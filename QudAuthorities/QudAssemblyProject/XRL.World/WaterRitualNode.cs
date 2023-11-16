using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using Wintellect.PowerCollections;
using XRL.Language;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Skills;
using XRL.World.Skills.Cooking;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class WaterRitualNode : ConversationNode
{
	private int nTinkerDataNodes;

	public static WaterRitualNode waterRitual = new WaterRitualNode();

	private static GameObject currentInitializedSpeaker = null;

	private static WaterRitualRecord currentRitualRecord = null;

	private static ConversationNode previousNode = null;

	private static int totalFactionRemaining => The.Game.PlayerReputation.get(currentRitualRecord.faction);

	public override bool Test()
	{
		return true;
	}

	public ConversationChoice CreateExitNode()
	{
		return new ConversationChoice
		{
			ParentNode = this,
			ID = Guid.NewGuid().ToString(),
			GotoID = previousNode.ID,
			Text = "{{G|Live and drink, =subject.waterRitualLiquid=-=pronouns.siblingTerm=.}} {{g|[end the water ritual]}}",
			onAction = delegate
			{
				currentRitualRecord = null;
				currentInitializedSpeaker = null;
				return true;
			},
			Ordinal = 1000000
		};
	}

	public static int SharesNPropertiesWith(IBaseJournalEntry entry, List<IBaseJournalEntry> choices)
	{
		int num = 0;
		for (int i = 0; i < choices.Count; i++)
		{
			if (entry.text == choices[i].text)
			{
				num++;
			}
			foreach (string attribute in choices[i].attributes)
			{
				if (entry.Has(attribute))
				{
					num++;
				}
			}
		}
		return num;
	}

	private string AssembleSellOptionText(string Message, bool Valid, int ReputationProvided, int BonusReputationProvided)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{").Append(Valid ? 'G' : 'K').Append('|')
			.Append(Message);
		if (Valid)
		{
			stringBuilder.Append("}} {{g|");
		}
		else
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append("[{{").Append(Valid ? 'C' : 'r').Append('|');
		if (ReputationProvided >= 0)
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(ReputationProvided);
		if (BonusReputationProvided != 0)
		{
			if (Valid)
			{
				stringBuilder.Append("{{c|");
			}
			if (BonusReputationProvided > 0)
			{
				stringBuilder.Append('+');
			}
			stringBuilder.Append(BonusReputationProvided);
			if (Valid)
			{
				stringBuilder.Append("}}");
			}
		}
		stringBuilder.Append("}} reputation]}}");
		return stringBuilder.ToString();
	}

	public ConversationChoice CreateSellSecret()
	{
		string Message = "I have a secret to share with you.";
		int ReputationProvided = 50;
		int BonusReputationProvided = 0;
		GetWaterRitualSellSecretBehaviorEvent.Send(The.Player, currentInitializedSpeaker, ref Message, ref ReputationProvided, ref BonusReputationProvided, IsSecret: true);
		if (currentRitualRecord.totalFactionAvailable < ReputationProvided)
		{
			return new ConversationChoice
			{
				ParentNode = this,
				ID = Guid.NewGuid().ToString(),
				GotoID = ID,
				Text = AssembleSellOptionText(Message, Valid: false, ReputationProvided, BonusReputationProvided),
				onAction = delegate
				{
					Popup.ShowFail(currentInitializedSpeaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " can't grant you any more reputation.");
					return false;
				}
			};
		}
		List<JournalSultanNote> sultanNotes = JournalAPI.GetSultanNotes((JournalSultanNote note) => Faction.WantsToBuySecret(currentRitualRecord.faction, note, currentInitializedSpeaker));
		List<JournalObservation> observations = JournalAPI.GetObservations((JournalObservation observation) => observation.category != "Gossip" && Faction.WantsToBuySecret(currentRitualRecord.faction, observation, currentInitializedSpeaker));
		List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote mapnote) => Faction.WantsToBuySecret(currentRitualRecord.faction, mapnote, currentInitializedSpeaker));
		List<JournalRecipeNote> recipes = JournalAPI.GetRecipes((JournalRecipeNote recipenote) => Faction.WantsToBuySecret(currentRitualRecord.faction, recipenote, currentInitializedSpeaker));
		List<IBaseJournalEntry> choices = new List<IBaseJournalEntry>(sultanNotes.Count + observations.Count + mapNotes.Count + recipes.Count);
		foreach (JournalSultanNote item2 in sultanNotes)
		{
			choices.Add(item2);
		}
		foreach (JournalObservation item3 in observations)
		{
			choices.Add(item3);
		}
		foreach (JournalMapNote item4 in mapNotes)
		{
			choices.Add(item4);
		}
		foreach (JournalRecipeNote item5 in recipes)
		{
			choices.Add(item5);
		}
		if (choices.Count == 0)
		{
			return null;
		}
		Algorithms.RandomShuffleInPlace(choices, new Random(currentRitualRecord.mySeed));
		return new ConversationChoice
		{
			ParentNode = this,
			ID = Guid.NewGuid().ToString(),
			GotoID = ID,
			Text = AssembleSellOptionText(Message, Valid: true, ReputationProvided, BonusReputationProvided),
			onAction = delegate
			{
				List<string> list = new List<string>();
				List<IBaseJournalEntry> list2 = new List<IBaseJournalEntry>();
				int num = 3;
				for (int i = 0; i < num && i < choices.Count; i++)
				{
					int index = -1;
					int num2 = int.MaxValue;
					for (int j = 0; j < choices.Count; j++)
					{
						if (!list2.Contains(choices[j]))
						{
							int num3 = SharesNPropertiesWith(choices[j], list2);
							if (num3 < num2)
							{
								index = j;
								num2 = num3;
							}
						}
					}
					string item = ((!(choices[index] is JournalMapNote)) ? choices[index].GetShortText() : ("The location of " + Grammar.LowerArticles(choices[index].GetShortText())));
					list.Add(item);
					list2.Add(choices[index]);
				}
				int num4 = Popup.ShowOptionList("Choose a secret to share:\n", list.ToArray(), new char[3] { 'a', 'b', 'c' }, 1, null, 60, RespectOptionNewlines: true, AllowEscape: true);
				if (num4 >= 0)
				{
					list2[num4].secretSold = true;
					if (list2[num4].history.Length > 0)
					{
						list2[num4].history += "\n";
					}
					IBaseJournalEntry baseJournalEntry = list2[num4];
					baseJournalEntry.history = baseJournalEntry.history + " {{K|-shared with " + Faction.getFormattedName(currentRitualRecord.faction) + "}}";
					list2[num4].Updated();
					list2[num4].Reveal();
					AwardReputation(currentRitualRecord.faction, ReputationProvided, BonusReputationProvided);
					return true;
				}
				return false;
			}
		};
	}

	public ConversationChoice CreateGossip()
	{
		string Message = "I have some gossip that may interest you.";
		int ReputationProvided = 100;
		int BonusReputationProvided = 0;
		GetWaterRitualSellSecretBehaviorEvent.Send(The.Player, currentInitializedSpeaker, ref Message, ref ReputationProvided, ref BonusReputationProvided, IsSecret: false, IsGossip: true);
		List<IBaseJournalEntry> choices = new List<IBaseJournalEntry>();
		foreach (JournalObservation observation in JournalAPI.GetObservations((JournalObservation observation) => Faction.WantsToBuySecret(currentRitualRecord.faction, observation, currentInitializedSpeaker)))
		{
			choices.Add(observation);
		}
		if (choices.Count == 0)
		{
			return null;
		}
		Algorithms.RandomShuffleInPlace(choices, new Random(currentRitualRecord.mySeed));
		return new ConversationChoice
		{
			ParentNode = this,
			ID = Guid.NewGuid().ToString(),
			GotoID = ID,
			Text = AssembleSellOptionText(Message, currentRitualRecord.totalFactionAvailable >= ReputationProvided, ReputationProvided, BonusReputationProvided),
			onAction = delegate
			{
				if (currentRitualRecord.totalFactionAvailable < ReputationProvided)
				{
					Popup.ShowFail(currentInitializedSpeaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " can't grant you enough reputation.");
					return false;
				}
				List<string> list = new List<string>();
				List<IBaseJournalEntry> list2 = new List<IBaseJournalEntry>();
				int num = 3;
				for (int i = 0; i < num && i < choices.Count; i++)
				{
					int index = -1;
					int num2 = int.MaxValue;
					for (int j = 0; j < choices.Count; j++)
					{
						if (!list2.Contains(choices[j]))
						{
							int num3 = SharesNPropertiesWith(choices[j], list2);
							if (num3 < num2)
							{
								index = j;
								num2 = num3;
							}
						}
					}
					list.Add(choices[index].GetShortText());
					list2.Add(choices[index]);
				}
				int num4 = Popup.ShowOptionList("Choose some gossip to share:", list.ToArray(), new char[3] { 'a', 'b', 'c' }, 1, null, 60, RespectOptionNewlines: true, AllowEscape: true);
				if (num4 >= 0)
				{
					if (list2[num4].history.Length > 0)
					{
						choices[0].history += "\n";
					}
					list2[num4].secretSold = true;
					IBaseJournalEntry baseJournalEntry = list2[num4];
					baseJournalEntry.history = baseJournalEntry.history + " {{K|-shared with " + Faction.getFormattedName(currentRitualRecord.faction) + "}}";
					list2[num4].Updated();
					list2[num4].Reveal();
					AwardReputation(currentRitualRecord.faction, ReputationProvided, BonusReputationProvided);
					return true;
				}
				return false;
			}
		};
	}

	public ConversationChoice CreateBuySecret()
	{
		List<IBaseJournalEntry> choices = new List<IBaseJournalEntry>();
		List<JournalSultanNote> sultanNotes = JournalAPI.GetSultanNotes((JournalSultanNote note) => Faction.WantsToSellSecret(currentRitualRecord.faction, note));
		List<JournalObservation> observations = JournalAPI.GetObservations((JournalObservation observation) => Faction.WantsToSellSecret(currentRitualRecord.faction, observation));
		List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote mapnote) => Faction.WantsToSellSecret(currentRitualRecord.faction, mapnote));
		List<JournalRecipeNote> recipes = JournalAPI.GetRecipes((JournalRecipeNote recipe) => Faction.WantsToSellSecret(currentRitualRecord.faction, recipe));
		foreach (JournalSultanNote item in sultanNotes)
		{
			choices.Add(item);
		}
		foreach (JournalObservation item2 in observations)
		{
			choices.Add(item2);
		}
		foreach (JournalMapNote item3 in mapNotes)
		{
			choices.Add(item3);
		}
		foreach (JournalRecipeNote item4 in recipes)
		{
			choices.Add(item4);
		}
		if (choices.Count <= 0)
		{
			return null;
		}
		Algorithms.RandomShuffleInPlace(choices, new Random(currentRitualRecord.mySeed));
		int cost = GetWaterRitualCostEvent.GetFor(The.Player, currentInitializedSpeaker, "Secret", 50);
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Share a secret with me, =subject.waterRitualLiquid=-=pronouns.siblingTerm=.}} {{g|[{{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		else if (currentRitualRecord.secretsRemaining <= 0)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text, flipRep: false);
		}
		conversationChoice.onAction = delegate
		{
			if (currentRitualRecord.secretsRemaining <= 0)
			{
				Popup.ShowFail(currentInitializedSpeaker.Does("have", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " no more secrets to share.");
				return true;
			}
			Faction faction = Factions.get(currentRitualRecord.faction);
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				List<string> list = new List<string>();
				List<IBaseJournalEntry> list2 = new List<IBaseJournalEntry>();
				int num = 3;
				for (int i = 0; i < num && i < choices.Count; i++)
				{
					int index = -1;
					int num2 = int.MaxValue;
					for (int j = 0; j < choices.Count; j++)
					{
						if (!list2.Contains(choices[j]))
						{
							int num3 = SharesNPropertiesWith(choices[j], list2);
							if (num3 < num2)
							{
								index = j;
								num2 = num3;
							}
						}
					}
					list.Add(choices[index].GetShortText());
					list2.Add(choices[index]);
				}
				currentRitualRecord.secretsRemaining--;
				IBaseJournalEntry randomElement = list2.GetRandomElement();
				randomElement.attributes.Add(faction.NoBuySecretString);
				JournalSultanNote journalSultanNote = randomElement as JournalSultanNote;
				if (journalSultanNote != null)
				{
					HistoricEvent @event = HistoryAPI.GetEvent(journalSultanNote.eventId);
					Popup.Show(currentInitializedSpeaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " an event from the life of a sultan with you.\n\n\"" + @event.GetEventProperty("gospel") + "\"");
					@event.Reveal();
				}
				if (randomElement is JournalMapNote journalMapNote)
				{
					Popup.Show(currentInitializedSpeaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " the location of " + Markup.Wrap(Grammar.LowerArticles(journalMapNote.text)) + ".");
				}
				if (randomElement is JournalObservation journalObservation)
				{
					string text = "";
					if (randomElement.Has("gossip"))
					{
						History sultanHistory = The.Game.sultanHistory;
						string text2 = HistoricStringExpander.ExpandString("<spice.gossip.leadIns.!random>", null, sultanHistory);
						text = ((!text2.Contains('?') && !text2.Contains('.') && !journalObservation.initCapAsFragment) ? (text2 + " " + Grammar.InitLower(journalObservation.text)) : (text2 + " " + journalObservation.text));
					}
					Popup.Show(currentInitializedSpeaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " some gossip with you.\n\n\"" + text + "\"");
				}
				if (randomElement is JournalRecipeNote)
				{
					Popup.Show(currentInitializedSpeaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " a recipe with you.");
				}
				if (faction.Visible)
				{
					if (randomElement.history.Length > 0)
					{
						randomElement.history += "\n";
					}
					randomElement.history = randomElement.history + " {{K|-learned from " + faction.getFormattedName() + "}}";
				}
				if (journalSultanNote == null || !randomElement._revealed)
				{
					randomElement.Reveal();
				}
				randomElement.Updated();
			}
			else
			{
				Popup.ShowFail("You don't have a high enough reputation with " + faction.getFormattedName() + ".");
			}
			return true;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateBuyRecipe()
	{
		if (currentRitualRecord.tinkerdata.Count <= nTinkerDataNodes)
		{
			List<int> list = new List<int>(TinkerData.TinkerRecipes.Count);
			list.AddRange(Enumerable.Range(0, TinkerData.TinkerRecipes.Count));
			Algorithms.RandomShuffleInPlace(list, new Random(currentRitualRecord.mySeed));
			for (int i = 0; i < list.Count; i++)
			{
				if (currentRitualRecord.tinkerdata.Count > nTinkerDataNodes && currentRitualRecord.tinkerdata.Count >= currentRitualRecord.numBlueprints)
				{
					break;
				}
				if (!TinkerData.TinkerRecipes[list[i]].Known() && !currentRitualRecord.tinkerdata.Contains(list[i]))
				{
					currentRitualRecord.tinkerdata.Add(list[i]);
				}
			}
		}
		TinkerData data = TinkerData.TinkerRecipes[currentRitualRecord.tinkerdata[nTinkerDataNodes]];
		if (data.Known())
		{
			return null;
		}
		int cost = GetRecipeCost(The.Player, currentInitializedSpeaker, data.Tier);
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		string text;
		if (data.Type == "Mod")
		{
			text = "[{{W|Item mod}}] - {{C|" + data.DisplayName + "}}";
		}
		else
		{
			GameObject gameObject = GameObject.createSample(data.Blueprint);
			text = ((!gameObject.IsPluralIfKnown) ? Grammar.Pluralize(data.DisplayName) : data.DisplayName);
			gameObject.Obliterate();
		}
		conversationChoice.Text = "{{G|Would you teach me to craft " + text + "?}} {{g|[{{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		int nChoiceNode = nTinkerDataNodes;
		conversationChoice.onAction = delegate
		{
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				currentRitualRecord.tinkerdata.RemoveAt(nChoiceNode);
				TinkerData.KnownRecipes.Add(data);
				if (data.Type == "Mod")
				{
					Popup.Show(currentInitializedSpeaker.Does("teach", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to craft the item modification {{W|" + data.DisplayName + "}}.");
				}
				else
				{
					GameObject gameObject2 = GameObject.createSample(data.Blueprint);
					gameObject2.MakeUnderstood();
					Popup.Show(currentInitializedSpeaker.Does("teach", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to craft " + (gameObject2.IsPlural ? data.DisplayName : Grammar.Pluralize(data.DisplayName)) + ".");
					gameObject2.Obliterate();
				}
				currentRitualRecord.numBlueprints--;
			}
			else
			{
				Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			}
			return true;
		};
		nTinkerDataNodes++;
		return conversationChoice;
	}

	public string Greyout(string str, bool flipRep = true)
	{
		if (flipRep)
		{
			return str.Replace("{{C|-", "{{~~|-").Replace("{{G|", "{{K|").Replace("{{g|", "{{K|")
				.Replace("{{~~|-", "{{r|-");
		}
		return str.Replace("{{G|", "{{K|").Replace("{{g|", "{{K|").Replace("{{C|", "{{|");
	}

	public ConversationChoice CreateJoinPartyNode()
	{
		if (currentInitializedSpeaker.pBrain.PartyLeader == The.Player)
		{
			return null;
		}
		if (currentInitializedSpeaker.GetBlueprint().GetxTag("WaterRitual", "NoJoin") == "true")
		{
			return null;
		}
		int repCost = GetJoinPartyCost(The.Player, currentInitializedSpeaker);
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = "End";
		conversationChoice.Text = "{{G|I would ask you to join me, =subject.waterRitualLiquid=-=pronouns.siblingTerm=.}} {{g|[{{C|-" + repCost + "}} reputation]}}";
		if (totalFactionRemaining < repCost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.onAction = delegate
		{
			if (UseReputation(currentRitualRecord.faction, repCost))
			{
				Brain pBrain = currentInitializedSpeaker.pBrain;
				pBrain.BecomeCompanionOf(The.Player);
				if (pBrain.GetFeeling(The.Player) < 0)
				{
					pBrain.SetFeeling(The.Player, 5);
				}
				if (currentInitializedSpeaker.GetEffect("Lovesick") is Lovesick lovesick)
				{
					lovesick.PreviousLeader = The.Player;
				}
				Popup.Show(currentInitializedSpeaker.Does("join", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you!");
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateLearnSkillNode(string skill, int cost)
	{
		string text = currentInitializedSpeaker.GetxTag("WaterRitual", "SellSkill");
		if (currentInitializedSpeaker.HasProperty("WaterRitualNoSellSkill"))
		{
			return null;
		}
		if (text != null)
		{
			if (text == "false")
			{
				return null;
			}
			skill = text;
		}
		SkillEntry skillEntry = null;
		PowerEntry value = null;
		if (!SkillFactory.Factory.SkillByClass.TryGetValue(skill, out skillEntry))
		{
			if (!SkillFactory.Factory.PowersByClass.TryGetValue(skill, out value))
			{
				return null;
			}
			if (value != null && value.ParentSkill != null && (value.ParentSkill.Initiatory == true || value.Cost == 0))
			{
				skillEntry = value.ParentSkill;
				value = null;
				skill = skillEntry.Class;
			}
		}
		int spCost = 0;
		string initiatoryKey = null;
		SkillEntry skillEntry2 = skillEntry;
		if (skillEntry2 != null && skillEntry2.Initiatory == true)
		{
			initiatoryKey = skillEntry.Class + "_Initiated_By_" + currentInitializedSpeaker.id;
			if (The.Player.GetIntProperty(initiatoryKey) > 0)
			{
				return null;
			}
			if (The.Player.HasSkill(skillEntry.Class))
			{
				bool flag = false;
				foreach (PowerEntry value2 in skillEntry.Powers.Values)
				{
					if (!The.Player.HasSkill(value2.Class) && value2.MeetsRequirements(The.Player))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return null;
				}
			}
			spCost = skillEntry.Cost;
		}
		else if (The.Player.HasSkill(skill))
		{
			return null;
		}
		string name = skillEntry?.Name ?? value?.Name ?? "<error: missing skill/power>";
		cost = GetWaterRitualCostEvent.GetFor(The.Player, currentInitializedSpeaker, "Skill", cost);
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		SkillEntry skillEntry3 = skillEntry;
		if (skillEntry3 != null && skillEntry3.Initiatory == true)
		{
			stringBuilder.Append("{{G|I seek ").Append(skillEntry.Name).Append(".}} {{g|[");
		}
		else
		{
			stringBuilder.Append("{{G|Would you teach me your ways?}} {{g|[learn {{W|").Append(name).Append("}}: ");
		}
		stringBuilder.Append("{{C|-").Append(cost).Append("}} reputation");
		if (spCost > 0)
		{
			stringBuilder.Append(", {{C|-").Append(spCost).Append("}} SP");
		}
		stringBuilder.Append("]}}");
		conversationChoice.Text = stringBuilder.ToString();
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.onAction = delegate
		{
			if (spCost > 0 && The.Player.Stat("SP") < spCost)
			{
				Popup.ShowFail("You don't have enough skill points.");
				return false;
			}
			SkillEntry skillEntry4 = skillEntry;
			if (skillEntry4 != null && skillEntry4.Initiatory == true)
			{
				if (!The.Player.HasSkill(skillEntry.Class))
				{
					if (!UseReputation(currentRitualRecord.faction, cost))
					{
						Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
						return false;
					}
					The.Player.AddSkill(skill);
					string text2 = null;
					foreach (PowerEntry value3 in skillEntry.Powers.Values)
					{
						if (The.Player.HasSkill(value3.Class))
						{
							text2 = value3.Name;
							break;
						}
					}
					Popup.Show(currentInitializedSpeaker.Does("lead") + " you through a rite of ancient mystery, one not for profane eyes or ears. You have begun your journey upon " + skillEntry.Name + ((text2 == null) ? "" : (" with initiation into " + text2)) + ".");
				}
				else
				{
					PowerEntry powerEntry = null;
					foreach (PowerEntry value4 in skillEntry.Powers.Values)
					{
						if (!The.Player.HasSkill(value4.Class) && value4.MeetsRequirements(The.Player))
						{
							powerEntry = value4;
							break;
						}
					}
					if (powerEntry == null)
					{
						Popup.ShowFail("You have completed " + skillEntry.Name + ".");
						return false;
					}
					if (!UseReputation(currentRitualRecord.faction, cost))
					{
						Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
						return false;
					}
					The.Player.AddSkill(powerEntry.Class);
					PowerEntry powerEntry2 = null;
					foreach (PowerEntry value5 in skillEntry.Powers.Values)
					{
						if (!The.Player.HasSkill(value5.Class) && value5.MeetsRequirements(The.Player))
						{
							powerEntry2 = value5;
							break;
						}
					}
					Popup.Show(currentInitializedSpeaker.Does("lead") + " you through a mysterious rite. Your journey upon " + skillEntry.Name + ((powerEntry2 == null) ? " has reached completion" : " continues") + (string.IsNullOrEmpty(powerEntry.Name) ? "" : (" with initiation into " + powerEntry.Name)) + ".");
				}
				The.Player.SetIntProperty(initiatoryKey, The.Player.Stat("Level"));
			}
			else
			{
				if (!UseReputation(currentRitualRecord.faction, cost))
				{
					Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
					return false;
				}
				Popup.Show(currentInitializedSpeaker.Does("teach") + " you {{W|" + name + "}}!");
				The.Player.AddSkill(skill);
				if (skill == "Acrobatics_Jump" && currentInitializedSpeaker != null && currentInitializedSpeaker.Blueprint.Contains("frog", CompareOptions.IgnoreCase))
				{
					AchievementManager.SetAchievement("ACH_LEARN_JUMP");
				}
			}
			if (spCost > 0)
			{
				The.Player.GetStat("SP").Penalty += spCost;
			}
			return true;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateLearnCookingRecipeNode(CookingRecipe recipe, string askText = "Would you share the favorite dish of your people?", bool bSellToTrueKin = true)
	{
		int cost = GetWaterRitualCostEvent.GetFor(The.Player, currentInitializedSpeaker, "CookingRecipe", 50);
		string text = currentInitializedSpeaker.GetxTag("WaterRitual", "SellCookingRecipe");
		if (text != null && text == "false")
		{
			return null;
		}
		if (CookingGamestate.KnowsRecipe(recipe))
		{
			return null;
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|" + askText + "}} {{g|[learn to cook {{W|" + recipe.GetDisplayName() + "}}: {{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost || (!bSellToTrueKin && The.Player.IsTrueKin()))
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.onAction = delegate
		{
			if (!bSellToTrueKin && The.Player.IsTrueKin())
			{
				Popup.ShowFail("True kin cannot digest this meal.");
				return false;
			}
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				Popup.Show(currentInitializedSpeaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " the recipe for {{W|" + recipe.GetDisplayName() + "}}!");
				CookingGamestate.LearnRecipe(recipe);
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateLearnCookingRecipeNode(string ClassName, string askText = "Would you share the favorite dish of your people?", bool bSellToTrueKin = true)
	{
		return CreateLearnCookingRecipeNode(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Skills.Cooking." + ClassName)) as CookingRecipe, askText, bSellToTrueKin);
	}

	public ConversationChoice CreateSkillPointNode(int sp, int cost)
	{
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Teach me to think like a child again.}} {{g|[gain {{C|" + sp + "}} {{W|skill points}}: {{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.onAction = delegate
		{
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				Popup.Show("Talking to " + currentInitializedSpeaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " rouses in you an inert truth. You once wore the frock of a child. You poured salt through the cracks of your fingers, and you watched worlds form. Can it be all so simple still?");
				Popup.Show("You gained {{C|" + sp + "}} skill points!");
				The.Player.Statistics["SP"].BaseValue += sp;
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateHermitOathNode(int reward)
	{
		if (currentRitualRecord.hermitChatPenalty > 0)
		{
			return null;
		}
		if (currentRitualRecord.Has("madeHermitOath"))
		{
			return null;
		}
		string propertyOrTag = currentInitializedSpeaker.GetPropertyOrTag("HermitOathAddressAs", "hermit");
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|I promise not to bother you again, " + propertyOrTag + ".}} {{g|[{{C|+" + reward + "}} reputation]}}";
		conversationChoice.onAction = delegate
		{
			currentRitualRecord.attributes.Add("madeHermitOath");
			currentRitualRecord.hermitChatPenalty = reward * 2;
			The.Game.PlayerReputation.modify(Factions.get(currentRitualRecord.faction).Name, reward);
			return true;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateBuyMostValuableItemNode()
	{
		if (currentRitualRecord.numGifts < 1)
		{
			return null;
		}
		GameObject go = currentInitializedSpeaker.GetMostValuableItem();
		if (go == null)
		{
			return null;
		}
		int cost = GetItemCost(The.Player, currentInitializedSpeaker, go);
		if (cost == 0)
		{
			return null;
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Would you gift me your <item>?}} {{g|[{{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.Text = conversationChoice.Text.Replace("<item>", go.ShortDisplayName);
		conversationChoice.onAction = delegate
		{
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				go.UnequipAndRemove();
				Popup.Show(currentInitializedSpeaker.Does("gift", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you " + go.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "!");
				The.Player.Inventory.AddObject(go);
				currentRitualRecord.numGifts--;
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateBuyItemNode(string blueprint, int cost = 0)
	{
		if (currentRitualRecord.numItems <= 0)
		{
			return null;
		}
		GameObject go = currentInitializedSpeaker.HasItemWithBlueprint(blueprint);
		if (go == null && currentRitualRecord.canGenerateItem)
		{
			currentInitializedSpeaker.TakeObject(blueprint, Silent: true, 0);
			go = currentInitializedSpeaker.HasItemWithBlueprint(blueprint);
		}
		currentRitualRecord.canGenerateItem = false;
		if (go == null)
		{
			return null;
		}
		if (cost == 0)
		{
			cost = GetItemCost(The.Player, currentInitializedSpeaker, go);
			if (cost == 0)
			{
				return null;
			}
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Would you gift me your <item>?}} {{g|[{{C|-" + cost + "}} reputation]}}";
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
		}
		conversationChoice.Text = conversationChoice.Text.Replace("<item>", go.ShortDisplayName);
		conversationChoice.onAction = delegate
		{
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				go.UnequipAndRemove();
				Popup.Show(currentInitializedSpeaker.Does("gift", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you " + go.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "!");
				The.Player.Inventory.AddObject(go);
				currentRitualRecord.numItems--;
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateMutationNode(string blueprint, int cost)
	{
		cost = GetWaterRitualCostEvent.GetFor(The.Player, currentInitializedSpeaker, "Mutation", cost);
		if (currentRitualRecord.Has("SoldMutation"))
		{
			return null;
		}
		MutationEntry entry = MutationFactory.GetMutationEntryByName(blueprint);
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Would you teach me your ways?}} {{g|[gain {{M|" + entry.DisplayName + "}}: {{C|-" + cost + "}} reputation]}}";
		bool eligible = true;
		string Message = null;
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".";
		}
		if (The.Player.HasPart(blueprint))
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You already have this mutation.";
		}
		if (entry.Type == "Mental" && The.Player.IsChimera())
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You can't gain mental mutations.";
		}
		conversationChoice.onAction = delegate
		{
			if (!eligible)
			{
				Popup.ShowFail(Message);
				return false;
			}
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				Popup.Show("Despite your genetic limitations, " + currentInitializedSpeaker.does("teach", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to improvise {{M|" + entry.DisplayName + "}}!");
				The.Player.GetPart<Mutations>().AddMutation(entry.CreateInstance(), 1);
				currentRitualRecord.attributes.Add("SoldMutation");
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public string GetRecordAttribute(string val, string def = null)
	{
		foreach (string attribute in currentRitualRecord.attributes)
		{
			if (attribute.StartsWith(val))
			{
				return attribute.Split(':')[1];
			}
		}
		return def;
	}

	public ConversationChoice CreateRandomMentalMutationNode(int cost)
	{
		if (currentRitualRecord.Has("SoldRandomMentalMutation"))
		{
			return null;
		}
		string mutation = GetRecordAttribute("RandomMentalMutation:");
		if (mutation == null)
		{
			MutationEntry mutationEntry = new List<MutationEntry>(from e in The.Player.GetPart<Mutations>().GetMutatePool()
				where e.IsMental()
				select e)?.GetRandomElement();
			if (mutationEntry != null)
			{
				mutation = mutationEntry.DisplayName;
				currentRitualRecord.attributes.Add("RandomMentalMutation:" + mutationEntry.DisplayName);
			}
		}
		if (mutation == null)
		{
			return null;
		}
		cost = MutationFactory.GetMutationEntryByName(mutation).Cost * cost;
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|Teach me a secret of the aggregate mind.}} {{g|[gain {{M|" + mutation + "}}: {{C|-" + cost + "}} reputation]}}";
		bool eligible = true;
		string Message = null;
		if (The.Player.IsTrueKin())
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You can't be mutated.";
		}
		if (totalFactionRemaining < cost)
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".";
		}
		if (The.Player.IsChimera())
		{
			conversationChoice.Text = Greyout(conversationChoice.Text);
			eligible = false;
			Message = "You can't gain mental mutations.";
		}
		conversationChoice.onAction = delegate
		{
			if (!eligible)
			{
				Popup.ShowFail(Message);
				return false;
			}
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				string text = currentInitializedSpeaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true);
				Popup.Show(text + currentInitializedSpeaker.GetVerb("grab") + " your hand and you close your eyes. You find " + The.Player.itself + " in a darkling thicket. " + ColorUtility.CapitalizeExceptFormatting(text) + currentInitializedSpeaker.GetVerb("tug") + " you over roots of braided nether, until finally you stumble into a glade. Others are here. You take their hands as " + text + " took yours.\n\nAfter a moment or an eon, you awaken knowing {{M|" + MutationFactory.GetMutationEntryByName(mutation).DisplayName + "}}.");
				The.Player.GetPart<Mutations>().AddMutation(MutationFactory.GetMutationEntryByName(mutation).CreateInstance(), 1);
				currentRitualRecord.attributes.Add("SoldRandomMentalMutation");
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public ConversationChoice CreateFungusNode(int cost)
	{
		if (currentRitualRecord.numFungusLeft <= 0)
		{
			return null;
		}
		if (GetRecordAttribute("FungusType:") == null)
		{
			currentRitualRecord.attributes.Add("FungusType:" + SporePuffer.InfectionObjectList.GetRandomElement());
		}
		string fungusType = GetRecordAttribute("FungusType:");
		string fungusName = "a fungus";
		if (fungusType == "LuminousInfection")
		{
			fungusName = "{{C|glowcrust}}";
		}
		if (fungusType == "PuffInfection")
		{
			fungusName = "{{G|fickle gill}}";
		}
		if (fungusType == "WaxInfection")
		{
			fungusName = "{{Y|waxflab}}";
		}
		if (fungusType == "MumblesInfection")
		{
			fungusName = "{{R-R-R-R-R-M-M sequence|mumble mouth}}";
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ParentNode = this;
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.GotoID = ID;
		conversationChoice.Text = "{{G|I am a friend to fungi. Colonize me.}} {{g|[become infected with " + fungusName + ": {{C|-" + cost + "}} reputation]}}";
		conversationChoice.onAction = delegate
		{
			List<BodyPart> list = new List<BodyPart>(8);
			List<string> list2 = new List<string>(8);
			List<BodyPart> parts = The.Player.Body.GetParts();
			foreach (BodyPart item in parts)
			{
				if (item.Equipped == null && FungalSporeInfection.BodyPartPreferableForFungalInfection(item))
				{
					list.Add(item);
					list2.Add(item.GetOrdinalName());
				}
			}
			foreach (BodyPart item2 in parts)
			{
				if (!list.Contains(item2) && item2.Equipped == null && FungalSporeInfection.BodyPartSuitableForFungalInfection(item2))
				{
					list.Add(item2);
					list2.Add(item2.GetOrdinalName());
				}
			}
			if (list.Count == 0)
			{
				Popup.ShowFail("You have no infectable, bare body parts.");
				return false;
			}
			int num = Popup.ShowOptionList("", list2.ToArray(), null, 1, "Choose a limb to infect with " + fungusName + ":", 75, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			if (UseReputation(currentRitualRecord.faction, cost))
			{
				FungalSporeInfection.ApplyFungalInfection(The.Player, fungusType, list[num]);
				currentRitualRecord.numFungusLeft--;
				return true;
			}
			Popup.ShowFail("You don't have a high enough reputation with " + Faction.getFormattedName(currentRitualRecord.faction) + ".");
			return false;
		};
		return conversationChoice;
	}

	public bool UseReputation(string faction, int amount)
	{
		if (The.Game.PlayerReputation.get(faction) < amount)
		{
			return false;
		}
		The.Game.PlayerReputation.modify(faction, -amount);
		return true;
	}

	public void AwardReputation(string faction, int awardAmount, int bonusAmount)
	{
		if (awardAmount >= currentRitualRecord.totalFactionAvailable)
		{
			currentRitualRecord.totalFactionAvailable = 0;
		}
		else
		{
			currentRitualRecord.totalFactionAvailable -= awardAmount;
		}
		The.Game.PlayerReputation.modify(faction, awardAmount + bonusAmount);
	}

	public override ConversationNode Enter(ConversationNode previous, GameObject speaker)
	{
		nTinkerDataNodes = 0;
		PrependUnspoken = null;
		AppendUnspoken = null;
		string waterRitualLiquid = speaker.GetWaterRitualLiquid(The.Player);
		BaseLiquid liquid = LiquidVolume.getLiquid(waterRitualLiquid);
		int num = 100;
		bool flag = false;
		if (previous != this)
		{
			if (!speaker.HasIntProperty("WaterRitualed"))
			{
				string sifrahWaterRitual = Options.SifrahWaterRitual;
				if (sifrahWaterRitual == "Always")
				{
					flag = true;
				}
				else if (sifrahWaterRitual != "Never")
				{
					switch (Popup.ShowYesNoCancel("Do you want to play a game of Sifrah to perform the formal water ritual with " + speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "? The formal ritual can be much more impactful. If you do not play the game of Sifrah, the informal water ritual will consume 1 dram of " + liquid.GetName() + "."))
					{
					case DialogResult.Yes:
						flag = true;
						break;
					case DialogResult.Cancel:
						return previous;
					}
				}
			}
			if (flag)
			{
				FormalWaterRitualSifrah formalWaterRitualSifrah = new FormalWaterRitualSifrah(speaker);
				formalWaterRitualSifrah.Play(speaker);
				if (formalWaterRitualSifrah.Abort)
				{
					return previous;
				}
				num = formalWaterRitualSifrah.Performance;
			}
			else
			{
				if (The.Player.GetFreeDrams(waterRitualLiquid) < 1)
				{
					Popup.ShowFail("You don't have enough " + liquid.GetName() + " to begin the ritual.");
					return previous;
				}
				The.Player.UseDrams(1, waterRitualLiquid);
			}
			int? personalFeeling = speaker.pBrain.GetPersonalFeeling(The.Player);
			int num2 = 50 * num / 100;
			if (!personalFeeling.HasValue || personalFeeling < num2)
			{
				speaker.pBrain.SetFeeling(The.Player, num2);
			}
		}
		ID = "*waterritual";
		bool flag2 = false;
		Faction faction;
		if (currentInitializedSpeaker == speaker)
		{
			faction = Factions.get(currentRitualRecord.faction);
			if (faction.UseAltBehavior(speaker))
			{
				flag2 = true;
			}
		}
		else
		{
			currentRitualRecord = speaker.GetPart("WaterRitualRecord") as WaterRitualRecord;
			if (currentRitualRecord != null)
			{
				faction = Factions.get(currentRitualRecord.faction);
				if (faction.UseAltBehavior(speaker))
				{
					flag2 = true;
				}
			}
			else
			{
				Faction faction2 = Factions.get(speaker.pBrain.GetPrimaryFaction());
				currentRitualRecord = speaker.AddPart(new WaterRitualRecord());
				currentRitualRecord.totalFactionAvailable = 50 * num / 50;
				currentRitualRecord.faction = faction2.Name;
				currentRitualRecord.mySeed = Stat.SeededRandom("ritualRecord" + speaker.GetCurrentCell().ParentZone.ZoneID + speaker.DisplayName, 0, 2147483646);
				faction = faction2;
				if (faction.UseAltBehavior(speaker))
				{
					flag2 = true;
				}
				GameObjectBlueprint blueprint = speaker.GetBlueprint();
				string text = blueprint.GetxTag("WaterRitual", "numSecrets");
				if (!string.IsNullOrEmpty(text))
				{
					currentRitualRecord.secretsRemaining = Math.Max(text.Roll(), 0);
				}
				if (blueprint.GetxTag("WaterRitual", "SellBlueprints") == "true")
				{
					string text2 = blueprint.GetxTag("WaterRitual", "numBlueprints");
					if (!string.IsNullOrEmpty(text2))
					{
						currentRitualRecord.numBlueprints = Math.Max(text2.Roll(), 0);
					}
				}
				else if (flag2)
				{
					if (!string.IsNullOrEmpty(faction.WaterRitualAltBlueprints))
					{
						currentRitualRecord.numBlueprints = Math.Max(faction.WaterRitualAltBlueprints.Roll(), 0);
					}
					if (!string.IsNullOrEmpty(faction.WaterRitualAltGifts))
					{
						currentRitualRecord.numGifts = Math.Max(faction.WaterRitualAltGifts.Roll(), 0);
					}
					if (!string.IsNullOrEmpty(faction.WaterRitualAltItems))
					{
						currentRitualRecord.numItems = Math.Max(faction.WaterRitualAltItems.Roll(), 0);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(faction.WaterRitualBlueprints))
					{
						currentRitualRecord.numBlueprints = Math.Max(faction.WaterRitualBlueprints.Roll(), 0);
					}
					if (!string.IsNullOrEmpty(faction.WaterRitualGifts))
					{
						currentRitualRecord.numGifts = Math.Max(faction.WaterRitualGifts.Roll(), 0);
					}
					if (!string.IsNullOrEmpty(faction.WaterRitualItems))
					{
						currentRitualRecord.numItems = Math.Max(faction.WaterRitualItems.Roll(), 0);
					}
				}
			}
			currentInitializedSpeaker = speaker;
			previousNode = previous;
			if (!speaker.HasIntProperty("WaterRitualed"))
			{
				The.Game.Systems.ForEach(delegate(IGameSystem s)
				{
					s.WaterRitualPerformed(speaker);
				});
				try
				{
					if (speaker.Blueprint == "Oboroqoru")
					{
						AchievementManager.SetAchievement("ACH_WATER_RITUAL_OBOROQORU");
					}
					if (speaker.Blueprint == "Mamon")
					{
						AchievementManager.SetAchievement("ACH_WATER_RITUAL_MAMON");
					}
					AchievementManager.IncrementAchievement("ACH_WATER_RITUAL_50_TIMES");
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Water ritual", x);
				}
				speaker.SetIntProperty("WaterRitualed", 1);
				if (faction.Visible)
				{
					if (!flag)
					{
						Popup.Show("You share your " + liquid.GetName(null) + " with " + currentInitializedSpeaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + " and begin the water ritual.");
					}
					JournalAPI.AddAccomplishment(muralWeight: (!WanderSystem.WanderEnabled()) ? JournalAccomplishment.MuralWeight.Medium : JournalAccomplishment.MuralWeight.Low, text: "In sacred ritual you shared your " + liquid.GetName(null) + " with " + currentInitializedSpeaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + ".", muralText: "In sacred ritual =name= shared " + The.Player.GetPronounProvider().PossessiveAdjective + " holy " + ColorUtility.StripFormatting(liquid.GetName(null)) + " with noted luminary " + currentInitializedSpeaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + ".", category: "general", muralCategory: JournalAccomplishment.MuralCategory.Treats, secretId: null, time: -1L);
					StringBuilder stringBuilder = Event.NewStringBuilder();
					int num3 = 100;
					if (The.Player.HasSkill("Customs_Tactful"))
					{
						num3 += 25;
						currentRitualRecord.attributes.Add("usedTactful");
					}
					num3 = num3 * num / 100;
					The.Game.PlayerReputation.modify(faction, num3, null, stringBuilder);
					GivesRep givesRep = speaker.GetPart("GivesRep") as GivesRep;
					if (givesRep != null)
					{
						foreach (string key in speaker.pBrain.FactionMembership.Keys)
						{
							if (key != currentRitualRecord.faction)
							{
								The.Game.PlayerReputation.modify(key, 50 * num / 50, "because they love " + speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
							}
						}
					}
					foreach (FriendorFoe relatedFaction in givesRep.relatedFactions)
					{
						if (relatedFaction.status == "friend")
						{
							The.Game.PlayerReputation.modify(relatedFaction.faction, 50 * (100 + (num - 100) / 10) / 50, "because they admire " + speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
						}
						else if (relatedFaction.status == "dislike")
						{
							The.Game.PlayerReputation.modify(relatedFaction.faction, -50 * (100 + (num - 100) / 10) / 100, "because they dislike " + speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
						}
						else if (relatedFaction.status == "hate")
						{
							The.Game.PlayerReputation.modify(relatedFaction.faction, -50 * (100 + (num - 100) / 10) / 50, "because they despise " + speaker.t(), stringBuilder);
						}
					}
					Popup.Show(stringBuilder.ToString());
					speaker.GetPart<GivesRep>().wasParleyed = true;
				}
				ItemNaming.Opportunity(speaker, null, The.Player, "WaterRitual", 7 - num / 100, 0, 0, num / 100);
				ItemNaming.Opportunity(The.Player, null, speaker, "WaterRitual", 7 - num / 100, 0, 0, num / 100);
			}
			ParentConversation = previous.ParentConversation;
		}
		if (The.Player.HasSkill("Customs_Tactful") && !currentRitualRecord.Has("usedTactful") && faction.Visible)
		{
			currentRitualRecord.attributes.Add("usedTactful");
			The.Game.PlayerReputation.modify(faction.Name, 25);
		}
		Choices = new List<ConversationChoice>();
		Choices.AddIfNotNull(CreateGossip());
		Choices.AddIfNotNull(CreateSellSecret());
		Choices.AddIfNotNull(CreateBuySecret());
		for (int num4 = currentRitualRecord.numBlueprints; num4 > 0; num4--)
		{
			Choices.AddIfNotNull(CreateBuyRecipe());
		}
		if (flag2)
		{
			if (!string.IsNullOrEmpty(faction.WaterRitualAltSkill))
			{
				Choices.AddIfNotNull(CreateLearnSkillNode(faction.WaterRitualAltSkill, (faction.WaterRitualAltSkillCost == -1) ? GetSkillCost(faction.WaterRitualAltSkill) : faction.WaterRitualAltSkillCost));
			}
			if (!string.IsNullOrEmpty(faction.WaterRitualAltItemBlueprint))
			{
				for (int num5 = currentRitualRecord.numItems; num5 > 0; num5--)
				{
					if (faction.WaterRitualAltItemCost == -1)
					{
						Choices.AddIfNotNull(CreateBuyItemNode(faction.WaterRitualAltItemBlueprint));
					}
					else
					{
						Choices.AddIfNotNull(CreateBuyItemNode(faction.WaterRitualAltItemBlueprint, faction.WaterRitualAltItemCost));
					}
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(faction.WaterRitualSkill))
			{
				Choices.AddIfNotNull(CreateLearnSkillNode(faction.WaterRitualSkill, (faction.WaterRitualSkillCost == -1) ? GetSkillCost(faction.WaterRitualSkill) : faction.WaterRitualSkillCost));
			}
			if (faction.WaterRitualBuyMostValuableItem)
			{
				Choices.AddIfNotNull(CreateBuyMostValuableItemNode());
			}
			if (faction.WaterRitualFungusInfect != -1)
			{
				Choices.AddIfNotNull(CreateFungusNode(faction.WaterRitualFungusInfect));
			}
			if (faction.WaterRitualHermitOath != -1)
			{
				Choices.AddIfNotNull(CreateHermitOathNode(faction.WaterRitualHermitOath));
			}
			if (faction.WaterRitualSkillPointAmount != -1 && faction.WaterRitualSkillPointCost != -1)
			{
				Choices.AddIfNotNull(CreateSkillPointNode(faction.WaterRitualSkillPointAmount, faction.WaterRitualSkillPointCost));
			}
			if (!string.IsNullOrEmpty(faction.WaterRitualMutation) && faction.WaterRitualMutationCost != -1)
			{
				Choices.AddIfNotNull(CreateMutationNode(faction.WaterRitualMutation, faction.WaterRitualMutationCost));
			}
			if (!string.IsNullOrEmpty(faction.WaterRitualItemBlueprint))
			{
				for (int num6 = currentRitualRecord.numItems; num6 > 0; num6--)
				{
					if (faction.WaterRitualItemCost == -1)
					{
						Choices.AddIfNotNull(CreateBuyItemNode(faction.WaterRitualItemBlueprint));
					}
					else
					{
						Choices.AddIfNotNull(CreateBuyItemNode(faction.WaterRitualItemBlueprint, faction.WaterRitualItemCost));
					}
				}
			}
			if (!string.IsNullOrEmpty(faction.WaterRitualRecipe))
			{
				if (!string.IsNullOrEmpty(faction.WaterRitualRecipeText))
				{
					Choices.AddIfNotNull(CreateLearnCookingRecipeNode(faction.WaterRitualRecipe, faction.WaterRitualRecipeText));
				}
				else
				{
					Choices.AddIfNotNull(CreateLearnCookingRecipeNode(faction.WaterRitualRecipe));
				}
			}
		}
		string stringProperty = currentInitializedSpeaker.GetStringProperty("WaterRitual_Skill");
		if (!string.IsNullOrEmpty(stringProperty) && stringProperty != (flag2 ? faction.WaterRitualAltSkill : faction.WaterRitualSkill))
		{
			Choices.AddIfNotNull(CreateLearnSkillNode(stringProperty, GetSkillCost(stringProperty) * 3 / 2));
		}
		string tag = currentInitializedSpeaker.GetTag("SharesRecipe");
		string tag2 = currentInitializedSpeaker.GetTag("SharesRecipeText");
		string tag3 = currentInitializedSpeaker.GetTag("SharesRecipeWithTrueKin");
		if (!string.IsNullOrEmpty(tag))
		{
			if (string.IsNullOrEmpty(tag2))
			{
				if (tag3 == "false")
				{
					Choices.AddIfNotNull(CreateLearnCookingRecipeNode(tag, "Would you share the favorite dish of your people?", bSellToTrueKin: false));
				}
				else
				{
					Choices.AddIfNotNull(CreateLearnCookingRecipeNode(tag));
				}
			}
			else if (tag3 == "false")
			{
				Choices.AddIfNotNull(CreateLearnCookingRecipeNode(tag, tag2, bSellToTrueKin: false));
			}
			else
			{
				Choices.AddIfNotNull(CreateLearnCookingRecipeNode(tag, tag2));
			}
		}
		if (currentInitializedSpeaker.HasPart("TeachesDish"))
		{
			Choices.AddIfNotNull(CreateLearnCookingRecipeNode(currentInitializedSpeaker.GetPart<TeachesDish>().Recipe, currentInitializedSpeaker.GetPart<TeachesDish>().Text));
		}
		if (!flag2 && faction.WaterRitualRandomMentalMutation != -1)
		{
			Choices.AddIfNotNull(CreateRandomMentalMutationNode(faction.WaterRitualRandomMentalMutation));
		}
		if (speaker.HasPart("Chef"))
		{
			Chef part = speaker.GetPart<Chef>();
			Choices.AddIfNotNull(CreateLearnCookingRecipeNode(part.signatureDishes[new Random(currentRitualRecord.mySeed).Next(0, part.signatureDishes.Count)], "Teach me to cook one of your signature dishes.\n"));
		}
		if (faction.WaterRitualJoin)
		{
			Choices.AddIfNotNull(CreateJoinPartyNode());
		}
		Choices.AddIfNotNull(CreateExitNode());
		Text = "Live and drink, =subject.waterRitualLiquid=-=player.siblingTerm=.\n\n";
		Faction ifExists = Factions.getIfExists(currentRitualRecord.faction);
		if (ifExists != null)
		{
			AppendUnspoken = "{{C|-----}}\n{{y|Your reputation with " + (ifExists.Visible ? ("{{C|" + ifExists.getFormattedName() + "}}") : speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + " is {{C|" + totalFactionRemaining + "}}.\n" + (ifExists.Visible ? speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) : speaker.It) + " can award an additional {{C|" + currentRitualRecord.totalFactionAvailable + "}} reputation.}}";
		}
		return this;
	}

	public static int GetJoinPartyCost(GameObject player, GameObject creature)
	{
		int baseCost = Math.Max(50, 200 + (creature.Stat("Level") - player.Stat("Level")) * 12);
		return GetWaterRitualCostEvent.GetFor(player, creature, "Join", baseCost);
	}

	public static int GetSkillCost(string skill)
	{
		if (SkillFactory.Factory.SkillByClass.TryGetValue(skill, out var value))
		{
			return value.Cost;
		}
		if (SkillFactory.Factory.PowersByClass.TryGetValue(skill, out var value2))
		{
			if (value2.Cost > 0)
			{
				return value2.Cost;
			}
			if (value2.ParentSkill != null)
			{
				return value2.ParentSkill.Cost;
			}
		}
		return 100;
	}

	public static int GetItemCost(GameObject Actor, GameObject Target, GameObject Item)
	{
		int baseCost = 5;
		if (Item.GetPart("Commerce") is Commerce commerce)
		{
			baseCost = Math.Max(5, (int)(commerce.Value / 4.0));
		}
		return GetWaterRitualCostEvent.GetFor(Actor, Target, "Item", baseCost);
	}

	public static int GetRecipeCost(GameObject Actor, GameObject Target, int tier)
	{
		int baseCost = 50 * tier / 3;
		return GetWaterRitualCostEvent.GetFor(Actor, Target, "TinkerRecipe", baseCost);
	}
}

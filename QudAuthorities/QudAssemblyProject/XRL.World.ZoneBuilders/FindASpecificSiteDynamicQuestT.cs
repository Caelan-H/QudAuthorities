using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	private string typeOfDirections;

	public DynamicQuestReward reward;

	private JournalMapNote targetLocation;

	private JournalMapNote landmarkLocation;

	private string direction = "";

	private string path = "";

	private int min = 12;

	private int max = 18;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public VillageDynamicQuestContext questContext;

	public new Zone zone;

	public string sanctityOfSacredThing;

	public QuestStoryType_FindASite QST;

	public void addQuestConversationToGiver(GameObject go, Quest quest)
	{
		go.SetStringProperty("GivesDynamicQuest", quest.ID);
		go.RequirePart<Interesting>();
		ConversationScript conversationScript = go.RequirePart<ConversationScript>();
		Conversation customConversation = conversationScript.customConversation;
		if (customConversation == null)
		{
			MetricsManager.LogEditorError("FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver::addQuestConversationToGiver", "Jason we tried to add a dynamic quest to an NPC with static conversations (" + go.Blueprint + ") maybe we want to merge the contents of this somehow?");
			conversationScript.customConversation = new Conversation();
			customConversation = conversationScript.customConversation;
		}
		go.SetIntProperty("QuestGiver", 1);
		if (go.pBrain != null)
		{
			go.pBrain.Wanders = false;
			go.pBrain.WandersRandomly = false;
		}
		customConversation.ID = Guid.NewGuid().ToString();
		string text = ColorUtility.StripFormatting(targetLocation.text);
		if (text.StartsWith("the snapjaw who wields"))
		{
			text = "the location of " + text;
		}
		string text2 = "";
		string text3 = HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.cameToOurVillage.!random.capitalize>");
		string text4 = HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.intro.!random.capitalize>").Replace("*siteInitLower*", Grammar.InitLowerIfArticle(text));
		Regex regex = new Regex("\\*.*\\*");
		Match match = Regex.Match(text3, "(?<=\\*)(.*?)(?=\\*)");
		if (match.Success)
		{
			text3 = regex.Replace(text3, Grammar.Pluralize(match.Value));
		}
		match = Regex.Match(text4, "(?<=\\*)(.*?)(?=\\*)");
		if (match.Success)
		{
			text4 = regex.Replace(text4, Grammar.Pluralize(match.Value));
		}
		text2 = Grammar.ConvertAtoAn((QST switch
		{
			QuestStoryType_FindASite.Travelers => string.Format("{0} {1}. {2}. But they wouldn't reveal the location. {3} {4}. {5}.\n\n{6}.", HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.intro.!random.capitalize>"), text3, Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.spokeOfPlace.!random.capitalize>")).Replace("*GuestActivity*", go.GetxTag_CommaDelimited("TextFragments", "GuestActivity", "breaking bread")), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.wouldYouFindIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.greatBoon.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.rewardYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.typeOfDirections." + typeOfDirections + ".intro.!random.capitalize>")), 
			QuestStoryType_FindASite.Records => string.Format("{0}. {1}? {2}? {3}? {4}? {5}. {6}. {7} {8}.", text4, HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.whatTreasures.!random.capitalize>"), Grammar.InitCap(go.GetxTag_CommaDelimited("TextFragments", "ValuedOre", "precious metals")), Grammar.InitialCap(go.GetxTag_CommaDelimited("TextFragments", "ArableLand", "arable land")), HistoricStringExpander.ExpandString("A <spice.commonPhrases.shrine.!random> to ") + questContext.getSacredThings().GetRandomElement(), "We must know", HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.ifYouFindIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.typeOfDirections." + typeOfDirections + ".intro.!random.capitalize>")), 
			_ => "ERROR: Failed to generate quest text from quest story type.", 
		}).Replace("*site*", text).Replace("*landmark*", landmarkLocation.text).Replace("*direction*", direction)
			.Replace("*min*", Math.Max(1, min / 3).ToString())
			.Replace("*max*", (max / 3).ToString())
			.Replace("*path*", path));
		ConversationNode conversationNode = ConversationsAPI.newNode(text2);
		ConversationChoice conversationChoice = DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will locate " + targetLocation.text + "&G as you ask.", conversationNode, quest);
		conversationChoice.SpecialRequirement = "!IsMapNoteRevealed:" + targetLocation.secretid;
		conversationChoice.RevealMapNoteId = landmarkLocation.secretid;
		ConversationChoice item = DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", conversationNode);
		ConversationChoice conversationChoice2 = DynamicQuestConversationHelper.fabricateIntroAdditionalChoice("I already know where " + targetLocation.text + " is.", conversationNode);
		conversationChoice2.StartQuest = quest.ID;
		conversationChoice2.CompleteQuestStep = quest.ID + "~a_locate;" + quest.ID + "~b_return";
		conversationChoice2.IfNotHaveQuest = quest.ID;
		conversationChoice2.SpecialRequirement = "IsMapNoteRevealed:" + targetLocation.secretid;
		conversationChoice2.RevealMapNoteId = landmarkLocation.secretid;
		conversationNode.Choices.Add(conversationChoice);
		conversationNode.Choices.Add(conversationChoice2);
		conversationNode.Choices.Add(item);
		conversationNode.Choices.Sort();
		customConversation.AddNode(conversationNode);
		DynamicQuestConversationHelper.appendQuestCompletionSequence(customConversation, quest, conversationNode, "I've located " + targetLocation.text + ".", "I haven't located " + targetLocation.text + " yet.", delegate
		{
		}, delegate(ConversationChoice gotoAcceptNodeFinalizer)
		{
			gotoAcceptNodeFinalizer.IfNotFinishedQuest = quest.ID;
			gotoAcceptNodeFinalizer.IfFinishedQuestStep = quest.ID + "~a_locate";
		}, delegate(ConversationChoice incompleteNodeFinalizer)
		{
			incompleteNodeFinalizer.IfNotFinishedQuest = quest.ID;
			incompleteNodeFinalizer.IfNotFinishedQuestStep = quest.ID + "~a_locate";
		}, delegate(ConversationChoice completeNodeFinalizer)
		{
			completeNodeFinalizer.CompleteQuestStep = quest.ID + "~b_return";
		}, delegate
		{
		});
	}

	public Quest fabricateFindASpecificSiteQuest(GameObject giver)
	{
		typeOfDirections = "path";
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		int num = Stat.Random(0, 12);
		min = 12;
		max = 18;
		targetLocation = null;
		int broaden = 0;
		while (targetLocation == null)
		{
			targetLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(zone, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
			broaden++;
		}
		while (true)
		{
			broaden = 0;
			if (typeOfDirections == "radius")
			{
				min = 0;
				max = 1;
				landmarkLocation = null;
				landmarkLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(targetLocation.zoneid, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
				if (landmarkLocation == null)
				{
					typeOfDirections = "direction";
					continue;
				}
			}
			if (typeOfDirections == "radius_Failsafe")
			{
				min = 0;
				max = 2;
				landmarkLocation = null;
				broaden = 0;
				while (landmarkLocation == null && broaden <= 79)
				{
					landmarkLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(targetLocation.zoneid, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
					broaden++;
				}
				break;
			}
			if (typeOfDirections == "direction")
			{
				List<JournalMapNote> mapNotesInCardinalDirections = JournalAPI.GetMapNotesInCardinalDirections(targetLocation.zoneid);
				min = 12;
				max = 18;
				for (broaden = 0; broaden < 79; broaden += 3)
				{
					List<JournalMapNote> list = mapNotesInCardinalDirections.FindAll((JournalMapNote l) => l.location.Distance(targetLocation.location) >= min - broaden && l.location.Distance(targetLocation.location) <= max + broaden && questContext.IsValidQuestDestination(targetLocation.location) && Math.Abs(l.cz - targetLocation.cz) == 0);
					if (list.Count > 0)
					{
						landmarkLocation = list.GetRandomElement();
						break;
					}
				}
				if (landmarkLocation != null)
				{
					min = Math.Max(1, landmarkLocation.location.Distance(targetLocation.location) - num);
					max = min + 12;
					if (landmarkLocation.x > targetLocation.x)
					{
						direction = "west";
					}
					if (landmarkLocation.x < targetLocation.x)
					{
						direction = "east";
					}
					if (landmarkLocation.y < targetLocation.y)
					{
						direction = "south";
					}
					if (landmarkLocation.y > targetLocation.y)
					{
						direction = "north";
					}
					break;
				}
				typeOfDirections = "radius_Failsafe";
			}
			else
			{
				if (!(typeOfDirections == "path"))
				{
					break;
				}
				if (questContext == null)
				{
					throw new Exception("questContext missimg");
				}
				if (questContext.worldInfo == null)
				{
					throw new Exception("worldInfo missing");
				}
				if (targetLocation == null)
				{
					throw new Exception("targetLocation missimg");
				}
				string directionToDestination;
				string directionFromLandmark;
				GeneratedLocationInfo generatedLocationInfo = questContext.worldInfo.FindLocationAlongPathFromLandmark(targetLocation.location, out path, out directionToDestination, out directionFromLandmark, questContext.IsValidQuestDestination);
				if (generatedLocationInfo != null)
				{
					direction = directionFromLandmark;
					landmarkLocation = JournalAPI.GetMapNote(generatedLocationInfo.secretID);
					break;
				}
				typeOfDirections = new List<string> { "radius", "direction" }.GetRandomElement();
			}
		}
		if (landmarkLocation == null)
		{
			MetricsManager.LogError("fabricateFindASpecificSiteQuest", "Couldn't find a site!");
			return quest;
		}
		quest.Name = Grammar.MakeTitleCase(ColorUtility.StripFormatting(targetLocation.text));
		quest.Manager = new FindASiteDynamicQuestManager(targetLocation.zoneid, targetLocation.secretid, quest.ID, "a_locate");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_locate";
		questStep.Name = Grammar.MakeTitleCase("Find " + ColorUtility.StripFormatting(targetLocation.text));
		if (typeOfDirections == "radius_Failsafe")
		{
			questStep.Text = "Locate " + targetLocation.text + ", located within " + max + " parasangs of " + landmarkLocation.text + ".";
		}
		if (typeOfDirections == "radius")
		{
			questStep.Text = "Locate " + targetLocation.text + ", located next to " + landmarkLocation.text + ".";
		}
		if (typeOfDirections == "direction")
		{
			questStep.Text = "Locate " + targetLocation.text + ", located " + Math.Max(min / 3, 1) + "-" + max / 3 + " parasangs " + direction + " of " + landmarkLocation.text + ".";
		}
		if (typeOfDirections == "path")
		{
			questStep.Text = "Locate " + targetLocation.text + ", located " + direction + " along the " + path + " that runs through " + landmarkLocation.text + ".";
		}
		questStep.XP = 100;
		questStep.Finished = false;
		quest.StepsByID.Add(questStep.ID, questStep);
		QuestStep questStep2 = new QuestStep();
		questStep2.ID = "b_return";
		questStep2.Name = Grammar.MakeTitleCase("Return to " + questContext.getQuestOriginZone());
		questStep2.Text = "Return to " + questContext.getQuestOriginZone() + " and speak to " + giver.DisplayNameOnlyDirectAndStripped + ".";
		questStep2.XP = 100;
		questStep2.Finished = false;
		quest.StepsByID.Add(questStep2.ID, questStep2);
		quest.dynamicReward = reward;
		DynamicQuestsGamestate.addQuest(quest);
		addQuestConversationToGiver(giver, quest);
		return quest;
	}

	public FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
	{
		this.questGiverFilter = questGiverFilter;
	}

	public bool BuildZone(Zone zone)
	{
		this.zone = zone;
		foreach (GameObject item in zone.GetObjects().ShuffleInPlace())
		{
			if (questGiverFilter(item))
			{
				fabricateFindASpecificSiteQuest(item);
				return true;
			}
		}
		return true;
	}
}

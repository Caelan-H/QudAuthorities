using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	public DynamicQuestReward reward;

	public DynamicQuestDeliveryTarget deliveryTarget;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public DynamicQuestContext questContext;

	public new Zone zone;

	public string sanctityOfSacredThing;

	public QuestStoryType_FindASpecificItem QST;

	public void addQuestConversationToGiver(GameObject go, Quest quest, GameObject fetchItem)
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
		customConversation.ID = Guid.NewGuid().ToString();
		go.SetIntProperty("QuestGiver", 1);
		if (go.pBrain != null)
		{
			go.pBrain.Wanders = false;
		}
		go.pBrain.WandersRandomly = false;
		GameObject gameObject = (from o in zone.GetObjects(questContext.getQuestActorFilter().Invoke)
			where o != go
			select o).ToList()?.GetRandomElement() ?? GameObject.create("Mehmet");
		ConversationNode conversationNode = ConversationsAPI.newNode(QST switch
		{
			QuestStoryType_FindASpecificItem.SacredItem => string.Format("{0}. {1}. {2}, {3}. {4} {5}. {6}.", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.intro.!random.capitalize>").Replace("*Activity*", go.GetxTag_CommaDelimited("TextFragments", "Activity")), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.afterLearning.!random.capitalize>").Replace("*sanctityOfSacredThing*", sanctityOfSacredThing), HistoricStringExpander.ExpandString("<spice.instancesOf.unfortunately.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.lostOur.!random>").Replace("*itemName*", fetchItem.ShortDisplayNameSingleStripped).Replace("*itemName.a*", fetchItem.a)
				.Replace("*name*", gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true))
				.Replace("*were*", fetchItem.GetVerb("were")), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.willingToRecover.!random.capitalize>").Replace("*itemTheAndName*", fetchItem.the + fetchItem.ShortDisplayNameSingleStripped).Replace("*it*", fetchItem.them), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.takenTo.!random.capitalize>").Replace("*deliveryTarget*", Grammar.LowerArticles(Grammar.TrimTrailingPunctuation(deliveryTarget.displayName))).Replace("*it*", fetchItem.them)
				.Replace("*has*", fetchItem.GetVerb("have")), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.rewardYou.!random.capitalize>")), 
			QuestStoryType_FindASpecificItem.PersonalFavor => string.Format("{0} {1}. {2}. {3}. {4}. {5}", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.intro.!random.capitalize>"), Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.iHaveATask.!random.capitalize>")), Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.rumor.!random.capitalize>").Replace("*itemLocation*", "{{|" + deliveryTarget.displayName + "}}").Replace("*itemName*", fetchItem.ShortDisplayNameSingleStripped)
				.Replace("*itemName.a*", fetchItem.a)
				.Replace("*villagerName*", gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true))), Grammar.InitCap(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.loveToHave.!random.capitalize>").Replace("*it*", fetchItem.them).Replace("*NeedsItemFor*", go.GetxTag_CommaDelimited("TextFragments", "NeedsItemFor"))), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.ifYouRetrieveIt.!random.capitalize>").Replace("*it*", fetchItem.them), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>")), 
			_ => "ERROR: Failed to generate quest text from quest story type.", 
		});
		ConversationChoice conversationChoice = DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will find " + fetchItem.the + fetchItem.ShortDisplayNameSingleStripped + " as you ask.", conversationNode, quest);
		conversationChoice.RevealMapNoteId = deliveryTarget.secretId;
		ConversationChoice item = DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", conversationNode);
		conversationNode.Choices.Add(conversationChoice);
		conversationNode.Choices.Add(item);
		conversationNode.Choices.Sort();
		customConversation.AddNode(conversationNode);
		DynamicQuestConversationHelper.appendQuestCompletionSequence(customConversation, quest, conversationNode, "I've found " + fetchItem.the + fetchItem.ShortDisplayNameSingleStripped + ".", "I don't have " + fetchItem.the + fetchItem.ShortDisplayNameSingleStripped + " yet.", delegate
		{
		}, delegate(ConversationChoice gotoAcceptNodeFinalizer)
		{
			gotoAcceptNodeFinalizer.IfNotFinishedQuest = quest.ID;
			gotoAcceptNodeFinalizer.IfHaveItemWithID = deliveryItemID;
		}, delegate(ConversationChoice incompleteNodeFinalizer)
		{
			incompleteNodeFinalizer.IfNotFinishedQuest = quest.ID;
			incompleteNodeFinalizer.IfHaveItemWithID = "!" + deliveryItemID;
		}, delegate(ConversationChoice completeNodeFinalizer)
		{
			completeNodeFinalizer.TakeItem = "[byid],[restorecategory],[removequestitem],[removenoaiequip]," + deliveryItemID;
			completeNodeFinalizer.CompleteQuestStep = quest.ID + "~b_return";
		}, delegate
		{
		});
	}

	public Quest fabricateFindASpecificItemQuest(GameObject giver, string objectToFetchCacheID)
	{
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		GameObject gameObject = ZoneManager.instance.peekCachedObject(objectToFetchCacheID);
		sanctityOfSacredThing = HistoricStringExpander.ExpandString("<spice.commonPhrases.sanctity.!random> of *sacredThing*").Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement());
		switch (QST)
		{
		case QuestStoryType_FindASpecificItem.SacredItem:
			if (Stat.Random(0, 1) == 0)
			{
				quest.Name = Grammar.MakeTitleCase(gameObject.the + gameObject.ShortDisplayNameSingleStripped);
			}
			else
			{
				quest.Name = Grammar.MakeTitleCase("The " + sanctityOfSacredThing);
			}
			break;
		case QuestStoryType_FindASpecificItem.PersonalFavor:
			switch (Stat.Random(0, 2))
			{
			case 0:
				quest.Name = Grammar.MakeTitleCase(gameObject.the + gameObject.ShortDisplayNameSingleStripped);
				break;
			case 1:
				quest.Name = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.commonPhrases.helping.!random> ") + giver.DisplayNameOnlyDirectAndStripped + " to find " + gameObject.the + gameObject.ShortDisplayNameSingleStripped);
				break;
			default:
				quest.Name = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.commonPhrases.helping.!random> ") + giver.DisplayNameOnlyDirectAndStripped);
				break;
			}
			break;
		default:
			quest.Name = Grammar.MakeTitleCase(gameObject.the + gameObject.ShortDisplayNameSingleStripped);
			break;
		}
		quest.Manager = new FindASpecificItemDynamicQuestManager(objectToFetchCacheID, quest.ID, "a_locate");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_locate";
		questStep.Name = Grammar.MakeTitleCase("Find " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true));
		questStep.Text = "Locate " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + " at " + deliveryTarget.displayName + ".";
		questStep.XP = 100;
		questStep.Finished = false;
		quest.StepsByID.Add(questStep.ID, questStep);
		QuestStep questStep2 = new QuestStep();
		questStep2.ID = "b_return";
		questStep2.Name = Grammar.MakeTitleCase("Return " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + " to " + questContext.getQuestOriginZone());
		questStep2.Text = "Return " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + " to " + questContext.getQuestOriginZone() + " and speak with " + giver.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + ".";
		questStep2.XP = 100;
		questStep2.Finished = false;
		quest.StepsByID.Add(questStep2.ID, questStep2);
		quest.dynamicReward = reward;
		DynamicQuestsGamestate.addQuest(quest);
		addQuestConversationToGiver(giver, quest, gameObject);
		return quest;
	}

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
	{
		this.questGiverFilter = questGiverFilter;
	}

	public bool BuildZone(Zone zone)
	{
		List<GameObject> list = new List<GameObject>();
		this.zone = zone;
		foreach (GameObject item in zone.GetObjects().ShuffleInPlace())
		{
			if (questGiverFilter(item))
			{
				fabricateFindASpecificItemQuest(item, deliveryItemID);
				list.Add(item);
				return true;
			}
		}
		return true;
	}
}
public class FindASpecificItemDynamicQuestTemplate_FabricateQuestItem : ZoneBuilderSandbox
{
	public string deliveryItemID;

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestItem()
	{
	}

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestItem(string deliveryItemID)
		: this()
	{
		this.deliveryItemID = deliveryItemID;
	}

	public bool BuildZone(Zone zone)
	{
		GameObject cachedObjects = The.ZoneManager.GetCachedObjects(deliveryItemID);
		cachedObjects.SetIntProperty("norestock", 1);
		List<GameObject> objectsWithTagOrProperty = zone.GetObjectsWithTagOrProperty("LairOwner");
		if (objectsWithTagOrProperty.Count > 0)
		{
			GameObject randomElement = objectsWithTagOrProperty.GetRandomElement();
			randomElement.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
			randomElement.pBrain.PerformEquip();
			return true;
		}
		List<GameObject> objectsWithTagOrProperty2 = zone.GetObjectsWithTagOrProperty("NamedVillager");
		if (objectsWithTagOrProperty2.Count > 0)
		{
			GameObject randomElement2 = objectsWithTagOrProperty2.GetRandomElement();
			randomElement2.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
			randomElement2.pBrain.PerformEquip();
			return true;
		}
		GameObject gameObject = zone.GetObjectWithTag("RelicContainer") ?? GameObject.create("RelicChest");
		gameObject.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
		gameObject.SetImportant(flag: true);
		if (gameObject.CurrentCell == null || gameObject.CurrentCell.IsSolid())
		{
			List<Cell> list = zone.GetEmptyCellsWithNoFurniture();
			if (list.Count == 0)
			{
				list = zone.GetEmptyCells();
			}
			if (list.Count == 0)
			{
				list = zone.GetEmptyReachableCells();
			}
			if (list.Count == 0)
			{
				list = zone.GetReachableCells();
			}
			if (list.Count == 0)
			{
				list = zone.GetCells();
			}
			list.GetRandomElement()?.AddObject(gameObject);
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class InteractWithAnObjectDynamicQuestManager : QuestManager
{
	[Serializable]
	public class InteractWithAnObjectDynamicQuestManager_QuestEntry
	{
		public string itemID;

		public string verb;

		public string questID;

		public string questStepID;
	}

	public string _itemID;

	public string _questID;

	public string _questStepID;

	public List<InteractWithAnObjectDynamicQuestManager_QuestEntry> quests;

	public InteractWithAnObjectDynamicQuestManager()
	{
	}

	public InteractWithAnObjectDynamicQuestManager(string itemID, string questID, string questStepID)
	{
		_itemID = itemID;
		_questID = questID;
		_questStepID = questStepID;
	}

	public override void OnQuestAdded()
	{
		InteractWithAnObjectDynamicQuestManager interactWithAnObjectDynamicQuestManager = The.Player.RequirePart<InteractWithAnObjectDynamicQuestManager>();
		InteractWithAnObjectDynamicQuestManager_QuestEntry interactWithAnObjectDynamicQuestManager_QuestEntry = new InteractWithAnObjectDynamicQuestManager_QuestEntry();
		GameObject gameObject = The.ZoneManager.peekCachedObject(_itemID);
		interactWithAnObjectDynamicQuestManager_QuestEntry.itemID = _itemID;
		interactWithAnObjectDynamicQuestManager_QuestEntry.verb = gameObject.GetStringProperty("QuestVerb");
		interactWithAnObjectDynamicQuestManager_QuestEntry.questID = _questID;
		interactWithAnObjectDynamicQuestManager_QuestEntry.questStepID = _questStepID;
		if (interactWithAnObjectDynamicQuestManager.quests == null)
		{
			interactWithAnObjectDynamicQuestManager.quests = new List<InteractWithAnObjectDynamicQuestManager_QuestEntry>();
		}
		interactWithAnObjectDynamicQuestManager.quests.Add(interactWithAnObjectDynamicQuestManager_QuestEntry);
		The.Game.Player.Body.RegisterPartEvent(interactWithAnObjectDynamicQuestManager, gameObject.GetStringProperty("QuestEvent"));
	}

	public override void OnQuestComplete()
	{
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}

	public override bool FireEvent(Event E)
	{
		if (!ParentObject.IsPlayer())
		{
			return true;
		}
		while (quests != null)
		{
			foreach (InteractWithAnObjectDynamicQuestManager_QuestEntry quest in quests)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				if (gameObjectParameter != null && gameObjectParameter.idmatch(quest.itemID))
				{
					The.Game.FinishQuestStep(quest.questID, quest.questStepID);
					GameObject gameObject = The.ZoneManager.peekCachedObject(quest.itemID);
					if (gameObject == null)
					{
						Debug.LogError("no cached object for item ID " + quest.itemID);
						return true;
					}
					string[] array = quest.verb.Split(' ');
					string text = ((array.Length > 1) ? (Grammar.PastTenseOf(array[0]) + " " + string.Join(" ", array.Skip(1).ToArray())) : Grammar.PastTenseOf(quest.verb));
					JournalAPI.AddAccomplishment("You " + text + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false) + ".", "While exploring " + gameObjectParameter.CurrentZone.DisplayName + ", =name= " + text + " the fabled contraption called " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + ".", "general", JournalAccomplishment.MuralCategory.FindsObject, JournalAccomplishment.MuralWeight.Medium, null, -1L);
					quests.Remove(quest);
					if (quests.Count == 0)
					{
						IComponent<GameObject>.ThePlayer.RemovePart(this);
						return true;
					}
					goto IL_000f;
				}
			}
			break;
			IL_000f:;
		}
		return base.FireEvent(E);
	}
}
[Serializable]
public class InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	public DynamicQuestReward reward;

	public DynamicQuestDeliveryTarget deliveryTarget;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public DynamicQuestContext questContext;

	public new Zone zone;

	public string plan;

	public QuestStoryType_InteractWithAnObject QST;

	public void addQuestConversationToGiver(GameObject go, Quest quest, GameObject fetchItem)
	{
		go.SetStringProperty("GivesDynamicQuest", quest.ID);
		go.RequirePart<Interesting>();
		ConversationScript conversationScript = go.RequirePart<ConversationScript>();
		Conversation customConversation = conversationScript.customConversation;
		if (customConversation == null)
		{
			MetricsManager.LogEditorError("InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver::addQuestConversationToGiver", "Jason we tried to add a dynamic quest to an NPC with static conversations (" + go.Blueprint + ") maybe we want to merge the contents of this somehow?");
			conversationScript.customConversation = new Conversation();
			customConversation = conversationScript.customConversation;
		}
		customConversation.ID = Guid.NewGuid().ToString();
		go.SetIntProperty("QuestGiver", 1);
		if (go.pBrain != null)
		{
			go.pBrain.Wanders = false;
			go.pBrain.WandersRandomly = false;
		}
		string text = null;
		string stringProperty = fetchItem.GetStringProperty("QuestVerb");
		switch (QST)
		{
		case QuestStoryType_InteractWithAnObject.HolyItem:
			text = ((!(stringProperty != "desecrate")) ? string.Format("{0}? {1}. {2}. {3}? {4}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.intro.!random.capitalize>").Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped).Replace("*deliveryTarget*", ConsoleLib.Console.ColorUtility.StripFormatting(deliveryTarget.displayName)), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.itIsDespicable.!random.capitalize>").Replace("*It*", fetchItem.It).Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.honorUsDesecrate.!random.capitalize>").Replace("*it*", fetchItem.it).Replace("*verb*", stringProperty), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.rewardYou.!random.capitalize>")) : string.Format("{0}? {1}. {2}. {3}. {4}? {5}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.intro.!random.capitalize>").Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped).Replace("*deliveryTarget*", ConsoleLib.Console.ColorUtility.StripFormatting(deliveryTarget.displayName)), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.itIsHoly.!random.capitalize>").Replace("*It*", fetchItem.It), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willInteract.!random.capitalize>").Replace("*it*", fetchItem.it).Replace("*verb*", stringProperty)
				.Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.honorUs.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.rewardYou.!random.capitalize>")));
			break;
		case QuestStoryType_InteractWithAnObject.StrangePlan:
			if (If.CoinFlip())
			{
				quest.Name = Grammar.MakeTitleCase(Grammar.MakePossessive(go.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true)) + " Strange " + plan);
			}
			text = string.Format("{0}\n\n{1} {2}. {3}. {4}. {5} {6}.\n\n{7}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.intro.!random>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.comeClose.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.myPlan.!random.capitalize>").Replace("*plan*", plan), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.goTo.!random.capitalize>").Replace("*verb*", stringProperty).Replace("*deliveryTarget*", ConsoleLib.Console.ColorUtility.StripFormatting(deliveryTarget.displayName))
				.Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped), "No, I cannot tell you why", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.IRewardYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.byThe_TellNoOne.!random.capitalize>").Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()));
			break;
		default:
			text = "ERROR: Failed to generate quest text from quest story type.";
			break;
		}
		ConversationNode conversationNode = ConversationsAPI.newNode(Grammar.ConvertAtoAn(text));
		ConversationChoice conversationChoice = DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will " + stringProperty + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + " as you ask.", conversationNode, quest);
		conversationChoice.RevealMapNoteId = deliveryTarget.secretId;
		ConversationChoice item = DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", conversationNode);
		conversationNode.Choices.Add(conversationChoice);
		conversationNode.Choices.Add(item);
		conversationNode.Choices.Sort();
		customConversation.AddNode(conversationNode);
		string[] array = stringProperty.Split(' ');
		string text2 = ((array.Length > 1) ? (Grammar.PastTenseOf(array[0]) + " " + string.Join(" ", array.Skip(1).ToArray())) : Grammar.PastTenseOf(stringProperty));
		DynamicQuestConversationHelper.appendQuestCompletionSequence(customConversation, quest, conversationNode, "I've " + text2 + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + ".", "I haven't " + text2 + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + " yet.", delegate
		{
		}, delegate(ConversationChoice gotoAcceptNodeFinalizer)
		{
			gotoAcceptNodeFinalizer.IfNotFinishedQuest = quest.ID;
			gotoAcceptNodeFinalizer.IfFinishedQuestStep = quest.ID + "~a_use";
		}, delegate(ConversationChoice incompleteNodeFinalizer)
		{
			incompleteNodeFinalizer.IfNotFinishedQuest = quest.ID;
			incompleteNodeFinalizer.IfNotFinishedQuestStep = quest.ID + "~a_use";
		}, delegate(ConversationChoice completeNodeFinalizer)
		{
			completeNodeFinalizer.CompleteQuestStep = quest.ID + "~b_return";
		}, delegate
		{
		});
	}

	public Quest fabricateInteractWithAnObjectQuest(GameObject giver, string objectToFetchCacheID)
	{
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		GameObject gameObject = ZoneManager.instance.peekCachedObject(objectToFetchCacheID);
		string stringProperty = gameObject.GetStringProperty("QuestVerb");
		plan = HistoricStringExpander.ExpandString("<spice.commonPhrases.plan.!random>");
		quest.Name = (If.CoinFlip() ? Grammar.MakeTitleCase(stringProperty + " " + gameObject.t()) : Grammar.MakeTitleCase(gameObject.T()));
		quest.Manager = new InteractWithAnObjectDynamicQuestManager(objectToFetchCacheID, quest.ID, "a_use");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_use";
		questStep.Name = Grammar.MakeTitleCase(stringProperty + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true));
		questStep.Text = "Travel to " + deliveryTarget.displayName + " and " + stringProperty + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true) + ".";
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
		addQuestConversationToGiver(giver, quest, gameObject);
		return quest;
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
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
				fabricateInteractWithAnObjectQuest(item, deliveryItemID);
				list.Add(item);
				return true;
			}
		}
		return true;
	}
}
public class InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem : ZoneBuilderSandbox
{
	public string deliveryItemID;

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem()
	{
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem(string deliveryItemID)
	{
		this.deliveryItemID = deliveryItemID;
	}

	public bool BuildZone(Zone zone)
	{
		GameObject cachedObjects = The.ZoneManager.GetCachedObjects(deliveryItemID);
		cachedObjects.SetIntProperty("norestock", 1);
		List<GameObject> objectsWithTagOrProperty = zone.GetObjectsWithTagOrProperty("LairOwner");
		if (objectsWithTagOrProperty.Count > 0 && cachedObjects.pPhysics.Owner == null)
		{
			GameObject randomElement = objectsWithTagOrProperty.GetRandomElement();
			if (randomElement.pBrain != null)
			{
				cachedObjects.pPhysics.Owner = randomElement.pBrain.GetPrimaryFaction();
			}
		}
		if (cachedObjects.pPhysics.Owner == null)
		{
			List<GameObject> objectsWithTagOrProperty2 = zone.GetObjectsWithTagOrProperty("Villager");
			if (objectsWithTagOrProperty2.Count > 0)
			{
				GameObject randomElement2 = objectsWithTagOrProperty2.GetRandomElement();
				if (randomElement2.pBrain != null)
				{
					cachedObjects.pPhysics.Owner = randomElement2.pBrain.GetPrimaryFaction();
				}
			}
		}
		if (cachedObjects.pPhysics.Owner == null)
		{
			List<GameObject> objectsWithPart = zone.GetObjectsWithPart("Brain");
			if (objectsWithPart.Count > 0)
			{
				GameObject randomElement3 = objectsWithPart.GetRandomElement();
				if (randomElement3.pBrain != null)
				{
					cachedObjects.pPhysics.Owner = randomElement3.pBrain.GetPrimaryFaction();
				}
			}
		}
		List<Cell> list = zone.GetEmptyCells();
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
		if (list.Count > 0)
		{
			list.ShuffleInPlace()[0].AddObject(cachedObjects);
		}
		return true;
	}
}

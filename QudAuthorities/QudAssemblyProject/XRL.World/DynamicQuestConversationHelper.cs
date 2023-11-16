using System;
using HistoryKit;
using Qud.API;

namespace XRL.World;

public static class DynamicQuestConversationHelper
{
	public static ConversationChoice fabricateIntroAcceptChoice(string text, ConversationNode questIntroNode, Quest quest)
	{
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.ID = Guid.NewGuid().ToString();
		conversationChoice.Text = text;
		conversationChoice.GotoID = "End";
		conversationChoice.ParentNode = questIntroNode;
		conversationChoice.StartQuest = quest.ID;
		conversationChoice.IfNotHaveQuest = quest.ID;
		if (quest.dynamicReward != null && !string.IsNullOrEmpty(quest.dynamicReward.getRewardAcceptQuestText()))
		{
			conversationChoice.AcceptQuestText = quest.dynamicReward.getRewardAcceptQuestText();
		}
		return conversationChoice;
	}

	public static ConversationChoice fabricateIntroRejectChoice(string text, ConversationNode questIntroNode)
	{
		return new ConversationChoice
		{
			ID = Guid.NewGuid().ToString(),
			Text = text,
			GotoID = "End",
			ParentNode = questIntroNode
		};
	}

	public static ConversationChoice fabricateIntroAdditionalChoice(string text, ConversationNode questIntroNode)
	{
		return new ConversationChoice
		{
			ID = Guid.NewGuid().ToString(),
			Text = text,
			GotoID = "End",
			ParentNode = questIntroNode
		};
	}

	public static void appendQuestCompletionSequence(Conversation conversation, Quest quest, ConversationNode questIntroNode, string completeText, string incompleteText, Action<ConversationChoice> questIntroChoiceFinalizer = null, Action<ConversationChoice> gotoAcceptNodeFinalizer = null, Action<ConversationChoice> incompleteNodeFinalizer = null, Action<ConversationChoice> completeNodeFinalizer = null, Action<ConversationNode> startNodeFinalizer = null)
	{
		ConversationNode conversationNode = null;
		string text = "Choice";
		if (quest.dynamicReward != null)
		{
			text = quest.dynamicReward.getRewardConversationType();
		}
		if (text == "Choice")
		{
			conversationNode = conversation.AddNode(HistoricStringExpander.ExpandString("<spice.quests.thanks.!random.capitalize>. Our village owes you a debt. For now, please choose a reward from our stockpile as payment for your service."));
			ConversationChoice obj = conversationNode.AddChoice("Live and drink.", "End");
			completeNodeFinalizer(obj);
		}
		else if (text == "VillageZeroMainQuest")
		{
			ConversationNode conversationNode2 = conversation.AddNode("They are disciples of Barathrum. Mostly they are Urshiib, like their mentor. Mutant albino cave bears. With quills to boot! A thousand years ago Barathrum and his kin crossed the Homs Delta into the heart of Qud. He has spent centuries fiddling with the tokens of antiquity in his underground workshops.");
			conversationNode2.AddChoice("I will take the disk to Grit Gate and speak with the Barathrumites.", "End", delegate(ConversationChoice choice)
			{
				choice.StartQuest = "A Signal in the Noise";
				choice.GiveItem = "Stamped Data Disk";
			});
			conversationNode2.AddChoice("I must pass on this offer, for now. Live and drink.", "End");
			ConversationNode conversationNode3 = conversation.AddNode("Are you seeking more work, =player.formalAddressTerm=? Recently we came into possession of a data disk bearing a peculiar stamp and encoded with a strange signal. The signal means nothing to us, but there's a sect of tinkers called the Barathrumites who might be interested in it. They are friends to our village and often trade for the scrap we tow out of the earth. Would you carry the disk to their enclave at Grit Gate? In exchange for the delivery, you might seek an apprenticeship with them.\n\nIf you are interested, take the disk now, and travel safely.");
			conversationNode3.AddChoice("Who are these Barathrumites?", conversationNode2.ID);
			conversationNode3.AddChoice("I will take the disk to Grit Gate and speak with the Barathrumites.", "End", delegate(ConversationChoice choice)
			{
				choice.StartQuest = "A Signal in the Noise";
				choice.GiveItem = "Stamped Data Disk";
			});
			conversationNode = conversation.AddNode(HistoricStringExpander.ExpandString("<spice.quests.thanks.!random.capitalize>. You've proven =player.reflexive= a friend to our village. Take this recoiler and return whenever your throat is dry."));
			ConversationChoice obj2 = conversationNode.AddChoice("My thanks, =pronouns.formalAddressTerm=.", conversationNode3.ID);
			completeNodeFinalizer(obj2);
			if (!The.Game.HasQuest("More Than a Willing Spirit"))
			{
				foreach (ConversationNode startNode in conversation.StartNodes)
				{
					startNode.AddChoice("Who are these Barathrumites?", conversationNode3.ID, delegate(ConversationChoice node)
					{
						node.IfFinishedQuest = quest.ID;
						node.IfNotHaveQuest = "A Signal in the Noise";
					});
				}
			}
		}
		foreach (ConversationNode startNode2 in conversation.StartNodes)
		{
			ConversationChoice conversationChoice = ConversationsAPI.GenericQuestIntro(quest.ID, startNode2, questIntroNode);
			questIntroChoiceFinalizer(conversationChoice);
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.Text = completeText;
			conversationChoice2.GotoID = conversationNode.ID;
			conversationChoice2.ParentNode = startNode2;
			conversationChoice2.IfHaveQuest = quest.ID;
			conversationChoice2.IfNotFinishedQuest = quest.ID;
			gotoAcceptNodeFinalizer(conversationChoice2);
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.Text = incompleteText;
			conversationChoice3.GotoID = "End";
			conversationChoice3.ParentNode = startNode2;
			conversationChoice3.IfHaveQuest = quest.ID;
			incompleteNodeFinalizer(conversationChoice3);
			startNode2.Choices.Add(conversationChoice);
			startNode2.Choices.Add(conversationChoice2);
			startNode2.Choices.Add(conversationChoice3);
			startNode2.Choices.Sort();
			startNodeFinalizer(startNode2);
		}
	}
}

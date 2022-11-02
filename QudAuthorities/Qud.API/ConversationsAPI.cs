using System;
using System.Collections.Generic;
using HistoryKit;
using UnityEngine;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Qud.API;

public static class ConversationsAPI
{
	public static ConversationNode newNode(string text, string ID = null)
	{
		ConversationNode conversationNode = new ConversationNode();
		if (ID == null)
		{
			ID = Guid.NewGuid().ToString();
		}
		conversationNode.ID = ID;
		conversationNode.Text = text;
		return conversationNode;
	}

	public static Conversation addSimpleRootInformationOption(XRL.World.GameObject go, string optionText, string choiceText)
	{
		Conversation customConversation = go.GetPart<ConversationScript>().customConversation;
		string text = Guid.NewGuid().ToString();
		customConversation.StartNodes[0].AddChoice(optionText, text);
		ConversationNode conversationNode = newNode(choiceText, text);
		conversationNode.AddChoice("I have more to ask.", "Start");
		customConversation.AddNode(conversationNode);
		return customConversation;
	}

	public static XRL.World.GameObject chooseOneItem(List<XRL.World.GameObject> objects, string title, bool allowEscape)
	{
		List<XRL.World.GameObject> list = new List<XRL.World.GameObject>();
		List<string> list2 = new List<string>();
		List<char> list3 = new List<char>();
		char c = 'a';
		foreach (XRL.World.GameObject @object in objects)
		{
			list.Add(@object);
			list2.Add(@object.DisplayName);
			list3.Add(c);
			c = (char)(c + 1);
		}
		int num = Popup.ShowOptionList("Choose a reward", list2.ToArray(), list3.ToArray(), 1, null, 60, RespectOptionNewlines: false, allowEscape);
		if (num < 0)
		{
			return null;
		}
		return list[num];
	}

	public static Conversation addSimpleConversationToObject(XRL.World.GameObject go, string introDefault, string endTextDefault, string Filter = null, string FilterExtras = null, string Append = null, bool TradeNote = false, bool ClearLost = false)
	{
		if (go.HasPart("ConversationScript"))
		{
			go.RemovePart("ConversationScript");
			Debug.LogWarning("Removing ConversationScript from " + go.Blueprint);
		}
		ConversationScript conversationScript = go.AddPart<ConversationScript>();
		conversationScript.customConversation = new Conversation();
		conversationScript.ClearLost = ClearLost;
		conversationScript.Filter = Filter;
		conversationScript.FilterExtras = FilterExtras;
		conversationScript.Append = Append;
		Conversation customConversation = conversationScript.customConversation;
		customConversation.ID = Guid.NewGuid().ToString();
		ConversationNode conversationNode = newNode(introDefault, "Start");
		conversationNode.AddChoice(endTextDefault, "End");
		conversationNode.TradeNote = TradeNote;
		customConversation.AddNode(conversationNode);
		return customConversation;
	}

	public static Conversation appendSimpleConversationToObject(XRL.World.GameObject go, string introDefault, string endTextDefault, string Filter = null, string FilterExtras = null, string Append = null, bool TradeNote = false, bool ClearLost = false)
	{
		Conversation customConversation;
		if (!go.HasPart("ConversationScript") || go.GetPart<ConversationScript>().customConversation == null)
		{
			go.RemovePart("ConversationScript");
			ConversationScript conversationScript = go.AddPart<ConversationScript>();
			conversationScript.customConversation = new Conversation();
			conversationScript.ClearLost = ClearLost;
			conversationScript.Filter = Filter;
			conversationScript.FilterExtras = FilterExtras;
			conversationScript.Append = Append;
			customConversation = conversationScript.customConversation;
			customConversation.ID = Guid.NewGuid().ToString();
			ConversationNode conversationNode = newNode(introDefault, "Start");
			conversationNode.AddChoice(endTextDefault, "End");
			conversationNode.TradeNote = TradeNote;
			customConversation.AddNode(conversationNode);
		}
		else
		{
			customConversation = go.GetPart<ConversationScript>().customConversation;
		}
		return customConversation;
	}

	public static void addSimpleConversationToObject(XRL.World.GameObject go, string introDefault, string endTextDefault, string Q1, string A1, string Filter = null, string FilterExtras = null, string Append = null, bool TradeNote = false, bool ClearLost = false)
	{
		Conversation conversation = addSimpleConversationToObject(go, introDefault, endTextDefault, Filter, FilterExtras, Append, TradeNote, ClearLost);
		ConversationNode conversationNode = newNode(A1, "A1");
		conversationNode.AddChoice(endTextDefault, "End");
		conversation.AddNode(conversationNode);
		conversation.StartNodes[0].AddChoice(Q1, "A1");
	}

	public static void appendSimpleConversationToObject(XRL.World.GameObject go, string introDefault, string endTextDefault, string Q1, string A1, string Filter = null, string FilterExtras = null, string Append = null, bool TradeNote = false, bool ClearLost = false)
	{
		Conversation conversation = appendSimpleConversationToObject(go, introDefault, endTextDefault, Filter, FilterExtras, Append, TradeNote, ClearLost);
		ConversationNode conversationNode = newNode(A1, "A1");
		conversationNode.AddChoice(endTextDefault, "End");
		conversation.AddNode(conversationNode);
		conversation.StartNodes[0].AddChoice(Q1, "A1");
	}

	public static ConversationChoice GenericQuestIntro(string questID, ConversationNode parentNode, ConversationNode goToNode)
	{
		return new ConversationChoice
		{
			ID = Guid.NewGuid().ToString(),
			Text = HistoricStringExpander.ExpandString("<spice.quests.intro.!random.capitalize>"),
			GotoID = goToNode.ID,
			ParentNode = parentNode,
			IfNotHaveQuest = questID,
			IfNotFinishedQuest = questID
		};
	}

	public static ConversationChoice GenericQuestOutro(string questID, ConversationNode parentNode, ConversationNode goToNode)
	{
		return new ConversationChoice
		{
			ID = Guid.NewGuid().ToString(),
			Text = HistoricStringExpander.ExpandString("<spice.quests.intro.!random.capitalize>"),
			GotoID = goToNode.ID,
			ParentNode = parentNode,
			IfNotHaveQuest = questID,
			IfNotFinishedQuest = questID
		};
	}
}

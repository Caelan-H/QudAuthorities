using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;

namespace XRL.World.Parts;

[Serializable]
public class DynamicQuestSignpostConversation : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (ParentObject.GetCurrentCell() == null)
		{
			return false;
		}
		List<(GameObject, Quest)> list = new List<(GameObject, Quest)>();
		foreach (GameObject @object in ParentObject.GetCurrentCell().ParentZone.GetObjects((GameObject o) => o.HasProperty("GivesDynamicQuest")))
		{
			string stringProperty = @object.GetStringProperty("GivesDynamicQuest");
			if (!The.Game.HasQuest(stringProperty))
			{
				if (The.Game.Quests.ContainsKey(stringProperty))
				{
					list.Add((@object, The.Game.Quests[stringProperty]));
				}
				else
				{
					list.Add((@object, null));
				}
			}
		}
		if (list.Count == 0 || list.Any(((GameObject, Quest) t) => t.Item1 == ParentObject))
		{
			foreach (ConversationNode startNode in E.Conversation.StartNodes)
			{
				foreach (ConversationChoice item2 in new List<ConversationChoice>(startNode.Choices.Where((ConversationChoice n) => n.GotoID == "*DynamicQuestSignpostConversationIntro")))
				{
					startNode.Choices.Remove(item2);
				}
			}
		}
		else
		{
			foreach (ConversationNode startNode2 in E.Conversation.StartNodes)
			{
				if (!startNode2.Choices.Any((ConversationChoice n) => n.GotoID == "*DynamicQuestSignpostConversationIntro"))
				{
					startNode2.AddChoice(HistoricStringExpander.ExpandString("<spice.quests.intro.!random>"), "*DynamicQuestSignpostConversationIntro");
				}
			}
		}
		if (E.Conversation.NodesByID.ContainsKey("*DynamicQuestSignpostConversationIntro"))
		{
			E.Conversation.NodesByID.Remove("*DynamicQuestSignpostConversationIntro");
		}
		if (!E.Conversation.NodesByID.ContainsKey("*DynamicQuestSignpostConversationIntro") && list.Count > 0 && !list.Any(((GameObject, Quest) t) => t.Item1 == ParentObject))
		{
			ConversationNode conversationNode = new ConversationNode();
			conversationNode.ID = "*DynamicQuestSignpostConversationIntro";
			string text = "";
			string text2 = null;
			bool flag = false;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				GameObject item = list[i].Item1;
				string text3 = ParentObject.DescribeDirectionToward(item, General: true);
				if (i > 0)
				{
					text = ((i != list.Count - 1) ? (text + ", ") : ((!flag) ? (text + " or ") : (text + ", or ")));
				}
				text = text + "{{Y|" + item.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true) + "}}";
				if (!string.IsNullOrEmpty(text3))
				{
					text = text + ", " + ((text3 == text2) ? "also " : "") + text3;
					flag = true;
					text2 = text3;
				}
			}
			conversationNode.Text = HistoricStringExpander.ExpandString("<spice.instancesOf.speakTo.!random.capitalize> ") + text + ".";
			conversationNode.AddChoice("Thank you. I have more to ask.", "Start");
			conversationNode.AddChoice("Live and drink.", "End");
			E.Conversation.AddNode(conversationNode);
		}
		return base.HandleEvent(E);
	}
}

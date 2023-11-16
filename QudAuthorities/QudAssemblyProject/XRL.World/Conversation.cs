using System;
using System.Collections.Generic;
using Qud.API;

namespace XRL.World;

[Serializable]
public class Conversation
{
	internal static Conversation Placeholder = new Conversation();

	public string ID;

	[NonSerialized]
	public string Introduction = "";

	public List<ConversationNode> StartNodes = new List<ConversationNode>();

	public Dictionary<string, ConversationNode> NodesByID = new Dictionary<string, ConversationNode>();

	public static GameObject Speaker;

	public ConversationNode AddNode(ConversationNode node)
	{
		node.ParentConversation = this;
		if (node.ID == "Start")
		{
			StartNodes.Add(node);
		}
		else
		{
			NodesByID.Add(node.ID, node);
		}
		return node;
	}

	public ConversationNode AddNode(string nodeText, string ID = null)
	{
		ConversationNode conversationNode = ConversationsAPI.newNode(nodeText, ID);
		conversationNode.ParentConversation = this;
		if (conversationNode.ID == "Start")
		{
			StartNodes.Add(conversationNode);
		}
		else
		{
			NodesByID.Add(conversationNode.ID, conversationNode);
		}
		return conversationNode;
	}

	public Conversation CloneDeep()
	{
		Conversation conversation = new Conversation
		{
			ID = ID,
			StartNodes = new List<ConversationNode>(StartNodes.Count),
			NodesByID = new Dictionary<string, ConversationNode>(NodesByID.Count)
		};
		foreach (ConversationNode startNode in StartNodes)
		{
			ConversationNode conversationNode = new ConversationNode();
			conversationNode.Copy(startNode);
			conversationNode.ParentConversation = conversation;
			conversation.StartNodes.Add(conversationNode);
		}
		foreach (ConversationNode value in NodesByID.Values)
		{
			ConversationNode conversationNode2 = new ConversationNode();
			conversationNode2.Copy(value);
			conversationNode2.ParentConversation = conversation;
			conversation.NodesByID[conversationNode2.ID] = conversationNode2;
		}
		return conversation;
	}
}

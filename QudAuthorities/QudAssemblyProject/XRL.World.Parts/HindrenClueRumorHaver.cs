using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class HindrenClueRumorHaver : IPart
{
	public string Category;

	[NonSerialized]
	private ConversationNode rumorNode;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ShowConversationChoices");
		Object.RegisterPartEvent(this, "GetConversationNode");
		base.Register(Object);
	}

	public void generateRumorNode()
	{
		if (Category == null)
		{
			Category = ParentObject.Blueprint;
		}
		rumorNode = new ConversationNode();
		rumorNode.ID = "*hindrenrumor";
		if (!ParentObject.HasProperty("VisitedRumorNode"))
		{
			rumorNode.alwaysShowAsNotVisted = true;
		}
		HindrenClueRumor rumor = HindrenMysteryGamestate.instance.getRumorForVillagerCategory(Category);
		if (rumor == null)
		{
			rumorNode.Text = "Sorry, I didn't see anything.";
			rumorNode.OnLeaveNode = delegate
			{
				ParentObject.SetStringProperty("VisitedRumorNode", "1");
			};
		}
		else
		{
			rumorNode.Text = rumor.text;
			rumorNode.OnLeaveNode = delegate
			{
				ParentObject.SetStringProperty("VisitedRumorNode", "1");
				if (The.Game.HasQuest("Kith and Kin") && !The.Game.FinishedQuest("Kith and Kin"))
				{
					rumor.trigger();
					HindrenMysteryGamestate.instance.foundClue();
				}
			};
		}
		rumorNode.Choices = new List<ConversationChoice>();
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.GotoID = "End";
		conversationChoice.Text = "Live and drink.";
		rumorNode.Choices.Add(conversationChoice);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetConversationNode")
		{
			if (rumorNode == null)
			{
				generateRumorNode();
			}
			else if (!ParentObject.HasProperty("VisitedRumorNode"))
			{
				rumorNode.alwaysShowAsNotVisted = true;
			}
			else
			{
				rumorNode.alwaysShowAsNotVisted = false;
			}
			if (E.GetStringParameter("GotoID") == "*hindrenrumor")
			{
				E.SetParameter("ConversationNode", rumorNode);
			}
		}
		else if (E.ID == "ShowConversationChoices" && The.Game.HasQuest("Kith and Kin") && !The.Game.FinishedQuest("Kith and Kin") && E.GetParameter<ConversationNode>("CurrentNode").ID == "Start")
		{
			List<ConversationChoice> list = new List<ConversationChoice>(E.GetParameter<List<ConversationChoice>>("Choices"));
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.Text = "Have you seen or heard anything peculiar?";
			conversationChoice.GotoID = "*hindrenrumor";
			list.Insert(0, conversationChoice);
			E.SetParameter("Choices", list);
		}
		return base.FireEvent(E);
	}
}

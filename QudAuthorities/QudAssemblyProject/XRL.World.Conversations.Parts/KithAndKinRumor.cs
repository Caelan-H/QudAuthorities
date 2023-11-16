namespace XRL.World.Conversations.Parts;

public class KithAndKinRumor : IConversationPart
{
	public ConversationXMLBlueprint Blueprint;

	public HindrenClueRumor Rumor;

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		this.Blueprint = Blueprint;
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		switch (The.Conversation.ID)
		{
		case "HindrenVillager":
			Rumor = HindrenMysteryGamestate.instance.getRumorForVillagerCategory("*villager");
			break;
		case "FaundrenVillager":
			Rumor = HindrenMysteryGamestate.instance.getRumorForVillagerCategory("*faundren");
			break;
		case "HindrenScout":
			Rumor = HindrenMysteryGamestate.instance.getRumorForVillagerCategory("*scout");
			break;
		}
		if (Rumor != null)
		{
			Blueprint.Text = Rumor.text;
			ParentElement.LoadChild(Blueprint);
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == LeftElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		if (Rumor != null)
		{
			Rumor.trigger();
			HindrenMysteryGamestate.instance.foundClue();
		}
		return base.HandleEvent(E);
	}
}

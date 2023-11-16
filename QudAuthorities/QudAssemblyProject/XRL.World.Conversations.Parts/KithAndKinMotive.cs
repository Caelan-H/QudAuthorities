using Qud.API;

namespace XRL.World.Conversations.Parts;

public class KithAndKinMotive : IKithAndKinPart
{
	public ConversationXMLBlueprint Blueprint;

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		this.Blueprint = Blueprint;
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		base.Motives = HindrenMysteryGamestate.instance.getKnownMotiveClues();
		foreach (JournalObservation motive in base.Motives)
		{
			Blueprint.ID = motive.id;
			Blueprint.ReplaceText(motive.text);
			ParentElement.LoadChild(Blueprint);
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != LeftElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		base.Motive = base.Motives.Find((JournalObservation x) => x.id == IConversationPart.LastChoiceID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Replace("=circumstance.influence=", base.CircumstanceInfluence);
		return base.HandleEvent(E);
	}
}

using Qud.API;

namespace XRL.World.Conversations.Parts;

public class KithAndKinCircumstance : IKithAndKinPart
{
	public ConversationXMLBlueprint Blueprint;

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		this.Blueprint = Blueprint;
		return base.LoadChild(Blueprint);
	}

	public override void Awake()
	{
		base.Circumstances = HindrenMysteryGamestate.instance.getKnownFreeClues();
		foreach (JournalObservation circumstance in base.Circumstances)
		{
			Blueprint.ID = circumstance.id;
			Blueprint.ReplaceText(circumstance.text);
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
		base.Circumstance = base.Circumstances.Find((JournalObservation x) => x.id == IConversationPart.LastChoiceID);
		return base.HandleEvent(E);
	}
}

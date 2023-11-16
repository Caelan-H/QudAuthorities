namespace XRL.World.Conversations.Parts;

public class StartFight : IConversationPart
{
	public bool BroadcastForHelp;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{R|[Fight]}}";
		return false;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (The.Speaker.PartyLeader == The.Player)
		{
			The.Speaker.PartyLeader = null;
		}
		if (BroadcastForHelp)
		{
			The.Speaker.GetAngryAt(The.Player);
		}
		else
		{
			The.Speaker.SetFeeling(The.Player, -100);
			The.Speaker.Target = The.Player;
		}
		return base.HandleEvent(E);
	}
}

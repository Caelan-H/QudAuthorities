namespace XRL.World.Conversations.Parts;

public class SlynthCandidatesReady : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnteredElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		The.Game.GetSystem<SlynthQuestSystem>()?.RollResult();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		if (system != null)
		{
			return system.candidateFactionsCount() >= SlynthQuestSystem.REQUIRED_CANDIDATES;
		}
		return false;
	}
}

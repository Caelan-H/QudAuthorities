namespace XRL.World.Conversations.Parts;

public class KithAndKinAccusation : IKithAndKinPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		string circumstanceInfluence = base.CircumstanceInfluence;
		string motiveInfluence = base.MotiveInfluence;
		string newValue = ((circumstanceInfluence == motiveInfluence) ? circumstanceInfluence : (circumstanceInfluence + " and " + motiveInfluence));
		E.Text.Replace("=all.influence=", newValue).Replace("=motive.influence=", motiveInfluence).Replace("=thief.name=", base.ThiefName);
		return base.HandleEvent(E);
	}
}

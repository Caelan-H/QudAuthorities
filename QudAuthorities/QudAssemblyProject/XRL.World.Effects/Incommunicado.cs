using System;

namespace XRL.World.Effects;

[Serializable]
public class Incommunicado : Effect
{
	public Incommunicado()
	{
		base.DisplayName = "incommunicado";
		base.Duration = 1;
	}

	public override string GetDetails()
	{
		return "Doesn't know how to rejoin leader.";
	}

	public override int GetEffectType()
	{
		return 1;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == JoinPartyLeaderPossibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject partyLeader = base.Object.PartyLeader;
		if (partyLeader == null || base.Object.InSameZone(partyLeader))
		{
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return E.Result = false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Incommunicado"))
		{
			return false;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
	}
}

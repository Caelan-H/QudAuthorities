using System;

namespace XRL.World.Parts;

[Serializable]
public class PreachOnPartyCombat : IPart
{
	public bool leaderInCombat;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CanPreach");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanPreach")
		{
			if (ParentObject.PartyLeader != null && ParentObject.PartyLeader.pBrain != null && ParentObject.PartyLeader.pBrain.Target != null)
			{
				return true;
			}
			return false;
		}
		if (E.ID == "BeginTakeAction" && ParentObject.PartyLeader != null && ParentObject.PartyLeader.pBrain != null && leaderInCombat != (ParentObject.PartyLeader.pBrain.Target != null))
		{
			if (!leaderInCombat && ParentObject.HasPart("Preacher"))
			{
				ParentObject.GetPart<Preacher>().PreacherHomily(Dialog: false);
			}
			leaderInCombat = !leaderInCombat;
		}
		return base.FireEvent(E);
	}
}

using System;
using HistoryKit;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class SultanMural : IPart
{
	public HistoricEvent secretEvent;

	public string secretID;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent != "Look") && HasUnrevealedSecret())
		{
			E.Want = true;
			E.FromAdjacent = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !string.IsNullOrEmpty(secretID))
		{
			JournalAPI.RevealSultanEventBySecretID(secretID);
		}
		return true;
	}

	public bool HasUnrevealedSecret()
	{
		if (secretID != null)
		{
			return JournalAPI.HasUnrevealedSultanEvent(secretID);
		}
		return false;
	}
}

using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealVillageHistoryOnLook : IPart
{
	public string HistoryId;

	public bool bLookedAt;

	public RevealVillageHistoryOnLook()
	{
	}

	public RevealVillageHistoryOnLook(string _HistoryId)
	{
		HistoryId = _HistoryId;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt")
		{
			if (bLookedAt)
			{
				return true;
			}
			bLookedAt = true;
			if (HistoryId == null)
			{
				return true;
			}
			JournalAPI.RevealVillageNote(HistoryId);
			return true;
		}
		return base.FireEvent(E);
	}
}

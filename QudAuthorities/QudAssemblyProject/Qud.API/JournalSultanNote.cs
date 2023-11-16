using System;
using HistoryKit;
using XRL.UI;

namespace Qud.API;

[Serializable]
public class JournalSultanNote : IBaseJournalEntry
{
	public long linkId;

	public long eventId;

	public string sultan;

	public override bool Forgettable()
	{
		HistoricEvent @event = HistoryAPI.GetEvent(eventId);
		if (@event == null)
		{
			return true;
		}
		if (@event.hasEventProperty("revealsRegion"))
		{
			return false;
		}
		if (@event.hasEventProperty("revealsItem"))
		{
			return false;
		}
		if (@event.hasEventProperty("revealsItemLocation"))
		{
			return false;
		}
		return true;
	}

	public override void Reveal(bool silent = false)
	{
		if (base.revealed)
		{
			return;
		}
		base.Reveal();
		Updated();
		if (!silent)
		{
			IBaseJournalEntry.DisplayMessage("You note this piece of information in the {{W|" + JournalScreen.STR_SULTANS + " > " + HistoryAPI.GetEntityName(sultan) + "}} section of your journal.");
		}
		bool flag = true;
		foreach (JournalSultanNote sultanNote in JournalAPI.SultanNotes)
		{
			if (sultanNote.sultan == sultan && !sultanNote.revealed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			AchievementManager.SetAchievement("ACH_LEARN_ONE_SULTAN_HISTORY");
		}
	}
}

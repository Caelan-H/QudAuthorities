using System;
using XRL.UI;

namespace Qud.API;

[Serializable]
public class JournalVillageNote : IBaseJournalEntry
{
	public string villageID;

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
			IBaseJournalEntry.DisplayMessage("You note this piece of information in the {{W|" + JournalScreen.STR_VILLAGES + " > " + HistoryAPI.GetEntityName(villageID) + "}} section of your journal.");
		}
		bool flag = true;
		foreach (JournalVillageNote villageNote in JournalAPI.VillageNotes)
		{
			if (villageNote.villageID == villageID && !villageNote.revealed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			AchievementManager.SetAchievement("ACH_LEARN_ONE_VILLAGE_HISTORY");
		}
	}

	public override string GetShortText()
	{
		return text.Split('|')[0];
	}

	public override string GetDisplayText()
	{
		if (history.Length > 0)
		{
			return text.Split('|')[0] + "\n" + history;
		}
		return text.Split('|')[0];
	}

	public long getEventId()
	{
		string[] array = text.Split('|');
		if (array.Length < 2)
		{
			throw new Exception("could not get event ID from story: " + text);
		}
		try
		{
			return Convert.ToInt64(array[1]);
		}
		catch
		{
			MetricsManager.LogError("JournalVillageNote::getEventId", "couldn't get ID from story: " + text);
			return 0L;
		}
	}
}

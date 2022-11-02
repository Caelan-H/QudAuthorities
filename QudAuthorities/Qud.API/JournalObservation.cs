using System;
using XRL.UI;

namespace Qud.API;

[Serializable]
public class JournalObservation : IBaseJournalEntry
{
	public string category;

	public string additionalRevealText;

	public long time;

	public string id;

	public bool initCapAsFragment;

	public override void Reveal(bool silent = false)
	{
		if (!base.revealed)
		{
			base.Reveal();
			Updated();
			if (!string.IsNullOrEmpty(additionalRevealText))
			{
				IBaseJournalEntry.DisplayMessage(additionalRevealText + "You note this piece of information in the {{W|" + JournalScreen.STR_OBSERVATIONS + "}} section of your journal.");
				return;
			}
			IBaseJournalEntry.DisplayMessage("{{W|" + text + "}}\n\nYou note this piece of information in the {{W|" + JournalScreen.STR_OBSERVATIONS + "}} section of your journal.");
		}
	}
}

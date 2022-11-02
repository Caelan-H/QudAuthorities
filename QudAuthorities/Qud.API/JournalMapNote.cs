using System;
using Genkit;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
public class JournalMapNote : IBaseJournalEntry
{
	public string zoneid;

	public string w;

	public int wx;

	public int wy;

	public int cx;

	public int cy;

	public int cz;

	public string category;

	public bool tracked;

	public bool shown;

	public long time;

	public bool Visited
	{
		get
		{
			if (zoneid != null)
			{
				return The.ZoneManager.VisitedTime.ContainsKey(zoneid);
			}
			return false;
		}
	}

	public long LastVisit
	{
		get
		{
			if (zoneid == null || !The.ZoneManager.VisitedTime.TryGetValue(zoneid, out var value))
			{
				return -1L;
			}
			return value;
		}
	}

	public Location2D location => Location2D.get(x, y);

	public int x => wx * 3 + cx;

	public int y => wy * 3 + cy;

	public bool SameAs(JournalMapNote note)
	{
		if (note == null)
		{
			return false;
		}
		if (zoneid == note.zoneid && w == note.w && wx == note.wx && wy == note.wy && cx == note.cx && cy == note.cy && cz == note.cz && category == note.category && text == note.text)
		{
			return secretid == note.secretid;
		}
		return false;
	}

	public override bool Forgettable()
	{
		return false;
	}

	public override string GetDisplayText()
	{
		IBaseJournalEntry.SB.Clear().Append(text);
		IBaseJournalEntry.SB.Compound(LoreGenerator.GenerateLandmarkDirectionsTo(zoneid), '\n');
		long lastVisit = LastVisit;
		if (lastVisit > 0)
		{
			IBaseJournalEntry.SB.Compound("Last visited on the ", '\n').Append(Calendar.GetDay(lastVisit)).Append(" of ")
				.Append(Calendar.GetMonth(lastVisit));
		}
		if (Options.DebugInternals)
		{
			IBaseJournalEntry.SB.Compound("\n{{internals|", '\n').Append('Ã').Append(' ');
			for (int i = 0; i < attributes.Count; i++)
			{
				IBaseJournalEntry.SB.Append(attributes[i]);
			}
			if (history.Length > 0)
			{
				IBaseJournalEntry.SB.Compound(history, '\n');
			}
			IBaseJournalEntry.SB.Append("}}");
		}
		return IBaseJournalEntry.SB.ToString();
	}

	public override void Reveal(bool silent = false)
	{
		if (!base.revealed)
		{
			JournalAPI._mapNoteCategories = null;
			base.Reveal(silent);
			string text = null;
			time = Calendar.TotalTimeTicks;
			if (!JournalAPI.MapNotes.Contains(this))
			{
				JournalAPI.MapNotes.Add(this);
			}
			if (category == "Artifacts")
			{
				text = "You note the location of " + Grammar.InitLower(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Artifacts}} section of your journal.";
			}
			else if (category == "Historic Sites")
			{
				text = "You note the location of " + Grammar.MakeTitleCaseWithArticle(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Historic Sites}} section of your journal.";
			}
			else if (category == "Lairs")
			{
				text = "You note the location of " + Grammar.InitLower(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Lairs}} section of your journal.";
			}
			else if (category == "Merchants")
			{
				text = "You note the location of " + Grammar.InitLower(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Merchants}} section of your journal.";
			}
			else if (category == "Natural Features")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Natural Features}} section of your journal.";
			}
			else if (category == "Oddities")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Oddities}} section of your journal.";
			}
			else if (category == "Baetyls")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Baetyls}} section of your journal.";
			}
			else if (category == "Ruins")
			{
				string text2 = ((base.text == "some forgotten ruins") ? Grammar.InitLower(base.text) : Grammar.MakeTitleCaseWithArticle(base.text));
				text = "You note the location of " + text2 + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Ruins}} section of your journal.";
			}
			else if (category == "Settlements")
			{
				text = "You note the location of " + Grammar.InitLowerIfArticle(base.text) + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Settlements}} section of your journal.";
			}
			else if (category == "Named Locations")
			{
				text = "You note the location of " + base.text + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Named Locations}} section of your journal.";
			}
			else if (category == "Ruins with Becoming Nooks")
			{
				string text3 = ((base.text == "some forgotten ruins") ? Grammar.InitLower(base.text) : Grammar.MakeTitleCaseWithArticle(base.text));
				text = "You note the location of " + text3 + " in the {{W|" + JournalScreen.STR_LOCATIONS + " > Ruins with Becoming Nooks}} section of your journal.";
			}
			else
			{
				text = "tell support@freeholdgames.com unknown location category: " + category;
			}
			if (category != "Miscellaneous" && !silent)
			{
				IBaseJournalEntry.DisplayMessage(text);
			}
		}
	}
}

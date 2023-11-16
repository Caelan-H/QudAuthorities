using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenSecret : SifrahPrioritizableToken
{
	private string UseFaction;

	private string RepresentativeID;

	public SocialSifrahTokenSecret()
	{
		Description = "tell a secret";
		Tile = "Items/sw_book1.bmp";
		RenderString = "\u000e";
		ColorString = "&K";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenSecret(GameObject Representative)
		: this()
	{
		UseFaction = Representative.GetPrimaryFaction();
		RepresentativeID = Representative.id;
	}

	public override int GetPriority()
	{
		return GetNumberAvailable();
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public List<IBaseJournalEntry> GetAvailable()
	{
		if (string.IsNullOrEmpty(RepresentativeID))
		{
			return null;
		}
		GameObject rep = GameObject.findById(RepresentativeID);
		if (rep == null)
		{
			return null;
		}
		List<JournalSultanNote> sultanNotes = JournalAPI.GetSultanNotes((JournalSultanNote note) => Faction.WantsToBuySecret(UseFaction, note, rep));
		List<JournalObservation> observations = JournalAPI.GetObservations((JournalObservation observation) => Faction.WantsToBuySecret(UseFaction, observation, rep));
		List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote mapnote) => Faction.WantsToBuySecret(UseFaction, mapnote, rep));
		List<JournalRecipeNote> recipes = JournalAPI.GetRecipes((JournalRecipeNote recipenote) => Faction.WantsToBuySecret(UseFaction, recipenote, rep));
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>(sultanNotes.Count + observations.Count + mapNotes.Count + recipes.Count);
		foreach (JournalSultanNote item in sultanNotes)
		{
			list.Add(item);
		}
		foreach (JournalObservation item2 in observations)
		{
			list.Add(item2);
		}
		foreach (JournalMapNote item3 in mapNotes)
		{
			list.Add(item3);
		}
		foreach (JournalRecipeNote item4 in recipes)
		{
			list.Add(item4);
		}
		return list;
	}

	public int GetNumberAvailable(int Chosen = 0)
	{
		if (string.IsNullOrEmpty(RepresentativeID))
		{
			return 0;
		}
		GameObject rep = GameObject.findById(RepresentativeID);
		if (rep == null)
		{
			return 0;
		}
		List<JournalSultanNote> sultanNotes = JournalAPI.GetSultanNotes((JournalSultanNote note) => Faction.WantsToBuySecret(UseFaction, note, rep));
		List<JournalObservation> observations = JournalAPI.GetObservations((JournalObservation observation) => Faction.WantsToBuySecret(UseFaction, observation, rep));
		List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote mapnote) => Faction.WantsToBuySecret(UseFaction, mapnote, rep));
		List<JournalRecipeNote> recipes = JournalAPI.GetRecipes((JournalRecipeNote recipenote) => Faction.WantsToBuySecret(UseFaction, recipenote, rep));
		return sultanNotes.Count + observations.Count + mapNotes.Count + recipes.Count - Chosen;
	}

	public bool IsAvailable(int Chosen = 0)
	{
		if (string.IsNullOrEmpty(RepresentativeID))
		{
			return false;
		}
		GameObject rep = GameObject.findById(RepresentativeID);
		if (rep == null)
		{
			return false;
		}
		int num = 0;
		List<JournalSultanNote> sultanNotes = JournalAPI.GetSultanNotes((JournalSultanNote note) => Faction.WantsToBuySecret(UseFaction, note, rep));
		num += sultanNotes.Count;
		if (num > Chosen)
		{
			return true;
		}
		List<JournalObservation> observations = JournalAPI.GetObservations((JournalObservation observation) => Faction.WantsToBuySecret(UseFaction, observation, rep));
		num += observations.Count;
		if (num > Chosen)
		{
			return true;
		}
		List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote mapnote) => Faction.WantsToBuySecret(UseFaction, mapnote, rep));
		num += mapNotes.Count;
		if (num > Chosen)
		{
			return true;
		}
		List<JournalRecipeNote> recipes = JournalAPI.GetRecipes((JournalRecipeNote recipenote) => Faction.WantsToBuySecret(UseFaction, recipenote, rep));
		num += recipes.Count;
		if (num > Chosen)
		{
			return true;
		}
		return false;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description + " [have {{C|" + GetNumberAvailable(Game.GetTimesChosen(this, Slot)) + "}}]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable(Game.GetTimesChosen(this, Slot)))
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this);
		if (!IsAvailable(timesChosen))
		{
			Popup.ShowFail("You do not have any " + ((timesChosen > 0) ? "more " : "") + "secrets " + ContextObject.t() + ContextObject.Is + " interested in.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		List<string> list = new List<string>();
		List<IBaseJournalEntry> list2 = new List<IBaseJournalEntry>();
		int num = 3;
		List<IBaseJournalEntry> available = GetAvailable();
		for (int i = 0; i < num && i < available.Count; i++)
		{
			int index = -1;
			int num2 = int.MaxValue;
			for (int j = 0; j < available.Count; j++)
			{
				if (!list2.Contains(available[j]))
				{
					int num3 = WaterRitualNode.SharesNPropertiesWith(available[j], list2);
					if (num3 < num2)
					{
						index = j;
						num2 = num3;
					}
				}
			}
			if (available[index] is JournalMapNote)
			{
				list.Add("The location of " + Grammar.LowerArticles(available[index].GetShortText()));
			}
			else
			{
				list.Add(available[index].GetShortText());
			}
			list2.Add(available[index]);
		}
		int index2 = Popup.ShowOptionList("Choose a secret to share:\n", list.ToArray(), new char[3] { 'a', 'b', 'c' }, 1, null, 60, RespectOptionNewlines: true);
		list2[index2].secretSold = true;
		if (Factions.get(UseFaction).Visible)
		{
			if (list2[index2].history.Length > 0)
			{
				list2[index2].history += "\n";
			}
			IBaseJournalEntry baseJournalEntry = list2[index2];
			baseJournalEntry.history = baseJournalEntry.history + " {{K|-shared with " + Faction.getFormattedName(UseFaction) + "}}";
		}
		list2[index2].Updated();
		base.UseToken(Game, Slot, ContextObject);
	}
}

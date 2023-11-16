using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using Wintellect.PowerCollections;
using XRL.Language;

namespace XRL.World.Conversations.Parts;

public abstract class IWaterRitualSecretPart : IWaterRitualPart
{
	public const int REP_SECRET = 50;

	public const int REP_GOSSIP = 100;

	public IEnumerable<IBaseJournalEntry> JournalNotes
	{
		get
		{
			foreach (JournalSultanNote sultanNote in JournalAPI.SultanNotes)
			{
				if (SultanNote(sultanNote))
				{
					yield return sultanNote;
				}
			}
			foreach (JournalObservation observation in JournalAPI.Observations)
			{
				if (Observation(observation))
				{
					yield return observation;
				}
			}
			foreach (JournalMapNote mapNote in JournalAPI.MapNotes)
			{
				if (MapNote(mapNote))
				{
					yield return mapNote;
				}
			}
			foreach (JournalRecipeNote recipeNote in JournalAPI.RecipeNotes)
			{
				if (RecipeNote(recipeNote))
				{
					yield return recipeNote;
				}
			}
		}
	}

	public abstract bool Filter(IBaseJournalEntry Entry);

	public virtual bool SultanNote(JournalSultanNote Entry)
	{
		return true;
	}

	public virtual bool Observation(JournalObservation Entry)
	{
		return true;
	}

	public virtual bool MapNote(JournalMapNote Entry)
	{
		return true;
	}

	public virtual bool RecipeNote(JournalRecipeNote Entry)
	{
		return true;
	}

	public override void Awake()
	{
		Visible = JournalNotes.Any(Filter);
	}

	protected int CountShared(IBaseJournalEntry Entry, List<IBaseJournalEntry> Notes)
	{
		int num = 0;
		foreach (IBaseJournalEntry Note in Notes)
		{
			if (Entry.text == Note.text)
			{
				num++;
			}
			num += Note.attributes.Count(Entry.Has);
		}
		return num;
	}

	public void GetShuffledSecrets(Random R, out List<IBaseJournalEntry> Notes, out List<string> Options)
	{
		List<IBaseJournalEntry> list = JournalNotes.Where(Filter).ToList();
		Algorithms.RandomShuffleInPlace(list, R);
		Notes = new List<IBaseJournalEntry>(3);
		Options = new List<string>(3);
		for (int i = 0; i < 3 && i < list.Count; i++)
		{
			int index = -1;
			int num = int.MaxValue;
			for (int j = 0; j < list.Count; j++)
			{
				if (!Notes.Contains(list[j]))
				{
					int num2 = CountShared(list[j], Notes);
					if (num2 < num)
					{
						index = j;
						num = num2;
					}
				}
			}
			IBaseJournalEntry baseJournalEntry = list[index];
			Notes.Add(baseJournalEntry);
			if (baseJournalEntry is JournalMapNote)
			{
				Options.Add("The location of " + Grammar.LowerArticles(baseJournalEntry.GetShortText()));
			}
			else
			{
				Options.Add(baseJournalEntry.GetShortText());
			}
		}
	}
}

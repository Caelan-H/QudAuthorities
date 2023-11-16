using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class FinalizeVillage : HistoricEvent
{
	public static int UNIQUE_PROVERB_CHANCE = 50;

	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		if (snapshotAtYear == null)
		{
			throw new Exception("Cannot load snapshot for year " + entity.lastYear);
		}
		if (Random(0, 100) <= UNIQUE_PROVERB_CHANCE)
		{
			string name = GameObjectFactory.Factory.GetFactionMembers(snapshotAtYear.GetProperty("baseFaction")).GetRandomElement().Name;
			Dictionary<string, string> dictionary = QudHistoryHelpers.BuildContextFromObjectTextFragments(name);
			if (dictionary == null)
			{
				throw new Exception("Could not load context from basis " + name);
			}
			dictionary.Add("*sacredThing*", snapshotAtYear.sacredThing);
			dictionary.Add("*profaneThing*", snapshotAtYear.profaneThing);
			string word = Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.proverbs.!random.capitalize>.", null, null, dictionary));
			setEntityProperty("proverb", Grammar.InitCap(word));
		}
		else
		{
			setEntityProperty("proverb", "Live and drink.");
		}
	}
}

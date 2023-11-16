using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class RampageRegion : HistoricEvent
{
	public string regionToReveal;

	public RampageRegion()
	{
	}

	public RampageRegion(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("region");
		string text = ((!string.IsNullOrEmpty(regionToReveal)) ? regionToReveal : QudHistoryHelpers.GetNewRegion(history, property));
		setEntityProperty("region", text);
		setEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, text));
		string newFaction = QudHistoryHelpers.GetNewFaction(entity);
		string newFaction2 = QudHistoryHelpers.GetNewFaction(entity);
		while (newFaction.Equals(newFaction2))
		{
			newFaction2 = QudHistoryHelpers.GetNewFaction(entity);
		}
		addEntityListItem("hatedFactions", newFaction);
		addEntityListItem("hatedFactions", newFaction2);
		string text2;
		if (Random(0, 1) == 0)
		{
			text2 = ExpandString("<spice.commonPhrases.scourge.!random> of " + QudHistoryHelpers.GetRegionNameRoot(history, text));
			if (snapshotAtYear.GetList("colors").Count != 0)
			{
				text2 = snapshotAtYear.GetList("colors").GetRandomElement() + " " + text2;
			}
			text2 = Grammar.MakeTitleCaseWithArticle("the " + text2);
		}
		else
		{
			text2 = Grammar.MakeTitleCaseWithArticle(ExpandString("the " + QudHistoryHelpers.GetRegionNameRoot(history, text) + " <spice.commonPhrases.scourge.!random>"));
		}
		text2 = text2.Replace("the the", "the");
		addEntityListItem("cognomen", text2);
		setEventProperty("gospel", "<spice.commonPhrases.allThroughout.!random.capitalize> %" + year + "%, <entity.name> <spice.commonPhrases.ravaged.!random> all of " + text + ", <spice.elements.entity$elements[random].ravaging.!random> of " + Faction.getFormattedName(newFaction) + " and " + Faction.getFormattedName(newFaction2) + ". <entity.subjectPronoun.capitalize> became known as " + text2 + ".");
		string value = string.Format("{7}, {8}! In the {0}, a {1} of {2} and {3} {4} {5} and {6}. {7} the {9} put upon them by {10}, {11}!", QudHistoryHelpers.GenerateSultanateYearName(), ExpandString("<spice.commonPhrases.coalition.!random>"), Faction.getFormattedName(newFaction), Faction.getFormattedName(newFaction2), ExpandString("<spice.instancesOf.brokeFaithWith.!random>"), "<entity.name>", ExpandString("<spice.history.gospels.CommittedWrongAgainstSultan." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), ExpandString("<spice.instancesOf.yeGodless.!random>"), ExpandString("<spice.commonPhrases.chastisement.!random>"), "<entity.name>", QudHistoryHelpers.GetRandomCognomen(snapshotAtYear));
		setEventProperty("tombInscription", value);
		setEventProperty("tombInscriptionCategory", "DoesSomethingDestructive");
	}
}

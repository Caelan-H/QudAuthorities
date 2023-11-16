using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class SecretRitual : HistoricEvent
{
	public string regionToReveal;

	public SecretRitual()
	{
	}

	public SecretRitual(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property;
		if (string.IsNullOrEmpty(regionToReveal))
		{
			property = snapshotAtYear.GetProperty("region");
		}
		else
		{
			property = regionToReveal;
			setEntityProperty("region", regionToReveal);
		}
		string newFaction = QudHistoryHelpers.GetNewFaction(entity);
		int num = Random(0, 2);
		int num2 = Random(0, 1);
		string text = num switch
		{
			0 => "deep in " + property + ", ", 
			1 => "while wandering around " + property + ", ", 
			_ => "deep in the wilds of " + property + ", ", 
		};
		setEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
		string value2;
		if (num2 == 0)
		{
			addEntityListItem("likedFactions", newFaction);
			string text2 = ExpandString("<spice.elements.entity$elements[random].quality.!random>");
			string value = Grammar.InitCap(ExpandString(text + "<entity.name> stumbled upon a clan of " + Faction.getFormattedName(newFaction) + " performing a secret ritual. Because of <entity.possessivePronoun> " + text2 + ", they accepted <entity.objectPronoun> into their fold and taught <entity.objectPronoun> their secrets."));
			setEventProperty("gospel", value);
			value2 = string.Format("{0} when, {1}{2} taught a clan of {3} the ritual of the {4}, which can only be performed by one with {8} {5}. The {3} were {6}, and in return they accepted {7} into their fold and taught {7} their secrets.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, "<entity.name>", Faction.getFormattedName(newFaction), Grammar.MakeTitleCase(ExpandString("<spice.history.gospels.RitualName." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>")), text2, ExpandString("<spice.commonPhrases.thankful.!random>"), "<entity.objectPronoun>", "<entity.possessivePronoun>");
		}
		else
		{
			addEntityListItem("hatedFactions", newFaction);
			string text2 = ExpandString("<spice.elements.entity$elements[random].quality.!random>");
			string value = Grammar.InitCap(ExpandString(text + "<entity.name> stumbled upon a clan of " + Faction.getFormattedName(newFaction) + " performing a secret ritual. Because of <entity.possessivePronoun> " + text2 + ", they furiously rebuked <entity.objectPronoun> and declared <entity.objectPronoun> a villain to their kind."));
			setEventProperty("gospel", value);
			value2 = string.Format("{0} when, {1}{2} taught a clan of {3} the ritual of the {4}, which can only be performed by one with {8} {5}. The treacherous {3} {6} {7} and stole the secrets of the ritual.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, "<entity.name>", Faction.getFormattedName(newFaction), Grammar.MakeTitleCase(ExpandString("<spice.history.gospels.RitualName." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>")), text2, ExpandString("<spice.instancesOf.brokeFaithWith.!random>"), "<entity.objectPronoun>", "<entity.possessivePronoun>");
		}
		setEventProperty("tombInscription", value2);
		setEventProperty("tombInscriptionCategory", "LearnsSecret");
	}
}

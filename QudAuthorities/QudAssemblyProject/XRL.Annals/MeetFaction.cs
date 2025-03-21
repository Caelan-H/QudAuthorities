using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class MeetFaction : HistoricEvent
{
	public string regionToReveal;

	public MeetFaction()
	{
	}

	public MeetFaction(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string text = QudHistoryHelpers.GenerateSultanateYearName();
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
		HistoricEntity newEntity = history.GetNewEntity(history.currentYear);
		newEntity.ApplyEvent(new InitializeLocation(property, int.Parse(snapshotAtYear.GetProperty("period"))));
		string property2 = newEntity.GetSnapshotAtYear(year).GetProperty("name");
		history.GetEntitiesWherePropertyEquals("name", property).GetRandomElement().ApplyEvent(new AddLocationToRegion(property2));
		setEntityProperty("location", property2);
		string newFaction = QudHistoryHelpers.GetNewFaction(entity);
		addEntityListItem("likedFactions", newFaction);
		string item = ExpandString("<spice.elements.!random>");
		while (snapshotAtYear.GetList("elements").Contains(item))
		{
			item = ExpandString("<spice.elements.!random>");
		}
		int num = Random(0, 2);
		int num2 = Random(0, 1);
		string text2 = num switch
		{
			0 => "deep in " + property + ", ", 
			1 => "while wandering around " + property + ", ", 
			_ => "deep in the wilds of " + property + ", ", 
		};
		setEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
		if (snapshotAtYear.GetProperty("profession").Equals("unknown"))
		{
			num2 = 0;
		}
		string text4;
		string value;
		if (num2 == 0)
		{
			string text3 = ExpandString("<spice.elements.entity$elements[random].professions.!random>");
			text4 = ExpandString("<spice.professions." + text3 + ".actions.!random>");
			value = Grammar.InitialCap(ExpandString(text2 + "<entity.name> discovered " + newEntity.GetSnapshotAtYear(history.currentYear).GetProperty("name") + ". There <entity.subjectPronoun> befriended " + Faction.getFormattedName(newFaction) + " and " + text4 + "."));
			setEventProperty("gospel", value);
		}
		else
		{
			text4 = ExpandString("<spice.professions.entity$profession.actions.!random>");
			value = Grammar.InitialCap(ExpandString(text2 + "<entity.name> discovered " + newEntity.GetSnapshotAtYear(history.currentYear).GetProperty("name") + ". There <entity.subjectPronoun> befriended " + Faction.getFormattedName(newFaction) + " and " + text4 + "."));
			setEventProperty("gospel", value);
		}
		string s = string.Format("Let us {0} the {1}, when, {2}{3} {4} {5} and {6} the {7} {8} who lived there. To {9} {10} {11} to them, {12} {13}.", ExpandString("<spice.commonPhrases.remember.!random>"), text, text2, snapshotAtYear.GetProperty("name"), ExpandString("<spice.commonPhrases.conquered.!random>"), property2, ExpandString("<spice.commonPhrases.tamed.!random>"), ExpandString("<spice.commonPhrases.wild.!random>"), Faction.getFormattedName(newFaction), ExpandString("<spice.commonPhrases.demonstrate.!random>"), "<entity.possessivePronoun>", ExpandString("<spice.commonPhrases.wisdom.!random>"), "<entity.subjectPronoun>", text4);
		if (If.Chance(80))
		{
			setEventProperty("tombInscription", ExpandString(s));
		}
		else
		{
			setEventProperty("tombInscription", value);
		}
		setEventProperty("tombInscriptionCategory", "Treats");
	}
}

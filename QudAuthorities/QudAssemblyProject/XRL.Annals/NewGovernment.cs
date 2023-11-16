using System;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Names;
using XRL.World;
using XRL.World.ZoneBuilders;

namespace XRL.Annals;

[Serializable]
public class NewGovernment : HistoricEvent
{
	public bool bVillageZero;

	public NewGovernment()
	{
		bVillageZero = false;
	}

	public NewGovernment(bool _bVillageZero)
	{
		bVillageZero = _bVillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num2;
		while (true)
		{
			int num = Random(1, 1000);
			num2 = 0;
			foreach (string item in snapshotAtYear.GetList("removeIfGovtChanges_sacredThings"))
			{
				removeEntityListItem("sacredThings", item);
			}
			foreach (string item2 in snapshotAtYear.GetList("removeIfGovtChanges_profaneThings"))
			{
				removeEntityListItem("profaneThings", item2);
			}
			if (If.Chance(50))
			{
				foreach (string item3 in snapshotAtYear.GetList("removeIfGovtChanges_sacredThings"))
				{
					addEntityListItem("profaneThings", item3);
				}
				foreach (string item4 in snapshotAtYear.GetList("removeIfGovtChanges_profaneThings"))
				{
					addEntityListItem("sacredThings", item4);
				}
			}
			foreach (string item5 in snapshotAtYear.GetList("removeIfGovtChanges_sacredThings"))
			{
				removeEntityListItem("removeIfGovtChanges_sacredThings", item5);
			}
			foreach (string item6 in snapshotAtYear.GetList("removeIfGovtChanges_profaneThings"))
			{
				removeEntityListItem("removeIfGovtChanges_profaneThings", item6);
			}
			applyToSnapshot(snapshotAtYear);
			if (num <= 200)
			{
				string text = (If.CoinFlip() ? "king" : "queen");
				setEntityProperty("government", "monarchy");
				setEntityProperty("mayorTemplate", Grammar.InitCap(text));
				string property = snapshotAtYear.GetProperty("region");
				string text2 = NameMaker.MakeName(VillageBase.getBaseVillager(snapshotAtYear.GetProperty("baseFaction"), property) ?? GameObject.create(PopulationManager.RollOneFrom("LairOwners_" + property).Blueprint));
				text2 = (If.CoinFlip() ? ("King " + text2 + " I") : ("Queen " + text2 + " I"));
				string value = Grammar.InitCap(string.Format("{0} the memory of {1}, who founded the monarchy of {2} for {3}.|{4}", ExpandString("<spice.commonPhrases.love.!random>"), text2, snapshotAtYear.GetProperty("name"), snapshotAtYear.sacredThing, id));
				addEntityListItem("Gospels", value);
				addEntityListItem("sacredThings", "the " + text);
				addEntityListItem("removeIfGovtChanges_sacredThings", "the " + text);
				string value2 = "those who " + ExpandString("<spice.commonPhrases.profane.!random> the ") + text;
				addEntityListItem("removeIfGovtChanges_profaneThings", value2);
				addEntityListItem("profaneThings", value2);
				setEntityProperty("governor", text);
				return;
			}
			num2 += 200;
			if (num <= num2 + 200)
			{
				string value3 = Grammar.InitCap(string.Format("In %{0}%, the villagers of {1} demanded that {2} {3} so the people could govern themselves. The two pillars of {4} in {1} thus became democracy and {5}.|{6}", year, snapshotAtYear.Name, snapshotAtYear.GetProperty("governor"), ExpandString("<spice.instancesOf.stepDown.!random>"), ExpandString("<spice.commonPhrases.society.!random>"), snapshotAtYear.sacredThing, id));
				setEntityProperty("mayorTemplate", "VillagerMayor");
				setEntityProperty("government", "direct democracy");
				addEntityListItem("Gospels", value3);
				addEntityListItem("sacredThings", "democracy");
				addEntityListItem("sacredThings", "the people");
				addEntityListItem("removeIfGovtChanges_sacredThings", "democracy");
				addEntityListItem("removeIfGovtChanges_sacredThings", "the people");
				string value4 = "those who " + ExpandString("<spice.commonPhrases.profane.!random> the ") + "democracy";
				addEntityListItem("profaneThings", value4);
				addEntityListItem("removeIfGovtChanges_profaneThings", value4);
				setEntityProperty("governor", "the people");
				return;
			}
			num2 += 200;
			if (num <= num2 + 200)
			{
				string text3 = ExpandString("<spice.villages.government.representativeDemocracy.!random>");
				string text4 = ExpandString("<spice.villages.government.representativeDemocracy." + text3 + ".governor.!random>");
				string text5 = ExpandString("<spice.villages.government.representativeDemocracy." + text3 + ".mayorTemplate.!random>");
				string value5 = Grammar.InitCap(string.Format("In %{0}%, the villagers of {1} deposed {2} and {3} power to an elected {4}. The {5} promised to forever uphold the {6} of {7}.|{8}", year, snapshotAtYear.Name, snapshotAtYear.GetProperty("governor"), ExpandString("<spice.commonPhrases.bequeathed.!random>"), text3, Grammar.InitLower(Grammar.Pluralize(text5)), ExpandString("<spice.commonPhrases.sanctity.!random>"), snapshotAtYear.sacredThing, id));
				setEntityProperty("mayorTemplate", text5);
				setEntityProperty("government", "representative democracy");
				addEntityListItem("Gospels", value5);
				addEntityListItem("sacredThings", "democracy");
				addEntityListItem("sacredThings", text4);
				addEntityListItem("removeIfGovtChanges_sacredThings", "democracy");
				addEntityListItem("removeIfGovtChanges_sacredThings", text4);
				string value6 = "those who " + ExpandString("<spice.commonPhrases.profane.!random> ") + text4;
				addEntityListItem("profaneThings", value6);
				addEntityListItem("removeIfGovtChanges_profaneThings", value6);
				setEntityProperty("governor", text4);
				return;
			}
			num2 += 200;
			if (num <= num2 + 200)
			{
				string aNonLegendaryCreatureBlueprint = EncountersAPI.GetANonLegendaryCreatureBlueprint((GameObjectBlueprint b) => !b.HasTag("Merchant") && !b.HasTag("ExcludeFromVillagePopulations") && !(b.GetxTag("Grammar", "Proper", "false") == "true") && b.HasPart("Combat"));
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(aNonLegendaryCreatureBlueprint);
				string text6 = "the " + gameObject.DisplayNameOnlyDirectAndStripped + " colonists";
				string value7 = (If.CoinFlip() ? "Viceroy" : "Governor");
				string value8;
				if (If.CoinFlip())
				{
					string sacredThing = snapshotAtYear.sacredThing;
					value8 = Grammar.InitCap(string.Format("In %{0}%, {1} settled in {2}, unseating {3} and imposing {4} that {5} {6}.|{7}", year, Grammar.Pluralize(gameObject.DisplayNameOnlyDirectAndStripped), snapshotAtYear.Name, snapshotAtYear.GetProperty("governor"), ExpandString("<spice.commonPhrases.laws.!random>"), ExpandString("<spice.commonPhrases.prohibited.!random>"), snapshotAtYear.sacredThing, id));
					removeEntityListItem("sacredThings", sacredThing);
					addEntityListItem("profaneThings", sacredThing);
					addEntityListItem("removeIfGovtChanges_profaneThings", sacredThing);
				}
				else
				{
					string profaneThing = snapshotAtYear.profaneThing;
					value8 = Grammar.InitCap(string.Format("In %{0}%, {1} settled in {2}, unseating {3} and imposing {4} in {5} of {6}.|{7}", year, Grammar.Pluralize(gameObject.DisplayNameOnlyDirectAndStripped), snapshotAtYear.Name, snapshotAtYear.GetProperty("governor"), ExpandString("<spice.commonPhrases.laws.!random>"), ExpandString("<spice.commonPhrases.protection.!random>"), snapshotAtYear.profaneThing, id));
					removeEntityListItem("profaneThings", profaneThing);
					addEntityListItem("sacredThings", profaneThing);
					addEntityListItem("removeIfGovtChanges_sacredThings", profaneThing);
				}
				setEntityProperty("mayorTemplate", value7);
				setEntityProperty("government", "colonialism");
				setEntityProperty("colonistType", aNonLegendaryCreatureBlueprint);
				setEntityProperty("governor", text6);
				addEntityListItem("Gospels", value8);
				addEntityListItem("sacredThings", text6);
				addEntityListItem("removeIfGovtChanges_sacredThings", text6);
				string value9 = "those who " + ExpandString("<spice.commonPhrases.profane.!random> ") + text6;
				addEntityListItem("profaneThings", value9);
				addEntityListItem("removeIfGovtChanges_profaneThings", value9);
				return;
			}
			num2 += 200;
			if (num > num2 + 200)
			{
				break;
			}
			if (!bVillageZero)
			{
				string value10 = Grammar.InitCap(string.Format("In %{0}%, the villagers of {1} demanded that {2} {3} and all forms of hierarchy be abolished from their village. The two pillars of {4} in {1} thus became anarchy and {5}.|{6}", year, snapshotAtYear.Name, snapshotAtYear.GetProperty("governor"), ExpandString("<spice.instancesOf.stepDown.!random>"), ExpandString("<spice.commonPhrases.society.!random>"), snapshotAtYear.sacredThing, id));
				setEntityProperty("mayorTemplate", "VillagerMayor");
				setEntityProperty("government", "anarchism");
				addEntityListItem("Gospels", value10);
				addEntityListItem("sacredThings", "anarchy");
				addEntityListItem("sacredThings", "the abolition of hierarchy");
				addEntityListItem("removeIfGovtChanges_sacredThings", "anarchy");
				addEntityListItem("removeIfGovtChanges_sacredThings", "the abolition of hierarchy");
				string value11 = "hierarchy";
				addEntityListItem("profaneThings", value11);
				addEntityListItem("removeIfGovtChanges_profaneThings", value11);
				setEntityProperty("governor", "the people");
				return;
			}
		}
		num2 += 200;
	}
}

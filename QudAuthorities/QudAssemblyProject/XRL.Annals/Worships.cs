using System;
using HistoryKit;
using XRL.Rules;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class Worships : HistoricEvent
{
	public const int CHANCE_FOR_NEARBY_SULTAN = 20;

	public const int CHANCE_FOR_RANDOM_SULTAN = 10;

	public const int CHANCE_TO_USE_COGNOMEN = 30;

	public bool bVillageZero;

	public Worships()
	{
		bVillageZero = false;
	}

	public Worships(bool _bVillageZero)
	{
		bVillageZero = _bVillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num = Random(1, 3);
		string randomProperty = QudHistoryHelpers.GetRandomProperty(snapshotAtYear, snapshotAtYear.GetProperty("defaultSacredThing"), "sacredThings");
		switch (num)
		{
		case 1:
		{
			HistoricEntitySnapshot currentSnapshot = GetSultanToWorshipByTier(snapshotAtYear.Tier, history).GetCurrentSnapshot();
			string text = currentSnapshot.GetProperty("name");
			string text2 = ((currentSnapshot.GetList("cognomen").Count > 0) ? currentSnapshot.GetList("cognomen").GetRandomElement() : null);
			setEntityProperty("worships_sultan", text);
			setEntityProperty("worships_sultan_id", currentSnapshot.entity.id);
			addEntityListItem("itemAdjectiveRoots", text);
			if (text2 != null)
			{
				text = (If.d100(30) ? text2 : text);
			}
			addEntityListItem("sacredThings", text);
			addEntityListItem("profaneThings", ExpandString("<spice.commonPhrases.profanity.!random> toward ") + text);
			string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.divineFeeling.!random>"), text, snapshotAtYear.GetProperty("name"), randomProperty, id);
			addEntityListItem("Gospels", value);
			break;
		}
		case 2:
		{
			string name = Factions.GetRandomFaction(snapshotAtYear.GetProperty("baseFaction")).Name;
			string formattedName = Faction.getFormattedName(name);
			addEntityListItem("sacredThings", formattedName);
			addEntityListItem("profaneThings", ExpandString("<spice.commonPhrases.profanity.!random> toward ") + formattedName);
			setEntityProperty("worships_faction", name);
			addEntityListItem("itemAdjectiveRoots", formattedName);
			string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.divineFeeling.!random>"), formattedName, snapshotAtYear.GetProperty("name"), randomProperty, id);
			addEntityListItem("Gospels", value);
			break;
		}
		case 3:
		{
			setEntityProperty("worships_creature", "*Worships.LegendaryCreature*");
			addEntityListItem("sacredThings", "*Worships.LegendaryCreature.DisplayName*");
			addEntityListItem("profaneThings", ExpandString("<spice.commonPhrases.profanity.!random> toward *Worships.LegendaryCreature.DisplayName*"));
			addEntityListItem("itemAdjectiveRoots", "*Worships.LegendaryCreature.ShortDisplayName*");
			string value = string.Format("The villagers of {0} laid offerings at the feet of {1} in exchange for {2} about {3}.|{4}", snapshotAtYear.GetProperty("name"), "*Worships.LegendaryCreature.DisplayName*", ExpandString("<spice.commonPhrases.wisdom.!random>"), randomProperty, id);
			addEntityListItem("Gospels", value);
			break;
		}
		}
	}

	public static HistoricEntity GetSultanToWorshipByTier(int tier, History history)
	{
		int basePeriod = 0;
		switch (tier)
		{
		case 0:
			basePeriod = 1;
			break;
		case 1:
			basePeriod = 1;
			break;
		case 2:
			basePeriod = 2;
			break;
		case 3:
			basePeriod = 2;
			break;
		case 4:
			basePeriod = 3;
			break;
		case 5:
			basePeriod = 3;
			break;
		case 6:
			basePeriod = 4;
			break;
		case 7:
			basePeriod = 4;
			break;
		case 8:
			basePeriod = 5;
			break;
		default:
			basePeriod = 1;
			break;
		}
		if (Stat.Random(0, 100) < 20)
		{
			if (If.CoinFlip())
			{
				basePeriod = Math.Max(1, basePeriod - 1);
			}
			else
			{
				basePeriod = Math.Min(5, basePeriod + 1);
			}
		}
		else if (Stat.Random(0, 100) < 10)
		{
			basePeriod = Stat.Random(1, 5);
		}
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetCurrentSnapshot().GetProperty("type") == "sultan" && entity.GetCurrentSnapshot().GetProperty("period") == basePeriod.ToString()).GetRandomElement();
	}

	public static void PostProcessEvent(HistoricEntity village, string creatureName, string creatureId)
	{
		village.SetPropertyAtCurrentYear("worships_creature", creatureName);
		village.SetPropertyAtCurrentYear("worships_creature_id", creatureId);
		village.MutateListPropertyAtCurrentYear("sacredThings", (string s) => s.Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
		village.MutateListPropertyAtCurrentYear("profaneThings", (string s) => s.Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
		village.MutateListPropertyAtCurrentYear("Gospels", (string s) => s.Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
		village.MutateListPropertyAtCurrentYear("itemAdjectiveRoots", (string s) => s.Replace("*Worships.LegendaryCreature.ShortDisplayName*", creatureName.Split(',')[0]));
		village.SetPropertyAtCurrentYear("proverb", village.GetCurrentSnapshot().GetProperty("proverb").Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
		village.SetPropertyAtCurrentYear("signatureDishName", village.GetCurrentSnapshot().GetProperty("signatureDishName").Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
		village.SetPropertyAtCurrentYear("newFactionName", village.GetCurrentSnapshot().GetProperty("newFactionName").Replace("*Worships.LegendaryCreature.DisplayName*", creatureName));
	}
}

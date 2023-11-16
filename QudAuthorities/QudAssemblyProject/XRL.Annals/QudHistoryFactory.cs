using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World;

namespace XRL.Annals;

public static class QudHistoryFactory
{
	public const int numSultans = 5;

	public const int avgYearsInSultanate = 6000;

	public const float avgNumVillages = 28f;

	public const float percentOfWorldmap_Saltdunes = 12f;

	public const float percentOfWorldmap_Saltmarsh = 2f;

	public const float percentOfWorldmap_DesertCanyon = 7f;

	public const float percentOfWorldmap_Jungle = 24f;

	public const float percentOfWorldmap_DeepJungle = 10f;

	public const float percentOfWorldmap_Hills = 9f;

	public const float percentOfWorldmap_Water = 8f;

	public const float percentOfWorldmap_BananaGrove = 1f;

	public const float percentOfWorldmap_Fungal = 2f;

	public const float percentOfWorldmap_LakeHinnom = 3f;

	public const float percentOfWorldmap_PalladiumReef = 2f;

	public const float percentOfWorldmap_Mountains = 10f;

	public const float percentOfWorldmap_Flowerfields = 3f;

	public const float percentOfWorldmap_Ruins = 3f;

	public const float percentOfWorldmap_BaroqueRuins = 2f;

	public const float percentOfWorldmap_Deathlands = 4f;

	public const float villageModifier_Saltdunes = 0.8f;

	public const float villageModifier_Saltmarsh = 1.4f;

	public const float villageModifier_DesertCanyon = 1.2f;

	public const float villageModifier_Jungle = 1f;

	public const float villageModifier_DeepJungle = 1f;

	public const float villageModifier_Hills = 1f;

	public const float villageModifier_Water = 0.8f;

	public const float villageModifier_BananaGrove = 1f;

	public const float villageModifier_Fungal = 1.2f;

	public const float villageModifier_LakeHinnom = 1.2f;

	public const float villageModifier_PalladiumReef = 1.2f;

	public const float villageModifier_Mountains = 0.8f;

	public const float villageModifier_Flowerfields = 1.2f;

	public const float villageModifier_Ruins = 1f;

	public const float villageModifier_BaroqueRuins = 1f;

	public const float villageModifier_Deathlands = 0.8f;

	public const int numVillageEvents = 2;

	public const int ruinedVillagesOneIn = 20;

	public static void AddSultanCultNames(History history)
	{
		history.GetEntitiesWherePropertyEquals("type", "sultan").ForEach(delegate(HistoricEntity sultan)
		{
			HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
			XRLCore.Core.Game.SetStringGameState("CultDisplayName_SultanCult" + currentSnapshot.GetProperty("period"), currentSnapshot.properties["cultName"]);
			string item = "include:SultanCult" + currentSnapshot.GetProperty("period");
			foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes((JournalSultanNote note) => note.sultan == sultan.id))
			{
				sultanNote.attributes.Add(item);
			}
		});
	}

	public static History GenerateNewSultanHistory()
	{
		History history = new History(1L);
		InitializeHistory(history);
		List<int> spreadOfSultanYears = QudHistoryHelpers.GetSpreadOfSultanYears(6000, 5);
		for (int i = 1; i <= 5; i++)
		{
			GenerateNewRegions(history, Stat.Random(2, 3), i);
			GenerateNewSultan(history, i);
			history.currentYear += spreadOfSultanYears[i - 1];
		}
		AddSultanCultNames(history);
		AddResheph(history);
		return history;
	}

	public static History GenerateVillageEraHistory(History history)
	{
		int num = int.Parse(history.GetEntitiesWithProperty("Resheph").GetRandomElement().GetCurrentSnapshot()
			.GetProperty("flipYear"));
		for (int i = 0; i < history.events.Count; i++)
		{
			if (history.events[i].hasEventProperty("gospel"))
			{
				string sentence = QudHistoryHelpers.ConvertGospelToSultanateCalendarEra(history.events[i].getEventProperty("gospel"), num);
				history.events[i].setEventProperty("gospel", Grammar.ConvertAtoAn(sentence));
			}
			if (history.events[i].hasEventProperty("tombInscription"))
			{
				history.events[i].setEventProperty("tombInscription", Grammar.ConvertAtoAn(history.events[i].getEventProperty("tombInscription")));
			}
		}
		GenerateNewVillage(history, num, "DesertCanyon", bVillageZero: true);
		GenerateNewVillage(history, num, "Saltdunes", bVillageZero: true);
		GenerateNewVillage(history, num, "Saltmarsh", bVillageZero: true);
		GenerateNewVillage(history, num, "Hills", bVillageZero: true);
		int num2 = (int)Math.Round(1.9600000000000002 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num3 = (int)Math.Round(3.36 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num4 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.3999999761581421);
		int num5 = (int)Math.Round(2.52 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num6 = (int)Math.Round(2.24 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num7 = (int)Math.Round(0.28 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num8 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num9 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num10 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		num7 += Stat.Random(-2, 2);
		int num11 = (int)Math.Round(2.8000000000000003 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num12 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num13 = (int)Math.Round(6.72 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num14 = (int)Math.Round(2.8000000000000003 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num15 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num16 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num17 = (int)Math.Round(1.12 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		for (int j = 1; j <= num2; j++)
		{
			GenerateNewVillage(history, num);
		}
		for (int k = 1; k <= num3; k++)
		{
			GenerateNewVillage(history, num, "Saltdunes");
		}
		for (int l = 1; l <= num4; l++)
		{
			GenerateNewVillage(history, num, "Saltmarsh");
		}
		for (int m = 1; m <= num5; m++)
		{
			GenerateNewVillage(history, num, "Hills");
		}
		for (int n = 1; n <= num6; n++)
		{
			GenerateNewVillage(history, num, "Water");
		}
		for (int num18 = 1; num18 <= num7; num18++)
		{
			GenerateNewVillage(history, num, "BananaGrove");
		}
		for (int num19 = 1; num19 <= num8; num19++)
		{
			GenerateNewVillage(history, num, "Fungal");
		}
		for (int num20 = 1; num20 <= num9; num20++)
		{
			GenerateNewVillage(history, num, "LakeHinnom");
		}
		for (int num21 = 1; num21 <= num10; num21++)
		{
			GenerateNewVillage(history, num, "PalladiumReef");
		}
		for (int num22 = 1; num22 <= num11; num22++)
		{
			GenerateNewVillage(history, num, "Mountains");
		}
		for (int num23 = 1; num23 <= num12; num23++)
		{
			GenerateNewVillage(history, num, "Flowerfields");
		}
		for (int num24 = 1; num24 <= num13; num24++)
		{
			GenerateNewVillage(history, num, "Jungle");
		}
		for (int num25 = 1; num25 <= num14; num25++)
		{
			GenerateNewVillage(history, num, "DeepJungle");
		}
		for (int num26 = 1; num26 <= num15; num26++)
		{
			GenerateNewVillage(history, num, "Ruins");
		}
		for (int num27 = 1; num27 <= num16; num27++)
		{
			GenerateNewVillage(history, num, "BaroqueRuins");
		}
		for (int num28 = 1; num28 <= num17; num28++)
		{
			GenerateNewVillage(history, num, "Deathlands");
		}
		history.currentYear = num + 1000;
		return history;
	}

	public static void InitializeHistory(History history)
	{
		history.GetNewEntity(history.currentYear).ApplyEvent(new Regionalize());
	}

	public static void GenerateNewVillage(History history, int flipYear, string region = "DesertCanyon", bool bVillageZero = false)
	{
		history.currentYear = flipYear + Stat.Random(400, 900);
		HistoricEntity newEntity = history.GetNewEntity(history.currentYear);
		newEntity.ApplyEvent(new InitializeVillage(region, bVillageZero));
		for (int i = 0; i < 2; i++)
		{
			if (!bVillageZero && i == 1 && If.OneIn(20))
			{
				newEntity.ApplyEvent(new Abandoned(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
				continue;
			}
			int num = Stat.Random(0, 6);
			if (num == 0)
			{
				newEntity.ApplyEvent(new BecomesKnownFor(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 1)
			{
				newEntity.ApplyEvent(new PopulationInflux(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 2)
			{
				newEntity.ApplyEvent(new Worships(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 3)
			{
				newEntity.ApplyEvent(new Despises(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 4)
			{
				newEntity.ApplyEvent(new SharedMutation(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 5)
			{
				newEntity.ApplyEvent(new NewGovernment(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
			if (num == 6)
			{
				newEntity.ApplyEvent(new ImportedFoodorDrink(bVillageZero), newEntity.lastYear + Stat.Random(10, 20));
			}
		}
		newEntity.ApplyEvent(new FinalizeVillage(), newEntity.lastYear);
		newEntity.MutateListPropertyAtCurrentYear("Gospels", (string s) => QudHistoryHelpers.ConvertGospelToSultanateCalendarEra(s, flipYear));
	}

	public static void GenerateNewSultan(History history, int period)
	{
		HistoricEntity newEntity = history.GetNewEntity(history.currentYear);
		newEntity.ApplyEvent(new InitializeSultan(period));
		newEntity.ApplyEvent(new SetEntityProperty("isCandidate", "true"));
		if (Stat.Random(0, 4) == 0)
		{
			newEntity.ApplyEvent(new BornAsHeir(), newEntity.lastYear + Stat.Random(6, 8));
		}
		else
		{
			newEntity.ApplyEvent(new FoundAsBabe(), newEntity.lastYear + Stat.Random(6, 8));
		}
		for (int i = 0; i < 8; i++)
		{
			int num = Stat.Random(0, 16);
			if (num == 0)
			{
				newEntity.ApplyEvent(new CorruptAdministrator(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 1)
			{
				newEntity.ApplyEvent(new CapturedByBandits(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 2)
			{
				newEntity.ApplyEvent(new InspiringExperience(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 3)
			{
				newEntity.ApplyEvent(new MeetFaction(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 4)
			{
				newEntity.ApplyEvent(new SecretRitual(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 5)
			{
				newEntity.ApplyEvent(new ChallengeSultan(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 6)
			{
				newEntity.ApplyEvent(new ForgeItem(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 7)
			{
				newEntity.ApplyEvent(new UnderWeirdSky(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 8)
			{
				newEntity.ApplyEvent(new LiberateCity(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 9)
			{
				newEntity.ApplyEvent(new RampageRegion(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 10)
			{
				newEntity.ApplyEvent(new FoundGuild(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 11)
			{
				newEntity.ApplyEvent(new BattleItem(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 12)
			{
				newEntity.ApplyEvent(new LoseItemAtTavern(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 13)
			{
				newEntity.ApplyEvent(new BloodyBattle(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 14)
			{
				newEntity.ApplyEvent(new ChariotDrivesOffCliff(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 15)
			{
				newEntity.ApplyEvent(new Abdicate(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 16)
			{
				newEntity.ApplyEvent(new Marry(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (newEntity.GetCurrentSnapshot().GetProperty("isAlive") != "true")
			{
				newEntity.ApplyEvent(new FakedDeath(), newEntity.lastYear + Stat.Random(1, 16));
			}
		}
		if (!newEntity.GetCurrentSnapshot().GetProperty("isSultan").EqualsNoCase("true"))
		{
			int num2 = Stat.Random(0, 1);
			if (num2 == 0)
			{
				newEntity.ApplyEvent(new ChallengeSultan(), newEntity.lastYear + Stat.Random(1, 16));
			}
			if (num2 == 1)
			{
				newEntity.ApplyEvent(new Abdicate(), newEntity.lastYear + Stat.Random(1, 16));
			}
		}
		HistoricEntityList entitiesByDelegate = history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("region") && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) == period);
		for (int j = 0; j < entitiesByDelegate.Count; j++)
		{
			string property = entitiesByDelegate.entities[j].GetCurrentSnapshot().GetProperty("newName");
			bool flag = false;
			for (int k = 0; k < newEntity.events.Count; k++)
			{
				if (newEntity.events[k].hasEventProperty("revealsRegion") && newEntity.events[k].getEventProperty("revealsRegion") == property)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				string property2 = entitiesByDelegate.entities[j].GetCurrentSnapshot().GetProperty("name");
				int num3 = Stat.Random(0, 5);
				if (num3 == 0)
				{
					newEntity.ApplyEvent(new CorruptAdministrator(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
				if (num3 == 1)
				{
					newEntity.ApplyEvent(new BloodyBattle(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
				if (num3 == 2)
				{
					newEntity.ApplyEvent(new LoseItemAtTavern(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
				if (num3 == 3)
				{
					newEntity.ApplyEvent(new MeetFaction(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
				if (num3 == 4)
				{
					newEntity.ApplyEvent(new RampageRegion(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
				if (num3 == 5)
				{
					newEntity.ApplyEvent(new SecretRitual(property2), newEntity.lastYear + Stat.Random(1, 16));
				}
			}
		}
		for (int l = 0; l < 3; l++)
		{
			if (newEntity.GetCurrentSnapshot().GetProperty("isAlive").EqualsNoCase("true"))
			{
				break;
			}
		}
		if (newEntity.GetCurrentSnapshot().GetProperty("isAlive").EqualsNoCase("true"))
		{
			newEntity.ApplyEvent(new GenericDeath());
		}
		FillOutLikedFactions(newEntity);
		GenerateCultName(newEntity, history);
	}

	public static void AddResheph(History history)
	{
		HistoricEntity newEntity = history.GetNewEntity(history.currentYear);
		newEntity.ApplyEvent(new InitializeResheph(6), newEntity.lastYear);
		newEntity.ApplyEvent(new SetEntityProperty("isCandidate", "true"));
		newEntity.ApplyEvent(new ReshephIsBorn(), newEntity.lastYear);
		newEntity.ApplyEvent(new ReshephHasStarExperience(), newEntity.lastYear + 33);
		newEntity.ApplyEvent(new ReshephMeetsRebekah(), newEntity.lastYear + 7);
		newEntity.ApplyEvent(new ReshephBecomesSultan(), newEntity.lastYear + 26);
		newEntity.ApplyEvent(new ReshephAppointsRebekah(), newEntity.lastYear + 41);
		newEntity.ApplyEvent(new ReshephFoundsHarborage(), newEntity.lastYear + 91);
		newEntity.ApplyEvent(new ReshephHealsGyre1(), newEntity.lastYear + 5);
		newEntity.ApplyEvent(new ReshephBetrayed(), newEntity.lastYear);
		newEntity.ApplyEvent(new ReshephHealsGyre2(), newEntity.lastYear + 1);
		newEntity.ApplyEvent(new ReshephLearnsCurse(), newEntity.lastYear + 1);
		newEntity.ApplyEvent(new ReshephRebuffsCurse(), newEntity.lastYear);
		newEntity.ApplyEvent(new ReshephClosesTomb(), newEntity.lastYear + 1);
		newEntity.ApplyEvent(new ReshephAbsolvesRebekah(), newEntity.lastYear + 1);
		newEntity.ApplyEvent(new ReshephCleansesGyre(), newEntity.lastYear + 1);
		newEntity.ApplyEvent(new ReshephWeirdSky(), newEntity.lastYear + 3);
		newEntity.ApplyEvent(new ReshephDies(), newEntity.lastYear);
	}

	public static void GenerateNewRegions(History history, int numRegions, int period)
	{
		for (int i = 0; i < numRegions; i++)
		{
			history.GetNewEntity(history.currentYear).ApplyEvent(new InitializeRegion(period));
		}
	}

	public static string NameRuinsSite(History history, out bool Proper, out string nameRoot)
	{
		HistoricEntitySnapshot currentSnapshot = history.GetEntitiesWherePropertyEquals("name", "regionalizationParameters").GetRandomElement().GetCurrentSnapshot();
		string text = HistoricStringExpander.ExpandString("<spice." + currentSnapshot.GetProperty("siteTopology1") + ".!random>", null, history);
		string text2 = HistoricStringExpander.ExpandString("<spice." + currentSnapshot.GetProperty("siteTopology2") + ".!random>", null, history);
		nameRoot = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site");
		string text3 = (int.Parse(currentSnapshot.GetProperty("siteTopologyTheChance")).in100() ? "the " : "");
		int num = Stat.Random(0, 80);
		if (num < 15)
		{
			Proper = true;
			return nameRoot;
		}
		if (80.in100())
		{
			nameRoot = currentSnapshot.GetProperty("siteName" + Stat.Random(1, 3));
		}
		if (1.in100())
		{
			nameRoot = "";
		}
		string text4;
		if (num < 30)
		{
			text4 = text3 + nameRoot;
			if (text4 != "")
			{
				text4 += " ";
			}
			text4 += text;
			Proper = true;
		}
		else if (num < 45)
		{
			text4 = text3 + text2 + " " + nameRoot;
			Proper = true;
		}
		else
		{
			if (num >= 60)
			{
				_ = 80;
				Proper = false;
				return "some forgotten ruins";
			}
			text4 = text2 + " " + text + " " + nameRoot;
			Proper = true;
		}
		return Grammar.MakeTitleCase(text4.Replace("  ", " "));
	}

	public static string NameRuinsSite(History history)
	{
		bool Proper;
		string nameRoot;
		return NameRuinsSite(history, out Proper, out nameRoot);
	}

	public static void GenerateCultName(HistoricEntity sultan, History history)
	{
		int num = Stat.Random(0, 100);
		HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (currentSnapshot.GetList("cognomen").Count == 0)
		{
			num = 100;
		}
		string text = HistoricStringExpander.ExpandString("<spice.commonPhrases.cult.!random.capitalize>", null, history);
		if (num < 70)
		{
			string randomElement = currentSnapshot.GetList("cognomen").GetRandomElement();
			if (Stat.Random(0, 1) == 0)
			{
				dictionary.Add("cultName", text + " of the " + Grammar.TrimLeadingThe(randomElement));
			}
			else
			{
				dictionary.Add("cultName", Grammar.TrimLeadingThe(randomElement + " " + text));
			}
		}
		else if (Stat.Random(0, 1) == 0)
		{
			dictionary.Add("cultName", "Cult of " + currentSnapshot.GetProperty("name"));
		}
		else
		{
			int num2 = int.Parse(currentSnapshot.GetProperty("suffix"));
			if (num2 > 0)
			{
				dictionary.Add("cultName", Grammar.MakeTitleCase(Grammar.Ordinal(num2) + " " + Grammar.GetWordRoot(currentSnapshot.GetProperty("nameRoot")) + "ian " + text));
			}
			else
			{
				dictionary.Add("cultName", Grammar.GetWordRoot(currentSnapshot.GetProperty("nameRoot")) + "ian " + text);
			}
		}
		sultan.ApplyEvent(new SetEntityProperties(dictionary, null));
	}

	public static void FillOutLikedFactions(HistoricEntity sultan)
	{
		HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
		int num = int.Parse(currentSnapshot.GetProperty("period"));
		if (num == 5 || num == 4)
		{
			for (int i = currentSnapshot.GetList("likedFactions").Count; i < 3; i++)
			{
				HistoricEntitySnapshot currentSnapshot2 = sultan.GetCurrentSnapshot();
				string blueprint;
				do
				{
					blueprint = PopulationManager.RollOneFrom("RandomFaction_Period" + num).Blueprint;
				}
				while (currentSnapshot2.GetList("likedFactions").Contains(blueprint));
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("likedFactions", blueprint);
				sultan.ApplyEvent(new SetEntityProperties(null, dictionary));
			}
			return;
		}
		for (int j = currentSnapshot.GetList("likedFactions").Count; j < 3; j++)
		{
			HistoricEntitySnapshot currentSnapshot3 = sultan.GetCurrentSnapshot();
			string name;
			do
			{
				name = Factions.GetRandomFactionWithAtLeastOneMember().Name;
			}
			while (currentSnapshot3.GetList("likedFactions").Contains(name));
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("likedFactions", name);
			sultan.ApplyEvent(new SetEntityProperties(null, dictionary2));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using XRL.Language;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.ZoneBuilders;

namespace XRL.Annals;

[Serializable]
public class PopulationInflux : HistoricEvent
{
	public static readonly int MAYOR_WEIGHT = 10;

	public static readonly int MERCHANT_WEIGHT = 10;

	public static readonly int WARDEN_WEIGHT = 10;

	public static readonly int TINKER_WEIGHT = 10;

	public static readonly int APOTHECARY_WEIGHT = 10;

	public static readonly int VILLAGER_WEIGHT = 70;

	public static readonly int CHANCE_MULTIPLE_PETS = 30;

	public bool bVillageZero;

	public PopulationInflux()
	{
		bVillageZero = false;
	}

	public PopulationInflux(bool _bVillageZero)
	{
		bVillageZero = _bVillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num = Random(1, 1000);
		if (num <= 400)
		{
			string text = new BallBag<string>
			{
				{ "mayor", MAYOR_WEIGHT },
				{ "warden", WARDEN_WEIGHT },
				{ "merchant", MERCHANT_WEIGHT },
				{ "tinker", TINKER_WEIGHT },
				{ "apothecary", APOTHECARY_WEIGHT },
				{ "villager", VILLAGER_WEIGHT }
			}.PeekOne();
			string blueprint = PopulationManager.RollOneFrom("LairOwners_" + snapshotAtYear.GetProperty("region")).Blueprint;
			GameObject gameObject;
			switch (text)
			{
			case "mayor":
			{
				string text2 = snapshotAtYear.GetProperty("mayorTemplate");
				if (text2 == "unknown")
				{
					text2 = "Mayor";
				}
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint), null, "SpecialVillagerHeroTemplate_" + text2, -1, text2);
				break;
			}
			case "warden":
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint), null, "SpecialVillagerHeroTemplate_Warden", -1, "Warden");
				break;
			case "merchant":
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint), null, "SpecialVillagerHeroTemplate_Merchant", -1, "Merchant");
				break;
			case "tinker":
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint), null, "SpecialVillagerHeroTemplate_Tinker", -1, "Tinker");
				setEntityProperty("techTier", Tier.Constrain(snapshotAtYear.TechTier + 1).ToString());
				break;
			case "apothecary":
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint), null, "SpecialVillagerHeroTemplate_Apothecary", -1, "Apothecary");
				break;
			default:
				gameObject = HeroMaker.MakeHero(GameObject.create(blueprint));
				break;
			}
			Dictionary<string, string> vars = QudHistoryHelpers.BuildContextFromObjectTextFragments(blueprint);
			string displayName = gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true);
			string displayName2 = gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true);
			try
			{
				string value = ExpandString("<spice.villages." + text + ".reasonIBecame.!random>", vars);
				addEntityListItem("immigrant_name", displayName, force: true);
				addEntityListItem("immigrant_gender", gameObject.GetGender().Name);
				addEntityListItem("immigrant_role", text, force: true);
				addEntityListItem("immigrant_dialogWhy_Q", (If.OneIn(3) ? "Stranger" : gameObject.FormalAddressTerm) + ", why did you come to this village?", force: true);
				addEntityListItem("immigrant_dialogWhy_A", value, force: true);
				addEntityListItem("immigrant_type", blueprint, force: true);
				addEntityListItem("sacredThings", displayName);
				addEntityListItem("profaneThings", ExpandString("those who would <spice.commonPhrases.harm.!random> ") + displayName);
				addEntityListItem("itemAdjectiveRoots", displayName2);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Failed to set up immigrant.", x);
			}
			string text3;
			switch (text)
			{
			case "mayor":
			case "warden":
				text3 = "The villagers of " + snapshotAtYear.GetProperty("name") + " asked " + displayName2 + " to " + ExpandString("<spice.villages." + text + ".villageTask.!random>").Replace("my", gameObject.its);
				break;
			case "villager":
				text3 = displayName2 + " settled down among the villagers";
				break;
			default:
				text3 = displayName2 + " set up a shop for their trade";
				break;
			}
			int num2 = int.Parse(history.GetEntitiesWithProperty("Resheph").GetRandomElement().GetCurrentSnapshot()
				.GetProperty("flipYear"));
			string value2 = Grammar.InitCap(string.Format("{0}, {1} grew tired of {2} and {3} to a place {4}. There {7} came upon {5} and its inhabitants. {6}.|{8}", ExpandString("<spice.instancesOf.openingTime.!random>").Replace("*year*", "%" + (num2 + Random(900, 999)) + "%"), gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true), gameObject.GetxTag_CommaDelimited("TextFragments", "Activity"), ExpandString("<spice.commonPhrases.trekked.!random>"), ExpandString("<spice.history.regions.terrain." + snapshotAtYear.GetProperty("region") + ".over.!random>"), snapshotAtYear.GetProperty("name"), text3, gameObject.it, id));
			addEntityListItem("Gospels", value2);
		}
		else if (num <= 800)
		{
			string petType = "nonHumanoid";
			int useTier = Math.Max(snapshotAtYear.Tier, 1);
			string text4 = ((!bVillageZero) ? GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && bp.Tier <= useTier && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant") && !(bp.GetxTag("Grammar", "Proper", "false") == "true") && ((!(petType == "humanoid")) ? (!bp.HasTag("Humanoid")) : bp.HasTag("Humanoid"))).GetRandomElement().Name : GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && bp.Tier <= useTier && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillageZero") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant") && !bp.HasPart("Breeder") && !(bp.GetxTag("Grammar", "Proper", "false") == "true") && ((!(petType == "humanoid")) ? (!bp.HasTag("Humanoid")) : bp.HasTag("Humanoid"))).GetRandomElement().Name);
			GameObject gameObject2 = GameObject.create(text4);
			string value3;
			int num3;
			if (If.Chance(CHANCE_MULTIPLE_PETS))
			{
				value3 = $"Why are there {Grammar.Pluralize(gameObject2.DisplayNameOnly)} here?";
				num3 = Random(2, 4);
			}
			else
			{
				value3 = $"Why{gameObject2.Is} there {gameObject2.a}{gameObject2.DisplayNameOnly} here?";
				num3 = 1;
			}
			addEntityListItem("pet_petType", petType, force: true);
			addEntityListItem("pet_dialogWhy_Q", value3, force: true);
			addEntityListItem("pet_number", num3.ToString(), force: true);
			addEntityListItem("pet_petSpecies", text4, force: true);
		}
		else if (num <= 1000)
		{
			addEntityListItem("populationMultiplier", Random(2, 4).ToString());
			string value4 = Grammar.InitCap(string.Format("As the gospel of {0} spread, {1} {2} the village of {3}.|{4}", snapshotAtYear.sacredThing, ExpandString("<spice.commonPhrases.folks.!random>"), ExpandString("<spice.instancesOf.flockedTo.!random>"), snapshotAtYear.GetProperty("name"), id));
			addEntityListItem("Gospels", value4);
		}
		else
		{
			string name = Factions.GetRandomFactionWithAtLeastOneMember().Name;
			string value5 = ExpandString("<spice.villages.immigrants.immigrantPopReason.!random>", QudHistoryHelpers.BuildContextFromObjectTextFragments(GameObjectFactory.Factory.GetFactionMembers(name).GetRandomElement().Name));
			string value6 = (If.CoinFlip() ? "half" : "whole");
			string value7 = "false";
			if (snapshotAtYear.GetList("immigrantPop_amount").Count > 0)
			{
				value7 = "true";
				value6 = "whole";
			}
			addEntityListItem("immigrantPop_type", name, force: true);
			addEntityListItem("immigrantPop_reason", value5, force: true);
			addEntityListItem("immigrantPop_dialogWhy_Q", $"Why is this village populated with {Faction.getFormattedName(name)}?", force: true);
			addEntityListItem("immigrantPop_dialogWhy_A", value5, force: true);
			addEntityListItem("immigrantPop_amount", value6, force: true);
			addEntityListItem("immigrantPop_secondWave", value7, force: true);
		}
	}
}

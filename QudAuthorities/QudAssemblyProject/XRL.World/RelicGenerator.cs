using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using SimpleJSON;
using UnityEngine;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

/// <summary>
///             Base game object
///             </summary>
public static class RelicGenerator
{
	public static string[] supportedTypes = new string[18]
	{
		"Axe", "ShortBlade", "LongBlade", "Cudgel", "Rifle", "Pistol", "Head", "Body", "Face", "Arm",
		"Feet", "Hands", "Floating", "Artifact", "Book", "Curio", "Shield", "Food"
	};

	public static Dictionary<string, string> supportedTypeMap = new Dictionary<string, string>
	{
		{ "Axe", "Axe" },
		{ "axe", "Axe" },
		{ "ShortBlade", "ShortBlade" },
		{ "dagger", "ShortBlade" },
		{ "dirk", "ShortBlade" },
		{ "LongBlade", "LongBlade" },
		{ "sword", "LongBlade" },
		{ "Cudgel", "Cudgel" },
		{ "hammer", "Cudgel" },
		{ "mace", "Cudgel" },
		{ "hammers", "Cudgel" },
		{ "pummel", "Cudgel" },
		{ "Head", "Head" },
		{ "helm", "Head" },
		{ "helmet", "Head" },
		{ "Body", "Body" },
		{ "vest", "Body" },
		{ "breastplate", "Body" },
		{ "Face", "Face" },
		{ "mask", "Face" },
		{ "Arm", "Arm" },
		{ "bracelet", "Arm" },
		{ "Feet", "Feet" },
		{ "pair of boots", "Feet" },
		{ "boots", "Feet" },
		{ "Hands", "Hands" },
		{ "iron gauntlet", "Hands" },
		{ "glove", "Hands" },
		{ "gloves", "Hands" },
		{ "gauntlet", "Hands" },
		{ "gauntlets", "Hands" },
		{ "Floating", "Floating" },
		{ "floating orb", "Floating" },
		{ "Artifact", "Artifact" },
		{ "artifact", "Artifact" },
		{ "star-tool", "Artifact" },
		{ "Rifle", "Rifle" },
		{ "rifle", "Rifle" },
		{ "long arm", "Rifle" },
		{ "Pistol", "Pistol" },
		{ "pistol", "Pistol" },
		{ "gun", "Pistol" },
		{ "short arm", "Pistol" },
		{ "Book", "Book" },
		{ "treatise", "Book" },
		{ "chronology", "Book" },
		{ "philosophy", "Book" },
		{ "hypothesis", "Book" },
		{ "account", "Book" },
		{ "horoscope reading", "Book" },
		{ "allegory", "Book" },
		{ "wintry truth", "Book" },
		{ "Food", "Food" },
		{ "meal", "Food" },
		{ "feast", "Food" },
		{ "Curio", "Curio" },
		{ "Skull", "Curio" },
		{ "skull", "Curio" },
		{ "one-sided die", "Curio" },
		{ "two-sided die", "Curio" },
		{ "three-sided die", "Curio" },
		{ "four-sided die", "Curio" },
		{ "five-sided die", "Curio" },
		{ "six-sided die", "Curio" },
		{ "seven-sided die", "Curio" },
		{ "eight-sided die", "Curio" },
		{ "nine-sided die", "Curio" },
		{ "ten-sided die", "Curio" },
		{ "twenty-sided die", "Curio" },
		{ "twelve-sided die", "Curio" },
		{ "coin", "Curio" },
		{ "Shield", "Shield" },
		{ "shield", "Shield" }
	};

	public static string GenerateRelicName(string type, HistoricEntitySnapshot snapRegion, List<string> elements, Dictionary<string, string> properties)
	{
		if (snapRegion != null)
		{
			return GenerateRelicNameByRegion(type, snapRegion, elements);
		}
		string randomElement = elements.GetRandomElement();
		string word = NameMaker.MakeName(EncountersAPI.GetACreature(), null, null, null, null, null, null, null, null, "Relic", null, FailureOkay: false, SpecialFaildown: true);
		Dictionary<string, string> vars = new Dictionary<string, string>
		{
			{ "*element*", randomElement },
			{ "*itemType*", type },
			{
				"*personNounPossessive*",
				Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
			},
			{
				"*creatureNamePossessive*",
				Grammar.MakePossessive(word)
			}
		};
		return QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars)));
	}

	public static string GenerateRelicNameByRegion(string type, HistoricEntitySnapshot snapRegion, List<string> elements)
	{
		History sultanHistory = The.Game.sultanHistory;
		string randomElement = elements.GetRandomElement();
		return QudHistoryHelpers.Ansify(Grammar.MakeTitleCase("the " + HistoricStringExpander.ExpandString("<spice.elements." + randomElement + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>", null, sultanHistory) + " of " + snapRegion.GetProperty("newName"), null, sultanHistory)));
	}

	public static GameObject GenerateSpindleNegotiationRelic(string itemType, string sparedFaction, string betrayedFaction, string playerNameAndAppositive)
	{
		List<string> adjectives = new List<string> { HistoricStringExpander.ExpandString("<spice.elements.!random>") };
		Dictionary<string, List<string>> listProperties = new Dictionary<string, List<string>>
		{
			{
				"likedFactions",
				new List<string> { sparedFaction }
			},
			{
				"hatedFactions",
				new List<string> { betrayedFaction }
			}
		};
		return GenerateRelic(itemType, 4, null, adjectives, null, listProperties, null, playerNameAndAppositive);
	}

	public static GameObject GenerateRelic(int tier, bool randomName = false)
	{
		return GenerateRelic(The.Game.sultanHistory.entities.GetRandomElement().GetCurrentSnapshot(), tier, null, randomName);
	}

	public static GameObject GenerateRelic(HistoricEntitySnapshot snap, int tier, string forcetype = null, bool randomName = false)
	{
		List<string> list = new List<string>();
		list.AddRange(snap.properties.Values);
		foreach (List<string> value in snap.listProperties.Values)
		{
			list.AddRange(value);
		}
		string type = forcetype ?? snap.GetProperty("itemType");
		if (randomName)
		{
			if (snap.GetProperty("type").Equals("region"))
			{
				return GenerateRelic(type, tier, snap, list, snap.properties, snap.listProperties);
			}
			return GenerateRelic(type, tier, null, list, snap.properties, snap.listProperties);
		}
		return GenerateRelic(type, tier, null, list, snap.properties, snap.listProperties, snap.GetProperty("name", null));
	}

	public static GameObject GenerateRelic(HistoricEntitySnapshot snap, string forcetype = null, bool randomName = false)
	{
		List<string> list = new List<string>();
		list.AddRange(snap.properties.Values);
		foreach (List<string> value in snap.listProperties.Values)
		{
			list.AddRange(value);
		}
		string text = forcetype;
		if (text == null)
		{
			text = snap.GetProperty("itemType");
		}
		int relicTierFromPeriod = GetRelicTierFromPeriod(int.Parse(snap.GetProperty("period")));
		if (randomName)
		{
			return GenerateRelic(text, relicTierFromPeriod, null, list, snap.properties, snap.listProperties);
		}
		return GenerateRelic(text, relicTierFromPeriod, null, list, snap.properties, snap.listProperties, snap.GetProperty("name", null));
	}

	public static int GetRelicTierFromPeriod(int period)
	{
		if (period == 5)
		{
			int num = (If.CoinFlip() ? 1 : 2);
		}
		if (period == 4)
		{
			int num = (If.CoinFlip() ? 3 : 4);
		}
		if (period == 3)
		{
			int num = (If.CoinFlip() ? 5 : 6);
		}
		if (period == 2)
		{
			int num = 7;
		}
		if (period == 1)
		{
			return 8;
		}
		return 4;
	}

	public static int GetPeriodFromRelicTier(int tier)
	{
		switch (tier)
		{
		case 0:
		case 1:
		case 2:
			return 5;
		case 3:
		case 4:
			return 4;
		case 5:
			return 3;
		case 6:
			return 2;
		case 7:
			return 1;
		default:
			return 4;
		}
	}

	public static int GetAttributeBonusFromRelicTier(int tier)
	{
		if (tier <= 4)
		{
			return If.CoinFlip() ? 1 : 2;
		}
		return If.CoinFlip() ? 3 : 4;
	}

	public static int GetResistanceBonusFromRelicTier(int tier)
	{
		int num = Stat.Random(20, 30);
		if (tier >= 4)
		{
			num += (tier - 4) * 20;
		}
		return num;
	}

	public static int GetMoveSpeedBonusFromRelicTier(int tier)
	{
		return -(8 + (tier - 1) * 3 + Stat.Random(1, 4));
	}

	public static string TranslateAdjective(string input)
	{
		input = input.ToLower();
		if (input == "none")
		{
			return input;
		}
		foreach (JSONNode child in HistoricSpice.root["elements"].Childs)
		{
			if (input == child.Key.ToLower())
			{
				return input;
			}
			foreach (JSONNode child2 in child["adjectives"].Childs)
			{
				if (input == child2.Value.ToLower())
				{
					return child.Key.ToLower();
				}
			}
		}
		return null;
	}

	public static bool GiveRandomMeleeWeaponMod(GameObject obj, int tier = 1, bool standard = false)
	{
		if (!(obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon))
		{
			return false;
		}
		switch (Stat.Random(0, 4))
		{
		case 0:
			if (meleeWeapon.AdjustDamageDieSize(2))
			{
				return true;
			}
			break;
		case 1:
			if (meleeWeapon.AdjustDamage(1))
			{
				return true;
			}
			break;
		case 2:
			if (meleeWeapon.AdjustBonusCap(3))
			{
				return true;
			}
			break;
		case 3:
			meleeWeapon.HitBonus += 2;
			return true;
		}
		meleeWeapon.PenBonus++;
		return true;
	}

	public static bool GiveRandomMissileWeaponMod(GameObject obj, int tier = 1, bool standard = false)
	{
		if (!(obj.GetPart("MissileWeapon") is MissileWeapon missileWeapon))
		{
			return false;
		}
		switch (Stat.Random(0, 7))
		{
		case 0:
			if (!missileWeapon.NoWildfire)
			{
				missileWeapon.NoWildfire = true;
				return true;
			}
			break;
		case 1:
			if (missileWeapon.AmmoPerAction == missileWeapon.ShotsPerAction)
			{
				missileWeapon.AmmoPerAction++;
			}
			missileWeapon.ShotsPerAction++;
			if (obj.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader && magazineAmmoLoader.MaxAmmo < missileWeapon.AmmoPerAction)
			{
				magazineAmmoLoader.MaxAmmo = missileWeapon.AmmoPerAction;
			}
			return true;
		case 2:
			obj.RequirePart<MissilePerformance>().PenetrationModifier++;
			return true;
		case 3:
			obj.RequirePart<MissilePerformance>().DamageDieModifier += 2;
			return true;
		case 4:
			obj.RequirePart<MissilePerformance>().DamageModifier++;
			return true;
		case 5:
			obj.RequirePart<MissilePerformance>().PenetrateCreatures = true;
			return true;
		case 6:
			obj.RequirePart<MissilePerformance>().WantAddAttribute("Vorpal");
			return true;
		}
		if (missileWeapon.WeaponAccuracy > 0)
		{
			missileWeapon.WeaponAccuracy = Math.Max(missileWeapon.WeaponAccuracy - Stat.Random(5, 10), 0);
		}
		else
		{
			missileWeapon.AimVarianceBonus += Stat.Random(2, 6);
		}
		return true;
	}

	public static bool GiveRandomArmorMod(GameObject armor, int tier = 1, bool standard = false)
	{
		if (!(armor.GetPart("Armor") is Armor armor2))
		{
			return false;
		}
		switch (Stat.Random(0, 14))
		{
		case 0:
			armor2.DV++;
			break;
		case 1:
			armor2.AV++;
			break;
		case 2:
			armor2.MA += 2;
			break;
		case 3:
			armor2.Acid += 10;
			break;
		case 4:
			armor2.Cold += 10;
			break;
		case 5:
			armor2.Heat += 10;
			break;
		case 6:
			armor2.Elec += 10;
			break;
		case 7:
			armor2.Strength++;
			break;
		case 8:
			armor2.Agility++;
			break;
		case 9:
			armor2.Toughness++;
			break;
		case 10:
			armor2.Intelligence++;
			break;
		case 11:
			armor2.Willpower++;
			break;
		case 12:
			armor2.Ego++;
			break;
		case 13:
			armor2.ToHit += 2;
			break;
		case 14:
			if (armor2.SpeedPenalty > 0)
			{
				armor2.SpeedPenalty = Math.Max(armor2.SpeedPenalty - Stat.Random(5, 10), 0);
			}
			else
			{
				armor2.SpeedBonus += Stat.Random(1, 5);
			}
			break;
		}
		return true;
	}

	public static bool GiveRandomShieldMod(GameObject obj, int tier = 1, bool standard = false)
	{
		if (!(obj.GetPart("Shield") is Shield shield))
		{
			return false;
		}
		switch ((!standard) ? Stat.Random(0, 3) : 0)
		{
		case 0:
			if (!obj.HasPart("ModImprovedBlock"))
			{
				obj.AddPart(new ModImprovedBlock(tier));
				return true;
			}
			if (standard)
			{
				if (shield.AV < 1)
				{
					shield.AV = 1;
				}
				return true;
			}
			break;
		case 1:
			if (shield.DV < 0)
			{
				shield.DV++;
				if (shield.DV < 0 && 50.in100())
				{
					shield.DV++;
				}
				return true;
			}
			break;
		case 2:
			if (shield.SpeedPenalty > 0)
			{
				shield.SpeedPenalty = Math.Max(shield.SpeedPenalty - Stat.Random(5, 10), 0);
				return true;
			}
			break;
		}
		shield.AV++;
		return true;
	}

	private static GameObject GenerateBaseRelic(ref string type, int tier)
	{
		GameObject gameObject = null;
		bool flag = 20.in100();
		if (type == null)
		{
			type = supportedTypes.GetRandomElement();
		}
		if (supportedTypeMap.ContainsKey(type))
		{
			type = supportedTypeMap[type];
		}
		if (type == "Axe" && flag)
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicAxe1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicAxe2th");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Battle Axe3th");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Battle Axe4th");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Battle Axe5th");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Battle Axe6th");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Battle Axe7th");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Battle Axe8th");
			}
		}
		else if (type == "Axe")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicAxe1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicAxe2");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Battle Axe3");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Battle Axe4");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Battle Axe5");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Battle Axe6");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Battle Axe7");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Battle Axe8");
			}
		}
		else if (type == "ShortBlade")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicDagger1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicDagger2");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Dagger3");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Dagger4");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Dagger5");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Dagger6");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Dagger7");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Dagger8");
			}
		}
		else if (type == "LongBlade" && flag)
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicLongsword1th");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicLongsword2th");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Long Sword3th");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Long Sword4th");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Long Sword5th");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Long Sword6th");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Long Sword7th");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Long Sword8th");
			}
		}
		else if (type == "LongBlade")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicLongsword1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicLongsword2");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Long Sword3");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Long Sword4");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Long Sword5");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Long Sword6");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Long Sword7");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Long Sword8");
			}
		}
		else if (type == "Cudgel" && flag)
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("Warhammer2");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("Steel War Hammerth");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Cudgel3th");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Cudgel4th");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Cudgel5th");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Cudgel6th");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Cudgel7th");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Cudgel8th");
			}
		}
		else if (type == "Cudgel")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("Warhammer2");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("Steel Hammer");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("Cudgel3");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Cudgel4");
			}
			if (tier == 5)
			{
				gameObject = GameObject.createUnmodified("Cudgel5");
			}
			if (tier == 6)
			{
				gameObject = GameObject.createUnmodified("Cudgel6");
			}
			if (tier == 7)
			{
				gameObject = GameObject.createUnmodified("Cudgel7");
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Cudgel8");
			}
		}
		else if (type == "Rifle")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicRifle1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicRifle2");
			}
			if (tier == 3)
			{
				gameObject = ((!75.in100()) ? GameObject.createUnmodified("BaseRelicRifle3B") : GameObject.createUnmodified("BaseRelicRifle3A"));
			}
			if (tier == 4)
			{
				gameObject = ((!80.in100()) ? GameObject.createUnmodified("Flamethrower") : GameObject.createUnmodified("Laser Rifle"));
			}
			if (tier == 5)
			{
				gameObject = (50.in100() ? GameObject.createUnmodified("Eigenrifle") : ((!65.in100()) ? GameObject.createUnmodified("Freeze Ray") : GameObject.createUnmodified("Chain Laser")));
			}
			if (tier == 6)
			{
				gameObject = ((!80.in100()) ? GameObject.createUnmodified("Hypertractor") : GameObject.createUnmodified("Light Rail"));
			}
			if (tier == 7)
			{
				gameObject = ((!80.in100()) ? GameObject.createUnmodified("Linear Cannon") : GameObject.createUnmodified("Spaser Rifle"));
			}
			if (tier == 8)
			{
				gameObject = GameObject.createUnmodified("Phase Cannon");
			}
		}
		else if (type == "Pistol")
		{
			if (tier <= 1)
			{
				gameObject = GameObject.createUnmodified("BaseRelicPistol1");
			}
			if (tier == 2)
			{
				gameObject = GameObject.createUnmodified("BaseRelicPistol2");
			}
			if (tier == 3)
			{
				gameObject = GameObject.createUnmodified("BaseRelicPistol3");
			}
			if (tier == 4)
			{
				gameObject = GameObject.createUnmodified("Laser Pistol");
			}
			if (tier == 5)
			{
				gameObject = ((!80.in100()) ? GameObject.createUnmodified("Arc Winder") : GameObject.createUnmodified("Eigenpistol"));
			}
			if (tier == 6)
			{
				gameObject = ((!75.in100()) ? GameObject.createUnmodified("Di-Thermo Beam") : GameObject.createUnmodified("Hand Rail"));
			}
			if (tier == 7)
			{
				gameObject = (50.in100() ? GameObject.createUnmodified("Spaser Pistol") : (40.in100() ? GameObject.createUnmodified("Space Inverter") : ((!66.in100()) ? GameObject.createUnmodified("Psychal Fleshgun") : GameObject.createUnmodified("High-Voltage Arc Winder"))));
			}
			if (tier == 8)
			{
				gameObject = (50.in100() ? GameObject.createUnmodified("Spaser Pistol") : (40.in100() ? GameObject.createUnmodified("Space Inverter") : ((!66.in100()) ? GameObject.createUnmodified("Psychal Fleshgun") : GameObject.createUnmodified("High-Voltage Arc Winder"))));
			}
		}
		else if (type == "Book")
		{
			gameObject = GameObject.createUnmodified("RelicBook");
		}
		else if (type == "Food")
		{
			gameObject = GameObject.createUnmodified("RelicTonic");
		}
		else if (type == "Curio")
		{
			type = "Artifact";
		}
		if (gameObject == null)
		{
			int num = tier;
			List<string> list = new List<string>(8);
			string value = "BaseTier" + type + num;
			for (int i = 0; i < GameObjectFactory.Factory.Blueprints.Count; i++)
			{
				if (GameObjectFactory.Factory.BlueprintList[i].Name.StartsWith(value))
				{
					list.Add(GameObjectFactory.Factory.BlueprintList[i].Name);
				}
			}
			if (list.Count > 0)
			{
				gameObject = GameObject.createUnmodified(list.GetRandomElement());
			}
		}
		else
		{
			gameObject.SetStringProperty("Mods", "None");
		}
		if (gameObject == null)
		{
			Debug.LogWarning("Unknown relic type: " + type + ", tier: " + tier);
			type = supportedTypes.GetRandomElement();
			gameObject = GenerateBaseRelic(ref type, tier);
		}
		gameObject.SetImportant(flag: true);
		return gameObject;
	}

	public static string GetType(GameObject obj)
	{
		if (obj.GetPart("Armor") is Armor armor)
		{
			string wornOn = armor.WornOn;
			switch (wornOn)
			{
			case "Head":
			case "Face":
			case "Arm":
			case "Feet":
			case "Hands":
				return wornOn;
			case "Floating Nearby":
				return "Floating";
			default:
				return "Body";
			}
		}
		if (obj.HasPart("Shield"))
		{
			return "Shield";
		}
		string inventoryCategory = obj.GetInventoryCategory();
		if (inventoryCategory == "Food")
		{
			return "Food";
		}
		if (inventoryCategory == "Books")
		{
			return "Book";
		}
		if (obj.GetPart("MissileWeapon") is MissileWeapon missileWeapon)
		{
			switch (missileWeapon.Skill)
			{
			case "Rifle":
			case "Bow":
				return "Rifle";
			case "Pistol":
				return "Pistol";
			}
		}
		if (obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
		{
			switch (meleeWeapon.Skill)
			{
			case "Axe":
				return "Axe";
			case "ShortBlades":
				return "ShortBlade";
			case "LongBlades":
				return "LongBlade";
			case "Cudgel":
				if (meleeWeapon.BaseDamage != "1d2")
				{
					return "Cudgel";
				}
				break;
			}
		}
		if (!60.in100())
		{
			return "Curio";
		}
		return "Artifact";
	}

	public static string GetSubtype(string type)
	{
		switch (type)
		{
		case "Axe":
		case "ShortBlade":
		case "LongBlade":
		case "Cudgel":
			return "weapon";
		case "Head":
		case "Body":
		case "Face":
		case "Arm":
		case "Feet":
		case "Hands":
		case "Floating":
			return "armor";
		case "Rifle":
		case "Pistol":
			return "ranged";
		case "Book":
			return "book";
		case "Food":
			return "food";
		case "Artifact":
		case "Curio":
			return "curio";
		case "Shield":
			return "shield";
		default:
			return "curio";
		}
	}

	public static string SelectElement(GameObject Item, GameObject Owner = null, GameObject Killed = null, GameObject InfluencedBy = null)
	{
		Dictionary<string, int> @for = GetItemElementsEvent.GetFor(Owner, Item, Killed, InfluencedBy);
		string text = HistoricStringExpander.ExpandString("<spice.elements.!random>") ?? "might";
		if (@for == null)
		{
			return text;
		}
		if (@for.ContainsKey(text))
		{
			@for[text]++;
		}
		else
		{
			@for.Add(text, 1);
		}
		return @for.GetRandomElement();
	}

	private static void AddAttributeBoost(GameObject obj, string Attribute, int tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(obj, Attribute + ":" + GetAttributeBonusFromRelicTier(tier), Attribute + "Boost", techScan: true);
		result = true;
	}

	private static void AddResistanceBoost(GameObject obj, string Resist, int tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(obj, Resist + ":" + GetResistanceBonusFromRelicTier(tier), Resist + "System", techScan: true);
		result = true;
	}

	private static void AddMoveSpeedBoost(GameObject obj, int tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(obj, "MoveSpeed:" + GetMoveSpeedBonusFromRelicTier(tier), "KineticDriver", techScan: true);
		result = true;
	}

	public static bool ApplyBasicBestowal(GameObject obj, string type = null, int tier = 1, string subtype = null, bool standard = false)
	{
		if (type == null)
		{
			type = GetType(obj);
		}
		if (subtype == null)
		{
			subtype = GetSubtype(type);
		}
		bool flag = false;
		switch (subtype)
		{
		case "ranged":
			flag = GiveRandomMissileWeaponMod(obj, tier, standard);
			break;
		case "weapon":
			flag = GiveRandomMeleeWeaponMod(obj, tier, standard);
			break;
		case "armor":
			flag = GiveRandomArmorMod(obj, tier, standard);
			break;
		case "shield":
			flag = GiveRandomShieldMod(obj, tier, standard);
			break;
		case "book":
			obj.RequirePart<TrainingBook>().AssignRandomTraining();
			break;
		}
		if (flag && obj.HasStat("Hitpoints"))
		{
			obj.GetStat("Hitpoints").BaseValue += 100;
		}
		return flag;
	}

	public static bool ApplyElementBestowal(GameObject obj, string element, string type, int tier = 1, string subtype = null)
	{
		bool result = false;
		if (subtype == null)
		{
			subtype = GetSubtype(type);
		}
		switch (subtype)
		{
		case "ranged":
			switch (element)
			{
			case "glass":
				if (!obj.HasPart("ModImprovedClairvoyance"))
				{
					obj.AddPart(new ModImprovedClairvoyance(tier));
					result = true;
				}
				break;
			case "jewels":
				AddAttributeBoost(obj, "Ego", tier, ref result);
				break;
			case "stars":
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(tier));
					result = true;
				}
				break;
			case "time":
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(obj, "Willpower", tier, ref result);
				break;
			case "ice":
				AddResistanceBoost(obj, "ColdResistance", tier, ref result);
				break;
			case "scholarship":
				AddAttributeBoost(obj, "Intelligence", tier, ref result);
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 50.in100() && !obj.HasPart("ModImprovedTelekinesis"))
				{
					obj.AddPart(new ModImprovedTelekinesis(tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(obj, "Strength", tier, ref result);
				}
				break;
			case "travel":
				if (50.in100() && !obj.HasPart("ModImprovedTeleportation"))
				{
					obj.AddPart(new ModImprovedTeleportation(tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(obj, tier, ref result);
				}
				break;
			case "chance":
				AddAttributeBoost(obj, "Agility", tier, ref result);
				break;
			case "circuitry":
				if (!obj.HasPart("ModImprovedElectricalGeneration"))
				{
					obj.AddPart(new ModImprovedElectricalGeneration(tier));
					result = true;
				}
				break;
			}
			break;
		case "weapon":
			switch (element)
			{
			case "glass":
				if (!obj.HasPart("ModGlazed"))
				{
					obj.AddPart(new ModGlazed(tier));
					result = true;
				}
				break;
			case "jewels":
				if (50.in100() && !obj.HasPart("ModTransmuteOnHit"))
				{
					obj.AddPart(new ModTransmuteOnHit(tier * 4, "Gemstones"));
					result = true;
				}
				else
				{
					AddAttributeBoost(obj, "Ego", tier, ref result);
				}
				break;
			case "stars":
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(tier));
					result = true;
				}
				break;
			case "time":
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(obj, "Willpower", tier, ref result);
				break;
			case "ice":
				if (!obj.HasPart("ModRelicFreezing"))
				{
					obj.AddPart(new ModRelicFreezing(tier));
					result = true;
				}
				break;
			case "scholarship":
				if (!obj.HasPart("ModBeetlehost"))
				{
					obj.AddPart(new ModBeetlehost(tier));
					result = true;
				}
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 50.in100() && !obj.HasPart("ModImprovedTelekinesis"))
				{
					obj.AddPart(new ModImprovedTelekinesis(tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(obj, "Strength", tier, ref result);
				}
				break;
			case "travel":
				if (!obj.HasPart("ModImprovedTeleportation"))
				{
					obj.AddPart(new ModImprovedTeleportation(tier));
					result = true;
				}
				break;
			case "chance":
				if (!obj.HasPart("ModFatecaller"))
				{
					obj.AddPart(new ModFatecaller(tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!obj.HasPart("ModImprovedElectricalGeneration"))
				{
					obj.AddPart(new ModImprovedElectricalGeneration(tier));
					result = true;
				}
				break;
			}
			break;
		case "armor":
			switch (element)
			{
			case "glass":
				if (50.in100() && !obj.HasPart("ModGlassArmor"))
				{
					obj.AddPart(new ModGlassArmor(tier));
					result = true;
				}
				else if (!obj.HasPart("ModImprovedClairvoyance"))
				{
					obj.AddPart(new ModImprovedClairvoyance(tier));
					result = true;
				}
				break;
			case "jewels":
				obj.GetPart<Armor>().Ego += GetAttributeBonusFromRelicTier(tier);
				result = true;
				break;
			case "stars":
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(tier));
					result = true;
				}
				break;
			case "time":
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(tier));
					result = true;
				}
				break;
			case "salt":
				obj.GetPart<Armor>().Willpower += GetAttributeBonusFromRelicTier(tier);
				result = true;
				break;
			case "ice":
				obj.GetPart<Armor>().Cold += GetResistanceBonusFromRelicTier(tier);
				result = true;
				break;
			case "scholarship":
				obj.GetPart<Armor>().Intelligence += GetAttributeBonusFromRelicTier(tier);
				result = true;
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 50.in100() && !obj.HasPart("ModImprovedTelekinesis"))
				{
					obj.AddPart(new ModImprovedTelekinesis(tier));
					result = true;
				}
				else
				{
					obj.GetPart<Armor>().Strength += GetAttributeBonusFromRelicTier(tier);
					result = true;
				}
				break;
			case "travel":
				if (50.in100() && !obj.HasPart("ModImprovedTeleportation"))
				{
					obj.AddPart(new ModImprovedTeleportation(tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(obj, tier, ref result);
				}
				break;
			case "chance":
				if (!obj.HasPart("ModBlinkEscape"))
				{
					obj.AddPart(new ModBlinkEscape(tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!obj.HasPart("ModImprovedElectricalGeneration"))
				{
					obj.AddPart(new ModImprovedElectricalGeneration(tier));
					result = true;
				}
				break;
			}
			break;
		case "shield":
			switch (element)
			{
			case "glass":
				if (50.in100() && !obj.HasPart("ModGlassArmor"))
				{
					obj.AddPart(new ModGlassArmor(tier));
					result = true;
				}
				else if (!obj.HasPart("ModImprovedClairvoyance"))
				{
					obj.AddPart(new ModImprovedClairvoyance(tier));
					result = true;
				}
				break;
			case "jewels":
				AddAttributeBoost(obj, "Ego", tier, ref result);
				break;
			case "stars":
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(tier));
					result = true;
				}
				break;
			case "time":
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(obj, "Willpower", tier, ref result);
				break;
			case "ice":
				AddResistanceBoost(obj, "ColdResistance", tier, ref result);
				break;
			case "scholarship":
				AddAttributeBoost(obj, "Intelligence", tier, ref result);
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 50.in100() && !obj.HasPart("ModImprovedTelekinesis"))
				{
					obj.AddPart(new ModImprovedTelekinesis(tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(obj, "Strength", tier, ref result);
				}
				break;
			case "chance":
				if (!obj.HasPart("ModBlinkEscape"))
				{
					obj.AddPart(new ModBlinkEscape(tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!obj.HasPart("ModImprovedElectricalGeneration"))
				{
					obj.AddPart(new ModImprovedElectricalGeneration(tier));
					result = true;
				}
				break;
			case "travel":
				if (50.in100() && !obj.HasPart("ModImprovedTeleportation"))
				{
					obj.AddPart(new ModImprovedTeleportation(tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(obj, tier, ref result);
				}
				break;
			}
			break;
		}
		if (element == "travel" && (subtype == "armor" || subtype == "shield") && !obj.HasPart("CarryBonus"))
		{
			int amount = Stat.Random(0, 4) * 5 + (tier - 1) * 10 + 20;
			obj.AddPart(new CarryBonus(amount));
			result = true;
		}
		if (result && obj.HasStat("Hitpoints"))
		{
			obj.GetStat("Hitpoints").BaseValue += 200;
		}
		return result;
	}

	public static GameObject GenerateRelic(string type, int tier, HistoricEntitySnapshot snapRegion, List<string> adjectives, Dictionary<string, string> properties, Dictionary<string, List<string>> listProperties, string name = null, string likedFactionDescriptionAddendum = null)
	{
		History sultanHistory = XRLCore.Core.Game.sultanHistory;
		GameObject gameObject = GenerateBaseRelic(ref type, tier);
		gameObject.pRender.ColorString = "&M";
		gameObject.pRender.TileColor = "&M";
		Description part = gameObject.GetPart<Description>();
		int targetPeriod = GetPeriodFromRelicTier(tier);
		HistoricEntityList entitiesByDelegate = sultanHistory.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("sultan") && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) == targetPeriod);
		if (entitiesByDelegate.Count > 0)
		{
			string property = entitiesByDelegate.GetRandomElement().GetCurrentSnapshot().GetProperty("name");
			part._Short = part._Short.Replace("*sultan*", property);
		}
		else
		{
			Debug.LogWarning("Could not find a sultan for period " + targetPeriod + " from tier " + tier);
			part._Short = part._Short.Replace("*sultan*", "the sultan");
		}
		List<string> list = new List<string>();
		for (int i = 0; i < adjectives.Count; i++)
		{
			string text = TranslateAdjective(adjectives[i]);
			if (!string.IsNullOrEmpty(text) && !list.Contains(text))
			{
				list.Add(text);
			}
		}
		if (list.Count == 0)
		{
			list.Add(HistoricStringExpander.ExpandString("<spice.elements.!random>", null, sultanHistory));
		}
		string subtype = GetSubtype(type);
		ApplyBasicBestowal(gameObject, type, tier, subtype, standard: true);
		foreach (string item in list)
		{
			ApplyElementBestowal(gameObject, item, type, tier, subtype);
			if (item != "none")
			{
				string newValue = HistoricStringExpander.ExpandString("<spice.elements." + item + ".nounsPlural.!random>", null, sultanHistory);
				Description description = part;
				description._Short = description._Short + " " + gameObject.Itis + " " + HistoricStringExpander.ExpandString("<spice.instancesOf.stamped_VAR.!random>", null, sultanHistory).Replace("*var*", newValue) + ".";
			}
		}
		foreach (string key in listProperties.Keys)
		{
			if (!(key == "likedFactions"))
			{
				continue;
			}
			foreach (string item2 in listProperties[key])
			{
				AddsRep.AddModifier(gameObject, item2 + ":200");
				Description description = part;
				description._Short = description._Short + " There's an engraving of " + Faction.getFormattedName(item2) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.venerated.!random>", null, sultanHistory);
				if (likedFactionDescriptionAddendum != null)
				{
					part._Short += likedFactionDescriptionAddendum;
				}
				part._Short += ".";
			}
		}
		foreach (string key2 in listProperties.Keys)
		{
			if (!(key2 == "lovedFactions"))
			{
				continue;
			}
			foreach (string item3 in listProperties[key2])
			{
				AddsRep.AddModifier(gameObject, item3 + ":400");
				Description description = part;
				description._Short = description._Short + " There's an engraving of " + Faction.getFormattedName(item3) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.venerated.!random>", null, sultanHistory) + ".";
			}
		}
		foreach (string key3 in listProperties.Keys)
		{
			if (!(key3 == "hatedFactions"))
			{
				continue;
			}
			foreach (string item4 in listProperties[key3])
			{
				AddsRep.AddModifier(gameObject, item4 + ":-200");
				if (!gameObject.HasPart("ModFactionSlayer"))
				{
					gameObject.AddPart(new ModFactionSlayer(tier, item4));
					Description description = part;
					description._Short = description._Short + " There's an engraving of " + Faction.getFormattedName(item4) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.disparaged.!random>", null, sultanHistory) + ".";
				}
			}
		}
		if (subtype == "curio")
		{
			switch (Stat.Random(1, 2))
			{
			case 1:
			{
				string populationName = "Curio_SummoningRelic" + tier;
				int num = 0;
				string text2 = null;
				string text3 = null;
				Faction faction = null;
				do
				{
					text2 = PopulationManager.RollOneFrom(populationName).Blueprint;
					text3 = GameObjectFactory.Factory.GetBlueprint(text2).GetPrimaryFaction();
					faction = ((text3 == null) ? null : Factions.get(text3));
				}
				while ((faction == null || !faction.Old) && ++num < 1000);
				gameObject.RequirePart<SummoningCurio>().Creature = text2;
				break;
			}
			case 2:
				gameObject.RequirePart<GenocideCurio>();
				break;
			}
		}
		if (type == "Book")
		{
			gameObject.GetPart<Commerce>().Value = 200 + 100 * tier;
		}
		else
		{
			gameObject.GetPart<Commerce>().Value = 500 + 150 * tier;
		}
		if (name == null)
		{
			name = GenerateRelicName(type, snapRegion, list, properties);
		}
		if (!name.Contains("{{"))
		{
			name = "{{M|" + name + "}}";
		}
		gameObject.pRender.DisplayName = name;
		gameObject.HasProperName = true;
		if (subtype == "book")
		{
			gameObject.GetPart<MarkovBook>().Title = name;
		}
		gameObject.SetStringProperty("RelicName", name);
		gameObject.AddPart(new TakenWXU(2));
		return gameObject;
	}
}

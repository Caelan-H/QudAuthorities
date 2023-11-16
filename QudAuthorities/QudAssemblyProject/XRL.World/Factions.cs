using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

[HasGameBasedStaticCache]
public static class Factions
{
	[GameBasedStaticCache(CreateInstance = false)]
	private static Dictionary<string, Faction> FactionTable;

	[GameBasedStaticCache(CreateInstance = false)]
	private static List<Faction> FactionList;

	public static void CheckInit()
	{
		if (FactionTable == null)
		{
			Loading.LoadTask("Loading Factions.xml", Init, showToUser: false);
		}
	}

	public static string isZoneHoly(string zoneID)
	{
		foreach (Faction faction in FactionList)
		{
			if (faction.HolyPlaces != null && faction.HolyPlaces.Contains(zoneID))
			{
				return faction.Name;
			}
		}
		return null;
	}

	public static Faction get(string name)
	{
		CheckInit();
		if (FactionTable.TryGetValue(name, out var value))
		{
			if (value == null)
			{
				Debug.LogError("faction table was out of sync for faction name" + name);
				FactionTable[name] = new Faction(name);
				return FactionTable[name];
			}
			return value;
		}
		throw new Exception("unknown faction \"" + name + "\"");
	}

	public static Faction getIfExists(string name)
	{
		if (name == null)
		{
			return null;
		}
		CheckInit();
		if (FactionTable.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public static IEnumerable<Faction> loop()
	{
		CheckInit();
		foreach (Faction faction in FactionList)
		{
			yield return faction;
		}
	}

	public static List<string> getFactionNames()
	{
		CheckInit();
		return new List<string>(FactionTable.Keys);
	}

	public static List<string> getVisibleFactionNames()
	{
		CheckInit();
		return (from kv in FactionTable
			where kv.Value.Visible
			select kv.Key).ToList();
	}

	public static int getFactionCount()
	{
		CheckInit();
		return FactionList.Count;
	}

	public static void AddNewFaction(Faction newFaction)
	{
		CheckInit();
		FactionTable.Add(newFaction.Name, newFaction);
		FactionList.Add(newFaction);
	}

	public static void Load(SerializationReader reader)
	{
		FactionTable = reader.ReadDictionary<string, Faction>();
		FactionList = new List<Faction>();
		foreach (Faction value in FactionTable.Values)
		{
			FactionList.Add(value);
		}
		Loading.LoadTask("Loading Factions.xml", LoadXml, showToUser: false);
	}

	public static void Save(SerializationWriter writer)
	{
		CheckInit();
		writer.Write(FactionTable);
	}

	private static void Init()
	{
		FactionTable = new Dictionary<string, Faction>(64);
		FactionList = new List<Faction>(64);
		LoadXml();
	}

	public static void LoadXml()
	{
		ProcessFactionXmlFile("Factions.xml", mod: false);
		ModManager.ForEachFile("Factions.xml", delegate(string file)
		{
			ProcessFactionXmlFile(file, mod: true);
		});
	}

	private static bool IsVisible(Faction f)
	{
		return f.Visible;
	}

	private static bool IsVisibleAndIsOld(Faction f)
	{
		if (f.Visible)
		{
			return f.Old;
		}
		return false;
	}

	private static bool IsVisibleAndCanBeExtradimensional(Faction f)
	{
		if (f.Visible)
		{
			return f.ExtradimensionalVersions;
		}
		return false;
	}

	private static bool HasAtLeastOneMember(Faction f)
	{
		return GameObjectFactory.Factory.GetFactionMembers(f.Name).Count > 0;
	}

	public static Faction GetRandomFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisible).GetRandomElement();
	}

	public static Faction GetRandomFaction(Predicate<Faction> pFilter)
	{
		CheckInit();
		return FactionTable.Values.Where((Faction f) => IsVisible(f) && pFilter(f)).GetRandomElement();
	}

	public static Faction GetRandomFaction(string Exception)
	{
		CheckInit();
		return (from f in FactionTable.Values.Where(IsVisible)
			where f.Name != Exception
			select f).GetRandomElement();
	}

	public static Faction GetRandomFaction(string[] Exceptions)
	{
		CheckInit();
		return (from f in FactionTable.Values.Where(IsVisible)
			where Array.IndexOf(Exceptions, f.Name) == -1
			select f).GetRandomElement();
	}

	public static Faction GetRandomPotentiallyExtradimensionalFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisibleAndCanBeExtradimensional).GetRandomElement();
	}

	public static Faction GetRandomOldFaction()
	{
		CheckInit();
		return FactionTable.Values.Where(IsVisibleAndIsOld).GetRandomElement();
	}

	public static Faction GetRandomOldFaction(string Exception)
	{
		CheckInit();
		return (from f in FactionTable.Values.Where(IsVisibleAndIsOld)
			where f.Name != Exception
			select f).GetRandomElement();
	}

	public static Faction GetRandomFactionWithAtLeastOneMember()
	{
		return GetRandomFaction(HasAtLeastOneMember);
	}

	public static Faction GetRandomFactionWithAtLeastOneMember(Predicate<Faction> pFilter)
	{
		return GetRandomFaction((Faction f) => HasAtLeastOneMember(f) && pFilter(f));
	}

	public static int GetFeelingFactionToFaction(string Faction1, string Faction2)
	{
		if (Faction1 == Faction2)
		{
			return 100;
		}
		try
		{
			return get(Faction1).GetFeelingTowardsFaction(Faction2);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error with faction " + Faction1 + " - " + ex.ToString());
			return 0;
		}
	}

	public static int GetFeelingFactionToObject(string Faction, GameObject Object)
	{
		try
		{
			return get(Faction).GetFeelingTowardsObject(Object);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error with faction " + Faction + " - " + ex.ToString());
			return 0;
		}
	}

	public static bool isPettable(string Faction)
	{
		return get(Faction).Pettable;
	}

	private static void ProcessFactionXmlFile(string file, bool mod)
	{
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.Name == "factions")
			{
				LoadFactionsNode(xmlTextReader, mod);
			}
			if (xmlTextReader.NodeType == XmlNodeType.EndElement && xmlTextReader.Name == "factions")
			{
				break;
			}
		}
		xmlTextReader.Close();
	}

	private static void LoadFactionsNode(XmlTextReader Reader, bool mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "faction")
			{
				LoadFactionNode(Reader, mod);
			}
			else if (Reader.Name == "removefaction")
			{
				string attribute = Reader.GetAttribute("Name");
				if (string.IsNullOrEmpty(attribute))
				{
					throw new Exception("removefaction tag had no Name attribute");
				}
				if (FactionTable.ContainsKey(attribute))
				{
					FactionTable.Remove(attribute);
				}
			}
		}
	}

	private static void LoadFactionNode(XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new Exception("faction tag had no Name attribute");
		}
		Faction faction;
		if (FactionTable.ContainsKey(attribute))
		{
			if (mod && Reader.GetAttribute("Load") != "Merge")
			{
				faction = new Faction(attribute);
				FactionTable[attribute] = faction;
			}
			else
			{
				faction = get(attribute);
			}
		}
		else
		{
			faction = new Faction(attribute);
			AddNewFaction(faction);
		}
		string attribute2 = Reader.GetAttribute("DisplayName");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.DisplayName = attribute2;
		}
		attribute2 = Reader.GetAttribute("Parent");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.setParent(attribute2);
		}
		attribute2 = Reader.GetAttribute("ExtradimensionalVersions");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.ExtradimensionalVersions = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("FormatWithArticle");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.FormatWithArticle = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("HatesPlayer");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.HatesPlayer = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Old");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.Old = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("InitialPlayerReputation");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.InitialPlayerReputation = TryInt(attribute2, "player initial reputation");
		}
		attribute2 = Reader.GetAttribute("HighlyEntropicBeingWorshipAttitude");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.HighlyEntropicBeingWorshipAttitude = TryInt(attribute2, "highly entropic being worship attitude");
		}
		attribute2 = Reader.GetAttribute("Pettable");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.Pettable = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Plural");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.Plural = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Visible");
		if (!string.IsNullOrEmpty(attribute2))
		{
			faction.Visible = attribute2.EqualsNoCase("true");
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "feeling")
				{
					string attribute3 = Reader.GetAttribute("About");
					string attribute4 = Reader.GetAttribute("Value");
					try
					{
						int faction_feeling = Convert.ToInt32(attribute4);
						faction.setFactionFeeling(attribute3, faction_feeling);
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
				}
				else if (Reader.Name == "partreputation")
				{
					string attribute5 = Reader.GetAttribute("About");
					string attribute6 = Reader.GetAttribute("Value");
					try
					{
						int feeling = Convert.ToInt32(attribute6);
						faction.setPartReputation(attribute5, feeling);
					}
					catch (Exception message2)
					{
						Debug.LogError(message2);
					}
				}
				else if (Reader.Name == "interests")
				{
					LoadInterestsNode(faction, Reader, mod);
				}
				else if (Reader.Name == "waterritual")
				{
					attribute2 = Reader.GetAttribute("Liquid");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualLiquid = attribute2;
					}
					attribute2 = Reader.GetAttribute("Skill");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualSkill = attribute2;
					}
					attribute2 = Reader.GetAttribute("SkillCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualSkillCost = TryInt(attribute2, "water ritual skill cost");
					}
					attribute2 = Reader.GetAttribute("BuyMostValuableItem");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualBuyMostValuableItem = attribute2.EqualsNoCase("true");
					}
					attribute2 = Reader.GetAttribute("FungusInfect");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualFungusInfect = TryInt(attribute2, "water ritual fungus infect");
					}
					attribute2 = Reader.GetAttribute("HermitOath");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualHermitOath = TryInt(attribute2, "water ritual hermit oath");
					}
					attribute2 = Reader.GetAttribute("SkillPointAmount");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualSkillPointAmount = TryInt(attribute2, "water ritual skill point amount");
					}
					attribute2 = Reader.GetAttribute("SkillPointCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualSkillPointCost = TryInt(attribute2, "water ritual skill point cost");
					}
					attribute2 = Reader.GetAttribute("Mutation");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualMutation = attribute2;
					}
					attribute2 = Reader.GetAttribute("MutationCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualMutationCost = TryInt(attribute2, "water ritual mutation cost");
					}
					attribute2 = Reader.GetAttribute("Gifts");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualGifts = attribute2;
					}
					attribute2 = Reader.GetAttribute("Items");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualItems = attribute2;
					}
					attribute2 = Reader.GetAttribute("ItemBlueprint");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualItemBlueprint = attribute2;
					}
					attribute2 = Reader.GetAttribute("ItemCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualItemCost = TryInt(attribute2, "water ritual item cost");
					}
					attribute2 = Reader.GetAttribute("Blueprints");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualBlueprints = attribute2;
					}
					attribute2 = Reader.GetAttribute("Recipe");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualRecipe = attribute2;
					}
					attribute2 = Reader.GetAttribute("RecipeText");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualRecipeText = attribute2;
					}
					attribute2 = Reader.GetAttribute("Join");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualJoin = attribute2.EqualsNoCase("true");
					}
					attribute2 = Reader.GetAttribute("RandomMentalMutation");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualRandomMentalMutation = TryInt(attribute2, "water ritual random mental mutation");
					}
					attribute2 = Reader.GetAttribute("AltBehaviorPart");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltBehaviorPart = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltBehaviorTag");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltBehaviorTag = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltLiquid");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltLiquid = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltSkill");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltSkill = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltSkillCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltSkillCost = TryInt(attribute2, "water ritual alt skill cost");
					}
					attribute2 = Reader.GetAttribute("AltGifts");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltGifts = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltItems");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltItems = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltItemBlueprint");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltItemBlueprint = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltItemCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltItemCost = TryInt(attribute2, "water ritual alt item cost");
					}
					attribute2 = Reader.GetAttribute("AltSkill");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltSkill = attribute2;
					}
					attribute2 = Reader.GetAttribute("AltSkillCost");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltSkillCost = TryInt(attribute2, "water ritual alt skill cost");
					}
					attribute2 = Reader.GetAttribute("AltBlueprints");
					if (!string.IsNullOrEmpty(attribute2))
					{
						faction.WaterRitualAltBlueprints = attribute2;
					}
				}
				else if (Reader.Name == "holyplace")
				{
					attribute2 = Reader.GetAttribute("ZoneID");
					if (!string.IsNullOrEmpty(attribute2) && !faction.HolyPlaces.Contains(attribute2))
					{
						faction.HolyPlaces.Add(attribute2);
					}
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "faction")
			{
				break;
			}
		}
	}

	private static void LoadInterestsNode(Faction Entry, XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("BuyTargetedSecrets");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.BuyTargetedSecrets = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("SellTargetedSecrets");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.SellTargetedSecrets = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("BuyDescription");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.BuyDescription = attribute;
		}
		attribute = Reader.GetAttribute("SellDescription");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.SellDescription = attribute;
		}
		attribute = Reader.GetAttribute("BothDescription");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.BothDescription = attribute;
		}
		attribute = Reader.GetAttribute("Blurb");
		if (!string.IsNullOrEmpty(attribute))
		{
			Entry.InterestsBlurb = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "interest")
			{
				LoadInterestNode(Entry, Reader, mod);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "interests")
			{
				break;
			}
		}
	}

	private static void LoadInterestNode(Faction Entry, XmlTextReader Reader, bool mod = false)
	{
		FactionInterest factionInterest = new FactionInterest();
		factionInterest.Tags = Reader.GetAttribute("Tags");
		string attribute = Reader.GetAttribute("WillBuy");
		if (!string.IsNullOrEmpty(attribute))
		{
			factionInterest.WillBuy = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("WillSell");
		if (!string.IsNullOrEmpty(attribute))
		{
			factionInterest.WillSell = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("MatchAny");
		if (!string.IsNullOrEmpty(attribute))
		{
			factionInterest.MatchAny = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("Inverse");
		if (!string.IsNullOrEmpty(attribute))
		{
			factionInterest.Inverse = attribute.EqualsNoCase("true");
		}
		attribute = Reader.GetAttribute("Description");
		if (!string.IsNullOrEmpty(attribute))
		{
			factionInterest.Description = attribute;
		}
		factionInterest.SourceFileName = "Factions.xml";
		factionInterest.SourceLineNumber = Reader.LineNumber;
		factionInterest.SourceWasMod = mod;
		Entry.AddInterestIfNew(factionInterest);
	}

	private static int TryInt(string Spec, string What)
	{
		try
		{
			return Convert.ToInt32(Spec);
		}
		catch
		{
			Debug.LogError("Error in " + What + ": " + Spec);
		}
		return -1;
	}

	public static void RequireCachedHeirlooms()
	{
		foreach (Faction item in loop())
		{
			item.RequireCachedHeirloom();
		}
	}

	public static void HighlyEntropicBeingWorshipped(string Name = null, int MaxAttitudeApply = 1)
	{
		if (Name != null)
		{
			string text = The.Game.GetStringGameState("HighlyEntropicBeingsWorshipped") ?? The.Player.GetStringProperty("HighlyEntropicBeingsWorshipped");
			if (text == null)
			{
				The.Game.SetStringGameState("HighlyEntropicBeingsWorshipped", Name);
			}
			else
			{
				List<string> list = text.CachedCommaExpansion();
				if (!list.Contains(Name))
				{
					List<string> list2 = new List<string>(list);
					list2.Add(Name);
					The.Game.SetStringGameState("HighlyEntropicBeingsWorshipped", string.Join(",", list2.ToArray()));
				}
			}
		}
		int intGameState = The.Game.GetIntGameState("HighlyEntropicBeingWorshipCount", The.Player.GetIntProperty("HighlyEntropicBeingWorshipCount"));
		intGameState++;
		The.Game.SetIntGameState("HighlyEntropicBeingWorshipCount", intGameState);
		if (The.Game.GetIntGameState("HighlyEntropicBeingWorshipAttitudeApplied", The.Player.GetIntProperty("HighlyEntropicBeingWorshipAttitudeApplied")) < MaxAttitudeApply)
		{
			ApplyHighlyEntropicBeingWorshipAttitudeToPlayer(Name);
		}
	}

	public static void ApplyHighlyEntropicBeingWorshipAttitudeToPlayer(string Name = null)
	{
		int intGameState = The.Game.GetIntGameState("HighlyEntropicBeingWorshipAttitudeApplied", The.Player.GetIntProperty("HighlyEntropicBeingWorshipAttitudeApplied"));
		intGameState++;
		The.Game.SetIntGameState("HighlyEntropicBeingWorshipAttitudeApplied", intGameState);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		stringBuilder.Append("Your worship of ").Append(Name ?? "a highly entropic being").Append(" has made you infamous to many across Qud.\n\n");
		foreach (Faction item in loop())
		{
			if (item.HighlyEntropicBeingWorshipAttitude != 0)
			{
				The.Game.PlayerReputation.modify(item, GivesRep.varyRep(item.HighlyEntropicBeingWorshipAttitude), null, item.Visible ? stringBuilder : stringBuilder2, !item.Visible);
			}
		}
		if (stringBuilder.Length > 0)
		{
			Popup.Show(stringBuilder.ToString());
		}
	}

	public static List<string> GetInvokableHighlyEntropicBeings()
	{
		List<string> list = new List<string>();
		list.Add("Ptoh");
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (blueprint.HasBeenSeen())
			{
				string partParameter = blueprint.GetPartParameter("Brain", "Factions");
				if (!string.IsNullOrEmpty(partParameter) && partParameter.Contains("highly entropic beings-100"))
				{
					list.Add(blueprint.CachedDisplayNameStripped);
				}
			}
		}
		list.Sort();
		return list;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Tinkering;

namespace XRL.World;

[HasModSensitiveStaticCache]
public class ModificationFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<ModEntry> _ModList;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<ModEntry>> _ModTable;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, ModEntry> _ModsByPart;

	[NonSerialized]
	private static List<ModEntry> modsEligible = new List<ModEntry>(32);

	[NonSerialized]
	private static Dictionary<ModEntry, int> modDist = new Dictionary<ModEntry, int>(32);

	[NonSerialized]
	private static Dictionary<int, int> rarityCodeWeights = new Dictionary<int, int>(4);

	public static List<ModEntry> ModList
	{
		get
		{
			CheckInit();
			return _ModList;
		}
	}

	public static Dictionary<string, List<ModEntry>> ModTable
	{
		get
		{
			CheckInit();
			return _ModTable;
		}
	}

	public static Dictionary<string, ModEntry> ModsByPart
	{
		get
		{
			CheckInit();
			return _ModsByPart;
		}
	}

	public static void CheckInit()
	{
		if (_ModTable == null)
		{
			Loading.LoadTask("Loading Mods.xml", LoadMods);
		}
	}

	private static void LoadMods()
	{
		_ModList = new List<ModEntry>(128);
		_ModTable = new Dictionary<string, List<ModEntry>>(32);
		_ModsByPart = new Dictionary<string, ModEntry>(128);
		TinkerData.TinkerRecipes = new List<TinkerData>(64);
		List<string> Paths = new List<string> { DataManager.FilePath("Mods.xml") };
		Paths.AddRange(Directory.GetFiles(DataManager.FilePath("."), "Mods_*.xml"));
		ModManager.ForEachFile("Mods.xml", delegate(string path)
		{
			Paths.Add(path);
		});
		foreach (string item in Paths)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(item);
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "mods")
				{
					LoadModsNode(xmlTextReader, isPrimary: false);
				}
			}
		}
		foreach (ModEntry mod in _ModList)
		{
			if (!mod.Tables.IsNullOrEmpty())
			{
				string[] array = mod.Tables.Split(',');
				foreach (string key in array)
				{
					if (!_ModTable.ContainsKey(key))
					{
						_ModTable.Add(key, new List<ModEntry>());
					}
					_ModTable[key].Add(mod);
				}
			}
			if (mod.TinkerAllowed)
			{
				TinkerData.TinkerRecipes.Add(new TinkerData
				{
					Blueprint = "[mod]" + mod.Part,
					DisplayName = mod.TinkerDisplayName,
					Cost = "",
					Ingredient = mod.TinkerIngredient,
					Tier = mod.TinkerTier,
					Type = "Mod"
				});
			}
		}
	}

	public static void LoadModsNode(XmlTextReader Reader, bool isPrimary = true)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "mod")
			{
				LoadModNode(Reader, isPrimary);
			}
		}
	}

	public static void LoadModNode(XmlTextReader Reader, bool isPrimary = true)
	{
		string attribute = Reader.GetAttribute("Part");
		if (!_ModsByPart.TryGetValue(attribute, out var value))
		{
			value = new ModEntry
			{
				Part = attribute
			};
			_ModsByPart.Add(attribute, value);
			_ModList.Add(value);
		}
		string attribute2 = Reader.GetAttribute("MinTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.MinTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("MaxTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.MaxTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("NativeTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.NativeTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("TinkerTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.TinkerTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("Value");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Value = double.Parse(attribute2);
		}
		attribute2 = Reader.GetAttribute("Description");
		if (attribute2 != null)
		{
			value.Description = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerDisplayName");
		if (attribute2 != null)
		{
			value.TinkerDisplayName = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerIngredient");
		if (attribute2 != null)
		{
			value.TinkerIngredient = attribute2;
		}
		attribute2 = Reader.GetAttribute("Tables");
		if (attribute2 != null)
		{
			value.Tables = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerAllowed");
		if (!attribute2.IsNullOrEmpty())
		{
			value.TinkerAllowed = !attribute2.EqualsNoCase("false");
		}
		attribute2 = Reader.GetAttribute("CanAutoTinker");
		if (!attribute2.IsNullOrEmpty())
		{
			value.CanAutoTinker = !attribute2.EqualsNoCase("false");
		}
		attribute2 = Reader.GetAttribute("NoSparkingQuest");
		if (!attribute2.IsNullOrEmpty())
		{
			value.NoSparkingQuest = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Rarity");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Rarity = getRarityCode(attribute2);
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "mod"))))
			{
			}
		}
	}

	public static int getRarityCode(string rarity)
	{
		return rarity switch
		{
			"C" => 0, 
			"U" => 1, 
			"R" => 2, 
			"M" => 3, 
			_ => throw new Exception("Unknown rarity " + rarity), 
		};
	}

	public static int getBaseRarityWeight(int rarityCode)
	{
		return rarityCode switch
		{
			0 => 100000, 
			1 => 40000, 
			2 => 12000, 
			3 => 150, 
			_ => throw new Exception("unknown rarity code " + rarityCode), 
		};
	}

	public static int getTierRarityWeight(int modNativeTier, int itemTier)
	{
		int num = 100;
		if (itemTier < modNativeTier)
		{
			num /= (modNativeTier - itemTier) * 5;
		}
		return num;
	}

	private static int fuzzTier(int tier)
	{
		while (true)
		{
			int num = Stat.Random(1, 100);
			if (num <= 10 && tier > 1)
			{
				tier--;
				continue;
			}
			if (num > 20 || tier >= 8)
			{
				break;
			}
			tier++;
		}
		return tier;
	}

	public static int ApplyModifications(GameObject GO, GameObjectBlueprint Blueprint, int BonusModChance, int SetModNumber, string Context = null)
	{
		int num = 0;
		CheckInit();
		if (BonusModChance <= -999 && SetModNumber <= 0)
		{
			return num;
		}
		try
		{
			int num2 = 3;
			if (GO.HasIntProperty("BaseModChance"))
			{
				num2 = GO.GetIntProperty("BaseModChance");
			}
			else
			{
				string tag = GO.GetTag("BaseModChance");
				if (!tag.IsNullOrEmpty())
				{
					try
					{
						num2 = Convert.ToInt32(tag);
					}
					catch
					{
					}
				}
			}
			int num3 = num2 + BonusModChance;
			if (num3 <= 0 && (!XRLCore.Core.CheatMaxMod || BonusModChance <= -999) && SetModNumber <= 0)
			{
				return num;
			}
			string tag2 = Blueprint.GetTag("Mods");
			if (string.IsNullOrEmpty(tag2))
			{
				return num;
			}
			List<string> list = tag2.CachedCommaExpansion();
			int Tier = 0;
			modDist.Clear();
			modsEligible.Clear();
			rarityCodeWeights.Clear();
			if (XRLCore.Core.CheatMaxMod)
			{
				num3 = 100;
			}
			int num4 = 0;
			while (true)
			{
				if (num4 < 3)
				{
					bool flag = SetModNumber > 0;
					if (flag || num3.in100())
					{
						if (flag)
						{
							SetModNumber--;
						}
						if (Tier == 0)
						{
							Tier = 1;
							string tag3 = Blueprint.GetTag("TechTier");
							if (!tag3.IsNullOrEmpty())
							{
								Tier = Convert.ToInt32(tag3);
							}
							else
							{
								string tag4 = Blueprint.GetTag("Tier");
								if (!tag4.IsNullOrEmpty())
								{
									Tier = Convert.ToInt32(tag4);
								}
							}
							XRL.World.Capabilities.Tier.Constrain(ref Tier);
						}
						modDist.Clear();
						modsEligible.Clear();
						rarityCodeWeights.Clear();
						foreach (string item in list)
						{
							if (!_ModTable.TryGetValue(item, out var value))
							{
								continue;
							}
							foreach (ModEntry item2 in value)
							{
								if (Tier >= item2.MinTier && Tier <= item2.MaxTier && (flag || item2.CanAutoTinker || Context != "Tinkering") && !modsEligible.Contains(item2) && TechModding.ModificationApplicable(item2.Part, GO))
								{
									modsEligible.Add(item2);
								}
							}
						}
						foreach (ModEntry item3 in modsEligible)
						{
							if (!rarityCodeWeights.ContainsKey(item3.Rarity))
							{
								rarityCodeWeights.Add(item3.Rarity, 0);
							}
							rarityCodeWeights[item3.Rarity] += getTierRarityWeight(item3.NativeTier, Tier);
						}
						foreach (ModEntry item4 in modsEligible)
						{
							int baseWeight = getBaseRarityWeight(item4.Rarity) * getTierRarityWeight(item4.NativeTier, Tier) / rarityCodeWeights[item4.Rarity];
							baseWeight = GetModRarityWeightEvent.GetFor(GO, item4, baseWeight);
							if (baseWeight > 0)
							{
								modDist.Add(item4, baseWeight);
							}
						}
						if (modDist.Count <= 0)
						{
							break;
						}
						ModEntry randomElement = modDist.GetRandomElement();
						if (randomElement != null && randomElement.Part != null && !GO.HasPart(randomElement.Part) && TechModding.ApplyModification(GO, randomElement.Part, fuzzTier(Tier)))
						{
							num++;
						}
					}
					num4++;
					continue;
				}
				return num;
			}
			return num;
		}
		catch (Exception ex)
		{
			XRLCore.LogError(ex);
			MetricsManager.LogException("ApplyModification", ex);
			return num;
		}
		finally
		{
			modDist.Clear();
			modsEligible.Clear();
			rarityCodeWeights.Clear();
		}
	}
}

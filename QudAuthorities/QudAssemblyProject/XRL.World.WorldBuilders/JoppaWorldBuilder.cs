using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Annals;
using XRL.Core;
using XRL.EditorFormats.Map;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;

namespace XRL.World.WorldBuilders;

public class JoppaWorldBuilder : WorldBuilder
{
	public MutabilityMap mutableMap = new MutabilityMap();

	public Dictionary<Location2D, string> terrainTypes = new Dictionary<Location2D, string>();

	public Dictionary<Location2D, TerrainTravel> terrainComponents = new Dictionary<Location2D, TerrainTravel>();

	public WorldInfo worldInfo;

	public Zone WorldZone;

	public List<IJoppaWorldBuilderExtension> extensions = new List<IJoppaWorldBuilderExtension>();

	private string World = "JoppaWorld";

	public int[,] Lairs;

	public static uint ROAD_NORTH = 1u;

	public static uint ROAD_SOUTH = 2u;

	public static uint ROAD_EAST = 4u;

	public static uint ROAD_WEST = 8u;

	public static uint ROAD_NONE = 16u;

	public static uint ROAD_START = 32u;

	public uint RIVER_NORTH = 1u;

	public uint RIVER_SOUTH = 2u;

	public uint RIVER_EAST = 4u;

	public uint RIVER_WEST = 8u;

	public uint RIVER_NONE = 16u;

	public uint RIVER_START = 32u;

	public static readonly int TELEPORT_GATE_RUINS_SURFACE_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_RUINS_DEEP_PERMILLAGE_CHANCE = 1;

	public static readonly int TELEPORT_GATE_RUINS_DEPTH = 30;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_SURFACE_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_DEEP_PERMILLAGE_CHANCE = 1;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_DEPTH = 30;

	public static readonly int TELEPORT_GATE_SECRET_RUIN_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_SURFACE_PERMILLAGE_CHANCE = 20;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_DEEP_PERMILLAGE_CHANCE = 20;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_CHECK_DEPTH = 30;

	public static readonly int TELEPORT_GATE_RANDOM_PROPORTION = 10;

	public static readonly int TELEPORT_GATE_RANDOM_SURFACE_TARGET_PERCENTAGE_CHANCE = 40;

	public static readonly int TELEPORT_GATE_RANDOM_DEEP_TARGET_DEPTH = 40;

	public static readonly bool TELEPORT_GATE_DEBUG = false;

	public uint[,] RoadSystem
	{
		get
		{
			return worldInfo.RoadSystem;
		}
		set
		{
			worldInfo.RoadSystem = value;
		}
	}

	public uint[,] RiverSystem
	{
		get
		{
			return worldInfo.RiverSystem;
		}
		set
		{
			worldInfo.RiverSystem = value;
		}
	}

	public void BuildMazes()
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(240, 75, bShow: false, XRLCore.Core.Game.GetWorldSeed("CanyonMaze"));
		maze.SetBorder(Value: true);
		XRLCore.Core.Game.WorldMazes.Add("QudCanyonMaze", maze);
		Maze maze2 = RecursiveBacktrackerMaze.Generate(80, 25, bShow: false, XRLCore.Core.Game.GetWorldSeed("WaterwayMaze"));
		maze2.Cell[25, 3].N = true;
		maze2.Cell[25, 3].S = true;
		maze2.Cell[25, 3].E = false;
		maze2.Cell[25, 3].W = false;
		maze2.Cell[24, 3].E = false;
		maze2.Cell[26, 3].W = false;
		maze2.Cell[25, 2].S = true;
		maze2.Cell[25, 4].N = true;
		XRLCore.Core.Game.WorldMazes.Add("QudWaterwayMaze", maze2);
	}

	public bool WorldCellHasTerrain(string World, int x, int y, string[] T)
	{
		WorldZone = ZM.GetZone(World);
		GameObject firstObjectWithPart = WorldZone.GetCell(x, y).GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart != null)
		{
			GameObjectBlueprint blueprint = firstObjectWithPart.GetBlueprint();
			for (int i = 0; i < T.Length; i++)
			{
				if (blueprint.DescendsFrom(T[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void BuildMutableEncounters()
	{
		try
		{
			ZoneManager zoneManager = The.ZoneManager;
			History sultanHistory = The.Game.sultanHistory;
			MetricsManager.rngCheckpoint("init " + World);
			mutableMap = new MutabilityMap();
			mutableMap.Init(240, 75);
			terrainComponents = new Dictionary<Location2D, TerrainTravel>();
			terrainTypes = new Dictionary<Location2D, string>();
			Zone zone = zoneManager.GetZone(World);
			extensions.ForEach(delegate(IJoppaWorldBuilderExtension e)
			{
				e.OnBeforeMutableInit(this);
			});
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					Event.ResetPool();
					string zoneID = World + "." + i + "." + j + ".";
					List<CellBlueprint> cellBlueprints = zoneManager.GetCellBlueprints(zoneID);
					int value = 1;
					string text = "None";
					int num = 1;
					TerrainTravel terrainTravel = null;
					GameObject firstObjectWithPart = zone.GetCell(i, j).GetFirstObjectWithPart("TerrainTravel");
					if (firstObjectWithPart != null)
					{
						terrainTravel = firstObjectWithPart.GetPart("TerrainTravel") as TerrainTravel;
						text = firstObjectWithPart.GetTag("Terrain", firstObjectWithPart.Blueprint);
						num = int.Parse(firstObjectWithPart.GetTag("RegionTier", "1"));
						if (terrainTravel == null)
						{
							MetricsManager.LogError($"Terrain object {firstObjectWithPart.Blueprint} is missing the TerrainTravel part in world map cell [{i},{j}]");
						}
					}
					else
					{
						MetricsManager.LogError($"Missing Terrain object in world map cell [{i},{j}]");
						value = 0;
					}
					foreach (CellBlueprint item in cellBlueprints)
					{
						if (!item.Mutable)
						{
							value = 0;
						}
						else if (firstObjectWithPart != null && firstObjectWithPart.GetBlueprint().DescendsFrom("TerrainWater"))
						{
							value = 1;
						}
					}
					int key = num;
					terrainTypes.Add(Location2D.get(i, j), text);
					terrainComponents.Add(Location2D.get(i, j), terrainTravel);
					if (!worldInfo.terrainLocations.ContainsKey(text))
					{
						worldInfo.terrainLocations.Add(text, new List<Location2D>());
					}
					worldInfo.terrainLocations[text].Add(Location2D.get(i, j));
					if (!worldInfo.tierLocations.ContainsKey(key))
					{
						worldInfo.tierLocations.Add(key, new List<Location2D>());
					}
					worldInfo.tierLocations[key].Add(Location2D.get(i, j));
					for (int k = 0; k < 3; k++)
					{
						for (int l = 0; l < 3; l++)
						{
							int x = i * 3 + k;
							int y = j * 3 + l;
							mutableMap.AddMutableLocation(Location2D.get(x, y), text, value);
							mutableMap.SetMutable(Location2D.get(x, y), value);
						}
					}
				}
			}
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.1.0.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.1.0.11");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.0.0.11");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.0.0.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.1.2.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.2.2.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.2.0.10");
			extensions.ForEach(delegate(IJoppaWorldBuilderExtension e)
			{
				e.OnAfterMutableInit(this);
			});
			RiverSystem = new uint[240, 75];
			MetricsManager.rngCheckpoint("mamon");
			if (World == "JoppaWorld")
			{
				AddMamonVillage();
				AddYonderPath(World);
			}
			MetricsManager.rngCheckpoint("biomes");
			for (int m = 0; m < 80; m++)
			{
				if (m % 2 == 0)
				{
					WorldCreationProgress.StepProgress("Creating encounters");
				}
				for (int n = 0; n < 25; n++)
				{
					string text2 = World + "." + m + "." + n + ".";
					string a = terrainTypes[Location2D.get(m, n)];
					TerrainTravel terrainTravel2 = terrainComponents[Location2D.get(m, n)];
					for (int num2 = 0; num2 < 3; num2++)
					{
						for (int num3 = 0; num3 < 3; num3++)
						{
							string text3 = text2 + num2 + "." + num3 + ".10";
							if (Options.ShowOverlandRegions)
							{
								if (BiomeManager.Biomes["Slimy"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", m, n, BiomeManager.Biomes["Slimy"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Tarry"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", m, n, BiomeManager.Biomes["Tarry"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Rusty"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", m, n, BiomeManager.Biomes["Rusty"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Fungal"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", m, n, BiomeManager.Biomes["Fungal"].GetBiomeValue(text3).ToString());
								}
							}
							if (!(World == "JoppaWorld") || mutableMap.GetMutable(m * 3 + num2, n * 3 + num3) != 1)
							{
								continue;
							}
							if (3.in100())
							{
								if (5.in100())
								{
									zoneManager.AddZoneMidBuilder(text3, "OverlandRuins");
									zoneManager.AddZoneMidBuilder(text3, new ZoneBuilderBlueprint("InsertPresetFromPopulation", "Population", "CyberStations"));
									bool Proper;
									string nameRoot;
									string text4 = QudHistoryFactory.NameRuinsSite(sultanHistory, out Proper, out nameRoot);
									zoneManager.SetZoneName(text3, text4, null, null, null, null, Proper);
									string text5 = AddSecret(text3, text4, new string[3] { "ruins", "tech", "cybernetics" }, "Ruins with Becoming Nooks");
									base.game.SetStringGameState(text5 + "_NameRoot", nameRoot);
									The.ZoneManager.SetZoneProperty(text3, "TeleportGateCandidateNameRoot", nameRoot);
									terrainTravel2.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text3, "", text5, _Optional: true));
									AddLocationFinder(text3, text5, text4);
									if (Options.ShowOverlandEncounters)
									{
										zoneManager.GetZone("JoppaWorld").GetCell(m, n).GetObjectInCell(0)
											.pRender.RenderString = "#";
									}
									GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
									generatedLocationInfo.name = text4;
									generatedLocationInfo.targetZone = text3;
									generatedLocationInfo.zoneLocation = Location2D.get(m * 3 + num2, n * 3 + num3);
									generatedLocationInfo.secretID = text5;
									worldInfo.ruins.Add(generatedLocationInfo);
									mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
								}
								else
								{
									zoneManager.AddZoneMidBuilder(text3, "OverlandRuins");
									bool Proper2;
									string nameRoot2;
									string text6 = QudHistoryFactory.NameRuinsSite(sultanHistory, out Proper2, out nameRoot2);
									zoneManager.SetZoneName(text3, text6, null, null, null, null, Proper2);
									string text7 = AddSecret(text3, text6, new string[2] { "ruins", "tech" }, "Ruins");
									base.game.SetStringGameState(text7 + "_NameRoot", nameRoot2);
									The.ZoneManager.SetZoneProperty(text3, "TeleportGateCandidateNameRoot", nameRoot2);
									terrainTravel2.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text3, "", text7, _Optional: true));
									AddLocationFinder(text3, text7, text6);
									if (Options.ShowOverlandEncounters)
									{
										zoneManager.GetZone("JoppaWorld").GetCell(m, n).GetObjectInCell(0)
											.pRender.RenderString = "#";
									}
									GeneratedLocationInfo generatedLocationInfo2 = new GeneratedLocationInfo();
									generatedLocationInfo2.name = text6;
									generatedLocationInfo2.targetZone = text3;
									generatedLocationInfo2.zoneLocation = Location2D.get(m * 3 + num2, n * 3 + num3);
									generatedLocationInfo2.secretID = text7;
									worldInfo.ruins.Add(generatedLocationInfo2);
									mutableMap.SetMutable(generatedLocationInfo2.zoneLocation, 0);
								}
								continue;
							}
							if (string.Equals(a, "Jungle") && 3.in100())
							{
								zoneManager.AddZonePostBuilderAfterTerrain(text3, new ZoneBuilderBlueprint("GoatfolkYurts"));
								string text8 = SettlementNames.GenerateGoatfolkVillageName(sultanHistory) + ", goatfolk village";
								zoneManager.SetZoneName(text3, text8, null, null, null, null, Proper: true);
								string text9 = AddSecret(text3, text8, new string[3] { "settlement", "goatfolk", "humanoid" }, "Settlements");
								terrainTravel2.AddEncounter(new EncounterEntry("You smell roasted boar nearby. Would you like to investigate?", text3, "", text9, _Optional: true));
								AddLocationFinder(text3, text9, text8);
								if (Options.ShowOverlandEncounters)
								{
									zoneManager.GetZone("JoppaWorld").GetCell(m, n).GetObjectInCell(0)
										.pRender.RenderString = "Y";
								}
								GeneratedLocationInfo generatedLocationInfo3 = new GeneratedLocationInfo();
								generatedLocationInfo3.name = text8;
								generatedLocationInfo3.targetZone = text3;
								generatedLocationInfo3.zoneLocation = Zone.zoneIDTo240x72Location(text3);
								generatedLocationInfo3.secretID = text9;
								worldInfo.enemySettlements.Add(generatedLocationInfo3);
								mutableMap.SetMutable(generatedLocationInfo3.zoneLocation, 0);
							}
							if (string.Equals(a, "DeepJungle") && 15.in1000())
							{
								zoneManager.AddZonePostBuilderAfterTerrain(text3, new ZoneBuilderBlueprint("GoatfolkQlippothYurts"));
								string text10 = SettlementNames.GenerateGoatfolkQlippothVillageName(sultanHistory) + ", goatfolk haunt";
								zoneManager.SetZoneName(text3, text10, null, null, null, null, Proper: true);
								string text11 = AddSecret(text3, text10, new string[3] { "settlement", "goatfolk", "humanoid" }, "Settlements");
								terrainTravel2.AddEncounter(new EncounterEntry("You experience a sense memory of roasted boar smell. Would you like to investigate?", text3, "", text11, _Optional: true));
								AddLocationFinder(text3, text11, text10);
								if (Options.ShowOverlandEncounters)
								{
									zoneManager.GetZone("JoppaWorld").GetCell(m, n).GetObjectInCell(0)
										.pRender.RenderString = "Y";
								}
								GeneratedLocationInfo generatedLocationInfo4 = new GeneratedLocationInfo();
								generatedLocationInfo4.name = text10;
								generatedLocationInfo4.targetZone = text3;
								generatedLocationInfo4.zoneLocation = Zone.zoneIDTo240x72Location(text3);
								generatedLocationInfo4.secretID = text11;
								worldInfo.enemySettlements.Add(generatedLocationInfo4);
								mutableMap.SetMutable(generatedLocationInfo4.zoneLocation, 0);
							}
						}
					}
				}
			}
			MetricsManager.rngCheckpoint("paths");
			BuildStep("Placing canyons", BuildCanyonSystems);
			BuildStep("Placing rivers", BuildRiverSystems);
			BuildStep("Placing roads", BuildRoadSystems);
			MetricsManager.rngCheckpoint("forts");
			BuildStep("Placing forts", BuildForts);
			MetricsManager.rngCheckpoint("farms");
			BuildStep("Placing farms", BuildFarms);
			MetricsManager.rngCheckpoint("statics");
			BuildStep("Placing static encounters", AddStaticEncounters);
			BuildStep("Placing Bey Lah", AddHindrenVillage);
			BuildStep("Placing Hydropon", AddHydropon);
			MetricsManager.rngCheckpoint("oboroqoru");
			if (World == "JoppaWorld")
			{
				BuildStep("Placing Oboroqoru's lair", AddOboroqorusLair);
			}
			MetricsManager.rngCheckpoint("waterway");
			BuildStep("Placing waterways", AddWaterway);
			MetricsManager.rngCheckpoint("sultans");
			BuildStep("Placing historic sites", AddSultanHistoryLocations);
			BuildStep("Recording sultan aliases", RecordSultanAliases);
			BuildStep("Renaming sultan tombs", RenameSultanTombs);
			MetricsManager.rngCheckpoint("lairs");
			BuildStep("Placing lairs", BuildLairs);
			MetricsManager.rngCheckpoint("villages");
			BuildStep("Placing villages", AddVillages);
			MetricsManager.rngCheckpoint("secrets");
			BuildStep("Placing secrets", BuildSecrets);
			MetricsManager.rngCheckpoint("teleportgates");
			BuildStep("Placing teleport gates", BuildTeleportGates);
			MetricsManager.rngCheckpoint("quests");
			BuildStep("Generating dynamic quests", BuildDynamicQuests);
			MetricsManager.rngCheckpoint("clams");
			BuildStep("Placing clams", PlaceClams);
			MetricsManager.rngCheckpoint("heirlooms");
			BuildStep("Require faction heirlooms", Factions.RequireCachedHeirlooms);
			MetricsManager.rngCheckpoint("gossip");
			BuildStep("Initialize gossip", JournalAPI.InitializeGossip);
			BuildStep("Initialize observations", JournalAPI.InitializeObservations);
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("BuildMutableEncounters", x2);
		}
	}

	public void BuildStep(string Context, Action<string> Action)
	{
		BuildStep(Context, (Action)delegate
		{
			Action(World);
		});
	}

	public void BuildStep(string Context, Action Action)
	{
		MetricsManager.LogInfo(Context);
		try
		{
			Action();
		}
		catch (Exception x)
		{
			MetricsManager.LogException(Context, x);
		}
	}

	public Location2D getLocationWithinNFromTerrainType(int min, int max, string terrainType)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (Location2D item in worldInfo.terrainLocations[terrainType])
		{
			for (int i = item.x - max; i < item.x + max; i++)
			{
				for (int j = item.y - max; j < item.y + max; j++)
				{
					if (i > 0 && i < 80 && j > 0 && j < 25)
					{
						Location2D location2D = Location2D.get(i, j);
						int num = location2D.Distance(item);
						if (num >= min && num <= max && !list.Contains(location2D) && mutableMap.GetMutable(i * 3 + 1, j * 3 + 1) > 0)
						{
							list.Add(Location2D.get(i, j));
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Location2D randomElement = list.GetRandomElement();
		mutableMap.SetMutable(randomElement, 0);
		return randomElement;
	}

	public Location2D getLocationWithinNFromTerrainTypeTier(int min, int max, string terrainType, int tier)
	{
		List<Location2D> list = new List<Location2D>();
		using (List<Location2D>.Enumerator enumerator = worldInfo.terrainLocations[terrainType].Shuffle().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Location2D current = enumerator.Current;
				list.Clear();
				for (int i = current.x - max; i < current.x + max; i++)
				{
					for (int j = current.y - max; j < current.y + max; j++)
					{
						if (i > 0 && i < 80 && j > 0 && j < 25)
						{
							Location2D location2D = Location2D.get(i, j);
							int num = location2D.Distance(current);
							if (num >= min && num <= max && !list.Contains(location2D) && mutableMap.GetMutable(i * 3 + 1, j * 3 + 1) > 0 && worldInfo.tierLocations[tier].Contains(Location2D.get(i, j)))
							{
								list.Add(Location2D.get(i, j));
							}
						}
					}
				}
				if (list.Count == 0)
				{
					return null;
				}
				Location2D randomElement = list.GetRandomElement();
				mutableMap.SetMutable(Location2D.get(randomElement.x * 3 + 1, randomElement.y * 3 + 1), 0);
				return randomElement;
			}
		}
		return null;
	}

	public Location2D getLocationWithinNFromTerrainBlueprintTier(int min, int max, string terrainBlueprint, int tier)
	{
		List<Location2D> list = new List<Location2D>();
		Location2D location = base.game.ZoneManager.GetZone("JoppaWorld").GetFirstObject((GameObject o) => o.Blueprint == terrainBlueprint).GetCurrentCell()
			.location;
		list.Clear();
		for (int i = location.x - max; i < location.x + max; i++)
		{
			for (int j = location.y - max; j < location.y + max; j++)
			{
				if (i > 0 && i < 80 && j > 0 && j < 25)
				{
					Location2D location2D = Location2D.get(i, j);
					int num = location2D.Distance(location);
					if (num >= min && num <= max && !list.Contains(location2D) && mutableMap.GetMutable(i * 3 + 1, j * 3 + 1) > 0 && worldInfo.tierLocations[tier].Contains(Location2D.get(i, j)))
					{
						list.Add(Location2D.get(i, j));
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Location2D randomElement = list.GetRandomElement();
		mutableMap.SetMutable(Location2D.get(randomElement.x * 3 + 1, randomElement.y * 3 + 1), 0);
		return randomElement;
	}

	public string GetZoneIdOfTerrain(string Terrain, string z = "10")
	{
		Location2D randomElement = worldInfo.terrainLocations[Terrain].GetRandomElement();
		return "JoppaWorld." + randomElement.x + "." + randomElement.y + "." + Stat.Random(0, 2) + "." + Stat.Random(0, 2) + "." + z;
	}

	public Location2D popMutableBlockOfTerrain(string Terrain)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			bool flag = true;
			int num = 0;
			while (true)
			{
				if (num <= 2)
				{
					int num2 = 0;
					while (num2 <= 2)
					{
						if (mutableMap.GetMutable(item.x * 3 + num, item.y * 3 + num2) > 0)
						{
							num2++;
							continue;
						}
						goto IL_0066;
					}
					num++;
					continue;
				}
				if (!flag)
				{
					break;
				}
				for (int i = 0; i <= 2; i++)
				{
					for (int j = 0; j <= 2; j++)
					{
						mutableMap.RemoveMutableLocation(Location2D.get(item.x * 3 + i, item.y * 3 + j));
					}
				}
				return Location2D.get(item.x * 3 + 1, item.y * 3 + 1);
				IL_0066:
				flag = false;
				break;
			}
		}
		return null;
	}

	/// <summary>
	///             Finds a radius x radius square of terrain and returns the center zone
	///             </summary><param name="Terrain" /><param name="where" /><param name="radius" /><returns />
	public Location2D popMutableLocationBlockOfTerrain(string Terrain, Predicate<Location2D> where = null, int radius = 1)
	{
		List<Location2D> list = new List<Location2D>();
		if (!worldInfo.terrainLocations.ContainsKey(Terrain))
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			int num = -radius;
			while (true)
			{
				if (num <= radius)
				{
					for (int i = -radius; i <= radius; i++)
					{
						if (mutableMap.GetMutable(item.x * 3 + num, item.y * 3 + i) <= 0)
						{
							goto end_IL_00c7;
						}
						Location2D location2D = Location2D.get(item.x * 3 + num, item.y * 3 + i);
						if (where != null && !where(location2D))
						{
							goto end_IL_00c7;
						}
						list.Add(location2D);
					}
					num++;
					continue;
				}
				if (list.Count != 9)
				{
					break;
				}
				Location2D result = list[4];
				list.ForEach(delegate(Location2D r)
				{
					mutableMap.RemoveMutableLocation(r);
				});
				return result;
				continue;
				end_IL_00c7:
				break;
			}
		}
		return null;
	}

	public IEnumerable<Location2D> YieldBlocksWithin(string Terrain, Box Box = null, bool Mutable = true)
	{
		if (worldInfo.terrainLocations.TryGetValue(Terrain, out var value))
		{
			value = value.Shuffle();
			foreach (Location2D item in value)
			{
				if ((Box == null || Box.contains(item)) && (!Mutable || mutableMap.GetWorldBlockMutable(item)))
				{
					yield return item;
				}
			}
		}
		else
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
	}

	public Location2D popMutableLocationOfTerrain(string Terrain, Predicate<Location2D> where = null, bool centerOnly = true)
	{
		List<Location2D> list = new List<Location2D>();
		if (!worldInfo.terrainLocations.ContainsKey(Terrain))
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			for (int i = 0; i <= 2; i++)
			{
				for (int j = 0; j <= 2; j++)
				{
					if ((!centerOnly || (i == 1 && j == 1)) && mutableMap.GetMutable(item.x * 3 + i, item.y * 3 + j) > 0)
					{
						Location2D location2D = Location2D.get(item.x * 3 + i, item.y * 3 + j);
						if (where == null || where(location2D))
						{
							list.Add(location2D);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				Location2D randomElement = list.GetRandomElement();
				mutableMap.RemoveMutableLocation(randomElement);
				return randomElement;
			}
		}
		return null;
	}

	public Location2D getLocationOfTier(int tier)
	{
		int num = 0;
		List<Location2D> value;
		while (!worldInfo.tierLocations.TryGetValue(tier, out value))
		{
			Debug.LogWarning("Couldn't find location of tier " + tier);
			tier--;
			if (tier < 1)
			{
				tier = 8;
			}
			num++;
			if (num > 9)
			{
				return null;
			}
		}
		foreach (Location2D item in value.Shuffle())
		{
			if (mutableMap.GetMutable(item.x * 3 + 1, item.y * 3 + 1) > 0)
			{
				mutableMap.SetMutable(Location2D.get(item.x * 3 + 1, item.y * 3 + 1), 0);
				return item;
			}
		}
		return null;
	}

	public Location2D getLocationOfTier(int minTier, int maxTier)
	{
		List<int> list = new List<int>();
		for (int i = minTier; i <= maxTier; i++)
		{
			list.Add(i);
		}
		list.ShuffleInPlace();
		foreach (int item in list)
		{
			if (!worldInfo.tierLocations.ContainsKey(item))
			{
				continue;
			}
			foreach (Location2D item2 in worldInfo.tierLocations[item].Shuffle())
			{
				if (mutableMap.GetMutable(item2.x * 3 + 1, item2.y * 3 + 1) > 0)
				{
					mutableMap.SetMutable(Location2D.get(item2.x * 3 + 1, item2.y * 3 + 1), 0);
					return item2;
				}
			}
		}
		return null;
	}

	public void BuildDynamicQuests(string WorldID)
	{
		foreach (HistoricEntity entitiesWherePropertyEqual in XRLCore.Core.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "village"))
		{
			int num = 1;
			HistoricEntitySnapshot currentSnapshot = entitiesWherePropertyEqual.GetCurrentSnapshot();
			if (currentSnapshot.hasProperty("isVillageZero"))
			{
				num = 2;
			}
			VillageDynamicQuestContext villageDynamicQuestContext = new VillageDynamicQuestContext(currentSnapshot);
			for (int i = 0; i < num; i++)
			{
				villageDynamicQuestContext.questNumber = i;
				string populationName = "Dynamic Village Quests";
				try
				{
					DynamicQuestFactory.fabricateQuestTemplate(PopulationManager.RollOneFrom(populationName).Blueprint, villageDynamicQuestContext);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("DynamicVillageQuestFab", x);
				}
			}
		}
	}

	public void AddVillages()
	{
		History sultanHistory = XRLCore.Core.Game.sultanHistory;
		HistoricEntityList entitiesWherePropertyEquals = sultanHistory.GetEntitiesWherePropertyEquals("type", "village");
		bool flag = false;
		int num = 0;
		foreach (HistoricEntity item in entitiesWherePropertyEquals)
		{
			HistoricEntitySnapshot currentSnapshot = item.GetCurrentSnapshot();
			string property = currentSnapshot.GetProperty("name");
			string property2 = currentSnapshot.GetProperty("region");
			bool flag2 = false;
			if (currentSnapshot.GetProperty("villageZero", "false") == "true")
			{
				if (flag)
				{
					sultanHistory.entities.Remove(item);
					continue;
				}
				if (XRLCore.Core.Game.GetStringGameState("VillageZeroRegion", "&YJoppa") == property2)
				{
					flag2 = true;
					flag = true;
				}
				if (!flag2)
				{
					sultanHistory.entities.Remove(item);
					continue;
				}
			}
			int num2;
			int num3;
			if (flag2)
			{
				num2 = 1;
				num3 = 1;
			}
			else
			{
				try
				{
					num2 = currentSnapshot.Tier;
				}
				catch
				{
					num2 = Tier.Constrain(num);
				}
				try
				{
					num3 = currentSnapshot.TechTier;
				}
				catch
				{
					num3 = Tier.Constrain(num);
				}
			}
			Location2D location2D = null;
			if (flag2)
			{
				Box box = new Box(0, 15, 26, 25);
				location2D = YieldBlocksWithin(property2, box).FirstOrDefault();
			}
			if (location2D == null)
			{
				location2D = YieldBlocksWithin(property2).FirstOrDefault();
			}
			if (location2D == null)
			{
				MetricsManager.LogError($"Unable to find map cell for village in {property2} (V0: {flag2})");
				sultanHistory.entities.Remove(item);
				continue;
			}
			mutableMap.SetWorldBlockMutable(location2D, 0);
			Location2D location2D2 = Location2D.get(location2D.x * 3 + 1, location2D.y * 3 + 1);
			GameObject firstObjectWithPart = WorldZone.GetCell(location2D.x, location2D.y).GetFirstObjectWithPart("TerrainTravel");
			VillageTerrain villageTerrain = new VillageTerrain(item);
			firstObjectWithPart.AddPart(villageTerrain);
			GameObject gameObject = GameObject.create("VillageSurface");
			VillageSurface villageSurface = gameObject.GetPart("VillageSurface") as VillageSurface;
			villageSurface.VillageName = property;
			villageSurface.RevealKey = "villageReveal_" + property;
			villageSurface.RevealLocation = new Vector2i(location2D.x, location2D.y);
			if (flag2)
			{
				villageSurface.IsVillageZero = true;
			}
			if (currentSnapshot.GetProperty("abandoned") == "true")
			{
				villageSurface.RevealString = "You discover the abandoned village of " + property + ".";
			}
			else
			{
				villageSurface.RevealString = "You discover the village of " + property + ".";
			}
			string text = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1.10";
			if (flag2)
			{
				base.game.SetStringGameState("villageZeroStartingLocation", text + "@37,22");
				item.SetPropertyAtCurrentYear("isVillageZero", "true");
			}
			item.SetPropertyAtCurrentYear("zoneID", text);
			currentSnapshot.setProperty("zoneID", text);
			if (currentSnapshot.hasProperty("worships_creature"))
			{
				Location2D villageLocation2 = Zone.zoneIDTo240x72Location(text);
				GeneratedLocationInfo randomElement = worldInfo.lairs.Where((GeneratedLocationInfo l) => villageLocation2.ManhattanDistance(l.zoneLocation) >= 0 && villageLocation2.ManhattanDistance(l.zoneLocation) <= 18).GetRandomElement();
				if (randomElement == null)
				{
					randomElement = worldInfo.lairs.GetRandomElement();
				}
				GameObject cachedObjects = XRLCore.Core.Game.ZoneManager.GetCachedObjects(randomElement.ownerID);
				Worships.PostProcessEvent(item, cachedObjects.DisplayNameOnlyDirectAndStripped, cachedObjects.id);
			}
			if (currentSnapshot.hasProperty("despises_creature"))
			{
				Location2D villageLocation = Zone.zoneIDTo240x72Location(text);
				GeneratedLocationInfo randomElement2 = worldInfo.lairs.Where((GeneratedLocationInfo l) => villageLocation.ManhattanDistance(l.zoneLocation) >= 0 && villageLocation.ManhattanDistance(l.zoneLocation) <= 18).GetRandomElement();
				if (randomElement2 == null)
				{
					randomElement2 = worldInfo.lairs.GetRandomElement();
				}
				GameObject cachedObjects2 = XRLCore.Core.Game.ZoneManager.GetCachedObjects(randomElement2.ownerID);
				Despises.PostProcessEvent(item, cachedObjects2.DisplayNameOnlyDirectAndStripped, cachedObjects2.id);
			}
			TerrainTravel terrainTravel = WorldZone.GetCell(location2D.x, location2D.y).GetFirstObjectWithPart("TerrainTravel").GetPart("TerrainTravel") as TerrainTravel;
			Faction faction = new Faction();
			faction.Old = false;
			faction.ExtradimensionalVersions = false;
			faction.Visible = ((!string.Equals(currentSnapshot.GetProperty("abandoned"), "true", StringComparison.CurrentCultureIgnoreCase)) ? true : false);
			faction.Name = "villagers of " + property;
			if (currentSnapshot.GetProperty("newFactionName") != "unknown")
			{
				faction.DisplayName = currentSnapshot.GetProperty("newFactionName");
			}
			else
			{
				faction.DisplayName = "villagers of " + property;
				faction.FormatWithArticle = true;
			}
			faction.setFactionFeeling("Wardens", 100);
			if (currentSnapshot.hasProperty("highlyEntropicBeingWorshipAttitude"))
			{
				faction.HighlyEntropicBeingWorshipAttitude = Convert.ToInt32(currentSnapshot.GetProperty("highlyEntropicBeingWorshipAttitude"));
			}
			if (currentSnapshot.hasProperty("signatureLiquid"))
			{
				if (50.in100())
				{
					faction.WaterRitualLiquid = currentSnapshot.GetProperty("signatureLiquid");
				}
			}
			else if (currentSnapshot.listProperties.ContainsKey("signatureLiquids") && 50.in100())
			{
				faction.WaterRitualLiquid = currentSnapshot.GetList("signatureLiquids").GetRandomElement();
			}
			Factions.AddNewFaction(faction);
			Factions.get("Wardens").setFactionFeeling(faction.Name, 100);
			string text2 = AddSecret(text, property, new string[3] { "settlement", "villages", "humanoid" }, "Settlements", null, flag2, flag2);
			JournalAPI.GetMapNote(text2).secretSold = flag2;
			JournalAPI.GetMapNote(text2).attributes.Add("nobuy:" + faction.Name);
			JournalAPI.GetMapNote(text2).attributes.Add("nosell:" + faction.Name);
			JournalMapNote mapNote = JournalAPI.GetMapNote(text2);
			mapNote.history = mapNote.history + " {{K|-learned from " + Faction.getFormattedName(faction.Name) + "}}";
			if (currentSnapshot.GetProperty("abandoned") == "true")
			{
				terrainTravel.AddEncounter(new EncounterEntry("You discover an abandoned village. Would you like to investigate?", text, "", text2, _Optional: true));
			}
			else
			{
				terrainTravel.AddEncounter(new EncounterEntry("You discover a village. Would you like to investigate?", text, "", text2, _Optional: true));
			}
			villageSurface.RevealSecret = text2;
			villageTerrain.secretId = text2;
			ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("Village", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
			ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
			ZM.SetZoneName(text, property, null, null, null, null, Proper: true);
			ZM.SetZoneIncludeStratumInZoneDisplay(text, false);
			string[] directionList = Directions.DirectionList;
			foreach (string d in directionList)
			{
				Location2D location2D3 = location2D2.FromDirection(d);
				string zoneID = Zone.XYToID("JoppaWorld", location2D3.x, location2D3.y, 10);
				ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("VillageOutskirts", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
				ZM.SetZoneProperty(zoneID, "NoBiomes", "Yes");
				ZM.SetZoneName(zoneID, "outskirts", property, "some");
				ZM.SetZoneIncludeStratumInZoneDisplay(zoneID, false);
				string zoneID2 = Zone.XYToID("JoppaWorld", location2D3.x, location2D3.y, 9);
				ZM.AddZonePostBuilderAfterTerrain(zoneID2, new ZoneBuilderBlueprint("VillageOver", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
				ZM.SetZoneName(zoneID2, "sky", property, null, null, "the");
				string zoneID3 = Zone.XYToID("JoppaWorld", location2D3.x, location2D3.y, 11);
				ZM.AddZonePostBuilderAfterTerrain(zoneID3, new ZoneBuilderBlueprint("VillageUnder", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
			}
			for (int num4 = 9; num4 >= 0; num4--)
			{
				string zoneID4 = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1." + num4;
				ZM.AddZonePostBuilderAfterTerrain(zoneID4, new ZoneBuilderBlueprint("VillageOver", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
				ZM.SetZoneName(zoneID4, "sky", property, null, null, "the");
			}
			string zoneID5 = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1.11";
			ZM.AddZonePostBuilderAfterTerrain(zoneID5, new ZoneBuilderBlueprint("VillageUnder", "villageEntity", item, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name));
			ZM.SetZoneName(zoneID5, "undervillage", property, null, null, "the");
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
			generatedLocationInfo.name = property;
			generatedLocationInfo.targetZone = text;
			generatedLocationInfo.zoneLocation = location2D2;
			generatedLocationInfo.secretID = text2;
			worldInfo.villages.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
			num++;
		}
		JournalAPI.InitializeVillageEntries();
	}

	public void AddSultanHistoryLocations()
	{
		History sultanHistory = The.Game.sultanHistory;
		JournalAPI.InitializeSultanEntries();
		HistoricEntityList entitiesWherePropertyEquals = sultanHistory.GetEntitiesWherePropertyEquals("type", "region");
		HistoricEntityList entitiesWherePropertyEquals2 = sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
		for (int i = 0; i < 8; i++)
		{
			int num = i + 1;
			int tier = num;
			bool flag = false;
			Location2D location2D = ((i == 0) ? getLocationWithinNFromTerrainBlueprintTier(3, 8, "TerrainJoppa", tier) : ((i >= 2) ? getLocationOfTier(tier) : getLocationWithinNFromTerrainBlueprintTier(1, 29, "TerrainJoppa", tier)));
			int num2 = 0;
			if (i == 0)
			{
				num2 = 5;
			}
			if (i == 1)
			{
				num2 = 5;
			}
			if (i == 2)
			{
				num2 = 4;
			}
			if (i == 3)
			{
				num2 = 4;
			}
			if (i == 4)
			{
				num2 = 3;
			}
			if (i == 5)
			{
				num2 = 3;
			}
			if (i == 6)
			{
				num2 = 2;
			}
			if (i == 7)
			{
				num2 = 1;
			}
			HistoricEntity historicEntity = null;
			HistoricEntitySnapshot historicEntitySnapshot = null;
			for (int j = 0; j < entitiesWherePropertyEquals.entities.Count; j++)
			{
				HistoricEntitySnapshot currentSnapshot = entitiesWherePropertyEquals.entities[j].GetCurrentSnapshot();
				if (Convert.ToInt32(currentSnapshot.GetProperty("period", "-1")) == num2 && currentSnapshot.hasListProperty("items"))
				{
					historicEntity = entitiesWherePropertyEquals.entities[j];
					historicEntitySnapshot = currentSnapshot;
					entitiesWherePropertyEquals.entities.RemoveAt(j);
					break;
				}
			}
			if (historicEntity == null)
			{
				for (int k = 0; k < entitiesWherePropertyEquals.entities.Count; k++)
				{
					HistoricEntitySnapshot regionSnap = entitiesWherePropertyEquals.entities[k].GetCurrentSnapshot();
					if (Convert.ToInt32(regionSnap.GetProperty("period", "-1")) != num2)
					{
						continue;
					}
					bool flag2 = false;
					for (int l = 0; l < entitiesWherePropertyEquals2.entities.Count; l++)
					{
						if (entitiesWherePropertyEquals2.entities[l].GetRandomEventWhereDelegate((HistoricEvent ev) => ev.hasEventProperty("revealsRegion") && ev.getEventProperty("revealsRegion") != null && ev.getEventProperty("revealsRegion") == regionSnap.GetProperty("newName"), Stat.Rnd) != null)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						historicEntity = entitiesWherePropertyEquals.entities[k];
						historicEntitySnapshot = regionSnap;
						entitiesWherePropertyEquals.entities.RemoveAt(k);
						break;
					}
				}
			}
			if (historicEntity == null)
			{
				int index = Stat.Random(0, entitiesWherePropertyEquals.entities.Count - 1);
				historicEntity = entitiesWherePropertyEquals.entities[index];
				historicEntitySnapshot = historicEntity.GetCurrentSnapshot();
				entitiesWherePropertyEquals.entities.RemoveAt(index);
			}
			int num3 = 1;
			switch (num)
			{
			case 0:
				num3 += 4;
				break;
			case 1:
				num3 += 4;
				break;
			case 2:
				num3 += Stat.Random(4, 5);
				break;
			case 3:
				num3 += Stat.Random(4, 5);
				break;
			case 4:
				num3 += Stat.Random(4, 5);
				break;
			case 5:
				num3 += Stat.Random(4, 5);
				break;
			case 6:
				num3 += Stat.Random(4, 6);
				break;
			case 7:
				num3 += Stat.Random(5, 6);
				break;
			case 8:
				num3 += Stat.Random(5, 7);
				break;
			}
			string property = historicEntitySnapshot.GetProperty("newName");
			XRLCore.Core.Game.SetStringGameState("SultanDungeonPlacementOrder_" + i, property);
			XRLCore.Core.Game.SetStringGameState("SultanDungeonPlaced_" + property, i.ToString());
			XRLCore.Core.Game.SetObjectGameState("sultanRegionPosition_" + property, location2D.vector2i);
			XRLCore.Core.Game.SetObjectGameState("sultanRegionPosition_" + historicEntitySnapshot.GetProperty("name"), location2D.vector2i);
			SultanRegion sultanRegion = WorldZone.GetCell(location2D.x, location2D.y).GetFirstObjectWithPart("TerrainTravel").AddPart(new SultanRegion(historicEntity));
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("SultanRegionSurface");
			SultanRegionSurface part = gameObject.GetPart<SultanRegionSurface>();
			part.RegionName = property;
			part.RevealKey = "sultanRegionReveal_" + property;
			part.RevealLocation = new Vector2i(location2D.x, location2D.y);
			part.RevealString = "You discover " + property + ".";
			string text = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1.10";
			TerrainTravel part2 = WorldZone.GetCell(location2D.x, location2D.y).GetFirstObjectWithPart("TerrainTravel").GetPart<TerrainTravel>();
			string text2 = AddSecret(text, property, new string[3] { "historic", "tech", "ruins" }, "Historic Sites");
			string property2 = historicEntitySnapshot.GetProperty("nameRoot");
			base.game.SetStringGameState(text2 + "_NameRoot", property2);
			The.ZoneManager.SetZoneProperty(text, "TeleportGateCandidateNameRoot", property2);
			if (i == 0)
			{
				JournalAPI.GetMapNote(text2).attributes.Add("nobuy:Joppa");
				JournalMapNote mapNote = JournalAPI.GetMapNote(text2);
				mapNote.history = mapNote.history + " {{K|-learned from " + Faction.getFormattedName("Joppa") + "}}";
			}
			part2.AddEncounter(new EncounterEntry("You discover some historic ruins. Would you like to investigate?", text, "", text2, _Optional: true));
			part.RevealSecret = text2;
			sultanRegion.secretId = text2;
			HistoricEntitySnapshot currentSnapshot2 = sultanHistory.GetEntitiesWherePropertyEquals("newName", property).entities[0].GetCurrentSnapshot();
			List<string> locationsInRegion = QudHistoryHelpers.GetLocationsInRegion(sultanHistory, currentSnapshot2.GetProperty("name"));
			if (num3 < locationsInRegion.Count)
			{
				num3 = locationsInRegion.Count;
			}
			SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
			if (num2 > 0)
			{
				HistoricEntity randomElement = entitiesWherePropertyEquals2.GetEntitiesWherePropertyEquals("period", num2.ToString()).GetRandomElement();
				if (randomElement == null)
				{
					randomElement = entitiesWherePropertyEquals2.GetRandomElement();
				}
				if (randomElement != null)
				{
					sultanDungeonArgs.UpdateFromEntity(randomElement.GetCurrentSnapshot());
				}
			}
			sultanDungeonArgs.UpdateWalls(num2);
			sultanDungeonArgs.UpdateFromEntity(currentSnapshot2);
			if (50.in100())
			{
				sultanDungeonArgs.wallTypes.Add("*SultanWall*");
			}
			Faction faction = Factions.get("SultanCult" + sultanDungeonArgs.cultPeriod);
			XRLCore.Core.Game.SetObjectGameState("sultanDungeonArgs_" + property, sultanDungeonArgs);
			List<string> list = new List<string>();
			for (int m = 0; m < locationsInRegion.Count; m++)
			{
				list.Add(locationsInRegion[m]);
			}
			for (int n = 0; n < num3 - locationsInRegion.Count; n++)
			{
				list.Add(null);
			}
			Algorithms.RandomShuffle(list);
			if (list[num3 - 1] == null)
			{
				for (int num4 = 0; num4 < num3; num4++)
				{
					if (list[num4] != null)
					{
						list[num3 - 1] = list[num4];
						list[num4] = null;
						break;
					}
				}
			}
			for (int num5 = 0; num5 < num3; num5++)
			{
				bool flag3 = false;
				string text3 = list[num5];
				string text4 = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1." + (num5 + 10);
				if (num5 > 0)
				{
					faction.HolyPlaces.Add(text4);
				}
				if (text3 == null)
				{
					flag3 = false;
					string text5 = Grammar.MakeTitleCase(property);
					if (num5 == 0)
					{
						ZM.SetZoneName(text4, text5, null, null, null, null, Proper: true);
					}
					else
					{
						ZM.SetZoneName(text4, "liminal floor", text5);
					}
					text3 = ((locationsInRegion.Count <= 0) ? sultanHistory.GetEntitiesWherePropertyEquals("type", "location").entities.GetRandomElement().GetCurrentSnapshot().GetProperty("name") : locationsInRegion.GetRandomElement());
				}
				else
				{
					flag3 = true;
					string context = Grammar.MakeTitleCase(property);
					ZM.SetZoneName(text4, Grammar.MakeTitleCase(text3), context, null, null, null, Proper: true);
				}
				ZM.SetZoneProperty(text4, "HistoricSite", property);
				The.ZoneManager.SetZoneProperty(text4, "TeleportGateCandidateNameRoot", property2);
				HistoricEntitySnapshot currentSnapshot3 = sultanHistory.GetEntitiesWherePropertyEquals("name", text3).entities[0].GetCurrentSnapshot();
				string text6 = "";
				if (num5 < num3 - 1)
				{
					text6 += "D";
				}
				if (num5 > 0)
				{
					text6 += "U";
				}
				if (num5 != 0)
				{
					ZM.ClearZoneBuilders(text4);
				}
				if (num5 != 0)
				{
					ZM.SetZoneProperty(text4, "SkipTerrainBuilders", true);
				}
				ZM.AddZonePostBuilderAfterTerrain(text4, new ZoneBuilderBlueprint("SultanDungeon", "locationName", text3, "regionName", property, "stairs", text6));
				ZM.AddZonePostBuilderAfterTerrain(text4, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject, cacheTwiceOk: true)));
				ZM.AddZonePostBuilderAfterTerrain(text4, new ZoneBuilderBlueprint("Music", "Track", "ofChromeAndHow"));
				ZM.SetZoneProperty(text4, "ZoneTierOverride", num.ToString());
				if (!flag3)
				{
					continue;
				}
				if (currentSnapshot3.listProperties.ContainsKey("items"))
				{
					foreach (string item in currentSnapshot3.listProperties["items"])
					{
						HistoricEntityList entitiesWherePropertyEquals3 = sultanHistory.GetEntitiesWherePropertyEquals("name", item);
						if (entitiesWherePropertyEquals3.Count > 0)
						{
							GameObject gameObject2 = RelicGenerator.GenerateRelic(entitiesWherePropertyEquals3.entities[0].GetCurrentSnapshot(), tier);
							gameObject2.AddPart(new TakenAchievement
							{
								AchievementID = "ACH_RECOVER_RELIC"
							});
							ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint("PlaceRelicBuilder", "Relic", ZM.CacheObject(gameObject2));
							ZM.SetZoneProperty(text4, "Relicstyle", "Vault");
							ZM.AddZonePostBuilderAfterTerrain(text4, zoneBuilderBlueprint);
							if (num5 == num3 - 1)
							{
								flag = true;
							}
							else
							{
								zoneBuilderBlueprint.AddParameter("AddCreditWedges", false);
							}
						}
						else
						{
							Debug.LogError("Unknown relic: " + item);
						}
					}
				}
				else if (3.in1000() && num5 != num3 - 1)
				{
					GameObject gameObject3 = RelicGenerator.GenerateRelic(currentSnapshot3, tier);
					gameObject3.AddPart(new TakenAchievement
					{
						AchievementID = "ACH_RECOVER_RELIC"
					});
					ZM.AddZonePostBuilderAfterTerrain(text4, new ZoneBuilderBlueprint("PlaceRelicBuilder", "Relic", ZM.CacheObject(gameObject3), "AddCreditWedges", false));
				}
			}
			if (!flag)
			{
				string zoneID = "JoppaWorld." + location2D.x + "." + location2D.y + ".1.1." + (10 + num3 - 1);
				GameObject gameObject4 = RelicGenerator.GenerateRelic(currentSnapshot2, tier, null, randomName: true);
				gameObject4.AddPart(new TakenAchievement
				{
					AchievementID = "ACH_RECOVER_RELIC"
				});
				ZM.SetZoneProperty(zoneID, "Relicstyle", "Vault");
				ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("PlaceRelicBuilder", "Relic", ZM.CacheObject(gameObject4)));
			}
			mutableMap.SetMutable(Location2D.get(location2D.x * 3 + 1, location2D.y * 3 + 1), 0);
			for (int num6 = 0; num6 < entitiesWherePropertyEquals.entities.Count; num6++)
			{
				HistoricEntitySnapshot currentSnapshot4 = entitiesWherePropertyEquals.entities[num6].GetCurrentSnapshot();
				if (Convert.ToInt32(currentSnapshot4.GetProperty("period", "-1")) == num2 && i != 0 && currentSnapshot4.hasListProperty("items"))
				{
					i--;
					break;
				}
			}
		}
	}

	public void RecordSultanAliases()
	{
		foreach (HistoricEntity entitiesWherePropertyEqual in The.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan"))
		{
			string property = entitiesWherePropertyEqual.GetCurrentSnapshot().GetProperty("period");
			base.game.SetStringGameState("*Sultan" + property + "Name*", entitiesWherePropertyEqual.GetCurrentSnapshot().GetProperty("name"));
		}
	}

	public void RenameSultanTombs()
	{
		int num = 5;
		int num2 = 1;
		while (num >= 1)
		{
			for (int i = 0; i <= 2; i++)
			{
				for (int j = 0; j <= 2; j++)
				{
					string zoneID = ZoneID.Assemble("JoppaWorld", 53, 3, i, j, num);
					The.ZoneManager.SetZoneBaseDisplayName(zoneID, The.ZoneManager.GetZoneBaseDisplayName(zoneID).Replace("*Sultan" + num2 + "Name*", base.game.GetStringGameState("*Sultan" + num2 + "Name*")));
				}
			}
			num--;
			num2++;
		}
	}

	public void AddMutableEncounterToTerrainRect(string Terrain, int n, Action<string, Location2D, TerrainTravel> encounter, bool unflagAsMutable = true)
	{
		for (int i = 0; i < n; i++)
		{
			Location2D mutableLocationWithTerrain = mutableMap.GetMutableLocationWithTerrain(Terrain);
			if (mutableLocationWithTerrain != null)
			{
				int x = mutableLocationWithTerrain.x;
				int y = mutableLocationWithTerrain.y;
				int x2 = mutableLocationWithTerrain.x / 3;
				int y2 = mutableLocationWithTerrain.y / 3;
				string arg = Zone.XYToID("JoppaWorld", x, y, 10);
				encounter(arg, mutableLocationWithTerrain, terrainComponents[Location2D.get(x2, y2)]);
				if (unflagAsMutable)
				{
					mutableMap.RemoveMutableLocation(mutableLocationWithTerrain);
				}
			}
		}
	}

	public void AddMutableEncounterToTerrain(string Terrain, int n, Action<string, Location2D, TerrainTravel> encounter, bool unflagAsMutable = true)
	{
		for (int i = 0; i < n; i++)
		{
			Location2D mutableLocationWithTerrain = mutableMap.GetMutableLocationWithTerrain(Terrain);
			if (mutableLocationWithTerrain != null)
			{
				int x = mutableLocationWithTerrain.x;
				int y = mutableLocationWithTerrain.y;
				int x2 = mutableLocationWithTerrain.x / 3;
				int y2 = mutableLocationWithTerrain.y / 3;
				string arg = Zone.XYToID("JoppaWorld", x, y, 10);
				encounter(arg, mutableLocationWithTerrain, terrainComponents[Location2D.get(x2, y2)]);
				if (unflagAsMutable)
				{
					mutableMap.RemoveMutableLocation(mutableLocationWithTerrain);
				}
			}
		}
	}

	public void BuildForts(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		ZoneManager ZM = XRLCore.Core.Game.ZoneManager;
		WorldCreationProgress.StepProgress("Generating forts");
		AddMutableEncounterToTerrain("DesertCanyon", 1, delegate(string zoneID, Location2D location, TerrainTravel pTravel)
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.pRender.RenderString = "]";
				pTravel.ParentObject.pRender.SetForegroundColor('m');
			}
			GameObject gameObject = GameObject.create("Snapjaw Hero Stopsvaalinn");
			gameObject.SetIntProperty("RequireVillagePlacement", 1);
			ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddObjectBuilder", "Object", ZM.CacheObject(gameObject)));
			string secretID4 = AddSecret(zoneID, "the snapjaw who wields {{R-r-K-y-Y sequence|Stopsvalinn}}", new string[4] { "artifact", "tech", "stopsvalinn", "old" }, "Artifacts");
			AddLocationFinder(zoneID, secretID4, "the snapjaw who wields {{R-r-K-y-Y sequence|Stopsvalinn}}");
		});
		AddMutableEncounterToTerrain("DesertCanyon", 25, delegate(string zoneID, Location2D location, TerrainTravel pTravel)
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.pRender.RenderString = "F";
				pTravel.ParentObject.pRender.SetForegroundColor('w');
			}
			ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("SnapjawStockadeMaker"));
			string secretID3 = AddSecret(zoneID, "a snapjaw fort", new string[3] { "snapjaw", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID3, "a snapjaw fort");
			GeneratedLocationInfo generatedLocationInfo3 = new GeneratedLocationInfo
			{
				name = "a snapjaw fort",
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID3
			};
			worldInfo.enemySettlements.Add(generatedLocationInfo3);
			mutableMap.SetMutable(generatedLocationInfo3.zoneLocation, 0);
		});
		AddMutableEncounterToTerrain("DesertCanyon", 25, delegate(string zoneID, Location2D location, TerrainTravel pTravel)
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.pRender.RenderString = "&RF";
			}
			ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("StarappleFarmMaker"));
			string text2 = SettlementNames.GenerateStarappleFarmName(The.Game.sultanHistory);
			ZM.SetZoneName(zoneID, text2, null, null, null, null, Proper: true);
			string secretID2 = AddSecret(zoneID, text2, new string[3] { "apple", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID2, text2);
			ZM.AddZonePostBuilder(zoneID, "IsCheckpoint");
			GeneratedLocationInfo generatedLocationInfo2 = new GeneratedLocationInfo
			{
				name = text2,
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID2
			};
			worldInfo.friendlySettlements.Add(generatedLocationInfo2);
			mutableMap.SetMutable(generatedLocationInfo2.zoneLocation, 0);
		});
		AddMutableEncounterToTerrain("DesertCanyon", 25, delegate(string zoneID, Location2D location, TerrainTravel pTravel)
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.pRender.RenderString = "&RF";
			}
			ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("PigFarmMaker"));
			string text = SettlementNames.GeneratePigFarmName(The.Game.sultanHistory);
			ZM.SetZoneName(zoneID, text, null, null, null, null, Proper: true);
			string secretID = AddSecret(zoneID, text, new string[3] { "pig", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID, text);
			ZM.AddZonePostBuilder(zoneID, "IsCheckpoint");
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo
			{
				name = text,
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID
			};
			worldInfo.friendlySettlements.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		});
	}

	public void BuildFarms(string WorldID)
	{
		WorldCreationProgress.StepProgress("Generating farms");
	}

	public void AddWaterway()
	{
		for (int i = 60; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					string zoneID = Zone.XYToID("JoppaWorld", i, j, 11);
					ZM.AddZonePostBuilder(zoneID, "Waterway");
				}
			}
		}
	}

	public void AddOboroqorusLair()
	{
		Location2D location2D = mutableMap.popMutableLocationInArea(81, 54, 94, 68);
		if (location2D == null)
		{
			XRLCore.LogError("worldgen", "no position for oboroqoru's lair");
			return;
		}
		int x = location2D.x;
		int y = location2D.y;
		Faction faction = Factions.get("Apes");
		string text = ZoneIDFromXY("JoppaWorld", x, y);
		string text2 = AddSecret(text, "{{M|the Lair of Oboroqoru, Ape God}}", new string[3] { "lair", "oboroqoru", "ape" }, "Lairs", "$oboroqorulair");
		(ZM.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetFirstObjectWithPart("TerrainTravel")?.GetPart("TerrainTravel") as TerrainTravel).AddEncounter(new EncounterEntry("You discover a lair. Would you like to investigate?", text, "", text2, _Optional: true));
		if (Options.ShowOverlandEncounters)
		{
			Render pRender = ZM.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetObjectInCell(0)
				.pRender;
			if (pRender != null)
			{
				pRender.RenderString = "A";
				pRender.ColorString = (pRender.TileColor = "&M");
			}
		}
		for (int i = -1; i < 1; i++)
		{
			for (int j = -1; j < 1; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", x + i, y + j, 10);
				if ((i == 0 && j == 0) || 75.in100())
				{
					ZM.AddZonePostBuilderAfterTerrain(zoneID, new ZoneBuilderBlueprint("Torchposts"));
				}
			}
		}
		for (int k = 0; k < 10; k++)
		{
			string text3 = Zone.XYToID("JoppaWorld", x, y, 10 + k);
			faction.HolyPlaces.Add(text3);
			GameObject gameObject = GameObject.create("LocationFinder");
			LocationFinder obj = gameObject.GetPart("LocationFinder") as LocationFinder;
			obj.ID = text2;
			obj.Text = "You discover the lair of {{M|Oboroqoru, Ape God}}!";
			obj.Value = 3000;
			if (k == 0)
			{
				ZM.AddZonePostBuilder(text3, "RedrockOutcrop");
				ZM.AddZoneMidBuilder(text3, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
				continue;
			}
			ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
			if (k == 9)
			{
				ZM.AddZoneBuilderOverride(text3, "BasicRoomHall");
				ZM.AddZoneBuilderOverride(text3, "StairsUp");
				GameObject gameObject2 = GameObject.create("Oboroqoru");
				gameObject2.SetStringProperty("nosecret", text2);
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("AddObjectBuilder", "Object", ZM.CacheObject(gameObject2)));
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "Chest6"));
			}
			else
			{
				ZM.AddZoneBuilderOverride(text3, "BasicRoomHall");
				ZM.AddZoneBuilderOverride(text3, "StairsUp");
				ZM.AddZoneBuilderOverride(text3, "StairsDown");
			}
			if (45.in100())
			{
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "Chest7"));
			}
			ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("ApegodCave"));
			if (k < 3)
			{
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("Population", "Table", "ApeGodLair1"));
			}
			else if (k < 6)
			{
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("Population", "Table", "ApeGodLair2"));
			}
			else
			{
				ZM.AddZoneBuilderOverride(text3, new ZoneBuilderBlueprint("Population", "Table", "ApeGodLair3"));
			}
		}
	}

	public void AddMamonVillage()
	{
		List<Location2D> list = BuildMamonRiver("JoppaWorld", 81, 62, 1, 8, 20, 3);
		while (list.Count <= 18)
		{
			list = BuildMamonRiver("JoppaWorld", 81, 62, 1, 8, 20, 3);
		}
		int num = Stat.Random(3, 5);
		int x = list[num].x;
		int y = list[num].y;
		string zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		ZM.AddZonePostBuilder(zoneID, "IdolFight");
		num += Stat.Random(3, 5);
		x = list[num].x;
		y = list[num].y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		ZM.AddZonePostBuilder(zoneID, "MinorRazedGoatfolkVillage");
		num += Stat.Random(3, 5);
		x = list[num].x;
		y = list[num].y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		ZM.AddZonePostBuilder(zoneID, "WildWatervineMerchant");
		num++;
		if (list[num - 1].x < list[num].x)
		{
			ZM.AddZonePostBuilder(zoneID, "SmokingAreaE");
		}
		if (list[num - 1].y < list[num].y)
		{
			ZM.AddZonePostBuilder(zoneID, "SmokingAreaS");
		}
		if (list[num - 1].y > list[num].y)
		{
			ZM.AddZonePostBuilder(zoneID, "SmokingAreaN");
		}
		x = list[num].x;
		y = list[num].y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		ZM.AddZonePostBuilder(zoneID, "RazedGoatfolkVillage");
		if (Options.ShowOverlandEncounters)
		{
			ZM.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetObjectInCell(0)
				.GetPart<Render>()
				.RenderString = "&Mg";
		}
		AddSecret(Zone.XYToID("JoppaWorld", x, y, 10), "Village Lair of {{M|Mamon Souldrinker}}", new string[4] { "lair", "mammon", "goatfolk", "humanoid" }, "Lairs", "$mamonvillage");
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("SecretRevealWidget");
		SecretRevealer part = gameObject.GetPart<SecretRevealer>();
		part.message = "You discover the village lair of Mamon Souldrinker.";
		part.id = "$mamonvillage";
		part.adjectives = "lair,mammon,goatfolk,humanoid";
		part.category = "Lairs";
		part.text = "the village lair of {{M|Mamon Souldrinker}}";
		ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
	}

	public string ZoneIDFromXYz(string World, int xp, int yp, int zp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, zp);
	}

	public string ZoneIDFromXY(string World, int xp, int yp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, 10);
	}

	public string AddSecret(string secretZone, string name, string[] adj, string category, string secretid = null, bool revealed = false, bool silent = false)
	{
		if (secretid != null && JournalAPI.GetMapNote(secretid) != null)
		{
			Debug.LogWarning("dupe secret: " + secretid);
			return secretid;
		}
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(secretZone);
		if (objectTypeForZone != "")
		{
			List<string> list = new List<string>(adj);
			string tag = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("SecretAttributes");
			if (tag != "")
			{
				list.AddRange(tag.Split(','));
			}
			else
			{
				list.Add(objectTypeForZone.Replace("Terrain", "").Replace("Watervine", "Saltmarsh").Replace("1", "")
					.Replace("2", "")
					.Replace("3", "")
					.Replace("4", "")
					.Replace("5", "")
					.Replace("6", "")
					.Replace("7", "")
					.Replace("8", "")
					.Replace("9", "")
					.Replace("0", "")
					.ToLower());
			}
			adj = list.ToArray();
		}
		if (secretid == null)
		{
			secretid = Guid.NewGuid().ToString();
		}
		JournalAPI.AddMapNote(secretZone, name, category, adj, secretid, revealed, sold: false, 0L, silent);
		return secretid;
	}

	public void AddHindrenVillage(string WorldID)
	{
		Location2D location2D = popMutableBlockOfTerrain("Flowerfields");
		string text = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10);
		Cell cell = ZM.GetZone(WorldID).GetCell(location2D.x / 3, location2D.y / 3);
		ZM.SetZoneProperty(text, "NoBiomes", "Yes");
		cell.GetFirstObjectWithPart("TerrainTravel").AddPart(new BeyLahTerrain());
		ZM.ClearZoneBuilders(text);
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("ClearAll"));
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("MapBuilder", "FileName", "BeyLah.rpm"));
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("HindrenClues"));
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("Music", "Track", "BeyLahHeritage"));
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("AddWidgetBuilder", "Blueprint", "BeyLahSurface"));
		ZM.SetZoneName(text, "Bey Lah", null, null, null, null, Proper: true);
		ZM.SetZoneIncludeStratumInZoneDisplay(text, false);
		ZM.SetZoneProperty(text, "SkipTerrainBuilders", true);
		string[] directionList = Directions.DirectionList;
		foreach (string d in directionList)
		{
			Location2D location2D2 = location2D.FromDirection(d);
			string zoneID = Zone.XYToID("JoppaWorld", location2D2.x, location2D2.y, 10);
			ZM.AddZoneMidBuilderAtStart(zoneID, new ZoneBuilderBlueprint("BeyLahOutskirts"));
			ZM.AddZoneMidBuilderAtStart(zoneID, new ZoneBuilderBlueprint("ClearAll"));
			ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("Music", "Track", "BeyLahHeritage"));
			ZM.SetZoneName(zoneID, "outskirts", "Bey Lah", "some");
			ZM.SetZoneIncludeStratumInZoneDisplay(zoneID, false);
		}
		List<Location2D> source = new List<Location2D>();
		List<int> list = new List<int> { 0, 1, 2, 3 };
		list.ShuffleInPlace();
		int num = 4;
		bool flag = false;
		while (true)
		{
			foreach (int item in list)
			{
				source = BuildEskhindRoad("JoppaWorld", location2D.x, location2D.y, item, 8, 4, -1, layRoad: false);
				if (source.Last().Distance(location2D) >= num)
				{
					flag = true;
					source = BuildEskhindRoad("JoppaWorld", location2D.x, location2D.y, item, 8, 4, -1, layRoad: true);
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num--;
		}
		string text2 = Zone.XYToID("JoppaWorld", source.Last().x, source.Last().y, 10);
		ZM.ClearZoneBuilders(text2);
		ZM.AddZonePostBuilderAfterTerrain(text2, new ZoneBuilderBlueprint("ClearAll"));
		ZM.AddZonePostBuilderAfterTerrain(text2, new ZoneBuilderBlueprint("MapBuilder", "FileName", "HollowTree.rpm"));
		ZM.AddZonePostBuilderAfterTerrain(text2, new ZoneBuilderBlueprint("Music", "Track", "Overworld1"));
		base.game.SetStringGameState("HollowTreeZoneId", text2);
		ZM.SetZoneProperty(text2, "NoBiomes", "Yes");
		mutableMap.SetMutable(Location2D.get(source.Last().x, source.Last().y), 0);
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		generatedLocationInfo.name = "Bey Lah";
		generatedLocationInfo.targetZone = text;
		generatedLocationInfo.zoneLocation = Location2D.get(location2D.x, location2D.y);
		generatedLocationInfo.secretID = null;
		worldInfo.villages.Add(generatedLocationInfo);
		mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		base.game.SetStringGameState("BeyLahZoneID", Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10));
		AddSecret(Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10), "Bey Lah", new string[2] { "settlement", "hindren" }, "Settlements", "$beylah");
		JournalAPI.GetMapNote("$beylah").attributes.Add("nobuy:Hindren");
	}

	public void AddHydropon(string WorldID)
	{
		Location2D location2D = popMutableLocationOfTerrain("PalladiumReef", null, centerOnly: false);
		string text = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10);
		Cell cell = ZM.GetZone(WorldID).GetCell(location2D.x / 3, location2D.y / 3);
		ZM.SetZoneProperty(text, "NoBiomes", "Yes");
		ZM.SetZoneProperty(text, "NoSvardymStorm", "Yes");
		cell.GetFirstObjectWithPart("TerrainTravel").AddPart(new HydroponTerrain());
		ZM.ClearZoneBuilders(text);
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("ClearAll"));
		ZM.AddZonePostBuilder(text, "Reef");
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("MapBuilder", "FileName", "Hydropon.rpm", "ClearBeforePlacingObjectsIfObjectsExist", true));
		ZM.SetZoneName(text, "Hydropon", null, null, null, "the", Proper: true);
		ZM.SetZoneIncludeStratumInZoneDisplay(text, false);
		ZM.SetZoneProperty(text, "SkipTerrainBuilders", true);
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		generatedLocationInfo.name = "Hydropon";
		generatedLocationInfo.targetZone = text;
		generatedLocationInfo.zoneLocation = location2D;
		generatedLocationInfo.secretID = null;
		worldInfo.villages.Add(generatedLocationInfo);
		mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		base.game.SetStringGameState("HydroponZoneID", Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10));
		string text2 = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10);
		AddSecret(text2, "the Hydropon", new string[1] { "settlement" }, "Settlements", "$hydropon");
		AddLocationFinder(text2, "$hydropon", "the Hydropon");
	}

	public List<Location2D> BuildEskhindRoad(string WorldID, int StartX, int StartY, int Direction, int Bias, int MinimumLength, int ExcludeDirection, bool layRoad)
	{
		if (layRoad)
		{
			string zoneID = ZoneIDFromXY(WorldID, StartX, StartY);
			switch (Direction)
			{
			case 0:
				base.game.SetStringGameState("EskhindRoadDirection", "north");
				ZM.AddZoneConnection(zoneID, "-", 43, 12, "Road");
				RoadSystem[StartX, StartY] |= ROAD_NORTH;
				break;
			case 1:
				base.game.SetStringGameState("EskhindRoadDirection", "east");
				ZM.AddZoneConnection(zoneID, "-", 72, 16, "Road");
				RoadSystem[StartX, StartY] |= ROAD_EAST;
				break;
			case 2:
				base.game.SetStringGameState("EskhindRoadDirection", "south");
				ZM.AddZoneConnection(zoneID, "-", 59, 19, "Road");
				RoadSystem[StartX, StartY] |= ROAD_SOUTH;
				break;
			case 3:
				base.game.SetStringGameState("EskhindRoadDirection", "west");
				ZM.AddZoneConnection(zoneID, "-", 20, 9, "Road");
				RoadSystem[StartX, StartY] |= ROAD_WEST;
				break;
			}
			if ((RoadSystem[StartX, StartY] & ROAD_NORTH) != 0)
			{
				ZM.AddZoneConnection(zoneID, "-", 36, 0, "RoadNorthMouth");
				ZM.AddZoneConnection(zoneID, "n", 36, 24, "RoadSouthMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_EAST) != 0)
			{
				ZM.AddZoneConnection(zoneID, "-", 79, 13, "RoadEastMouth");
				ZM.AddZoneConnection(zoneID, "e", 0, 13, "RoadWestMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_SOUTH) != 0)
			{
				ZM.AddZoneConnection(zoneID, "-", 57, 24, "RoadSouthMouth");
				ZM.AddZoneConnection(zoneID, "s", 57, 0, "RoadNorthMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_WEST) != 0)
			{
				ZM.AddZoneConnection(zoneID, "-", 0, 8, "RoadWestMouth");
				ZM.AddZoneConnection(zoneID, "w", 79, 8, "RoadEastMouth");
			}
			ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("RoadBuilder", "ClearSolids", false, "Noise", false));
			RoadSystem[StartX, StartY] |= ROAD_START;
		}
		List<Location2D> list = new List<Location2D>();
		ContinueEskhindRoad(StartX, StartY, Direction, 0, Bias, MinimumLength, ExcludeDirection, list, layRoad);
		return list;
	}

	public void ContinueEskhindRoad(int StartX, int StartY, int Direction, int Depth, int Bias, int MinimumLength, int ExcludeDirection, List<Location2D> Points, bool layRoad)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (layRoad)
		{
			mutableMap.RemoveMutableLocation(Location2D.get(num, num2));
		}
		Points.Add(Location2D.get(num, num2));
		if (Depth > MinimumLength && Stat.Random(0, 100) < 50 + (Depth - MinimumLength) * 25)
		{
			if (layRoad)
			{
				ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
				RoadSystem[num, num2] |= ROAD_START;
				ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			}
			return;
		}
		int num3 = Direction;
		if (Bias <= 0 && 50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
			if (num3 == ExcludeDirection)
			{
				num3 = Direction;
			}
		}
		if (Direction == 0 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (Direction == 1 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if (Direction == 2 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (Direction == 3 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || (Depth > 0 && RoadSystem[num5, num6] != 0))
		{
			if (layRoad)
			{
				RoadSystem[num, num2] |= ROAD_START;
				ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			}
			return;
		}
		if (num3 == 0 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (num3 == 1 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		if (num3 == 2 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (num3 == 3 && layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		bool flag = false;
		if (layRoad)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
		}
		if (layRoad && Options.ShowOverlandEncounters)
		{
			Render render = ZM.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.GetPart("Render") as Render;
			render.RenderString = "=";
			if (flag)
			{
				render.RenderString = "t";
			}
		}
		ContinueEskhindRoad(num, num2, num3, Depth + 1, Bias - 1, MinimumLength, ExcludeDirection, Points, layRoad);
	}

	public void AddYonderPath(string WorldID)
	{
		Location2D location = ZM.GetZone(WorldID).GetFirstObject("TerrainFungalCenter").CurrentCell.location;
		Location2D location2D = Location2D.get(location.x * 3 + 1, location.y * 3 + 1);
		string text = ZoneIDFromXY(WorldID, location2D.x, location2D.y);
		ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("FungalTrailExileCorpse"));
		ZM.AddZoneConnection(text, "-", Stat.Random(20, 50), Stat.Random(10, 15), "FungalTrailStart");
		mutableMap.SetMutable(location2D, 0);
		List<Location2D> list = new List<Location2D>(4) { location2D };
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num < 3 && num4 < 100)
		{
			string randomCardinalDirection = Directions.GetRandomCardinalDirection();
			num2 = list[num].x;
			num3 = list[num].y;
			Directions.ApplyDirection(randomCardinalDirection, ref num2, ref num3);
			Location2D location2D2 = Location2D.get(num2, num3);
			if (!list.Contains(location2D2) && mutableMap.GetMutable(location2D2) > 0)
			{
				int x = Stat.Random(10, 70);
				int y = Stat.Random(5, 20);
				switch (randomCardinalDirection)
				{
				case "N":
					ZM.AddZoneConnection(text, "-", x, 0, "FungalTrailNorthMouth");
					ZM.AddZoneConnection(text, randomCardinalDirection, x, 24, "FungalTrailSouthMouth");
					break;
				case "S":
					ZM.AddZoneConnection(text, "-", x, 24, "FungalTrailSouthMouth");
					ZM.AddZoneConnection(text, randomCardinalDirection, x, 0, "FungalTrailNorthMouth");
					break;
				case "E":
					ZM.AddZoneConnection(text, "-", 79, y, "FungalTrailEastMouth");
					ZM.AddZoneConnection(text, randomCardinalDirection, 0, y, "FungalTrailWestMouth");
					break;
				case "W":
					ZM.AddZoneConnection(text, "-", 0, y, "FungalTrailWestMouth");
					ZM.AddZoneConnection(text, randomCardinalDirection, 79, y, "FungalTrailEastMouth");
					break;
				}
				list.Add(location2D2);
				ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("FungalTrailBuilder"));
				mutableMap.SetMutable(location2D2, 0);
				text = ZoneIDFromXY(WorldID, num2, num3);
				num++;
			}
			num4++;
		}
		ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("FungalTrailKlanqHut"));
		ZM.AddZonePostBuilderAfterTerrain(text, new ZoneBuilderBlueprint("FungalTrailBuilder"));
		ZM.AddZoneConnection(text, "-", Stat.Random(20, 50), Stat.Random(10, 15), "FungalTrailStart");
		base.game.SetStringGameState("FungalTrailEnd", text);
	}

	public void AddStaticEncounters(string WorldID)
	{
		ZM = XRLCore.Core.Game.ZoneManager;
		ZM.GetZone("JoppaWorld");
		WorldCreationProgress.StepProgress("Generating static encounters");
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Location2D location2D = popMutableLocationOfTerrain("Saltmarsh");
		string text = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10);
		GameObject gameObject = ZM.GetZone(WorldID).GetCell(location2D.x / 3, location2D.y / 3).Objects[0];
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "SkrefCorpse"));
		if (Options.ShowOverlandEncounters)
		{
			gameObject.pRender.RenderString = "&r%";
		}
		AddSecret(text, "some flattened remains", new string[3] { "encounter", "special", "oddity" }, "Oddities", "$skrefcorpse");
		Location2D location2D2 = popMutableLocationOfTerrain("DesertCanyon");
		string text2 = Zone.XYToID("JoppaWorld", location2D2.x, location2D2.y, 10);
		gameObject = ZM.GetZone(WorldID).GetCell(location2D2.x / 3, location2D2.y / 3).Objects[0];
		XRLCore.Core.Game.SetStringGameState("$TrembleEntranceEncounter", text2);
		ZM.AddZonePostBuilder(text2, "TrembleEntrance");
		if (Options.ShowOverlandEncounters)
		{
			gameObject.pRender.RenderString = "&Wx";
		}
		Location2D location2D3 = popMutableLocationOfTerrain("Saltmarsh");
		string text3 = Zone.XYToID("JoppaWorld", location2D3.x, location2D3.y, 10);
		gameObject = ZM.GetZone(WorldID).GetCell(location2D3.x / 3, location2D3.y / 3).Objects[0];
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", location2D3.x + i, location2D3.y + j, 10);
				ZM.AddZonePostBuilder(zoneID, "DenseBrinestalk");
			}
		}
		ZM.AddZonePostBuilder(text3, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "OasisGlowpad"));
		if (Options.ShowOverlandEncounters)
		{
			gameObject.pRender.RenderString = "&W$";
		}
		AddSecret(text3, "a secluded merchant from the Consortium of Phyta", new string[4] { "encounter", "special", "oddity", "consortium" }, "Oddities", "$glowpadmerchant");
		Location2D location2D4 = mutableMap.popMutableLocationInArea(0, 0, 119, 74);
		int num = location2D4.x / 3;
		int num2 = location2D4.y / 3;
		string text4 = Zone.XYToID("JoppaWorld", location2D4.x, location2D4.y, 10);
		XRLCore.Core.Game.SetIntGameState("RuinofHouseIsner_xCoordinate", num);
		XRLCore.Core.Game.SetIntGameState("RuinofHouseIsner_yCoordinate", num2);
		Event.ResetPool();
		ZM.AddZonePostBuilder(text4, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "ChestIsner"));
		if (Options.ShowOverlandEncounters)
		{
			ZM.GetZone(WorldID).GetCell(num, num2).Objects[0].pRender.RenderString = "&M*";
		}
		AddSecret(text4, "the Ruin of House Isner", new string[2] { "artifact", "special" }, "Artifacts", "$ruinofhouseisner");
		Location2D location2D5 = popMutableLocationOfTerrain("LakeHinnom");
		string text5 = Zone.XYToID("JoppaWorld", location2D5.x, location2D5.y, 10);
		XRLCore.Core.Game.SetStringGameState("Recorporealization_ZoneID", text5);
		ZM.AddZonePostBuilder(text5, new ZoneBuilderBlueprint("AddWidgetBuilder", "Blueprint", "RecorporealizationBoothSpawner"));
		ZM.SetZoneName(text5, "Gyl", null, null, null, null, Proper: true);
		AddSecret(text5, "Gyl", new string[2] { "oddity", "ruins" }, "Oddities", "$recomingnook");
		WorldZone.GetCell(location2D5.x / 3, location2D5.y / 3).GetFirstObjectWithPart("TerrainTravel").GetPart<TerrainTravel>()
			.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text5, "", "$recomingnook", _Optional: true));
		GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("SecretRevealWidget");
		SecretRevealer part = gameObject2.GetPart<SecretRevealer>();
		part.message = "You discover the recoming nook at Gyl.";
		part.id = "$recomingnook";
		part.adjectives = "oddity,ruins";
		part.category = "Oddities";
		part.text = "Recoming nook at Gyl";
		ZM.AddZonePostBuilder(text5, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject2)));
	}

	public void BuildSecrets(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Location2D location2D;
		string text;
		for (int i = 0; i < 32; i++)
		{
			location2D = mutableMap.popMutableLocationInArea(0, 0, 119, 74);
			text = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, 10);
			AddSecret(text, "a {{w|dromad}} caravan", new string[2] { "dromad", "merchant" }, "Merchants", "DromadMerchant_" + text);
			ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "DromadTrader1"));
		}
		location2D = getLocationOfTier(4, 6);
		location2D = Location2D.get(location2D.x * 3 + 1, location2D.y * 3 + 1);
		text = Zone.XYToID("JoppaWorld", location2D.x, location2D.y, Stat.Random(10, 19));
		ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("PlaceRelicBuilder", "Relic", ZM.CacheObject(GameObject.create("Kindrish"))));
		AddSecret(text, "Kindrish, the ancestral bracelet of the hindren", new string[2] { "artifact", "kindrish" }, "Artifacts", "~kindrish");
		foreach (string key in BiomeManager.Biomes.Keys)
		{
			List<Point2D> list = new List<Point2D>();
			for (int j = 0; j < 240; j++)
			{
				for (int k = 0; k < 75; k++)
				{
					text = Zone.XYToID("JoppaWorld", j, k, 10);
					if (BiomeManager.BiomeValue(key, text) >= 3)
					{
						list.Add(new Point2D(j, k));
					}
				}
			}
			foreach (Point2D item in list)
			{
				text = Zone.XYToID("JoppaWorld", item.x, item.y, 10);
				string text2 = ((!string.Equals(key, "slimy", StringComparison.CurrentCultureIgnoreCase)) ? ((!string.Equals(key, "tarry", StringComparison.CurrentCultureIgnoreCase)) ? ((!string.Equals(key, "rusty", StringComparison.CurrentCultureIgnoreCase)) ? ((!string.Equals(key, "fungal", StringComparison.CurrentCultureIgnoreCase)) ? (Grammar.A(key) + " region") : "a {{m|fungus}} forest") : "a {{rusty|rust}} bog") : "some {{fiery|flaming}} {{K|tar}} pits") : "a {{g|slime}} bog");
				AddSecret(text, text2, new string[2]
				{
					"biome",
					key.ToLower()
				}, "Natural Features", "Biome_" + text);
				GameObject gameObject = GameObjectFactory.Factory.CreateObject("SecretRevealWidget");
				SecretRevealer part = gameObject.GetPart<SecretRevealer>();
				part.message = "You discover " + text2 + ".";
				part.id = "Biome_" + text;
				part.adjectives = "biome," + key.ToLower();
				part.category = "Natural Features";
				part.text = Grammar.InitialCap(text2);
				ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
			}
		}
		List<Point3D> list2 = new List<Point3D>(800);
		for (int l = 0; l < 240; l++)
		{
			for (int m = 0; m < 75; m++)
			{
				for (int n = 10; n <= 29; n++)
				{
					int num = ((FungalBiome.BiomeLevels != null) ? FungalBiome.BiomeLevels[l, m, n % 10] : BiomeManager.BiomeValue("Fungal", Zone.XYToID("JoppaWorld", l, m, n)));
					if (num >= 1)
					{
						if (n == 10)
						{
							list2.Add(new Point3D(l, m, n));
						}
						if (n > 10 && Stat.Random(1, 100) <= 5)
						{
							list2.Add(new Point3D(l, m, n));
						}
					}
				}
			}
		}
		string[] obj = new string[14]
		{
			"waterLichen Minor", "honeyLichen Minor", "lavaLichen Minor", "acidLichen Minor", "wineLichen Minor", "slimeLichen Minor", "ciderLichen Minor", "gelLichen Minor", "asphaltLichen Minor", "saltLichen Minor",
			"oilLichen Minor", "sapLichen Minor", "waxLichen Minor", "inkLichen Minor"
		};
		list2.ShuffleInPlace();
		int num2 = 0;
		string[] array = obj;
		foreach (string objectBlueprint in array)
		{
			int num4 = Stat.Random(3, 4);
			for (int num5 = 0; num5 < num4; num5++)
			{
				if (num2 >= list2.Count)
				{
					break;
				}
				text = Zone.XYToID("JoppaWorld", list2[num2].x, list2[num2].y, list2[num2].z);
				GameObject gameObject2 = GameObjectFactory.Factory.CreateObject(objectBlueprint);
				string text3 = "surface";
				if (list2[num2].z > 10)
				{
					text3 = "underground";
				}
				ZM.AddZonePostBuilder(text, new ZoneBuilderBlueprint("AddObjectBuilder", "Object", ZM.CacheObject(gameObject2)));
				AddSecret(text, gameObject2.a + gameObject2.DisplayName, new string[3]
				{
					"weep",
					text3,
					gameObject2.GetPart<LiquidFont>().Liquid
				}, "Natural Features", gameObject2.GetPart<SecretObject>().id);
				num2++;
			}
		}
	}

	public void BuildLairs(string WorldID)
	{
		if (WorldID != "JoppaWorld")
		{
			return;
		}
		MetricsManager.rngCheckpoint("%LAIRS 1");
		Lairs = new int[240, 75];
		ZM = XRLCore.Core.Game.ZoneManager;
		List<Location2D> list = new List<Location2D>();
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) > 0 && ((RoadSystem[i, j] & ROAD_START) != 0 || (RiverSystem[i, j] & RIVER_START) != 0))
				{
					list.Add(Location2D.get(i, j));
				}
			}
		}
		int num = 125;
		int num2 = 300;
		int num3 = num - list.Count + num2;
		MetricsManager.rngCheckpoint("%LAIRS 2");
		for (int k = 0; k < num3; k++)
		{
			list.Add(mutableMap.popMutableLocation());
		}
		MetricsManager.rngCheckpoint("%LAIRS 3");
		Coach.StartSection("Generate Lairs");
		list.ShuffleInPlace();
		MetricsManager.rngCheckpoint("%LAIRS 4");
		Event.PinCurrentPool();
		for (int l = 0; l < num; l++)
		{
			Event.ResetPool();
			try
			{
				if (AddLairAt(list[l], l))
				{
					mutableMap.SetMutable(list[l], 0);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException($"AddLairAt::{list[l]}", x);
			}
			Event.ResetToPin();
		}
		Coach.EndSection();
		MetricsManager.rngCheckpoint("%LAIRS 5");
	}

	public void PlaceClams()
	{
		MapFile mapFile = MapFile.LoadWithMods("Tzimtzlum.rpm");
		BallBag<string> ballBag = new BallBag<string>
		{
			{ "reef_surface", 15 },
			{ "reef_cave", 5 },
			{ "lake_surface", 7 },
			{ "lake_cave", 3 }
		};
		ClamSystem clamSystem = The.Game.RequireSystem(() => new ClamSystem());
		foreach (MapFileCellReference item in mapFile.Cells.AllCells())
		{
			if (item.cell.Objects.Any((MapFileObjectBlueprint o) => o.Name == "Giant Clam"))
			{
				string text = ballBag.PeekOne();
				string text2 = null;
				switch (text)
				{
				case "reef_surface":
					text2 = GetZoneIdOfTerrain("PalladiumReef");
					break;
				case "reef_cave":
					text2 = GetZoneIdOfTerrain("PalladiumReef", Stat.Random(11, 15).ToString());
					break;
				case "lake_surface":
					text2 = GetZoneIdOfTerrain("LakeHinnom");
					break;
				case "lake_cave":
					text2 = GetZoneIdOfTerrain("LakeHinnom", Stat.Random(11, 15).ToString());
					break;
				}
				The.Game.ZoneManager.AddZonePostBuilder(text2, new ZoneBuilderBlueprint("PlaceAClam", "clamNumber", clamSystem.clamJoppaZone.Count));
				clamSystem.clamJoppaZone.Add(text2);
			}
		}
		clamSystem.clamJoppaZone.Add("JoppaWorld.67.17.1.1.10");
	}

	public void AddLocationFinder(string ZoneID, string SecretID, string Text, int Value = 0)
	{
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("LocationFinder");
		LocationFinder obj = gameObject.GetPart("LocationFinder") as LocationFinder;
		obj.ID = SecretID;
		obj.Text = Text;
		obj.Value = Value;
		ZM.AddZonePostBuilder(ZoneID, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject)));
	}

	public bool AddLairAt(Location2D pos, int nLair)
	{
		MetricsManager.rngCheckpoint("LAIR " + nLair + " start");
		ZM = XRLCore.Core.Game.ZoneManager;
		string text = Zone.XYToID("JoppaWorld", pos.x, pos.y, 10);
		int num = 0;
		if (25.in100())
		{
			num += Stat.Random(1, 25);
		}
		string text2 = Zone.XYToID("JoppaWorld", pos.x, pos.y, 10 + num);
		string text3 = null;
		int zoneTier = ZM.GetZoneTier(text);
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(pos.x / 3, pos.y / 3, "JoppaWorld");
		string tag = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("LairOwnerTable", "GenericLairOwner");
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		GameObject gameObject = ((!10.in100()) ? GameObjectFactory.Factory.CreateObject(PopulationManager.RollOneFrom(tag, new Dictionary<string, string> { 
		{
			"zonetier",
			zoneTier.ToString()
		} }).Blueprint) : GameObjectFactory.Factory.CreateObject(PopulationManager.RollOneFrom("GenericLairOwner", new Dictionary<string, string> { 
		{
			"zonetier",
			zoneTier.ToString()
		} }).Blueprint));
		MetricsManager.rngCheckpoint("LAIR " + nLair + " createowner");
		if (gameObject == null)
		{
			MetricsManager.LogError("AddLairAt: Couldn't get a lair monster for " + text2 + " in a " + objectTypeForZone + ".");
			return false;
		}
		GameObjectBlueprint blueprint = gameObject.GetBlueprint();
		gameObject.SetIntProperty("LairOwner", 1);
		if (gameObject.HasTag("LairMinionsInherit"))
		{
			text3 = "DynamicInheritsTable:" + gameObject.GetTag("LairMinionsInherit") + ":Tier" + The.ZoneManager.GetZoneTier(text);
		}
		else if (gameObject.HasTag("LairMinions"))
		{
			text3 = gameObject.GetTag("LairMinions");
		}
		else
		{
			string text4 = gameObject.GetBlueprint().Inherits;
			string text5 = text4;
			while (text5 != null && !text5.StartsWith("Base") && !GameObjectFactory.Factory.Blueprints[text5].Tags.ContainsKey("BaseObject"))
			{
				text5 = GameObjectFactory.Factory.Blueprints[text5].Inherits;
			}
			if (text5 != null)
			{
				text4 = text5;
			}
			if (text3 == null)
			{
				text3 = "DynamicInheritsTable:" + text4 + ":Tier" + XRLCore.Core.Game.ZoneManager.GetZoneTier(text);
			}
		}
		if (!gameObject.HasPart("GivesRep"))
		{
			gameObject = HeroMaker.MakeHero(gameObject, new string[0], new string[0], zoneTier, "Lair");
			if (gameObject == null)
			{
				return false;
			}
		}
		MetricsManager.rngCheckpoint("LAIR " + nLair + " makehero");
		if (gameObject.HasStat("Strength"))
		{
			gameObject.GetStat("Strength").BoostStat(1);
		}
		if (gameObject.HasStat("Intelligence"))
		{
			gameObject.GetStat("Intelligence").BoostStat(1);
		}
		if (gameObject.HasStat("Toughness"))
		{
			gameObject.GetStat("Toughness").BoostStat(1);
		}
		if (gameObject.HasStat("Willpower"))
		{
			gameObject.GetStat("Willpower").BoostStat(1);
		}
		if (gameObject.HasStat("Ego"))
		{
			gameObject.GetStat("Ego").BoostStat(1);
		}
		if (gameObject.HasStat("Agility"))
		{
			gameObject.GetStat("Agility").BoostStat(1);
		}
		if (gameObject.HasStat("Hitpoints"))
		{
			gameObject.GetStat("Hitpoints").BaseValue *= 2;
		}
		if (gameObject.HasStat("XP"))
		{
			gameObject.GetStat("XP").BaseValue = gameObject.Statistics["Level"].Value * 250;
		}
		int value = gameObject.Stat("Level") * 75;
		int num2 = gameObject.Stat("Level") / 5 + 1;
		int num3 = num2 - 2;
		if (num2 < 2)
		{
			num2 = 2;
		}
		if (num2 > 8)
		{
			num2 = 8;
		}
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (num3 > 8)
		{
			num3 = 8;
		}
		MetricsManager.rngCheckpoint("LAIR " + nLair + " statboost");
		if (gameObject.Property.ContainsKey("Role"))
		{
			gameObject.Property["Role"] = "Hero";
		}
		if (gameObject.HasPart("Inventory") && gameObject.HasTag("LairInventory"))
		{
			if (gameObject.HasTag("LairAddMakersMark"))
			{
				if (!gameObject.HasStringProperty("MakersMark"))
				{
					gameObject.SetStringProperty("MakersMark", MakersMark.Generate());
				}
				gameObject.EquipFromPopulationTable(gameObject.GetTag("LairInventory"), gameObject.GetTier(), GenericInventoryRestocker.GetCraftmarkApplication(gameObject));
			}
			else
			{
				gameObject.EquipFromPopulationTable(gameObject.GetTag("LairInventory"), zoneTier);
			}
		}
		MetricsManager.rngCheckpoint("LAIR " + nLair + " inventory");
		string text6 = Guid.NewGuid().ToString();
		gameObject.SetStringProperty("nosecret", text6);
		string text7 = "";
		if (gameObject.HasTag("LairAdjectives"))
		{
			text7 += gameObject.GetTag("LairAdjectives");
		}
		if (text7.Length > 0)
		{
			text7 += ",";
		}
		text7 += GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("LairAdjectives", "lair");
		string tag2 = gameObject.GetTag("LairDepth", "2-4");
		string name = "the %l of %o".Replace("%l", blueprint.GetTag("LairName", "lair")).Replace("%o", gameObject.DisplayName).Replace("%p", Grammar.MakePossessive(gameObject.DisplayNameOnlyStripped))
			.Replace("%s", gameObject.the + gameObject.DisplayNameOnlyStripped);
		int num4 = tag2.RollCached();
		MetricsManager.rngCheckpoint("LAIR " + nLair + " lairstats");
		for (int i = 0; i < num4; i++)
		{
			string zoneID = Zone.XYToID("JoppaWorld", pos.x, pos.y, 10 + i);
			GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("LocationFinder");
			LocationFinder obj = gameObject2.GetPart("LocationFinder") as LocationFinder;
			obj.ID = text6;
			string displayNameOnlyDirectAndStripped = gameObject.DisplayNameOnlyDirectAndStripped;
			string text8 = gameObject.the + displayNameOnlyDirectAndStripped;
			string newValue = Grammar.MakePossessive(text8);
			obj.Text = blueprint.GetTag("LairDiscoveredMessage", "You discover the lair of %o.").Replace("%o", displayNameOnlyDirectAndStripped).Replace("%n", i.ToString())
				.Replace("%p", newValue)
				.Replace("%s", text8);
			obj.Value = value;
			string tag3 = blueprint.GetTag("LairNameContext", "lair of %o");
			string text9 = null;
			bool proper = false;
			if (i != num4 - 1)
			{
				text9 = ((i != 0) ? blueprint.GetTag("LairLevelNameSubsurface") : blueprint.GetTag("LairLevelNameSurface"));
			}
			else
			{
				text9 = blueprint.GetTag("LairLevelNameFinal");
				proper = true;
			}
			if (text9 != null)
			{
				text9 = text9.Replace("%o", displayNameOnlyDirectAndStripped).Replace("%n", i.ToString()).Replace("%p", newValue)
					.Replace("%s", text8);
			}
			tag3 = tag3.Replace("%o", displayNameOnlyDirectAndStripped).Replace("%n", i.ToString()).Replace("%p", newValue)
				.Replace("%s", text8);
			The.ZoneManager.SetZoneName(zoneID, text9, tag3, null, null, null, proper);
			if (i == 0)
			{
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "D"));
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject2)));
				continue;
			}
			if (i == num4 - 1)
			{
				ZM.ClearZoneBuilders(zoneID);
				ZM.SetZoneProperty(zoneID, "SkipTerrainBuilders", true);
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "U"));
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddObjectBuilder", "Object", ZM.CacheObject(gameObject)));
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddBlueprintBuilder", "Object", "Chest" + num2));
			}
			else
			{
				ZM.ClearZoneBuilders(zoneID);
				ZM.SetZoneProperty(zoneID, "SkipTerrainBuilders", true);
				ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "UD"));
			}
			ZM.AddZonePostBuilder(zoneID, new ZoneBuilderBlueprint("AddWidgetBuilder", "Object", ZM.CacheObject(gameObject2)));
		}
		MetricsManager.rngCheckpoint("LAIR " + nLair + " levelgen");
		string secretZone = Zone.XYToID("JoppaWorld", pos.x, pos.y, 10);
		List<string> list = new List<string>();
		string tag4 = gameObject.GetTag("LairCategory", "Lairs");
		string tag5 = gameObject.GetTag("LairName", "lair");
		if (tag4 == "Lairs")
		{
			list.Add("lair");
		}
		if (gameObject.HasTag("LairAdjectives"))
		{
			list.AddRange(gameObject.GetTag("LairAdjectives").Split(','));
		}
		if (gameObject.HasTag("SecretAdjectives"))
		{
			list.AddRange(gameObject.GetTag("SecretAdjectives").Split(','));
		}
		list.Add(gameObject.GetPropertyOrTag("Species") ?? gameObject.GetBlueprint().GetBaseTypeName().ToLower());
		AddSecret(secretZone, "the " + tag5 + " of " + gameObject.DisplayNameOnlyDirectAndStripped, list.ToArray(), tag4, text6);
		MetricsManager.rngCheckpoint("LAIR " + nLair + " secretgen");
		Coach.EndSection();
		Coach.StartSection("Add pulldowns");
		Zone zone = ZM.GetZone("JoppaWorld");
		TerrainTravel terrainTravel = null;
		Render render = null;
		GameObject firstObjectWithPart = zone.GetCell(pos.x / 3, pos.y / 3).GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart != null)
		{
			terrainTravel = firstObjectWithPart.GetPart("TerrainTravel") as TerrainTravel;
			render = firstObjectWithPart.GetPart("Render") as Render;
		}
		if (Options.ShowOverlandEncounters && render != null)
		{
			render.RenderString = "&W*";
			render.ParentObject.SetStringProperty("OverlayColor", "&M");
		}
		terrainTravel?.AddEncounter(new EncounterEntry(blueprint.GetTag("LairPulldownMessage", "You discover a lair. Would you like to investigate?"), text, "", text6, _Optional: true));
		generatedLocationInfo.targetZone = Zone.XYToID("JoppaWorld", pos.x, pos.y, 10 + num4 - 1);
		generatedLocationInfo.zoneLocation = pos;
		generatedLocationInfo.name = name;
		generatedLocationInfo.ownerID = gameObject.id;
		generatedLocationInfo.secretID = text6;
		worldInfo.lairs.Add(generatedLocationInfo);
		MetricsManager.rngCheckpoint("LAIR " + nLair + " travelgen");
		Coach.EndSection();
		return true;
	}

	public Location2D GetRoadHead()
	{
		int num = Stat.Random(0, 239);
		int num2 = Stat.Random(0, 74);
		while (RoadSystem[num, num2] != 0)
		{
			num = Stat.Random(0, 239);
			num2 = Stat.Random(0, 74);
		}
		return Location2D.get(num, num2);
	}

	public void BuildCanyonSystems(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Zone zone = ZM.GetZone("JoppaWorld");
		Maze maze = XRLCore.Core.Game.WorldMazes["QudCanyonMaze"];
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", i, j, 10);
				int x = i / 3;
				int y = j / 3;
				GameObject firstObjectWithPart = zone.GetCell(x, y).GetFirstObjectWithPart("TerrainTravel");
				if (firstObjectWithPart != null && (firstObjectWithPart.Blueprint.Contains("TerrainJoppaRedrockChannel") || firstObjectWithPart.Blueprint.Contains("Canyon") || firstObjectWithPart.Blueprint.Contains("Hills") || firstObjectWithPart.Blueprint.Contains("Asphalt") || firstObjectWithPart.Blueprint.Contains("TerrainRustedArchway") || firstObjectWithPart.Blueprint.Contains("TerrainBethesdaSusa")))
				{
					MazeCell mazeCell = maze.Cell[i, j];
					ZM.AddZoneMidBuilder(zoneID, "CanyonStartMouth");
					if (mazeCell.N)
					{
						ZM.AddZoneMidBuilder(zoneID, "CanyonNorthMouth");
					}
					if (mazeCell.S)
					{
						ZM.AddZoneMidBuilder(zoneID, "CanyonSouthMouth");
					}
					if (mazeCell.E)
					{
						ZM.AddZoneMidBuilder(zoneID, "CanyonEastMouth");
					}
					if (mazeCell.W)
					{
						ZM.AddZoneMidBuilder(zoneID, "CanyonWestMouth");
					}
					ZM.AddZoneMidBuilder(zoneID, "CanyonBuilder");
					ZM.AddZonePostBuilder(zoneID, "CanyonReacher");
				}
			}
		}
	}

	public void BuildRoadSystems(string WorldID)
	{
		RoadSystem = new uint[240, 75];
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					RoadSystem[i, j] = 0u;
				}
				else
				{
					RoadSystem[i, j] = ROAD_NONE;
				}
			}
		}
		if (WorldID == "JoppaWorld")
		{
			for (int k = 0; k < 160; k++)
			{
				Location2D roadHead = GetRoadHead();
				BuildRoad(WorldID, roadHead.x, roadHead.y);
			}
		}
	}

	public void BuildRoad(string WorldID, int StartX, int StartY)
	{
		ZM = XRLCore.Core.Game.ZoneManager;
		int num = Stat.Random(0, 3);
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadStartMouth");
		RoadSystem[StartX, StartY] |= ROAD_START;
		if (num == 0)
		{
			RoadSystem[StartX, StartY] |= ROAD_NORTH;
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadNorthMouth");
		}
		if (num == 1)
		{
			RoadSystem[StartX, StartY] |= ROAD_EAST;
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadEastMouth");
		}
		if (num == 2)
		{
			RoadSystem[StartX, StartY] |= ROAD_SOUTH;
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadSouthMouth");
		}
		if (num == 3)
		{
			RoadSystem[StartX, StartY] |= ROAD_WEST;
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadWestMouth");
		}
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadBuilder");
		ContinueRoad(StartX, StartY, num, 0);
	}

	public void ContinueRoad(int StartX, int StartY, int Direction, int Depth)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (Stat.Random(0, 100) < 2 + Depth * 5)
		{
			RoadSystem[num, num2] |= ROAD_START;
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			return;
		}
		int num3 = Direction;
		if (50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
		}
		if (Direction == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (Direction == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if (Direction == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (Direction == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || RoadSystem[num5, num6] != 0)
		{
			RoadSystem[num, num2] |= ROAD_START;
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			return;
		}
		if (num3 == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (num3 == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		if (num3 == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (num3 == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if (Stat.Random(0, 100) < 5)
		{
			int num7 = Direction;
			if (num7 == num3)
			{
				int num8 = Stat.Random(0, 1);
				if (num8 == 0)
				{
					num7--;
				}
				if (num8 == 1)
				{
					num7++;
				}
				if (num7 < 0)
				{
					num7 = 3;
				}
				if (num7 > 3)
				{
					num7 = 0;
				}
			}
			if (num7 == 0)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadNorthMouth");
				RoadSystem[StartX, StartY] |= ROAD_NORTH;
			}
			if (num7 == 1)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadEastMouth");
				RoadSystem[StartX, StartY] |= ROAD_EAST;
			}
			if (num7 == 2)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadSouthMouth");
				RoadSystem[StartX, StartY] |= ROAD_SOUTH;
			}
			if (num7 == 3)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadWestMouth");
				RoadSystem[StartX, StartY] |= ROAD_WEST;
			}
			ContinueRoad(num, num2, num7, Depth + 1);
		}
		ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
		if (Options.ShowOverlandEncounters)
		{
			ZM.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.pRender.RenderString = ".";
		}
		ContinueRoad(num, num2, num3, Depth);
	}

	public Location2D GetRiverHead()
	{
		int num = Stat.Random(0, 239);
		int num2 = Stat.Random(0, 74);
		string[] t = new string[3] { "TerrainWater", "TerrainSaltmarsh", "TerrainSaltmarsh2" };
		while (RiverSystem[num, num2] != 0 || !WorldCellHasTerrain("JoppaWorld", num / 3, num2 / 3, t))
		{
			num = Stat.Random(0, 239);
			num2 = Stat.Random(0, 74);
		}
		return Location2D.get(num, num2);
	}

	public void BuildRiverSystems(string WorldID)
	{
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					RiverSystem[i, j] = 0u;
				}
				else
				{
					RiverSystem[i, j] = RIVER_NONE;
				}
			}
		}
		if (WorldID == "JoppaWorld")
		{
			for (int k = 0; k < 160; k++)
			{
				Location2D riverHead = GetRiverHead();
				BuildRiver(WorldID, riverHead.x, riverHead.y);
			}
		}
	}

	public void BuildRiver(string WorldID, int StartX, int StartY)
	{
		ZM = The.ZoneManager;
		int num = Stat.Random(0, 3);
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverStartMouth");
		RiverSystem[StartX, StartY] |= RIVER_START;
		if (num == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverNorthMouth");
			RiverSystem[StartX, StartY] |= RIVER_NORTH;
		}
		if (num == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverEastMouth");
			RiverSystem[StartX, StartY] |= RIVER_EAST;
		}
		if (num == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverSouthMouth");
			RiverSystem[StartX, StartY] |= RIVER_SOUTH;
		}
		if (num == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverWestMouth");
			RiverSystem[StartX, StartY] |= RIVER_WEST;
		}
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverBuilder");
		ContinueRiver(StartX, StartY, num, 0);
	}

	public List<Location2D> BuildMamonRiver(string WorldID, int StartX, int StartY, int Direction, int Bias, int MinimumLength, int ExcludeDirection)
	{
		ZM = XRLCore.Core.Game.ZoneManager;
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverStartMouth");
		RiverSystem[StartX, StartY] |= RIVER_START;
		if (Direction == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverNorthMouth");
			RiverSystem[StartX, StartY] |= RIVER_NORTH;
		}
		if (Direction == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverEastMouth");
			RiverSystem[StartX, StartY] |= RIVER_EAST;
		}
		if (Direction == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverSouthMouth");
			RiverSystem[StartX, StartY] |= RIVER_SOUTH;
		}
		if (Direction == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverWestMouth");
			RiverSystem[StartX, StartY] |= RIVER_WEST;
		}
		ZM.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverBuilder");
		List<Location2D> list = new List<Location2D>();
		ContinueMamonRiver(StartX, StartY, Direction, 0, Bias, MinimumLength, ExcludeDirection, list);
		return list;
	}

	public void ContinueRiver(int StartX, int StartY, int Direction, int Depth)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (Stat.Random(0, 100) < Depth - 8)
		{
			RiverSystem[num, num2] |= RIVER_START;
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		int num3 = Direction;
		if (Stat.Random(1, 100) < 20)
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
		}
		if (Direction == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (Direction == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		if (Direction == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (Direction == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 >= 75 || num5 >= 240 || RiverSystem[num5, num6] != 0)
		{
			RiverSystem[num, num2] |= RIVER_START;
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		if (num3 == 0)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (num3 == 1)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		if (num3 == 2)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (num3 == 3)
		{
			ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		bool flag = false;
		if (5.in100())
		{
			flag = true;
			int num7 = Direction;
			if (num7 == num3)
			{
				int num8 = Stat.Random(0, 1);
				if (num8 == 0)
				{
					num7--;
				}
				if (num8 == 1)
				{
					num7++;
				}
				if (num7 < 0)
				{
					num7 = 3;
				}
				if (num7 > 3)
				{
					num7 = 0;
				}
			}
			if (num7 == 0)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverNorthMouth");
			}
			if (num7 == 1)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverEastMouth");
			}
			if (num7 == 2)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverSouthMouth");
			}
			if (num7 == 3)
			{
				ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverWestMouth");
			}
			ContinueRiver(num, num2, num7, Depth + 1);
		}
		ZM.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
		if (Options.ShowOverlandEncounters)
		{
			Render pRender = ZM.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.pRender;
			pRender.RenderString = (flag ? "T" : "~");
			pRender.Tile = null;
		}
		ContinueRiver(num, num2, num3, Depth + 1);
	}

	public void ContinueMamonRiver(int StartX, int StartY, int Direction, int Depth, int Bias, int MinimumLength, int ExcludeDirection, List<Location2D> Points)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		mutableMap.RemoveMutableLocation(Location2D.get(num, num2));
		Points.Add(Location2D.get(num, num2));
		if (Depth > MinimumLength && Stat.Random(0, 100) < 2 + Depth * 5)
		{
			RiverSystem[num, num2] |= RIVER_START;
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		int num3 = Direction;
		if (Bias <= 0 && 50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
			if (num3 == ExcludeDirection)
			{
				num3 = Direction;
			}
		}
		if (Direction == 0)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (Direction == 1)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		if (Direction == 2)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (Direction == 3)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || RiverSystem[num5, num6] != 0)
		{
			RiverSystem[num, num2] |= RIVER_START;
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		if (num3 == 0)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (num3 == 1)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		if (num3 == 2)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (num3 == 3)
		{
			ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		bool flag = false;
		ZM.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
		if (Options.ShowOverlandEncounters)
		{
			Render render = ZM.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.GetPart("Render") as Render;
			render.RenderString = "=";
			if (flag)
			{
				render.RenderString = "t";
			}
		}
		ContinueMamonRiver(num, num2, num3, Depth, Bias - 1, MinimumLength, ExcludeDirection, Points);
	}

	public override bool BuildWorld(string worldName)
	{
		extensions = ModManager.GetInstancesWithAttribute<IJoppaWorldBuilderExtension>(typeof(JoppaWorldBuilderExtension));
		World = worldName;
		worldInfo = new WorldInfo();
		XRLCore.Core.Game.ObjectGameState.Add("JoppaWorldInfo", worldInfo);
		extensions.ForEach(delegate(IJoppaWorldBuilderExtension e)
		{
			e.OnBeforeBuild(this);
		});
		MetricsManager.rngCheckpoint("JWB start");
		JournalAPI.SuspendSorting();
		ZM = XRLCore.Core.Game.ZoneManager;
		MetricsManager.rngCheckpoint("mazes");
		WorldCreationProgress.NextStep("Building world connection maps", 1);
		BuildMazes();
		MetricsManager.rngCheckpoint("mutable");
		WorldCreationProgress.NextStep("Building mutable world encounters", WorldFactory.Factory.countWorlds() * 80);
		BuildMutableEncounters();
		if (The.Game.AlternateStart)
		{
			Factions.get("Joppa").Visible = false;
		}
		base.game.SetStringGameState("BlackOrbLocation", "OrbWorld.40.15.1.1.12@19,7");
		JournalAPI.ResumeSorting();
		MetricsManager.rngCheckpoint("embark");
		if (base.game.GetStringGameState("RuinedJoppa", "No") == "Yes")
		{
			Factions.get("Joppa").Visible = false;
			Cell currentCell = base.game.ZoneManager.GetZone("JoppaWorld").GetFirstObject((GameObject o) => o.Blueprint == "TerrainJoppa").GetCurrentCell();
			currentCell.Clear();
			currentCell.AddObject("TerrainJoppaRuins");
			base.game.ZoneManager.SetZoneName("JoppaWorld.11.22.1.1.10", "abandoned village");
			for (int i = 0; i <= 2; i++)
			{
				for (int j = 0; j <= 2; j++)
				{
					if (i != 1 || j != 1)
					{
						base.game.ZoneManager.SetZoneName("JoppaWorld.11.22." + i + "." + j + ".10", "salt marsh", "");
					}
				}
			}
		}
		extensions.ForEach(delegate(IJoppaWorldBuilderExtension e)
		{
			e.OnAfterBuild(this);
		});
		return true;
	}

	public void BuildTeleportGates(string WorldID)
	{
		List<string> list = GenerateTeleportGateZones(WorldID);
		base.game.SetObjectGameState("JoppaWorldTeleportGateZones", list);
		PlaceTeleportGates(WorldID, list);
	}

	public List<string> GenerateTeleportGateZones(string WorldID)
	{
		List<string> list = new List<string>();
		if (WorldID != "JoppaWorld")
		{
			return list;
		}
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 80; j++)
			{
				GameObject firstObjectWithPart = WorldZone.GetCell(j, i).GetFirstObjectWithPart("TerrainTravel");
				if (firstObjectWithPart == null)
				{
					continue;
				}
				if (firstObjectWithPart.Blueprint.Contains("TerrainRuins"))
				{
					for (int k = 0; k <= 2; k++)
					{
						for (int l = 0; l <= 2; l++)
						{
							if (TELEPORT_GATE_RUINS_SURFACE_PERMILLAGE_CHANCE.in1000())
							{
								string text = Zone.XYToID("JoppaWorld", j * 3 + l, i * 3 + k, 10);
								list.Add(text);
								if (TELEPORT_GATE_DEBUG)
								{
									Debug.LogError("GATE FOR " + text + " " + The.ZoneManager.GetZoneDisplayName(text) + " BASED ON RUINS SURFACE");
								}
							}
							for (int m = 1; m <= TELEPORT_GATE_RUINS_DEPTH; m++)
							{
								if (TELEPORT_GATE_RUINS_DEEP_PERMILLAGE_CHANCE.in1000())
								{
									string text2 = Zone.XYToID("JoppaWorld", j * 3 + l, i * 3 + k, 10 + m);
									list.Add(text2);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text2 + " " + The.ZoneManager.GetZoneDisplayName(text2) + " BASED ON RUINS DEEP");
									}
								}
							}
						}
					}
				}
				else
				{
					if (!firstObjectWithPart.Blueprint.Contains("TerrainBaroqueRuins"))
					{
						continue;
					}
					for (int n = 0; n <= 2; n++)
					{
						for (int num = 0; num <= 2; num++)
						{
							if (TELEPORT_GATE_BAROQUE_RUINS_SURFACE_PERMILLAGE_CHANCE.in1000())
							{
								string text3 = Zone.XYToID("JoppaWorld", j * 3 + num, i * 3 + n, 10);
								list.Add(text3);
								if (TELEPORT_GATE_DEBUG)
								{
									Debug.LogError("GATE FOR " + text3 + " " + The.ZoneManager.GetZoneDisplayName(text3) + " BASED ON BAROQUE RUINS SURFACE");
								}
							}
							for (int num2 = 1; num2 <= TELEPORT_GATE_BAROQUE_RUINS_DEPTH; num2++)
							{
								if (TELEPORT_GATE_BAROQUE_RUINS_DEEP_PERMILLAGE_CHANCE.in1000())
								{
									string text4 = Zone.XYToID("JoppaWorld", j * 3 + num, i * 3 + n, 10 + num2);
									list.Add(text4);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text4 + " " + The.ZoneManager.GetZoneDisplayName(text4) + " BASED ON BAROQUE RUINS DEEP");
									}
								}
							}
						}
					}
				}
			}
		}
		foreach (JournalMapNote mapNote in JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") || note.Has("historic")))
		{
			if (mapNote.Has("historic"))
			{
				string text5 = Zone.XYToID("JoppaWorld", mapNote.x, mapNote.y, 10);
				if (TELEPORT_GATE_HISTORIC_SITE_SURFACE_PERMILLAGE_CHANCE.in1000() && !list.Contains(text5))
				{
					list.Add(text5);
					if (TELEPORT_GATE_DEBUG)
					{
						Debug.LogError("GATE FOR " + text5 + " " + The.ZoneManager.GetZoneDisplayName(text5) + " BASED ON HISTORIC RUIN SURFACE");
					}
				}
				string text6 = The.ZoneManager.GetZoneProperty(text5, "HistoricSite") as string;
				if (string.IsNullOrEmpty(text6))
				{
					continue;
				}
				for (int num3 = 1; num3 <= TELEPORT_GATE_HISTORIC_SITE_CHECK_DEPTH; num3++)
				{
					string text7 = Zone.XYToID("JoppaWorld", mapNote.x, mapNote.y, 10 + num3);
					try
					{
						The.ZoneManager.GetZoneDisplayName(text7);
					}
					catch (Exception)
					{
					}
					if (The.ZoneManager.GetZoneProperty(text7, "HistoricSite") != text6)
					{
						break;
					}
					if (TELEPORT_GATE_HISTORIC_SITE_DEEP_PERMILLAGE_CHANCE.in1000() && !list.Contains(text7))
					{
						list.Add(text7);
						if (TELEPORT_GATE_DEBUG)
						{
							Debug.LogError("GATE FOR " + text7 + " " + The.ZoneManager.GetZoneDisplayName(text7) + " BASED ON HISTORIC RUIN DEEP");
						}
					}
				}
			}
			else
			{
				if (!TELEPORT_GATE_SECRET_RUIN_PERMILLAGE_CHANCE.in1000())
				{
					continue;
				}
				string text8 = Zone.XYToID("JoppaWorld", mapNote.x, mapNote.y, 10);
				if (!list.Contains(text8))
				{
					list.Add(text8);
					if (TELEPORT_GATE_DEBUG)
					{
						Debug.LogError("GATE FOR " + text8 + " " + The.ZoneManager.GetZoneDisplayName(text8) + " BASED ON SECRET RUIN");
					}
				}
			}
		}
		int num4 = list.Count * TELEPORT_GATE_RANDOM_PROPORTION / 100;
		for (int num5 = 0; num5 < num4; num5++)
		{
			int z = (TELEPORT_GATE_RANDOM_SURFACE_TARGET_PERCENTAGE_CHANCE.in100() ? 10 : Stat.Random(11, TELEPORT_GATE_RANDOM_DEEP_TARGET_DEPTH));
			int num6 = Stat.Random(0, 79);
			int num7 = Stat.Random(0, 24);
			int num8 = Stat.Random(0, 2);
			int num9 = Stat.Random(0, 2);
			string text9 = Zone.XYToID("JoppaWorld", num6 * 3 + num8, num7 * 3 + num9, z);
			if (list.Contains(text9))
			{
				num5--;
				continue;
			}
			list.Add(text9);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("GATE FOR " + text9 + " " + The.ZoneManager.GetZoneDisplayName(text9) + " BASED ON RANDOM " + (num5 + 1) + " OF " + num4);
			}
		}
		return list;
	}

	public void PlaceTeleportGates(string WorldID, List<string> zones = null)
	{
		if (zones == null)
		{
			zones = GenerateTeleportGateZones(WorldID);
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		foreach (string zone in zones)
		{
			switch (Stat.Random(0, 2))
			{
			case 0:
				list2.Add(zone);
				break;
			case 1:
				list3.Add(zone);
				break;
			case 2:
				list4.Add(zone);
				break;
			}
		}
		list2.ShuffleInPlace();
		list3.ShuffleInPlace();
		list4.ShuffleInPlace();
		while (list2.Count % 2 != 0)
		{
			string randomElement = list2.GetRandomElement();
			list.Add(randomElement);
			list2.Remove(randomElement);
		}
		while (list3.Count % 3 != 0)
		{
			string randomElement2 = list3.GetRandomElement();
			list.Add(randomElement2);
			list3.Remove(randomElement2);
		}
		while (list4.Count % 4 != 0)
		{
			string randomElement3 = list4.GetRandomElement();
			list.Add(randomElement3);
			list4.Remove(randomElement3);
		}
		List<List<string>> list5 = new List<List<string>>();
		List<List<string>> list6 = new List<List<string>>();
		List<List<string>> list7 = new List<List<string>>();
		Dictionary<string, List<List<string>>> nameRootMap = new Dictionary<string, List<List<string>>>(64);
		int i = 0;
		for (int count = list2.Count; i < count; i += 2)
		{
			string text = list2[i];
			string text2 = list2[i + 1];
			The.ZoneManager.SetZoneProperty(text, "TeleportGateDestinationZone", text2);
			The.ZoneManager.SetZoneProperty(text2, "TeleportGateDestinationZone", text);
			The.ZoneManager.SetZoneProperty(text, "TeleportGateRingSize", 2);
			The.ZoneManager.SetZoneProperty(text2, "TeleportGateRingSize", 2);
			List<string> list8 = new List<string> { text, text2 };
			list5.Add(list8);
			ConfigureRingName(list8, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 2-RING FROM " + text + " " + The.ZoneManager.GetZoneDisplayName(text) + " TO " + text2 + " " + The.ZoneManager.GetZoneDisplayName(text2));
			}
		}
		int j = 0;
		for (int count2 = list3.Count; j < count2; j += 3)
		{
			string text3 = list3[j];
			string text4 = list3[j + 1];
			string text5 = list3[j + 2];
			The.ZoneManager.SetZoneProperty(text3, "TeleportGateDestinationZone", text4);
			The.ZoneManager.SetZoneProperty(text4, "TeleportGateDestinationZone", text5);
			The.ZoneManager.SetZoneProperty(text5, "TeleportGateDestinationZone", text3);
			The.ZoneManager.SetZoneProperty(text3, "TeleportGateRingSize", 3);
			The.ZoneManager.SetZoneProperty(text4, "TeleportGateRingSize", 3);
			The.ZoneManager.SetZoneProperty(text5, "TeleportGateRingSize", 3);
			List<string> list9 = new List<string> { text3, text4, text5 };
			list6.Add(list9);
			ConfigureRingName(list9, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 3-RING FROM " + text3 + " " + The.ZoneManager.GetZoneDisplayName(text3) + " TO " + text4 + " " + The.ZoneManager.GetZoneDisplayName(text4) + " TO " + text5 + " " + The.ZoneManager.GetZoneDisplayName(text5));
			}
		}
		int k = 0;
		for (int count3 = list4.Count; k < count3; k += 4)
		{
			string text6 = list4[k];
			string text7 = list4[k + 1];
			string text8 = list4[k + 2];
			string text9 = list4[k + 3];
			The.ZoneManager.SetZoneProperty(text6, "TeleportGateDestinationZone", text7);
			The.ZoneManager.SetZoneProperty(text7, "TeleportGateDestinationZone", text8);
			The.ZoneManager.SetZoneProperty(text8, "TeleportGateDestinationZone", text9);
			The.ZoneManager.SetZoneProperty(text9, "TeleportGateDestinationZone", text6);
			The.ZoneManager.SetZoneProperty(text6, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text7, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text8, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text9, "TeleportGateRingSize", 4);
			List<string> list10 = new List<string> { text6, text7, text8, text9 };
			list7.Add(list10);
			ConfigureRingName(list10, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 4-RING FROM " + text6 + " " + The.ZoneManager.GetZoneDisplayName(text6) + " TO " + text7 + " " + The.ZoneManager.GetZoneDisplayName(text7) + " TO " + text8 + " " + The.ZoneManager.GetZoneDisplayName(text8) + " TO " + text9 + " " + The.ZoneManager.GetZoneDisplayName(text9));
			}
		}
		foreach (string item in list)
		{
			int num = 0;
			string randomElement4;
			do
			{
				randomElement4 = zones.GetRandomElement();
			}
			while (randomElement4 == item && ++num < 10);
			The.ZoneManager.SetZoneProperty(item, "TeleportGateDestinationZone", randomElement4);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED SECANT FROM " + item + " " + The.ZoneManager.GetZoneDisplayName(item) + " TO " + randomElement4 + " " + The.ZoneManager.GetZoneDisplayName(randomElement4));
			}
		}
		The.Game.SetObjectGameState("JoppaWorldTeleportGate2Rings", list5);
		The.Game.SetObjectGameState("JoppaWorldTeleportGate3Rings", list6);
		The.Game.SetObjectGameState("JoppaWorldTeleportGate4Rings", list7);
		The.Game.SetObjectGameState("JoppaWorldTeleportGateSecants", list);
		foreach (string zone2 in zones)
		{
			The.ZoneManager.AddZonePostBuilderAfterTerrain(zone2, "ZoneTemplate:RingGate");
		}
	}

	private string GetNameRoot(List<string> ring)
	{
		string text = null;
		List<string> list = null;
		foreach (string item in ring)
		{
			string text2 = The.ZoneManager.GetZoneProperty(item, "TeleportGateCandidateNameRoot") as string;
			if (!string.IsNullOrEmpty(text2))
			{
				list?.Add(text2);
				if (text == null)
				{
					text = text2;
					continue;
				}
				list = new List<string> { text, text2 };
			}
		}
		if (list != null)
		{
			return list.GetRandomElement();
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site");
	}

	private void ConfigureRingName(List<string> ring, Dictionary<string, List<List<string>>> nameRootMap)
	{
		string text = GetNameRoot(ring);
		List<List<string>> value;
		bool flag = nameRootMap.TryGetValue(text, out value);
		if (flag)
		{
			switch (value.Count)
			{
			case 1:
			{
				string text2 = "Aleph-" + text;
				switch (value[0].Count)
				{
				case 1:
					The.ZoneManager.SetZoneProperty(value[0][0], "TeleportGateName", text2 + " moon gate");
					The.ZoneManager.SetZoneProperty(value[0][0], "TeleportGateNameRoot", text2);
					break;
				case 2:
					The.ZoneManager.SetZoneProperty(value[0][1], "TeleportGateName", text2 + " sun gate");
					The.ZoneManager.SetZoneProperty(value[0][1], "TeleportGateNameRoot", text2);
					goto case 1;
				case 3:
					The.ZoneManager.SetZoneProperty(value[0][2], "TeleportGateName", text2 + " fool gate");
					The.ZoneManager.SetZoneProperty(value[0][2], "TeleportGateNameRoot", text2);
					goto case 2;
				default:
					The.ZoneManager.SetZoneProperty(value[0][3], "TeleportGateName", text2 + " milk gate");
					The.ZoneManager.SetZoneProperty(value[0][3], "TeleportGateNameRoot", text2);
					goto case 3;
				case 0:
					break;
				}
				text = "Bet-" + text;
				break;
			}
			case 2:
				text = "Gimel-" + text;
				break;
			case 3:
				text = "Daled-" + text;
				break;
			default:
				text = "He-" + text;
				break;
			case 0:
				break;
			}
		}
		switch (ring.Count)
		{
		case 1:
			The.ZoneManager.SetZoneProperty(ring[0], "TeleportGateName", text + " moon gate");
			The.ZoneManager.SetZoneProperty(ring[0], "TeleportGateNameRoot", text);
			break;
		case 2:
			The.ZoneManager.SetZoneProperty(ring[1], "TeleportGateName", text + " sun gate");
			The.ZoneManager.SetZoneProperty(ring[1], "TeleportGateNameRoot", text);
			goto case 1;
		case 3:
			The.ZoneManager.SetZoneProperty(ring[2], "TeleportGateName", text + " fool gate");
			The.ZoneManager.SetZoneProperty(ring[2], "TeleportGateNameRoot", text);
			goto case 2;
		default:
			The.ZoneManager.SetZoneProperty(ring[3], "TeleportGateName", text + " milk gate");
			The.ZoneManager.SetZoneProperty(ring[3], "TeleportGateNameRoot", text);
			goto case 3;
		case 0:
			break;
		}
		if (!flag)
		{
			nameRootMap[text] = new List<List<string>> { ring };
		}
	}
}

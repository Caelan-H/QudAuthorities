using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World;

[Serializable]
public class Zone
{
	public const int NAV_SMART = 1;

	public const int NAV_BURROWER = 2;

	public const int NAV_AUTOEXPLORING = 4;

	public const int NAV_FLYING = 8;

	public const int NAV_WALL_WALKER = 16;

	public const int NAV_IGNORES_WALLS = 32;

	public const int NAV_SWIMMING = 64;

	public const int NAV_SLIMEWALKING = 128;

	public const int NAV_AQUATIC = 256;

	public const int NAV_POLYPWALKING = 512;

	public const int NAV_STRUTWALKING = 1024;

	public const int NAV_JUGGERNAUT = 2048;

	public const int NAV_REEFER = 4096;

	public const int NAV_LAZY_INIT = 268435456;

	public const int NAV_REEFWALKING = 1536;

	public const int REEFER_NAV_WEIGHT = 25;

	public bool bSuspended;

	public long LastActive;

	public long GeneratedOn;

	[NonSerialized]
	public long LastPlayerPresence = -1L;

	public string GroundLiquid = "salt-1000";

	[NonSerialized]
	private static LocationList _locationList;

	[NonSerialized]
	private int _NewTier;

	public string _ZoneID;

	public string ZoneWorld = "";

	public int BaseTemperature = 25;

	public int Tier = 1;

	public int BuildTries;

	public int X;

	public int Y;

	public int Z;

	public int wX;

	public int wY;

	public bool Built;

	public int Width;

	public int Height;

	[NonSerialized]
	public static int[,] SoundWalls = new int[80, 25];

	[NonSerialized]
	public static InfluenceMap SoundMap = new InfluenceMap(80, 25);

	public static bool SoundMapDirty = true;

	public LightLevel[] LightMap;

	public bool[] ExploredMap;

	public bool[] FakeUnexploredMap;

	public bool[] VisibilityMap;

	public bool[,] ReachableMap;

	public NavigationWeight[,] NavigationMap;

	[NonSerialized]
	public List<CachedZoneConnection> ZoneConnectionCache = new List<CachedZoneConnection>();

	[NonSerialized]
	public Cell[][] Map;

	[NonSerialized]
	public MissileMapType[][] MissileMap;

	private string region;

	private string _DisplayName;

	public static Dictionary<string, int[,]> LOSCache = null;

	public static int LOSCacheValue = 0;

	[NonSerialized]
	private static CleanQueue<Location2D> CellQueue = new CleanQueue<Location2D>();

	[NonSerialized]
	private static Dictionary<Location2D, bool> VisitedList = new Dictionary<Location2D, bool>(2000);

	[NonSerialized]
	private static int FloodValue = 0;

	[NonSerialized]
	public int[,] FloodVisMap = new int[80, 25];

	[NonSerialized]
	private static List<Cell> FloodNext = new List<Cell>(64);

	[NonSerialized]
	private static List<int> FloodNextRadii = new List<int>(64);

	[NonSerialized]
	private static List<Cell> FloodCurrent = new List<Cell>(64);

	[NonSerialized]
	private static List<int> FloodCurrentRadii = new List<int>(64);

	[NonSerialized]
	private static Dictionary<Cell, int> Flooded = null;

	[NonSerialized]
	private static string InFloodMethod = null;

	[NonSerialized]
	public int RenderedObjects;

	public static List<GameObject> wantsToPaint = new List<GameObject>();

	[NonSerialized]
	private List<Cell> CellList;

	[NonSerialized]
	public List<IEvent> QueuedEvents;

	[NonSerialized]
	public List<IEvent> QueuedEventsToFire;

	[NonSerialized]
	private static ZoneThawedEvent eZoneThawed = new ZoneThawedEvent();

	[NonSerialized]
	private static ZoneActivatedEvent eZoneActivated = new ZoneActivatedEvent();

	[NonSerialized]
	private static ZoneDeactivatedEvent eZoneDeactivated = new ZoneDeactivatedEvent();

	[NonSerialized]
	private static SynchronizeExistenceEvent eSynchronizeExistence = new SynchronizeExistenceEvent();

	public LocationList area
	{
		get
		{
			if (_locationList == null)
			{
				List<Location2D> list = new List<Location2D>();
				for (int i = 0; i < 80; i++)
				{
					for (int j = 0; j < 25; j++)
					{
						list.Add(Location2D.get(i, j));
					}
				}
				_locationList = new LocationList(list);
			}
			return _locationList;
		}
	}

	public int Level => NewTier * 5;

	public int NewTier
	{
		get
		{
			if (_NewTier > 0)
			{
				return _NewTier;
			}
			string text = The.ZoneManager.GetZoneProperty(ZoneID, "ZoneTierOverride") as string;
			if (!text.IsNullOrEmpty() && int.TryParse(text, out _NewTier))
			{
				return _NewTier;
			}
			ZoneBlueprint blueprint = GetBlueprint();
			if (blueprint != null && blueprint.Tier > 0)
			{
				return _NewTier = blueprint.Tier;
			}
			text = GetTerrainObject()?.GetTag("RegionTier") ?? "1";
			if (!int.TryParse(text, out _NewTier))
			{
				_NewTier = 1;
			}
			if (Z > 15)
			{
				_NewTier = Math.Abs(Z - 16) / 5 + 2;
			}
			if (_NewTier < 1)
			{
				_NewTier = 1;
			}
			if (_NewTier > 8)
			{
				_NewTier = 8;
			}
			return _NewTier;
		}
	}

	public Location2D resolvedLocation => Location2D.get(resolvedX, resolvedY);

	public int resolvedX => wX * 3 + X;

	public int resolvedY => wY * 3 + Y;

	public string ZoneID
	{
		get
		{
			return _ZoneID;
		}
		set
		{
			_ZoneID = value;
			if (_ZoneID.Contains("."))
			{
				string[] array = _ZoneID.Split('.');
				ZoneWorld = array[0];
				wX = Convert.ToInt32(array[1]);
				wY = Convert.ToInt32(array[2]);
				X = Convert.ToInt32(array[3]);
				Y = Convert.ToInt32(array[4]);
				Z = Convert.ToInt32(array[5]);
			}
			else
			{
				ZoneWorld = value;
			}
		}
	}

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				_DisplayName = The.ZoneManager.GetZoneDisplayName(ZoneID);
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
			The.ZoneManager.SetZoneDisplayName(ZoneID, value, Sync: false);
		}
	}

	public string BaseDisplayName
	{
		get
		{
			return The.ZoneManager.GetZoneBaseDisplayName(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneBaseDisplayName(ZoneID, value);
		}
	}

	public string ReferenceDisplayName => The.ZoneManager.GetZoneReferenceDisplayName(ZoneID);

	public string NameContext
	{
		get
		{
			return The.ZoneManager.GetZoneNameContext(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNameContext(ZoneID, value);
		}
	}

	public bool HasProperName
	{
		get
		{
			return The.ZoneManager.GetZoneHasProperName(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneHasProperName(ZoneID, value);
		}
	}

	public string IndefiniteArticle
	{
		get
		{
			return The.ZoneManager.GetZoneIndefiniteArticle(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIndefiniteArticle(ZoneID, value);
		}
	}

	public string DefiniteArticle
	{
		get
		{
			return The.ZoneManager.GetZoneDefiniteArticle(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneDefiniteArticle(ZoneID, value);
		}
	}

	public bool IncludeContextInZoneDisplay
	{
		get
		{
			return The.ZoneManager.GetZoneIncludeContextInZoneDisplay(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIncludeContextInZoneDisplay(ZoneID, value);
		}
	}

	public bool IncludeStratumInZoneDisplay
	{
		get
		{
			return The.ZoneManager.GetZoneIncludeStratumInZoneDisplay(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(ZoneID, value);
		}
	}

	public bool NamedByPlayer
	{
		get
		{
			return The.ZoneManager.GetZoneNamedByPlayer(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNamedByPlayer(ZoneID, value);
		}
	}

	public bool HasWeather
	{
		get
		{
			return The.ZoneManager.GetZoneHasWeather(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneHasWeather(ZoneID, value);
		}
	}

	public string WindSpeed
	{
		get
		{
			return The.ZoneManager.GetZoneWindSpeed(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindSpeed(ZoneID, value);
		}
	}

	public string WindDirections
	{
		get
		{
			return The.ZoneManager.GetZoneWindDirections(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindDirections(ZoneID, value);
		}
	}

	public string WindDuration
	{
		get
		{
			return The.ZoneManager.GetZoneWindDuration(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindDuration(ZoneID, value);
		}
	}

	public int CurrentWindSpeed
	{
		get
		{
			return The.ZoneManager.GetZoneCurrentWindSpeed(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneCurrentWindSpeed(ZoneID, value);
		}
	}

	public string CurrentWindDirection
	{
		get
		{
			return The.ZoneManager.GetZoneCurrentWindDirection(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneCurrentWindDirection(ZoneID, value);
		}
	}

	public long NextWindChange
	{
		get
		{
			return The.ZoneManager.GetZoneNextWindChange(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNextWindChange(ZoneID, value);
		}
	}

	public string DebugName => ZoneID ?? "null-ID zone";

	public string GetCheckpointKey()
	{
		return GetCell(0, 0)?.GetFirstObject("CheckpointWidget")?.GetStringProperty("CheckpointKey");
	}

	public bool IsCheckpoint()
	{
		return GetCell(0, 0)?.HasObject("CheckpointWidget") ?? false;
	}

	public void Release()
	{
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject item in new List<GameObject>(cell.Objects))
			{
				GameObjectFactory.Factory.Pool(item, allowGameObjectPool: true);
			}
			cell.Objects.Clear();
		}
	}

	public void MarkActive()
	{
		LastActive = XRLCore.CurrentTurn;
	}

	public void MarkActive(Zone AlongWith)
	{
		LastActive = XRLCore.CurrentTurn;
		if (AlongWith != null && AlongWith != this)
		{
			AlongWith.MarkActive();
		}
	}

	public string GetDefaultWall()
	{
		string zoneProperty = GetZoneProperty("DefaultWall");
		if (zoneProperty != "")
		{
			return zoneProperty;
		}
		string text = "Shale";
		GameObject terrainObject = GetTerrainObject();
		if (terrainObject != null && Z <= 15)
		{
			text = terrainObject.GetTag("DefaultWall", "Shale");
		}
		if ((text == "Shale" || text == "Limestone") && Z > 49)
		{
			text = "Granite";
		}
		return text;
	}

	public void loadMap(string file)
	{
		MapBuilder mapBuilder = new MapBuilder();
		mapBuilder.FileName = file;
		mapBuilder.BuildZone(this);
	}

	public void SetActive()
	{
		The.ZoneManager.SetActiveZone(ZoneID);
	}

	public void AddSemanticTag(string newTag)
	{
		string text = GetZoneProperty("SemanticTags");
		if (text != "")
		{
			text += ",";
		}
		text += newTag;
		SetZoneProperty("SemanticTags", text);
	}

	public List<string> GetSemanticTags()
	{
		return GetZoneProperty("SemanticTags", "*Default").Split(',').ToList();
	}

	public string getZoneWorld()
	{
		if (ZoneID == null)
		{
			return null;
		}
		if (!ZoneID.Contains("."))
		{
			return ZoneID;
		}
		return ZoneID.Substring(0, ZoneID.IndexOf('.'));
	}

	public void SetInfluenceMapWalls(int[,] Ret)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.HasObjectWithIntProperty("Wall") || cell.HasObjectWithTag("EnsureVoidBlocker") || cell.HasObjectWithTag("InfluenceMapBlocker"))
				{
					Ret[j, i] = 1;
				}
				else
				{
					Ret[j, i] = 0;
				}
			}
		}
	}

	public void SetInfluenceMapWallsDoors(int[,] Ret)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.HasObjectWithIntProperty("Wall") || cell.HasObjectWithPart("Door"))
				{
					Ret[j, i] = 1;
				}
				else
				{
					Ret[j, i] = 0;
				}
			}
		}
	}

	public void UpdateSoundMap()
	{
		if (!SoundMapDirty)
		{
			return;
		}
		if (The.Player.CurrentCell != null)
		{
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					Cell cell = GetCell(j, i);
					if (cell.HasWall() && cell.IsOccludingFor(The.Player))
					{
						SoundWalls[j, i] = 1;
					}
					else
					{
						SoundWalls[j, i] = 0;
					}
				}
			}
			SoundMap.bDraw = false;
			SoundMap.ClearSeeds();
			SoundMap.Walls = SoundWalls;
			SoundMap.AddSeed(The.Player.CurrentCell.location, bRecalculate: false);
			SoundMap.RecalculateCostOnly();
		}
		SoundMapDirty = false;
	}

	public int defaultPathfinderWeightFunc(int x, int y, Cell cell)
	{
		if (cell.HasWall())
		{
			return 9999;
		}
		if (cell.IsSolid())
		{
			return 9999;
		}
		return 0;
	}

	public Pathfinder getPathfinder(Func<int, int, Cell, int> weightFunc = null)
	{
		if (weightFunc == null)
		{
			weightFunc = defaultPathfinderWeightFunc;
		}
		Pathfinder pathfinder = new Pathfinder(Width, Height);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				pathfinder.CurrentNavigationMap[j, i] = weightFunc(j, i, GetCell(j, i));
			}
		}
		return pathfinder;
	}

	public void SetInfluenceMapAutoexploreWeightsAndWalls(int[,] Weights, int[,] Walls, bool ExploredOnly = true)
	{
		int nav = CalculateNav(The.Player, Autoexploring: true);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (ExploredOnly && !GetExplored(j, i))
				{
					Weights[j, i] = 100;
					Walls[j, i] = 1;
				}
				else
				{
					Walls[j, i] = (((Weights[j, i] = GetCell(j, i).NavigationWeight(The.Player, nav)) >= 95) ? 1 : 0);
				}
			}
		}
	}

	public int SetInfluenceAutoexploreSeeds(InfluenceMap Map)
	{
		bool autoexploreChests = Options.AutoexploreChests;
		bool autoexploreAutopickups = Options.AutoexploreAutopickups;
		bool autoexploreBookshelves = Options.AutoexploreBookshelves;
		bool flag = true;
		int num = 0;
		Map.ClearSeeds();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				bool flag2 = false;
				Cell cell = GetCell(j, i);
				if ((autoexploreChests || autoexploreAutopickups || autoexploreBookshelves || flag) && !cell.IsSolidFor(The.Player))
				{
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (autoexploreChests && gameObject.HasTag("AutoexploreChest") && !gameObject.HasIntProperty("Autoexplored") && gameObject.Owner == null && gameObject.HasPart("Inventory") && gameObject.Inventory.Objects.Count > 0)
						{
							flag2 = true;
							break;
						}
						if (autoexploreBookshelves && gameObject.HasTag("AutoexploreShelf") && !gameObject.HasIntProperty("Autoexplored") && gameObject.Owner == null && gameObject.Inventory.Objects.Count > 0)
						{
							flag2 = true;
							break;
						}
						if (autoexploreAutopickups && gameObject.ShouldAutoget())
						{
							flag2 = true;
							break;
						}
						if (flag && AutoexploreObjectEvent.Check(The.Player, gameObject))
						{
							flag2 = true;
							break;
						}
					}
				}
				else if (flag)
				{
					int l = 0;
					for (int count2 = cell.Objects.Count; l < count2; l++)
					{
						GameObject gameObject2 = cell.Objects[l];
						if (gameObject2.ConsiderSolidFor(The.Player) && AutoexploreObjectEvent.Check(The.Player, gameObject2))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2 && !cell.IsReallyExplored())
				{
					if (!cell.HasWall())
					{
						flag2 = true;
					}
					else if (cell.HasAdjacentLocalNonwallCell())
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					Map.AddSeed(j, i, bRecalculate: false);
					num++;
				}
			}
		}
		return num;
	}

	public int SetInfluenceMapStairsUp(InfluenceMap Map)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				for (int k = 0; k < cell.Objects.Count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.HasPart("StairsUp") && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell.Explored && (!(gameObject.GetPart("Hidden") is Hidden hidden) || hidden.Found) && (!(gameObject.GetPart("HiddenRender") is HiddenRender hiddenRender) || hiddenRender.Found))
					{
						Map.AddSeed(j, i, bRecalculate: false);
						num++;
					}
				}
			}
		}
		return num;
	}

	public int SetInfluenceMapStairsDown(InfluenceMap Map)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				for (int k = 0; k < cell.Objects.Count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.HasPart("StairsDown") && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell.Explored && (!(gameObject.GetPart("Hidden") is Hidden hidden) || hidden.Found) && (!(gameObject.GetPart("HiddenRender") is HiddenRender hiddenRender) || hiddenRender.Found))
					{
						Map.AddSeed(j, i, bRecalculate: false);
						num++;
					}
				}
			}
		}
		return num;
	}

	public static string XYToID(string World, int xp, int yp, int z)
	{
		int parasangX = xp / 3;
		int parasangY = yp / 3;
		int zoneX = xp % 3;
		int zoneY = yp % 3;
		return XRL.World.ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, z);
	}

	public void FillRoundHollowBox(Box B, string Blueprint)
	{
		if (B.Width < 8 || B.Height < 5)
		{
			for (int i = B.x1 + 1; i <= B.x2 - 1; i++)
			{
				GetCell(i, B.y1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				GetCell(i, B.y2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
			for (int j = B.y1 + 1; j < B.y2; j++)
			{
				GetCell(B.x1, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				GetCell(B.x2, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
			GetCell(B.x1 + 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2 - 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x1 + 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2 - 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			return;
		}
		for (int k = B.x1 + 3; k <= B.x2 - 3; k++)
		{
			GetCell(k, B.y1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(k, B.y2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		}
		for (int l = B.y1 + 2; l <= B.y2 - 2; l++)
		{
			GetCell(B.x1, l).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2, l).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		}
		GetCell(B.x1 + 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 2, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 3, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 2, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 3, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 2, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 3, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 2, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 3, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y1 + 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y1 + 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y2 - 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y2 - 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
	}

	public void ClearWalkableBorders()
	{
		try
		{
			string zoneIDFromDirection = GetZoneIDFromDirection("U");
			if (zoneIDFromDirection != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection))
			{
				Zone zone = The.Game.ZoneManager.CachedZones[zoneIDFromDirection];
				for (int i = 0; i < 80; i++)
				{
					for (int j = 0; j < 24; j++)
					{
						Cell cell = zone.GetCell(i, j);
						if (cell.HasObjectWithBlueprint("OpenShaft"))
						{
							GetCell(i, j).ClearObjectsWithIntProperty("Wall");
						}
						if (cell.HasObjectWithBlueprint("StairsDown"))
						{
							GetCell(i, j).ClearObjectsWithIntProperty("Wall");
							GetCell(i, j).AddObject("StairsUp");
						}
					}
				}
			}
			string zoneIDFromDirection2 = GetZoneIDFromDirection("D");
			if (zoneIDFromDirection2 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection2))
			{
				Zone zone2 = The.Game.ZoneManager.CachedZones[zoneIDFromDirection2];
				for (int k = 0; k < 80; k++)
				{
					for (int l = 0; l < 24; l++)
					{
						if (zone2.GetCell(k, l).HasObjectWithBlueprint("StairsUp"))
						{
							GetCell(k, l).ClearObjectsWithIntProperty("Wall");
							GetCell(k, l).AddObject("StairsDown");
						}
					}
				}
			}
			string zoneIDFromDirection3 = GetZoneIDFromDirection("N");
			if (zoneIDFromDirection3 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection3))
			{
				for (int m = 0; m < 80; m++)
				{
					if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection3].GetCell(m, 24).IsSolid())
					{
						GetCell(m, 0).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			string zoneIDFromDirection4 = GetZoneIDFromDirection("S");
			if (zoneIDFromDirection4 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection4))
			{
				for (int n = 0; n < 80; n++)
				{
					if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection4].GetCell(n, 0).IsSolid())
					{
						GetCell(n, 24).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			string zoneIDFromDirection5 = GetZoneIDFromDirection("E");
			if (zoneIDFromDirection5 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection5))
			{
				for (int num = 0; num < 24; num++)
				{
					if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection5].GetCell(0, num).IsSolid())
					{
						GetCell(79, num).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			string zoneIDFromDirection6 = GetZoneIDFromDirection("W");
			if (zoneIDFromDirection6 == null || !XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection6))
			{
				return;
			}
			for (int num2 = 0; num2 < 24; num2++)
			{
				if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection6].GetCell(0, num2).IsSolid())
				{
					GetCell(0, num2).ClearObjectsWithIntProperty("Wall");
				}
			}
		}
		catch
		{
		}
	}

	public void FillBox(Box B, string Blueprint, bool clearFirst = false)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null)
				{
					if (clearFirst)
					{
						cell.Clear();
					}
					cell.AddObject(GameObject.create(Blueprint));
				}
			}
		}
	}

	public void ProcessHollowBox(Box B, Action<Cell> A)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			A(GetCell(i, B.y1));
			A(GetCell(i, B.y2));
		}
		for (int j = B.y1 + 1; j < B.y2; j++)
		{
			A(GetCell(B.x1, j));
			A(GetCell(B.x2, j));
		}
	}

	public void FillHollowBox(Box B, string Blueprint)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			GetCell(i, B.y1).AddObject(GameObject.create(Blueprint));
			GetCell(i, B.y2).AddObject(GameObject.create(Blueprint));
		}
		for (int j = B.y1 + 1; j < B.y2; j++)
		{
			GetCell(B.x1, j).AddObject(GameObject.create(Blueprint));
			GetCell(B.x2, j).AddObject(GameObject.create(Blueprint));
		}
	}

	public void Fill(string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).AddObject(GameObject.create(Blueprint));
			}
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteObject(this);
		Writer.Write(LastPlayerPresence);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Map[i][j].Save(Writer);
			}
		}
	}

	public static Zone Load(SerializationReader Reader)
	{
		Zone zone = (Zone)Reader.ReadObject();
		zone.Built = false;
		zone.LastPlayerPresence = Reader.ReadInt64();
		zone.FloodVisMap = new int[80, 25];
		zone.Map = new Cell[zone.Width][];
		zone.MissileMap = new MissileMapType[zone.Width][];
		for (int i = 0; i < zone.Width; i++)
		{
			zone.Map[i] = new Cell[zone.Height];
			zone.MissileMap[i] = new MissileMapType[zone.Height];
		}
		for (int j = 0; j < zone.Width; j++)
		{
			for (int k = 0; k < zone.Height; k++)
			{
				zone.Map[j][k] = Cell.Load(Reader, j, k, zone);
			}
		}
		zone.Built = true;
		zone.BroadcastEvent(Event.New("ZoneLoaded"));
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
		return zone;
	}

	public string GetRegion()
	{
		if (region == null)
		{
			region = ZoneManager.GetRegionForZone(this);
		}
		return region;
	}

	public ZoneBlueprint GetBlueprint()
	{
		return The.ZoneManager.GetZoneBlueprint(ZoneWorld, wX, wY, X, Y, Z);
	}

	public GameObject GetTerrainObject()
	{
		return ZoneManager.GetTerrainObjectForZone(wX, wY, ZoneWorld);
	}

	public GameObject GetTerrainObjectFromDirection(string dir)
	{
		int num = wX;
		int num2 = wY;
		if (dir.Contains("n", CompareOptions.IgnoreCase))
		{
			num2--;
		}
		if (dir.Contains("s", CompareOptions.IgnoreCase))
		{
			num2++;
		}
		if (dir.Contains("e", CompareOptions.IgnoreCase))
		{
			num++;
		}
		if (dir.Contains("w", CompareOptions.IgnoreCase))
		{
			num--;
		}
		return ZoneManager.GetTerrainObjectForZone(num, num2, ZoneWorld);
	}

	public string GetTerrainNameFromDirection(string dir)
	{
		int num = wX;
		int num2 = wY;
		if (dir.Contains("n", CompareOptions.IgnoreCase))
		{
			num2--;
		}
		if (dir.Contains("s", CompareOptions.IgnoreCase))
		{
			num2++;
		}
		if (dir.Contains("e", CompareOptions.IgnoreCase))
		{
			num++;
		}
		if (dir.Contains("w", CompareOptions.IgnoreCase))
		{
			num--;
		}
		GameObject terrainObjectForZone = ZoneManager.GetTerrainObjectForZone(num, num2, ZoneWorld);
		if (terrainObjectForZone == null)
		{
			return "";
		}
		return terrainObjectForZone.Blueprint;
	}

	public void ReplaceAll(string blueprint, string with)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				List<GameObject> objects = GetCell(j, i).GetObjects(blueprint);
				if (objects == null || objects.Count <= 0)
				{
					continue;
				}
				foreach (GameObject item in objects)
				{
					item.Destroy();
				}
				GetCell(j, i).AddObject(with);
			}
		}
	}

	public GameObject findObjectById(string id)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).findObjectById(id);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public int GetObjectCount(string Blueprint)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				num += GetCell(j, i).GetObjectCount(Blueprint);
			}
		}
		return num;
	}

	public GameObject FindFirstObject(string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObject = GetCell(j, i).GetFirstObject(Blueprint);
				if (firstObject != null)
				{
					return firstObject;
				}
			}
		}
		return null;
	}

	public GameObject FindFirstObject(Predicate<GameObject> Filter)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObject = GetCell(j, i).GetFirstObject(Filter);
				if (firstObject != null)
				{
					return firstObject;
				}
			}
		}
		return null;
	}

	public void FindObjects(List<GameObject> List, Predicate<GameObject> Filter)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).FindObjects(List, Filter);
			}
		}
	}

	public List<GameObject> FindObjects(Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjects(list, Filter);
		return list;
	}

	public void FindObjects(List<GameObject> List, string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjects(List, Blueprint);
			}
		}
	}

	public List<GameObject> FindObjects(string Blueprint)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjects(list, Blueprint);
		return list;
	}

	public void FindObjectsWithTagOrProperty(List<GameObject> List, string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjectsWithTagOrProperty(List, Name);
			}
		}
	}

	public List<GameObject> FindObjectsWithTagOrProperty(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjectsWithTagOrProperty(list, Name);
		return list;
	}

	public void FindObjectsWithPart(List<GameObject> List, string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjectsWithPart(Name, List);
			}
		}
	}

	public List<GameObject> FindObjectsWithPart(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjectsWithPart(list, Name);
		return list;
	}

	public GameObject FindObject(Predicate<GameObject> test)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObject(test);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindObject(string Blueprint)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				GameObject gameObject = GetCell(i, j).FindObject(Blueprint);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindObjectExcludingSelf(string Blueprint, GameObject self)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObjectExcept(Blueprint, self);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindClosestObjectWithPart(GameObject Source, string Part, bool ExploredOnly = false, bool IncludeSelf = true)
	{
		int num = 9999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.HasPart(Part) && (IncludeSelf || gameObject != Source) && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell != null && gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell) < num && (!ExploredOnly || gameObject.CurrentCell.Explored) && (!(gameObject.GetPart("Hidden") is Hidden hidden) || hidden.Found) && (!(gameObject.GetPart("HiddenRender") is HiddenRender hiddenRender) || hiddenRender.Found))
					{
						num = gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell);
						result = gameObject;
					}
				}
			}
		}
		return result;
	}

	public GameObject FindClosestObjectWithTag(GameObject Source, string Tag, bool ExploredOnly = false)
	{
		int num = 9999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.HasTag(Tag) && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell != null && gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell) < num && (!ExploredOnly || gameObject.CurrentCell.Explored) && (!(gameObject.GetPart("Hidden") is Hidden hidden) || hidden.Found) && (!(gameObject.GetPart("HiddenRender") is HiddenRender hiddenRender) || hiddenRender.Found))
					{
						num = gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell);
						result = gameObject;
					}
				}
			}
		}
		return result;
	}

	public List<GameObject> GetObjects()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				num += Map[j][i].GetObjectCount();
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					list.AddRange(Map[l][k].Objects);
					if (list.Count >= num)
					{
						goto end_IL_0081;
					}
				}
				continue;
				end_IL_0081:
				break;
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsReadonly()
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				list.AddRange(Map[j][i].Objects);
			}
		}
		return list;
	}

	public List<GameObject> GetObjects(string Blueprint)
	{
		List<Cell> cellsWithObject = GetCellsWithObject(Blueprint);
		int num = 0;
		foreach (Cell item in cellsWithObject)
		{
			num += item.GetObjectCount(Blueprint);
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			foreach (Cell item2 in cellsWithObject)
			{
				list.AddRange(item2.GetObjects(Blueprint));
				if (list.Count >= num)
				{
					return list;
				}
			}
			return list;
		}
		return list;
	}

	public IEnumerable<GameObject> YieldObjects()
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int i = 0; i < Map[x][y].Objects.Count; i++)
				{
					yield return Map[x][y].Objects[i];
				}
			}
		}
	}

	public int CountObjects(Predicate<GameObject> pFilter)
	{
		int num = 0;
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject @object in cell.Objects)
			{
				if (@object != null && pFilter(@object))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int CountObjects(string Blueprint)
	{
		int num = 0;
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject @object in cell.Objects)
			{
				if (@object != null && @object.Blueprint == Blueprint)
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<GameObject> GetObjectsNoAlloc(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject @object in cell.Objects)
			{
				if (pFilter(@object))
				{
					list.Add(@object);
				}
			}
		}
		return list;
	}

	public List<GameObject> GetObjects(Predicate<GameObject> pFilter)
	{
		List<Cell> cellsWithObject = GetCellsWithObject(pFilter);
		int num = 0;
		foreach (Cell item in cellsWithObject)
		{
			num += item.GetObjectCount(pFilter);
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			foreach (Cell item2 in cellsWithObject)
			{
				list.AddRange(item2.GetObjects(pFilter));
				if (list.Count >= num)
				{
					return list;
				}
			}
			return list;
		}
		return list;
	}

	public static Location2D zoneIDTo240x72Location(string zoneID)
	{
		string[] array = zoneID.Split('.');
		return Location2D.get(Convert.ToInt32(array[1]) * 3 + Convert.ToInt32(array[3]), Convert.ToInt32(array[2]) * 3 + Convert.ToInt32(array[4]));
	}

	public List<GameObject> GetObjectsThatInheritFrom(string Blueprint)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				list.AddRange(Map[i][j].GetObjectsThatInheritFrom(Blueprint));
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagOrProperty(string Property)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				list.AddRange(Map[i][j].GetObjectsWithTagOrProperty(Property));
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithProperty(string Property)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithProperty(Property, list);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPart(string Part)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithPart(Part, list);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPartReadonly(string Part)
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithPart(Part, list);
			}
		}
		return list;
	}

	public GameObject GetFirstObjectWithPart(string Part)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObjectWithPart = Map[j][i].GetFirstObjectWithPart(Part);
				if (firstObjectWithPart != null)
				{
					return firstObjectWithPart;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObjectWithPropertyOrTag = Map[j][i].GetFirstObjectWithPropertyOrTag(Name);
				if (firstObjectWithPropertyOrTag != null)
				{
					return firstObjectWithPropertyOrTag;
				}
			}
		}
		return null;
	}

	public IEnumerable<GameObject> LoopObjectsWithPart(string Part)
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				foreach (GameObject item in Map[x][y].LoopObjectsWithPart(Part))
				{
					yield return item;
				}
			}
		}
	}

	public void ForeachObjectWithPart(string Part, Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObjectWithPart(Part, aProc);
			}
		}
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObject(aProc);
			}
		}
	}

	public bool HasBuilder(string builderName)
	{
		return The.ZoneManager.ZoneHasBuilder(ZoneID, builderName);
	}

	public bool HasTryingToJoinPartyLeader()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasTryingToJoinPartyLeader())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeaderForZoneUncaching()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasTryingToJoinPartyLeaderForZoneUncaching())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPlayerLed()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasPlayerLed())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasWasPlayer()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasWasPlayer())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLeftBehindByPlayer()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasLeftBehindByPlayer())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachObjectWithTagOrProperty(string Name, Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObjectWithTagOrProperty(Name, aProc);
			}
		}
	}

	public GameObject GetObjectWithTag(string tag)
	{
		List<GameObject> objectsWithTag = GetObjectsWithTag(tag);
		if (objectsWithTag != null && objectsWithTag.Count > 0)
		{
			return objectsWithTag.GetRandomElement();
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTag(string Tag)
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				list.AddRange(Map[j][i].GetObjectsWithTag(Tag));
			}
		}
		return list;
	}

	public GameObject GetAnyObjectWithTag(string Tag)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject objectWithTag = Map[j][i].GetObjectWithTag(Tag);
				if (objectWithTag != null)
				{
					return objectWithTag;
				}
			}
		}
		return null;
	}

	public List<Point> LineFromAngle(int x0, int y0, int degrees)
	{
		float num = (float)degrees / 58f;
		float num2 = (float)Math.Sin(num);
		float num3 = (float)Math.Cos(num);
		List<Point> list = new List<Point>();
		char displaychar = '2';
		float num4 = x0;
		float num5 = y0;
		int num6 = -1;
		int num7 = -1;
		for (int i = 0; i < 10000; i++)
		{
			int num8 = (int)Math.Round(num4, MidpointRounding.AwayFromZero);
			int num9 = (int)Math.Round(num5, MidpointRounding.AwayFromZero);
			if (num8 < 0 || num9 < 0 || num8 >= Width || num9 >= Height)
			{
				break;
			}
			if (num8 != num6 || num9 != num7)
			{
				list.Add(new Point(num8, num9, 0, displaychar));
				if (num6 != -1 && num7 != -1)
				{
					displaychar = ((num8 != num6 && num9 != num7) ? (((num8 <= num6 || num9 <= num7) && (num8 >= num6 || num9 >= num7)) ? '\\' : '/') : ((num8 == num6) ? '|' : '-'));
				}
				num6 = num8;
				num7 = num9;
				num4 += num2;
				num5 += num3;
			}
		}
		list[0].DisplayChar = '@';
		list[list.Count - 1].DisplayChar = 'X';
		return list;
	}

	public static List<Point> Line(int x0, int y0, int x1, int y1)
	{
		List<Point> list = new List<Point>(Math.Abs(y1 - y0) + Math.Abs(x1 - x0) + 1);
		bool flag = false;
		if (x0 == x1 && y0 == y1)
		{
			list.Add(new Point(x0, y0, 0, 'X'));
		}
		else
		{
			bool flag2 = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (flag2)
			{
				int num = x0;
				x0 = y0;
				y0 = num;
				int num2 = x1;
				x1 = y1;
				y1 = num2;
			}
			if (x0 > x1)
			{
				flag = true;
				int num3 = x1;
				x1 = x0;
				x0 = num3;
				int num4 = y1;
				y1 = y0;
				y0 = num4;
			}
			int num5 = x1 - x0;
			int num6 = Math.Abs(y1 - y0);
			char displaychar = '2';
			double num7 = 0.0;
			double num8 = (double)num6 / (double)num5;
			int num9 = 0;
			int num10 = y0;
			num9 = ((y0 < y1) ? 1 : (-1));
			int num11 = 0;
			for (int i = x0; i <= x1; i++)
			{
				num11++;
				if (flag2)
				{
					list.Add(new Point(num10, i, 0, displaychar));
				}
				else
				{
					list.Add(new Point(i, num10, 0, displaychar));
				}
				num7 += num8;
				if (num7 >= 0.5)
				{
					num10 += num9;
					num7 -= 1.0;
					displaychar = ((num9 >= 0) ? '\\' : '/');
				}
				else
				{
					displaychar = ((!flag2) ? '-' : '|');
				}
			}
		}
		if (flag)
		{
			list.Reverse();
		}
		list[0].DisplayChar = '@';
		list[list.Count - 1].DisplayChar = 'X';
		return list;
	}

	public bool IsOutside()
	{
		return !IsInside();
	}

	public bool IsInside()
	{
		if (Z <= 10)
		{
			return GetZoneProperty("inside") == "1";
		}
		return true;
	}

	public string SpecialUpMessage()
	{
		return GetZoneProperty("SpecialUpMessage");
	}

	public bool HasZoneProperty(string name)
	{
		return The.ZoneManager.GetZoneProperty(ZoneID, name) != null;
	}

	public void SetZoneProperty(string name, string value)
	{
		The.ZoneManager.SetZoneProperty(ZoneID, name, value);
	}

	public string GetZoneProperty(string name, string defaultvalue = "")
	{
		return The.ZoneManager.GetZoneProperty(ZoneID, name, bClampToLevel30: false, defaultvalue) as string;
	}

	public void ClearZoneConnectionCache()
	{
		ZoneConnectionCache.Clear();
	}

	public void WriteZoneConnectionCache()
	{
		foreach (CachedZoneConnection item in ZoneConnectionCache)
		{
			AddZoneConnection(item.TargetDirection, item.X, item.Y, item.Type, item.Object);
		}
		ZoneConnectionCache.Clear();
	}

	public void CacheZoneConnection(string TargetZoneDirection, Location2D location, string Type, string ConnectionObject = null)
	{
		CacheZoneConnection(TargetZoneDirection, location.x, location.y, Type, ConnectionObject);
	}

	public void CacheZoneConnection(string TargetZoneDirection, int X, int Y, string Type, string ConnectionObject)
	{
		ZoneConnectionCache.Add(new CachedZoneConnection(TargetZoneDirection, X, Y, Type, ConnectionObject));
	}

	public void CacheZoneConnection(string TargetZoneDirection, Point P, string Type, string ConnectionObject)
	{
		ZoneConnectionCache.Add(new CachedZoneConnection(TargetZoneDirection, P.X, P.Y, Type, ConnectionObject));
	}

	public void AddZoneConnection(string TargetZoneDirection, int X, int Y, string Type, string ConnectionObject)
	{
		if (!Built)
		{
			CacheZoneConnection(TargetZoneDirection, X, Y, Type, ConnectionObject);
		}
		else
		{
			The.ZoneManager.AddZoneConnection(ZoneID, TargetZoneDirection, X, Y, Type, ConnectionObject);
		}
	}

	public IEnumerable<ZoneConnection> EnumerateConnections()
	{
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(ZoneID))
		{
			yield return zoneConnection;
		}
		if (ZoneConnectionCache == null)
		{
			yield break;
		}
		foreach (CachedZoneConnection item in ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				yield return item;
			}
		}
	}

	public Zone()
	{
		Width = 1;
		Height = 1;
		Cell[][] array = (Map = new GraveyardCell[1][]);
		Cell[] array2 = (Map[0] = new GraveyardCell[1]);
		Map[0][0] = new GraveyardCell(this);
		VisibilityMap = new bool[1];
		ExploredMap = new bool[1];
		LightMap = new LightLevel[1];
		NavigationMap = new NavigationWeight[1, 1];
		NavigationMap[0, 0] = new NavigationWeight();
	}

	public Zone(int _Width, int _Height)
	{
		Width = _Width;
		Height = _Height;
		Map = new Cell[Width][];
		MissileMap = new MissileMapType[Width][];
		ClearReachableMap(bValue: true);
		for (int i = 0; i < Width; i++)
		{
			Map[i] = new Cell[Height];
			MissileMap[i] = new MissileMapType[Height];
			for (int j = 0; j < Height; j++)
			{
				Map[i][j] = new Cell(this);
				Map[i][j].X = i;
				Map[i][j].Y = j;
				MissileMap[i][j] = MissileMapType.Empty;
			}
		}
		ExploredMap = new bool[Width * Height];
		VisibilityMap = new bool[Width * Height];
		LightMap = new LightLevel[Width * Height];
		NavigationMap = new NavigationWeight[Width, Height];
		for (int k = 0; k < Height; k++)
		{
			for (int l = 0; l < Width; l++)
			{
				NavigationMap[l, k] = new NavigationWeight();
			}
		}
	}

	public void Clear(string BlueprintsToKeep = null)
	{
		ClearBox(new Box(0, 0, Width - 1, Height - 1), BlueprintsToKeep);
	}

	public void Clear(IEnumerable<Location2D> cells, string blueprint = null)
	{
		foreach (Location2D cell in cells)
		{
			GetCell(cell)?.Clear(blueprint);
		}
	}

	public void ClearBox(Box B, Action<Cell> after = null)
	{
		ClearBox(B, null, null, after);
	}

	public void ClearBox(Rect2D R)
	{
		ClearBox(new Box(R.x1, R.y1, R.x2, R.y2));
	}

	public void ClearBoxWith(Rect2D rect, string blueprint, Action<Cell> after = null)
	{
		ClearBox(new Box(rect.x1, rect.y1, rect.x2, rect.y2), null, blueprint, after);
	}

	public void ClearBox(Box B, string BlueprintsToKeep, string with = null, Action<Cell> after = null)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				if (!cell.HasObject(The.Player))
				{
					if (BlueprintsToKeep == null)
					{
						cell.Clear();
						if (with != null)
						{
							GetCell(j, i).AddObject(with);
						}
					}
					else
					{
						List<GameObject> list = Event.NewGameObjectList();
						foreach (GameObject @object in cell.Objects)
						{
							if ((BlueprintsToKeep == null || !BlueprintsToKeep.Contains(@object.Blueprint)) && @object.CanClear())
							{
								list.Add(@object);
							}
						}
						foreach (GameObject item in list)
						{
							cell.Objects.Remove(item);
						}
						if (with != null)
						{
							cell.AddObject(with);
						}
					}
				}
				after?.Invoke(cell);
			}
		}
	}

	public static bool ColorsVisible(LightLevel Light)
	{
		switch (Light)
		{
		case LightLevel.Light:
		case LightLevel.Interpolight:
		case LightLevel.LitRadar:
		case LightLevel.Omniscient:
			return true;
		default:
			return false;
		}
	}

	public void ClearLightMap()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			LightMap[i] = LightLevel.None;
		}
	}

	public void ClearVisiblityMap()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			VisibilityMap[i] = false;
		}
	}

	public void ClearExploredMap()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			ExploredMap[i] = false;
		}
	}

	public void ClearFakeUnexploredMap()
	{
		FakeUnexploredMap = null;
	}

	public void SetReachable(int x, int y, bool reachable = true)
	{
		ReachableMap[x, y] = reachable;
	}

	public void SetLight(int x, int y, LightLevel v)
	{
		LightMap[x + y * Width] = v;
	}

	public void SetVisibility(int x, int y, bool v)
	{
		VisibilityMap[x + y * Width] = v;
		if (v)
		{
			ExploredMap[x + y * Width] = true;
			if (FakeUnexploredMap != null)
			{
				FakeUnexploredMap[x + y * Width] = false;
			}
		}
	}

	public static void IncrementLOSCacheValue()
	{
		LOSCacheValue++;
		if (LOSCacheValue == int.MaxValue)
		{
			LOSCacheValue = 0;
		}
	}

	public List<Tuple<Cell, char>> GetLine(int x0, int y0, int x1, int y1, bool IncludeSolid = false, GameObject UseTargetability = null)
	{
		List<Tuple<Cell, char>> list = new List<Tuple<Cell, char>>();
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			int num = x0;
			x0 = y0;
			y0 = num;
			int num2 = x1;
			x1 = y1;
			y1 = num2;
		}
		int value = x1 - x0;
		int num3 = Math.Abs(y1 - y0);
		int num4 = Math.Abs(value) / 2;
		int num5 = y0;
		int num6 = ((y0 < y1) ? 1 : (-1));
		if (x0 > x1)
		{
			for (int num7 = x0; num7 >= x1; num7--)
			{
				char c = '.';
				Cell cell;
				if (flag)
				{
					c = ((num6 == 0) ? '|' : ((num6 >= 0) ? '\\' : '/'));
					cell = Map[num5][num7];
				}
				else
				{
					c = ((num6 == 0) ? '-' : ((num6 >= 0) ? '\\' : '/'));
					cell = Map[num7][num5];
				}
				list.Add(new Tuple<Cell, char>(cell, c));
				if (num7 != x0 && num7 != x1)
				{
					if (cell.IsOccluding())
					{
						return list;
					}
					if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true))
							{
								return list;
							}
						}
						else if (cell.IsSolidFor(UseTargetability, UseTargetability))
						{
							return list;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				char c2 = '.';
				Cell cell;
				if (flag)
				{
					c2 = ((num6 == 0) ? '|' : ((num6 >= 0) ? '/' : '\\'));
					cell = Map[num5][i];
				}
				else
				{
					c2 = ((num6 == 0) ? '-' : ((num6 >= 0) ? '/' : '\\'));
					cell = Map[i][num5];
				}
				list.Add(new Tuple<Cell, char>(cell, c2));
				if (i != x0 && i != x1)
				{
					if (cell.IsOccluding())
					{
						return list;
					}
					if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true))
							{
								return list;
							}
						}
						else if (cell.IsSolidFor(UseTargetability, UseTargetability))
						{
							return list;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		return list;
	}

	public bool CalculateLOS(int x0, int y0, int x1, int y1, bool IncludeSolid = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false)
	{
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			int num = x0;
			x0 = y0;
			y0 = num;
			int num2 = x1;
			x1 = y1;
			y1 = num2;
		}
		int value = x1 - x0;
		int num3 = Math.Abs(y1 - y0);
		int num4 = Math.Abs(value) / 2;
		int num5 = y0;
		int num6 = ((y0 < y1) ? 1 : (-1));
		int num7 = 0;
		if (x0 > x1)
		{
			for (int num8 = x0; num8 >= x1; num8--)
			{
				Cell cell = ((!flag) ? Map[num8][num5] : Map[num5][num8]);
				if (num8 != x0 && num8 != x1)
				{
					if (BlockContinue && num7 > 0)
					{
						if (++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (OverrideBlocking != null)
					{
						if (OverrideBlocking(cell) && ++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (cell.IsOccluding())
					{
						if (++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true) && ++num7 > PassBlocks)
							{
								return false;
							}
						}
						else if (cell.IsSolidFor(UseTargetability, UseTargetability) && ++num7 > PassBlocks)
						{
							return false;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				Cell cell = ((!flag) ? Map[i][num5] : Map[num5][i]);
				if (i != x0 && i != x1)
				{
					if (BlockContinue && num7 > 0)
					{
						if (++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (OverrideBlocking != null)
					{
						if (OverrideBlocking(cell) && ++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (cell.IsOccluding())
					{
						if (++num7 > PassBlocks)
						{
							return false;
						}
					}
					else if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true) && ++num7 > PassBlocks)
							{
								return false;
							}
						}
						else if (cell.IsSolidFor(UseTargetability, UseTargetability) && ++num7 > PassBlocks)
						{
							return false;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		return true;
	}

	public bool CalculateLOS(Cell C, int x1, int y1, bool IncludeSolid = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false)
	{
		if (C == null)
		{
			return false;
		}
		return CalculateLOS(C.X, C.Y, x1, y1, IncludeSolid, UseTargetability, OverrideBlocking, PassBlocks, BlockContinue);
	}

	public bool CalculateLOS(GameObject GO, int x1, int y1, bool IncludeSolid = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false)
	{
		return CalculateLOS(GO?.CurrentCell, x1, y1, IncludeSolid, UseTargetability, OverrideBlocking, PassBlocks, BlockContinue);
	}

	public bool HasUnobstructedLineTo(int x0, int y0, int x1, int y1, GameObject UseTargetability = null)
	{
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			int num = x0;
			x0 = y0;
			y0 = num;
			int num2 = x1;
			x1 = y1;
			y1 = num2;
		}
		int value = x1 - x0;
		int num3 = Math.Abs(y1 - y0);
		int num4 = Math.Abs(value) / 2;
		int num5 = y0;
		int num6 = ((y0 < y1) ? 1 : (-1));
		if (x0 > x1)
		{
			for (int num7 = x0; num7 >= x1; num7--)
			{
				Cell cell = ((!flag) ? Map[num7][num5] : Map[num5][num7]);
				if (num7 != x0 && num7 != x1)
				{
					if (UseTargetability == null)
					{
						if (cell.IsSolid(ForFluid: true))
						{
							return false;
						}
					}
					else if (cell.IsSolidFor(UseTargetability, UseTargetability))
					{
						return false;
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				Cell cell = ((!flag) ? Map[i][num5] : Map[num5][i]);
				if (i != x0 && i != x1 && cell.IsSolid(ForFluid: true))
				{
					if (UseTargetability == null)
					{
						if (cell.IsSolid(ForFluid: true))
						{
							return false;
						}
					}
					else if (cell.IsSolidFor(UseTargetability, UseTargetability))
					{
						return false;
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		return true;
	}

	public bool CalculateLOSFloat(int xstart, int ystart, int xend, int yend)
	{
		int num = (int)Math.Sqrt((xstart - xend) * (xstart - xend) + (ystart - yend) * (ystart - yend));
		float num2 = (float)xstart + 0.5f;
		float num3 = (float)ystart + 0.5f;
		float num4 = ((float)xend + 0.5f - num2) / (float)num;
		float num5 = ((float)yend + 0.5f - num3) / (float)num;
		for (int i = 0; i < num - 1; i++)
		{
			num2 += num4;
			num3 += num5;
			int num6 = (int)Math.Floor(num2);
			int num7 = (int)Math.Floor(num3);
			if (Map[num6][num7].IsOccluding())
			{
				return false;
			}
		}
		return true;
	}

	public void LightAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			if ((int)LightMap[i] < 200)
			{
				LightMap[i] = LightLevel.Light;
			}
		}
	}

	public void UnlightAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			if ((int)LightMap[i] < 210)
			{
				LightMap[i] = LightLevel.None;
			}
		}
	}

	public void VisAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			VisibilityMap[i] = true;
		}
	}

	public void RemoveLight(int x, int y, int r)
	{
		RemoveLight(x, y, r, LightLevel.None);
	}

	public void AddLight(int x, int y, int r)
	{
		AddLight(x, y, r, LightLevel.Light);
	}

	public void RemoveLight(int x, int y, int r, LightLevel Level, LightLevel RemoveUpTo = LightLevel.Interpolight)
	{
		if ((int)GetLight(x, y) < (int)RemoveUpTo)
		{
			SetLight(x, y, Level);
		}
		int i = x - r;
		if (i < 0)
		{
			i = 0;
		}
		int num = y - r;
		if (num < 0)
		{
			num = 0;
		}
		for (r *= r; i < Width && i <= x + r; i++)
		{
			for (int j = num; j < Height && j <= y + r; j++)
			{
				if ((i - x) * (i - x) + (j - y) * (j - y) <= r && (int)GetLight(i, j) < (int)RemoveUpTo && CalculateLOS(x, y, i, j))
				{
					SetLight(i, j, Level);
				}
			}
		}
	}

	public static bool BlocksRadar(Cell C)
	{
		return C.BlocksRadar();
	}

	public void MixLight(int x, int y, LightLevel Level, bool Force = false)
	{
		if (Force)
		{
			SetLight(x, y, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(x, y, v: true);
			}
			return;
		}
		LightLevel light = GetLight(x, y);
		if (light == LightLevel.Darkvision && (int)Level < 210)
		{
			return;
		}
		if (Level == LightLevel.Darkvision && (int)light < 210)
		{
			SetLight(x, y, Level);
		}
		else if (Level == LightLevel.Radar && (int)light > 0 && (int)light < 228)
		{
			SetLight(x, y, LightLevel.LitRadar);
			SetVisibility(x, y, v: true);
		}
		else if (light == LightLevel.Radar && (int)Level > 0 && (int)Level < 228)
		{
			if (CalculateLOS(The.Player, x, y))
			{
				SetLight(x, y, LightLevel.LitRadar);
				SetVisibility(x, y, v: true);
			}
		}
		else if ((int)Level > (int)light)
		{
			SetLight(x, y, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(x, y, v: true);
			}
		}
	}

	public void MixLight(int x, int y, int xp, int yp, LightLevel Level, bool Force = false)
	{
		if (Force)
		{
			SetLight(xp, yp, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(xp, yp, v: true);
			}
			return;
		}
		LightLevel light = GetLight(xp, yp);
		if (light == LightLevel.Darkvision && (int)Level < 210)
		{
			return;
		}
		if (Level == LightLevel.Darkvision && (int)light < 210)
		{
			SetLight(xp, yp, Level);
		}
		else if (Level == LightLevel.Radar && (int)light > 0 && (int)light < 228 && CalculateLOS(x, y, xp, yp, IncludeSolid: true, null, BlocksRadar))
		{
			if (CalculateLOS(The.Player, xp, yp))
			{
				SetLight(xp, yp, LightLevel.LitRadar);
			}
			else
			{
				SetLight(xp, yp, LightLevel.Radar);
			}
			SetVisibility(xp, yp, v: true);
		}
		else if (light == LightLevel.Radar && (int)Level > 0 && (int)Level < 228 && CalculateLOS(The.Player, xp, yp))
		{
			SetLight(xp, yp, LightLevel.LitRadar);
			SetVisibility(xp, yp, v: true);
		}
		else
		{
			if ((int)Level <= (int)light)
			{
				return;
			}
			if (Level == LightLevel.Radar)
			{
				if (CalculateLOS(x, y, xp, yp, IncludeSolid: true, null, BlocksRadar))
				{
					SetLight(xp, yp, Level);
					SetVisibility(xp, yp, v: true);
				}
			}
			else if (Level == LightLevel.Interpolight)
			{
				if (CalculateLOS(x, y, xp, yp, IncludeSolid: false, null, null, 2, BlockContinue: true))
				{
					SetLight(xp, yp, Level);
					SetVisibility(xp, yp, v: true);
				}
			}
			else if ((int)Level >= 255 || CalculateLOS(x, y, xp, yp))
			{
				SetLight(xp, yp, Level);
				if ((int)Level >= 228)
				{
					SetVisibility(xp, yp, v: true);
				}
			}
		}
	}

	public void AddLight(int x, int y, int r, LightLevel Level, bool Force = false)
	{
		MixLight(x, y, Level, Force);
		if (r <= 0)
		{
			return;
		}
		int i = Math.Max(x - r, 0);
		int num = Math.Max(y - r, 0);
		for (r *= r; i < Width && i <= x + r; i++)
		{
			for (int j = num; j < Height && j <= y + r; j++)
			{
				if ((i - x) * (i - x) + (j - y) * (j - y) <= r && (x != i || y != j))
				{
					MixLight(x, y, i, j, Level, Force);
				}
			}
		}
	}

	public void AddLight(LightLevel Level, bool Force = false)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				MixLight(j, i, Level, Force);
			}
		}
	}

	public void AddExplored(int x, int y, int r)
	{
		try
		{
			SetVisibility(x, y, v: true);
			int i = Math.Max(x - r, 0);
			int num = Math.Max(y - r, 0);
			int num2 = r * r;
			for (; i < Width && i <= x + r; i++)
			{
				for (int j = num; j < Height && j <= y + r; j++)
				{
					if ((i - x) * (i - x) + (j - y) * (j - y) <= num2 && !GetVisibility(i, j) && CalculateLOS(x, y, i, j))
					{
						SetVisibility(i, j, v: true);
					}
				}
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Vis error", ex);
			throw ex;
		}
	}

	public void AddVisibility(int x, int y, int r)
	{
		SetVisibility(x, y, v: true);
		int i = Math.Max(x - r, 0);
		int num = Math.Max(y - r, 0);
		int num2 = r * r;
		for (; i < Width && i <= x + r; i++)
		{
			for (int j = num; j < Height && j <= y + r; j++)
			{
				if ((i - x) * (i - x) + (j - y) * (j - y) <= num2 && GetLight(i, j) != 0 && !GetVisibility(i, j) && CalculateLOS(x, y, i, j))
				{
					SetVisibility(i, j, v: true);
				}
			}
		}
	}

	private void CalculateXYRadius(int x, int y, int Radius, out int x1, out int x2, out int y1, out int y2)
	{
		x1 = x - Radius;
		x2 = x + Radius;
		y1 = y - Radius;
		y2 = y + Radius;
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (x2 > 79)
		{
			x2 = 79;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (y2 > 24)
		{
			y2 = 24;
		}
	}

	public List<GameObject> FastSquareSearchNoImpassible(int x, int y, int Radius, string SearchPart, GameObject Looker)
	{
		List<GameObject> Return = Event.NewGameObjectList();
		if (IsWorldMap())
		{
			return Return;
		}
		int Nav = 268435456;
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.NavigationWeight(Looker, ref Nav) < 100)
				{
					cell.ForeachObjectWithPart(SearchPart, delegate(GameObject o)
					{
						Return.Add(o);
					});
				}
			}
		}
		return Return;
	}

	public List<GameObject> FastSquareSearch(int x, int y, int Radius, string SearchPart)
	{
		if (IsWorldMap())
		{
			return Event.NewGameObjectList();
		}
		List<GameObject> Return = Event.NewGameObjectList();
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				GetCell(j, i).ForeachObjectWithPart(SearchPart, delegate(GameObject o)
				{
					Return.Add(o);
				});
			}
		}
		return Return;
	}

	public List<GameObject> FastSquareSearch(int x, int y, int Radius, Predicate<GameObject> filter, bool CachedOkay = false)
	{
		if (IsWorldMap())
		{
			return new List<GameObject>();
		}
		List<GameObject> list = (CachedOkay ? Event.NewGameObjectList() : null);
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				foreach (GameObject @object in GetCell(j, i).Objects)
				{
					if (filter(@object))
					{
						if (list == null)
						{
							list = new List<GameObject> { @object };
						}
						else
						{
							list.Add(@object);
						}
					}
				}
			}
		}
		return list;
	}

	public bool FastSquareSearchAny(int x, int y, int Radius, Predicate<GameObject> filter)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				foreach (GameObject @object in GetCell(j, i).Objects)
				{
					if (filter(@object))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public List<GameObject> FastCombatSquareVisibility(int x, int y, int Radius, GameObject Looker, Predicate<GameObject> Filter = null, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, bool IncludeLooker = true, bool NoBrainOnly = true, GameObject skip = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (IsWorldMap())
		{
			return list;
		}
		int Nav = 268435456;
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null || (VisibleToPlayerOnly && !cell.IsVisible()) || !(VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()))
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					if (cell.Objects[k].IsCombatObject(NoBrainOnly) && (IncludeLooker || cell.Objects[k] != Looker) && cell.Objects[k] != skip && (Filter == null || Filter(cell.Objects[k])) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (!VisibleToPlayerOnly || cell.Objects[k].IsVisible()))
					{
						list.Add(cell.Objects[k]);
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> FastSquareVisibility(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return Event.NewGameObjectList();
		}
		List<GameObject> list = Event.NewGameObjectList();
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null || (VisibleToPlayerOnly && !cell.IsVisible()) || !(VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) || (!IncludeWalls && cell.NavigationWeight(Looker, ref Nav) >= 100))
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					if (cell.Objects[k] != skip && cell.Objects[k].HasPart(SearchPart) && (!VisibleToPlayerOnly || cell.Objects[k].IsVisible()))
					{
						list.Add(cell.Objects[k]);
					}
				}
			}
		}
		return list;
	}

	public GameObject FastSquareVisibilityFirst(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null, int IgnoreFartherThan = 9999999, int IgnoreEasierThan = int.MinValue)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (IgnoreFartherThan >= 9999999 || Looker == null || Looker.DistanceTo(cell) <= IgnoreFartherThan) && (VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject = ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstObjectWithPartExcept(SearchPart, skip, IgnoreEasierThan, Looker) : cell.GetFirstObjectWithPartExcept(SearchPart, skip));
					if (gameObject != null && (!VisibleToPlayerOnly || gameObject.IsVisible()))
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public GameObject FastSquareVisibilityFirstBlueprint(int x, int y, int Radius, string blueprint, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject = (VisibleToPlayerOnly ? cell.GetFirstVisibleObjectExcept(blueprint, skip) : cell.GetFirstObjectExcept(blueprint, skip));
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public GameObject FastSquareVisibilityFirst(int x, int y, int Radius, string SearchPart, Predicate<GameObject> Filter, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null, int IgnoreFartherThan = 9999999, int IgnoreEasierThan = int.MinValue)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (IgnoreFartherThan >= 9999999 || Looker == null || Looker.DistanceTo(cell) <= IgnoreFartherThan) && (VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject = ((!VisibleToPlayerOnly) ? ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstObjectWithPartExcept(SearchPart, Filter, skip, IgnoreEasierThan, Looker) : cell.GetFirstObjectWithPartExcept(SearchPart, Filter, skip)) : ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstVisibleObjectWithPartExcept(SearchPart, Filter, skip, IgnoreEasierThan, Looker) : cell.GetFirstVisibleObjectWithPartExcept(SearchPart, Filter, skip)));
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public bool FastSquareVisibilityAny(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (VisibleToPlayerOnly ? cell.HasVisibleObjectWithPartExcept(SearchPart, skip) : cell.HasObjectWithPartExcept(SearchPart, skip)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool FastSquareVisibilityAny(int x, int y, int Radius, string SearchPart, Predicate<GameObject> Filter, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : cell.IsReallyExplored()) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (VisibleToPlayerOnly ? cell.HasVisibleObjectWithPartExcept(SearchPart, Filter, skip) : cell.HasObjectWithPartExcept(SearchPart, Filter, skip)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearReachableMap(bool bValue)
	{
		if (ReachableMap == null)
		{
			ReachableMap = new bool[Width, Height];
		}
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				ReachableMap[j, i] = bValue;
			}
		}
	}

	public void ClearReachableMap()
	{
		ClearReachableMap(bValue: false);
	}

	public int BuildReachableMap(int StartX, int StartY)
	{
		return BuildReachableMap(StartX, StartY, bClearFirst: true);
	}

	public int BuildReachableMap(int StartX, int StartY, bool bClearFirst)
	{
		if (bClearFirst)
		{
			ClearReachableMap();
		}
		if (ReachableMap == null)
		{
			ClearReachableMap();
		}
		return BuildReachableMapHelper(StartX, StartY, ReachableMap);
	}

	public bool BuildReachabilityFromEdges()
	{
		if (BuildReachableMap(Width / 2, Height - 1) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(Width / 2, 0) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(0, Height / 2) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(Width - 1, Height / 2) >= 400)
		{
			return true;
		}
		for (int i = 0; i < Height; i++)
		{
			if (BuildReachableMap(0, i) >= 400)
			{
				return true;
			}
			if (BuildReachableMap(Width - 1, i) >= 400)
			{
				return true;
			}
		}
		for (int j = 0; j < Width; j++)
		{
			if (BuildReachableMap(j, Height - 1) >= 400)
			{
				return true;
			}
			if (BuildReachableMap(j, 0) >= 400)
			{
				return true;
			}
		}
		return false;
	}

	public void BuildReachableMap()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (!cell.IsOccluding() && !cell.IsReachable())
				{
					BuildReachableMap(j, i, bClearFirst: false);
				}
			}
		}
	}

	public void RebuildReachableMap()
	{
		ClearReachableMap();
		BuildReachableMap();
	}

	public bool IsActive()
	{
		return The.PlayerCell?.ParentZone == this;
	}

	public bool IsReachable(int X, int Y)
	{
		if (ReachableMap == null)
		{
			return true;
		}
		return ReachableMap[X, Y];
	}

	private int BuildReachableMapHelper(int StartX, int StartY, bool[,] Reachable)
	{
		VisitedList.Clear();
		CellQueue.Clear();
		int num = 0;
		if (StartX < 0)
		{
			return num;
		}
		if (StartY < 0)
		{
			return num;
		}
		if (StartX >= Width)
		{
			return num;
		}
		if (StartY >= Height)
		{
			return num;
		}
		if (GetCell(StartX, StartY).HasWall())
		{
			return num;
		}
		CellQueue.Enqueue(Location2D.get(StartX, StartY));
		while (CellQueue.Count > 0)
		{
			Location2D location2D = CellQueue.Dequeue();
			if (VisitedList.ContainsKey(location2D))
			{
				continue;
			}
			VisitedList.Add(location2D, value: true);
			if (!GetCell(location2D).HasWall())
			{
				Reachable[location2D.x, location2D.y] = true;
				num++;
				if (location2D.x - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x - 1, location2D.y));
				}
				if (location2D.x + 1 < Width)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x + 1, location2D.y));
				}
				if (location2D.y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x, location2D.y - 1));
				}
				if (location2D.y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x, location2D.y + 1));
				}
				if (location2D.x - 1 >= 0 && location2D.y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x - 1, location2D.y - 1));
				}
				if (location2D.x + 1 < Width && location2D.y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x + 1, location2D.y - 1));
				}
				if (location2D.x - 1 >= 0 && location2D.y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x - 1, location2D.y + 1));
				}
				if (location2D.x + 1 < Width && location2D.y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.get(location2D.x + 1, location2D.y + 1));
				}
			}
		}
		VisitedList.Clear();
		return num;
	}

	private void BuildReachableMapHelperFlood(int StartX, int StartY, bool[,] Reachable)
	{
		if (StartX >= 0 && StartY >= 0 && StartX < Width && StartY < Height && !Reachable[StartX, StartY] && GetCell(StartX, StartY).IsEmpty() && (GetCell(StartX, StartY).Objects.Count <= 0 || GetCell(StartX, StartY).IsEmpty()))
		{
			Reachable[StartX, StartY] = true;
			BuildReachableMapHelper(StartX - 1, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX - 1, StartY, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY, Reachable);
			BuildReachableMapHelper(StartX - 1, StartY + 1, Reachable);
			BuildReachableMapHelper(StartX, StartY + 1, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY + 1, Reachable);
		}
	}

	public List<Cell> FastFloodNeighbors(int x1, int y1, int Radius)
	{
		List<Cell> list = new List<Cell>();
		FastFloodNeighborsRecurse(x1, y1, x1 - 1, y1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1 + 1, y1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1, y1 + 1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1, y1 - 1, Radius, list);
		return list;
	}

	public void FastFloodNeighborsRecurse(int xs, int ys, int x1, int y1, int Radius, List<Cell> Return)
	{
		if (x1 >= 0 && x1 <= Width - 1 && y1 >= 0 && y1 <= Height - 1 && Math.Abs(xs - x1) <= Radius && Math.Abs(ys - y1) <= Radius && !GetCell(x1, y1).IsOccluding() && !Return.CleanContains(GetCell(x1, y1)))
		{
			Return.Add(GetCell(x1, y1));
			FastFloodNeighborsRecurse(xs, ys, x1 - 1, y1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1 + 1, y1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1, y1 + 1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1, y1 - 1, Radius, Return);
		}
	}

	public List<GameObject> FastFloodVisibility(int x1, int y1, int Radius, string SearchPart, GameObject Looker, Predicate<GameObject> ExtraVisibility = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart("DarkVision") || Looker.HasPart("NightVision")))
		{
			darkVision = true;
		}
		FastFloodVisibilityRecurse(x1, y1, x1 - 1, y1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1 + 1, y1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1, y1 + 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1, y1 - 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1 - 1, y1 - 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1 + 1, y1 + 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1 - 1, y1 + 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		FastFloodVisibilityRecurse(x1, y1, x1 + 1, y1 - 1, Radius, SearchPart, ExtraVisibility, list, Looker, darkVision);
		if (Options.DrawFloodVis)
		{
			for (int i = 0; i < 25; i++)
			{
				for (int j = 0; j < 80; j++)
				{
					if (FloodVisMap[j, i] == FloodValue)
					{
						Popup._ScreenBuffer.Goto(j, i);
						Popup._ScreenBuffer.Write("*");
					}
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			Keyboard.getch();
		}
		return list;
	}

	private void FastFloodVisibilityRecurse(int xs, int ys, int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> ExtraVisibility, List<GameObject> Return, GameObject Looker, bool DarkVision)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1 || FloodVisMap[x1, y1] == FloodValue || Math.Abs(xs - x1) > Radius || Math.Abs(ys - y1) > Radius)
		{
			return;
		}
		Cell cell = null;
		if (!DarkVision && GetLight(x1, y1) == LightLevel.None)
		{
			if (ExtraVisibility != null)
			{
				if (cell == null)
				{
					cell = GetCell(x1, y1);
				}
				cell.HasObject(ExtraVisibility);
			}
			return;
		}
		FloodVisMap[x1, y1] = FloodValue;
		if (cell == null)
		{
			cell = GetCell(x1, y1);
		}
		int i = 0;
		for (int count = cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = cell.Objects[i];
			if (gameObject.HasPart(SearchPart))
			{
				Return.Add(gameObject);
			}
		}
		if (!cell.IsOccluding())
		{
			FastFloodVisibilityRecurse(xs, ys, x1 - 1, y1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1 + 1, y1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1, y1 + 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1, y1 - 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1 - 1, y1 - 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1 + 1, y1 + 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1 - 1, y1 + 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
			FastFloodVisibilityRecurse(xs, ys, x1 + 1, y1 - 1, Radius, SearchPart, ExtraVisibility, Return, Looker, DarkVision);
		}
	}

	public bool FastFloodVisibilityAny(int x1, int y1, int Radius, string SearchPart, GameObject Looker)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart("DarkVision") || Looker.HasPart("NightVision")))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 + 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 - 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 - 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 + 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 + 1, Radius, SearchPart, Looker, darkVision))
		{
			return FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 - 1, Radius, SearchPart, Looker, darkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyRecurse(int xs, int ys, int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool DarkVision)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		if (!DarkVision && GetLight(x1, y1) == LightLevel.None)
		{
			return false;
		}
		FloodVisMap[x1, y1] = FloodValue;
		if (GetCell(x1, y1).HasObjectWithPart(SearchPart))
		{
			return true;
		}
		if (GetCell(x1, y1).IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 + 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 - 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 - 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 + 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 + 1, Radius, SearchPart, Looker, DarkVision))
		{
			return FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 - 1, Radius, SearchPart, Looker, DarkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Looker)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart("DarkVision") || Looker.HasPart("NightVision")))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 + 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 - 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Filter, Looker, darkVision))
		{
			return FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Filter, Looker, darkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyRecurse(int xs, int ys, int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Looker, bool DarkVision)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		if (!DarkVision && GetLight(x1, y1) == LightLevel.None)
		{
			return false;
		}
		FloodVisMap[x1, y1] = FloodValue;
		if (GetCell(x1, y1).HasObject(Filter))
		{
			return true;
		}
		if (GetCell(x1, y1).IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 + 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 - 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Filter, Looker, DarkVision))
		{
			return FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Filter, Looker, DarkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleToPlayerOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart("DarkVision") || Looker.HasPart("NightVision")))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly))
		{
			return FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool DarkVision, bool VisibleToPlayerOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		if (!DarkVision && GetLight(x1, y1) == LightLevel.None)
		{
			return false;
		}
		FloodVisMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		if (VisibleToPlayerOnly && !cell.IsVisible())
		{
			return false;
		}
		if ((VisibleToPlayerOnly ? cell.GetFirstObject(Blueprint) : cell.GetFirstVisibleObject(Blueprint)) != null)
		{
			return true;
		}
		if (cell.IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly))
		{
			return FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly);
		}
		return true;
	}

	public void DebugDrawSemantics()
	{
		if (!(Options.GetOption("OptionDrawZoneSemantics") == "Yes"))
		{
			return;
		}
		ScreenBuffer newBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> colorCodes = new List<string> { "G", "B", "Y", "W", "M", "K", "C", "R" };
		Dictionary<string, string> tagColors = new Dictionary<string, string>();
		GetCells().ForEach(delegate(Cell c)
		{
			c.SemanticTags?.ForEach(delegate(string t)
			{
				if (t != null && !tagColors.ContainsKey(t))
				{
					tagColors.Add(t, "&" + colorCodes[tagColors.Count % colorCodes.Count]);
				}
			});
		});
		GetCells().ForEach(delegate(Cell c)
		{
			newBuffer.Goto(c.X, c.Y);
			List<string> semanticTags = GetCell(c.X, c.Y).SemanticTags;
			if (semanticTags != null && semanticTags.Count > 0)
			{
				newBuffer.Write(tagColors[GetCell(c.X, c.Y).SemanticTags[0]] + "#");
			}
		});
		newBuffer.Draw();
		Keyboard.getch();
	}

	public bool FastFloodFindAnyBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly = false, bool ExploredOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		if (!FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly))
		{
			return FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
		}
		return true;
	}

	public bool FastFloodFindAnyBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly, bool ExploredOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		FloodVisMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		if (VisibleOnly && !cell.IsVisible())
		{
			return false;
		}
		if (ExploredOnly && !cell.IsExploredFor(Looker))
		{
			return false;
		}
		if ((VisibleOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint)) != null)
		{
			return true;
		}
		if (cell.IsOccluding())
		{
			return false;
		}
		if (!FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly))
		{
			return FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
		}
		return true;
	}

	public GameObject FastFloodVisibilityFirstBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleToPlayerOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart("DarkVision") || Looker.HasPart("NightVision")))
		{
			darkVision = true;
		}
		return FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly);
	}

	public GameObject FastFloodVisibilityFirstBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool DarkVision, bool VisibleToPlayerOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return null;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return null;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return null;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return null;
		}
		if (!DarkVision && GetLight(x1, y1) == LightLevel.None)
		{
			return null;
		}
		FloodVisMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		if (VisibleToPlayerOnly && !cell.IsVisible())
		{
			return null;
		}
		GameObject gameObject = (VisibleToPlayerOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint));
		if (gameObject != null)
		{
			return gameObject;
		}
		if (cell.IsOccluding())
		{
			return null;
		}
		return FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly);
	}

	public GameObject FastFloodFindFirstBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly = false, bool ExploredOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		return FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
	}

	public GameObject FastFloodFindFirstBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly, bool ExploredOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return null;
		}
		if (FloodVisMap[x1, y1] == FloodValue)
		{
			return null;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return null;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return null;
		}
		FloodVisMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		if (VisibleOnly && !cell.IsVisible())
		{
			return null;
		}
		if (ExploredOnly && !cell.IsExploredFor(Looker))
		{
			return null;
		}
		GameObject gameObject = (VisibleOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint));
		if (gameObject != null)
		{
			return gameObject;
		}
		if (cell.IsOccluding())
		{
			return null;
		}
		return FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
	}

	private void AddFloodNext(int xs, int ys, int x1, int y1, int Radius)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return;
		}
		Cell cell = GetCell(x1, y1);
		int num = FloodNext.IndexOf(cell);
		if (num != -1)
		{
			if (Radius > FloodNextRadii[num])
			{
				FloodNextRadii[num] = Radius;
			}
		}
		else if (FloodVisMap[x1, y1] != FloodValue && Math.Abs(xs - x1) <= Radius && Math.Abs(ys - y1) <= Radius && cell != null)
		{
			FloodNext.Add(cell);
			FloodNextRadii.Add(Radius);
			FloodVisMap[x1, y1] = FloodValue;
		}
	}

	public List<GameObject> FastFloodAudibility(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedHearing = false;
		if (Hearer != null)
		{
			HeightenedHearing part = Hearer.GetPart<HeightenedHearing>();
			if (part != null)
			{
				heightenedHearing = true;
				Radius += part.Level;
			}
		}
		FloodNext.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		while (FloodNext.Count > 0)
		{
			FloodCurrent.Clear();
			FloodCurrent.AddRange(FloodNext);
			FloodCurrentRadii.Clear();
			FloodCurrentRadii.AddRange(FloodNextRadii);
			FloodNext.Clear();
			FloodNextRadii.Clear();
			for (int i = 0; i < FloodCurrent.Count; i++)
			{
				FastFloodAudibilityProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, list, Hearer, heightenedHearing);
			}
		}
		FloodCurrent.Clear();
		FloodCurrentRadii.Clear();
		if (Options.DrawFloodAud)
		{
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 80; k++)
				{
					if (FloodVisMap[k, j] == FloodValue)
					{
						Popup._ScreenBuffer.Goto(k, j);
						Popup._ScreenBuffer.Write("*");
					}
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			Keyboard.getch();
		}
		return list;
	}

	public void FastFloodAudibilityProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, List<GameObject> Return, GameObject Hearer, bool HeightenedHearing)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (Filter(@object))
			{
				Return.Add(@object);
			}
		}
		if (C.IsSolid())
		{
			Radius = ((!HeightenedHearing) ? (Radius - 10) : (Radius - 1));
			if (Radius <= 0)
			{
				return;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
	}

	public bool FastFloodAudibilityAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedHearing = false;
		if (Hearer != null)
		{
			HeightenedHearing part = Hearer.GetPart<HeightenedHearing>();
			if (part != null)
			{
				heightenedHearing = true;
				Radius += part.Level;
			}
		}
		FloodNext.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		try
		{
			while (FloodNext.Count > 0)
			{
				FloodCurrent.Clear();
				FloodCurrent.AddRange(FloodNext);
				FloodCurrentRadii.Clear();
				FloodCurrentRadii.AddRange(FloodNextRadii);
				FloodNext.Clear();
				FloodNextRadii.Clear();
				for (int i = 0; i < FloodCurrent.Count; i++)
				{
					if (FastFloodAudibilityAnyProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, Hearer, heightenedHearing))
					{
						return true;
					}
				}
			}
		}
		finally
		{
			FloodCurrent.Clear();
			FloodCurrentRadii.Clear();
		}
		return false;
	}

	public bool FastFloodAudibilityAnyProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, GameObject Hearer, bool HeightenedHearing)
	{
		if (C.HasObject(Filter))
		{
			return true;
		}
		if (C.IsSolid())
		{
			Radius = ((!HeightenedHearing) ? (Radius - 10) : (Radius - 1));
			if (Radius <= 0)
			{
				return false;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
		return false;
	}

	public List<GameObject> FastFloodOlfaction(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedSmell = false;
		if (Smeller != null)
		{
			HeightenedSmell part = Smeller.GetPart<HeightenedSmell>();
			if (part != null)
			{
				heightenedSmell = true;
				Radius += part.Level * 4;
			}
		}
		FloodNext.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		while (FloodNext.Count > 0)
		{
			FloodCurrent.Clear();
			FloodCurrent.AddRange(FloodNext);
			FloodCurrentRadii.Clear();
			FloodCurrentRadii.AddRange(FloodNextRadii);
			FloodNext.Clear();
			FloodNextRadii.Clear();
			for (int i = 0; i < FloodCurrent.Count; i++)
			{
				FastFloodOlfactionProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, list, Smeller, heightenedSmell);
			}
		}
		FloodCurrent.Clear();
		FloodCurrentRadii.Clear();
		if (Options.DrawFloodOlf)
		{
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 80; k++)
				{
					if (FloodVisMap[k, j] == FloodValue)
					{
						Popup._ScreenBuffer.Goto(k, j);
						Popup._ScreenBuffer.Write("*");
					}
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			Keyboard.getch();
		}
		return list;
	}

	public void FastFloodOlfactionProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, List<GameObject> Return, GameObject Smeller, bool HeightenedSmell)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (Filter(@object))
			{
				Return.Add(@object);
			}
		}
		if (C.IsSolid())
		{
			if (!HeightenedSmell || !C.HasObjectWithPart("Door"))
			{
				return;
			}
			Radius = Radius / 2 - 1;
			if (Radius <= 0)
			{
				return;
			}
		}
		if (C.IsOccluding() || C.HasWadingDepthLiquid())
		{
			if (!HeightenedSmell)
			{
				Radius /= 2;
			}
			Radius--;
			if (Radius <= 0)
			{
				return;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
	}

	public bool FastFloodOlfactionAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedSmell = false;
		if (Smeller != null)
		{
			HeightenedSmell part = Smeller.GetPart<HeightenedSmell>();
			if (part != null)
			{
				heightenedSmell = true;
				Radius += part.Level * 4;
			}
		}
		FloodNext.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		try
		{
			while (FloodNext.Count > 0)
			{
				FloodCurrent.Clear();
				FloodCurrent.AddRange(FloodNext);
				FloodCurrentRadii.Clear();
				FloodCurrentRadii.AddRange(FloodNextRadii);
				FloodNext.Clear();
				FloodNextRadii.Clear();
				for (int i = 0; i < FloodCurrent.Count; i++)
				{
					if (FastFloodOlfactionAnyProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, Smeller, heightenedSmell))
					{
						return true;
					}
				}
			}
		}
		finally
		{
			FloodCurrent.Clear();
			FloodCurrentRadii.Clear();
		}
		return false;
	}

	public bool FastFloodOlfactionAnyProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, GameObject Smeller, bool HeightenedSmell)
	{
		if (C.HasObject(Filter))
		{
			return true;
		}
		if (C.IsSolid())
		{
			if (!HeightenedSmell || !C.HasObjectWithPart("Door"))
			{
				return false;
			}
			Radius = Radius / 2 - 1;
			if (Radius <= 0)
			{
				return false;
			}
		}
		if (C.IsOccluding() || C.HasWadingDepthLiquid())
		{
			if (!HeightenedSmell)
			{
				Radius /= 2;
			}
			Radius--;
			if (Radius <= 0)
			{
				return false;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
		return false;
	}

	private void InitFlood(string Method, int Radius)
	{
		if (InFloodMethod == null)
		{
			InFloodMethod = Method;
		}
		else
		{
			MetricsManager.LogError("Call to " + Method + "() inside " + InFloodMethod + "(), global flood methods are mutually non-reentrant");
		}
		if (Flooded == null)
		{
			Flooded = new Dictionary<Cell, int>((Radius + 1) * (Radius + 1));
		}
		else
		{
			Flooded.Clear();
		}
	}

	private void DoneFlood()
	{
		Flooded.Clear();
		InFloodMethod = null;
	}

	public List<GameObject> GlobalFloodObjects(int x1, int y1, int Radius, string SearchPart, GameObject Looker, List<GameObject> Return, bool CheckInWalls = false, bool ForFluid = false)
	{
		Return.Clear();
		if (Looker == null)
		{
			return Return;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return Return;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return Return;
		}
		InitFlood("GlobalFloodObjects", Radius);
		GlobalFloodObjectsRecurse(cell, Radius, SearchPart, Return, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return Return;
	}

	public void GlobalFloodObjectsRecurse(Cell C, int Radius, string SearchPart, List<GameObject> Return, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null || Radius < 0 || (!First && C.IsSolid(ForFluid)))
		{
			return;
		}
		if (!Flooded.ContainsKey(C))
		{
			Flooded.Add(C, 0);
			C.ForeachObjectWithPart(SearchPart, delegate(GameObject o)
			{
				Return.Add(o);
			});
		}
		if (Flooded[C] >= Radius)
		{
			return;
		}
		Flooded[C] = Radius;
		int num = Radius - 1;
		if (num > 0)
		{
			string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
			foreach (string direction in directionListCardinalFirst)
			{
				GlobalFloodObjectsRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, Return, ForFluid, CheckInWalls);
			}
			GlobalFloodObjectsRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, Return, ForFluid, CheckInWalls);
			GlobalFloodObjectsRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, Return, ForFluid, CheckInWalls);
		}
	}

	public GameObject GlobalFloodFirstObject(int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return null;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return null;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		InitFlood("GlobalFloodFirstObject", Radius);
		GameObject result = GlobalFloodFirstObjectRecurse(cell, Radius, SearchPart, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public GameObject GlobalFloodFirstObjectRecurse(Cell C, int Radius, string SearchPart, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return null;
		}
		if (Radius < 0)
		{
			return null;
		}
		if (!First && C.IsSolid(ForFluid))
		{
			return null;
		}
		if (!Flooded.ContainsKey(C))
		{
			GameObject firstObjectWithPart = C.GetFirstObjectWithPart(SearchPart);
			if (firstObjectWithPart != null)
			{
				return firstObjectWithPart;
			}
			Flooded.Add(C, 0);
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				GameObject gameObject;
				foreach (string direction in directionListCardinalFirst)
				{
					gameObject = GlobalFloodFirstObjectRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, ForFluid, CheckInWalls);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls);
				if (gameObject != null)
				{
					return gameObject;
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls);
				if (gameObject == null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GlobalFloodFirstObject(int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> pFilter, GameObject Looker, bool ForFluid = false)
	{
		if (Looker == null)
		{
			return null;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return null;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		InitFlood("GlobalFloodFirstObject", Radius);
		GameObject result = GlobalFloodFirstObjectRecurse(cell, Radius, SearchPart, pFilter, ForFluid, CheckInWalls: true);
		DoneFlood();
		return result;
	}

	public GameObject GlobalFloodFirstObjectRecurse(Cell C, int Radius, string SearchPart, Predicate<GameObject> pFilter, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return null;
		}
		if (Radius < 0)
		{
			return null;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return null;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			GameObject firstObjectWithPart = C.GetFirstObjectWithPart(SearchPart, pFilter);
			if (firstObjectWithPart != null)
			{
				return firstObjectWithPart;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return null;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				GameObject gameObject;
				foreach (string direction in directionListCardinalFirst)
				{
					gameObject = GlobalFloodFirstObjectRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, pFilter, ForFluid, CheckInWalls);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls);
				if (gameObject != null)
				{
					return gameObject;
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls);
				if (gameObject == null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public bool GlobalFloodAny(int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return false;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		InitFlood("GlobalFloodAny", Radius);
		bool result = GlobalFloodAnyRecurse(cell, Radius, SearchPart, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public bool GlobalFloodAnyRecurse(Cell C, int Radius, string SearchPart, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return false;
		}
		if (Radius < 0)
		{
			return false;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return false;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			if (C.HasObjectWithPart(SearchPart))
			{
				return true;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return false;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				foreach (string direction in directionListCardinalFirst)
				{
					if (GlobalFloodAnyRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, ForFluid, CheckInWalls))
					{
						return true;
					}
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls))
				{
					return true;
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, ForFluid, CheckInWalls))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool GlobalFloodAny(int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> pFilter, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return false;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		InitFlood("GlobalFloodAny", Radius);
		bool result = GlobalFloodAnyRecurse(cell, Radius, SearchPart, pFilter, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public bool GlobalFloodAnyRecurse(Cell C, int Radius, string SearchPart, Predicate<GameObject> pFilter, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return false;
		}
		if (Radius < 0)
		{
			return false;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return false;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			if (C.HasObjectWithPart(SearchPart, pFilter))
			{
				return true;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return false;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				foreach (string direction in directionListCardinalFirst)
				{
					if (GlobalFloodAnyRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, pFilter, ForFluid, CheckInWalls))
					{
						return true;
					}
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls))
				{
					return true;
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Render(ScreenBuffer Buf)
	{
		if (GameManager.bDraw != 12)
		{
			Event.PinCurrentPool();
			Render(Buf, 0, 0, 0, 0, 80, 25);
			Event.ResetToPin();
		}
	}

	public void Render(ScreenBuffer Buf, int BufX, int BufY, int x1, int y1, int width, int height)
	{
		if (The.Core.HostileWalkObjects == null)
		{
			The.Core.HostileWalkObjects = new List<GameObject>();
		}
		RenderedObjects = 0;
		bool bAlt = Keyboard.bAlt;
		bool bSkulk = The.Player?.HasEffect("Skulk_Tonic") ?? false;
		bool bDisableColorEffects = false;
		if (Options.DisableFullscreenColorEffects)
		{
			bSkulk = false;
			bDisableColorEffects = true;
		}
		wantsToPaint.Clear();
		int num = 0;
		int i = y1;
		for (int num2 = Math.Min(y1 + height, Height - BufY); i < num2; i++)
		{
			Buf.Goto(BufX, i + BufY);
			int j = x1;
			for (int num3 = Math.Min(x1 + width, Width - BufX); j < num3; j++)
			{
				if (i + BufY >= 0)
				{
					ConsoleChar consoleChar = Buf.Buffer[j, i];
					consoleChar.Tile = null;
					consoleChar.HFlip = false;
					consoleChar.VFlip = false;
					string s = Map[j][i].Render(consoleChar, VisibilityMap[num], LightMap[num], ExploredMap[num] && (FakeUnexploredMap == null || !FakeUnexploredMap[num]), bAlt, bSkulk, bDisableColorEffects, wantsToPaint);
					Buf.Write(s, processMarkup: false, consoleChar.HFlip, consoleChar.VFlip);
					if (consoleChar.Tile != null)
					{
						consoleChar.BackupChar = consoleChar.Char;
						consoleChar.Char = '\0';
					}
				}
				num++;
			}
		}
		foreach (GameObject item in wantsToPaint)
		{
			item.Paint(Buf);
		}
		The.Core.RenderedObjects = RenderedObjects;
	}

	public void ForeachCell(Action<Cell> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				aProc(GetCell(j, i));
			}
		}
	}

	public bool ForeachCell(Predicate<Cell> pProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (!pProc(GetCell(j, i)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<Cell> GetEmptyReachableCellsWithout(string[] Blueprint)
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && !cell.HasObjectWithBlueprint(Blueprint))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetReachableCells()
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetReachableCellsWithout(string[] Blueprint)
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsReachable() && !cell.HasObjectWithBlueprint(Blueprint))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyReachableCells(Rect2D r)
	{
		List<Cell> list = new List<Cell>();
		for (int i = r.y1; i <= r.y2; i++)
		{
			for (int j = r.x1; j <= r.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell GetSpawnCell()
	{
		Cell randomElement = GetEmptyReachableCells().GetRandomElement();
		if (randomElement == null)
		{
			randomElement = GetEmptyCells().GetRandomElement();
		}
		if (randomElement == null)
		{
			randomElement = (from c in GetReachableCells()
				where !c.HasWall()
				select c).GetRandomElement();
		}
		if (randomElement == null)
		{
			randomElement = GetCells().GetRandomElement();
		}
		return randomElement;
	}

	public List<Cell> GetEmptyReachableCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyReachableCells(Event E)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && cell.FireEvent(E))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public bool AnyEmptyReachableCells()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyEmptyReachableCells(Event E)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && cell.FireEvent(E))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<Cell> GetEmptyCellsWithNoFurniture()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && !cell.HasFurniture())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetPassableCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsPassable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyCells(Predicate<Cell> filter = null)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && (filter == null || filter(cell)))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyCells(Rect2D R)
	{
		List<Cell> list = new List<Cell>();
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public int GetEmptyCellCount()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).IsEmpty())
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<Cell> GetEmptyCells(Event E)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.FireEvent(E))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetExploredCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsExplored())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell[] GetExploredCellsShuffled()
	{
		return Algorithms.RandomShuffle(GetExploredCells(), Stat.Rand);
	}

	public List<Cell> GetReallyExploredCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsReallyExplored())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell[] GetReallyExploredCellsShuffled()
	{
		return Algorithms.RandomShuffle(GetReallyExploredCells(), Stat.Rand);
	}

	public Cell[] GetEmptyCellsShuffled()
	{
		return Algorithms.RandomShuffle(GetEmptyCells(), Stat.Rand);
	}

	public Cell GetCellWithEmptyBorder(int Size)
	{
		List<Cell> list = new List<Cell>();
		Cell[] emptyCellsShuffled = GetEmptyCellsShuffled();
		foreach (Cell cell in emptyCellsShuffled)
		{
			if (cell.X <= Size || cell.X >= Width - Size || cell.Y <= Size || cell.Y >= Height - Size)
			{
				continue;
			}
			list.Clear();
			cell.GetAdjacentCells(Size, list);
			using List<Cell>.Enumerator enumerator = list.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return cell;
				}
			}
			while (enumerator.Current.IsEmpty());
		}
		return null;
	}

	public List<Cell> GetEmptyVisibleCells()
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).IsEmpty() && GetCell(j, i).IsVisible())
				{
					list.Add(GetCell(j, i));
				}
			}
		}
		return list;
	}

	public IEnumerable<Cell> GetCells(Box b)
	{
		for (int y = b.y1; y <= b.y2; y++)
		{
			for (int x = b.x1; x <= b.x2; x++)
			{
				yield return GetCell(x, y);
			}
		}
	}

	public List<Cell> GetCells()
	{
		if (CellList == null)
		{
			CellList = new List<Cell>(Width * Height);
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					CellList.Add(Map[j][i]);
				}
			}
		}
		return CellList;
	}

	public List<Cell> GetCells(Predicate<Cell> pFilter)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (pFilter(Map[j][i]))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (pFilter(Map[l][k]))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithObject(string Blueprint)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObject(Blueprint))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObject(Blueprint))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public IEnumerable<Cell> LoopCells()
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				yield return Map[x][y];
			}
		}
	}

	public List<Cell> GetCellsWithObject(Predicate<GameObject> pFilter)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObject(pFilter))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObject(pFilter))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithTaggedObject(string What)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObjectWithTag(What))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObjectWithTag(What))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithTagOrPropertyObject(string What)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObjectWithTagOrProperty(What))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					if (Map[k][l].HasObjectWithTagOrProperty(What))
					{
						list.Add(Map[k][l]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithFloor(string OkFloor)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].PaintTile == OkFloor)
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].PaintTile == OkFloor)
					{
						list.Add(Map[l][k]);
						if (list.Count > num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public GameObject GetFirstObject()
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject()) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(string Blueprint)
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject(Blueprint)) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> pFilter)
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject(pFilter)) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public int[,] GetUnreachableGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!GetCell(i, j).IsReachable())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public int[,] GetWallGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetCell(i, j).HasWall())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public int[,] GetUnreachableOrWallGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetCell(i, j).HasWall() || !GetCell(i, j).IsReachable())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public Cell GetCell(Point2D p)
	{
		return GetCell(p.x, p.y);
	}

	public Cell GetCell(Location2D p)
	{
		return GetCell(p.x, p.y);
	}

	public Cell GetCell(Point p)
	{
		return GetCell(p.X, p.Y);
	}

	public Cell GetCell(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			return null;
		}
		return Map[x][y];
	}

	public Cell GetCellChecked(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			MetricsManager.LogError("cell at invalid coordinates " + x + ", " + y + " requested from " + DebugName);
			return null;
		}
		Cell cell = Map[x][y];
		if (cell == null)
		{
			MetricsManager.LogError("no cell at coordinates " + x + ", " + y + " in " + DebugName);
		}
		return cell;
	}

	public Cell GetCellGlobal(int XD, int YD, bool LocalOnly = false, bool BuiltOnly = true)
	{
		if (XD >= 0 && YD >= 0 && XD < Width && YD < Height)
		{
			return Map[XD][YD];
		}
		if (LocalOnly)
		{
			return null;
		}
		int num = wX;
		int num2 = X;
		int num3 = wY;
		int num4 = Y;
		do
		{
			if (XD < 0)
			{
				if (num2 == 0)
				{
					num--;
					num2 += Definitions.Width;
				}
				num2--;
				XD += Width;
			}
			else if (XD >= Width)
			{
				if (num2 == Definitions.Width - 1)
				{
					num++;
					num2 -= Definitions.Width;
				}
				num2++;
				XD -= Width;
			}
			if (YD < 0)
			{
				if (num4 == 0)
				{
					num3--;
					num4 += Definitions.Height;
				}
				num4--;
				YD += Height;
			}
			else if (YD >= Height)
			{
				if (num4 == Definitions.Height - 1)
				{
					num3++;
					num4 -= Definitions.Height;
				}
				num4++;
				YD -= Height;
			}
		}
		while (XD < 0 || YD < 0 || XD >= Width || YD >= Height);
		if (num < 0)
		{
			return null;
		}
		if (num3 < 0)
		{
			return null;
		}
		if (num > 79)
		{
			return null;
		}
		if (num3 > 24)
		{
			return null;
		}
		string zoneID = XRL.World.ZoneID.Assemble(ZoneWorld, num, num3, num2, num4, Z);
		if (BuiltOnly && !The.ZoneManager.IsZoneLive(zoneID))
		{
			return null;
		}
		return The.ZoneManager.GetZone(zoneID)?.GetCell(XD, YD);
	}

	public Cell GetRandomCell(int withinBorder = 0)
	{
		return GetCell(Stat.Random(withinBorder, Width - 1 - withinBorder), Stat.Random(withinBorder, Height - 1 - withinBorder));
	}

	public void ExploreAll()
	{
		for (int i = 0; i <= ExploredMap.GetUpperBound(0); i++)
		{
			ExploredMap[i] = true;
		}
		FakeUnexploredMap = null;
	}

	public bool FireEvent(string ID)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEvent(ID))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool FireEvent(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEvent(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool BroadcastEvent(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].BroadcastEvent(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool BroadcastEvent(string Name)
	{
		return BroadcastEvent(Event.New(Name));
	}

	public bool FireEventDirect(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEventDirect(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void CalculateMissileMap(GameObject Shooter)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetCell(i, j).IsOccluding())
				{
					MissileMap[i][j] = MissileMapType.Wall;
				}
				else if (!GetCell(i, j).HasObjectWithPart("Combat"))
				{
					MissileMap[i][j] = MissileMapType.Hostile;
				}
				else
				{
					MissileMap[i][j] = MissileMapType.Empty;
				}
			}
		}
	}

	public static int CalculateNav(GameObject Looker, bool Autoexploring = false, bool Juggernaut = false)
	{
		int num = 0;
		if (GameObject.validate(ref Looker))
		{
			if (!Looker.HasStat("Intelligence") || Looker.Stat("Intelligence") > 6)
			{
				num |= 1;
			}
			if (Looker.IsBurrower || Looker.HasPropertyOrTag("PathAsIfBurrowing"))
			{
				num |= 2;
			}
			else if (!Looker.PhaseMatches(1))
			{
				num |= 2;
			}
			if (Autoexploring)
			{
				num |= 4;
			}
			if (Looker.IsFlying || Looker.HasTagOrProperty("PathAsIfFlying"))
			{
				num |= 8;
			}
			else
			{
				Brain pBrain = Looker.pBrain;
				if (pBrain != null && pBrain.WallWalker)
				{
					num |= 0x10;
				}
			}
			if (Looker.HasPropertyOrTag("IgnoresWalls"))
			{
				num |= 0x20;
			}
			if (Looker.HasSkill("Endurance_Swimming"))
			{
				num |= 0x40;
			}
			if (Looker.Slimewalking)
			{
				num |= 0x80;
			}
			if (Looker.LimitToAquatic())
			{
				num |= 0x100;
			}
			if (Looker.Polypwalking)
			{
				num |= 0x200;
			}
			if (Looker.Strutwalking)
			{
				num |= 0x400;
			}
			if (Looker.Reefer)
			{
				num |= 0x1000;
			}
			if (Juggernaut)
			{
				num |= 0x800;
			}
		}
		return num;
	}

	public static int CalculateNav(bool Smart, bool Burrower, bool Autoexploring, bool Flying, bool WallWalker, bool IgnoresWalls, bool Swimming, bool Slimewalking, bool Aquatic, bool Polypwalking, bool Strutwalking, bool Juggernaut, bool Reefer)
	{
		int num = 0;
		if (Smart)
		{
			num |= 1;
		}
		if (Burrower)
		{
			num |= 2;
		}
		if (Autoexploring)
		{
			num |= 4;
		}
		if (Flying)
		{
			num |= 8;
		}
		if (WallWalker)
		{
			num |= 0x10;
		}
		if (IgnoresWalls)
		{
			num |= 0x20;
		}
		if (Swimming)
		{
			num |= 0x40;
		}
		if (Slimewalking)
		{
			num |= 0x80;
		}
		if (Aquatic)
		{
			num |= 0x100;
		}
		if (Polypwalking)
		{
			num |= 0x200;
		}
		if (Strutwalking)
		{
			num |= 0x400;
		}
		if (Juggernaut)
		{
			num |= 0x800;
		}
		if (Reefer)
		{
			num |= 0x1000;
		}
		return num;
	}

	public static void UnpackNav(int Nav, out bool Smart, out bool Burrower, out bool Autoexploring, out bool Flying, out bool WallWalker, out bool IgnoresWalls, out bool Swimming, out bool Slimewalking, out bool Aquatic, out bool Polypwalking, out bool Strutwalking, out bool Juggernaut, out bool Reefer)
	{
		Smart = (Nav & 1) != 0;
		Burrower = (Nav & 2) != 0;
		Autoexploring = (Nav & 4) != 0;
		Flying = (Nav & 8) != 0;
		WallWalker = (Nav & 0x10) != 0;
		IgnoresWalls = (Nav & 0x20) != 0;
		Swimming = (Nav & 0x40) != 0;
		Slimewalking = (Nav & 0x80) != 0;
		Aquatic = (Nav & 0x100) != 0;
		Polypwalking = (Nav & 0x200) != 0;
		Strutwalking = (Nav & 0x400) != 0;
		Juggernaut = (Nav & 0x800) != 0;
		Reefer = (Nav & 0x1000) != 0;
	}

	public void CalculateNavigationMap(GameObject Looker, bool AddNoise = false, bool ExploredOnly = false, bool Juggernaut = false)
	{
		NoiseMap noiseMap = null;
		if (AddNoise)
		{
			noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
		}
		int nav = CalculateNav(Looker, Autoexploring: false, Juggernaut);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (ExploredOnly && !GetExplored(j, i))
				{
					NavigationMap[j, i].Weight = 100;
					continue;
				}
				NavigationMap[j, i].Weight = Map[j][i].NavigationWeight(Looker, nav);
				if (AddNoise && noiseMap.Noise[j, i] > 1)
				{
					NavigationMap[j, i].Weight = 98;
				}
			}
		}
		if (!Options.DrawNavigationWeightMaps)
		{
			return;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		for (int k = 0; k < scrapBuffer.Width; k++)
		{
			for (int l = 0; l < scrapBuffer.Height; l++)
			{
				string text = Math.Min(NavigationMap[k, l].Weight / 6, 15).ToString("X");
				scrapBuffer.Goto(k, l);
				string text2 = "&g";
				if (NavigationMap[k, l].Weight == 0)
				{
					text2 = "&G";
				}
				if (NavigationMap[k, l].Weight >= 50)
				{
					text2 = "&W";
				}
				if (NavigationMap[k, l].Weight >= 98)
				{
					text2 = "&r";
					text = "E";
				}
				if (NavigationMap[k, l].Weight >= 100)
				{
					text2 = "&K";
					text = "-";
				}
				scrapBuffer.Write(text2 + text[0]);
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		Keyboard.getch();
	}

	public bool IsWorldMap()
	{
		if (ZoneID == null)
		{
			return false;
		}
		return ZoneID.IndexOf('.') == -1;
	}

	public string GetZoneIDFromDirection(string Direction, int n = 1)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return ZoneID;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy, n);
		if (wx < 0)
		{
			return null;
		}
		if (wx > 79)
		{
			return null;
		}
		if (wy < 0)
		{
			return null;
		}
		if (wy > 24)
		{
			return null;
		}
		return XRL.World.ZoneID.Assemble(GetZoneWorld(), wx, wy, x, y, z);
	}

	public Zone GetZoneAtLevel(int Level)
	{
		int zoneX = GetZoneX();
		int zoneY = GetZoneY();
		int zonewX = GetZonewX();
		int zonewY = GetZonewY();
		return The.ZoneManager.GetZone(GetZoneWorld(), zonewX, zonewY, zoneX, zoneY, Level);
	}

	public Zone GetZoneFromDirection(string Direction)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return this;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy);
		return The.ZoneManager.GetZone(GetZoneWorld(), wx, wy, x, y, z);
	}

	public Zone GetZoneFromDirection(string Direction, int n)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return this;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy, n);
		return The.ZoneManager.GetZone(GetZoneWorld(), wx, wy, x, y, z);
	}

	public string GetZoneWorld()
	{
		return ZoneWorld;
	}

	public int GetZonewX()
	{
		return wX;
	}

	public int GetZonewY()
	{
		return wY;
	}

	public int GetZoneX()
	{
		return X;
	}

	public int GetZoneY()
	{
		return Y;
	}

	public int GetZoneZ()
	{
		return Z;
	}

	public bool GetVisibility(int x, int y)
	{
		return VisibilityMap[x + y * Width];
	}

	public LightLevel GetLight(int x, int y)
	{
		return LightMap[x + y * Width];
	}

	public bool GetExplored(int x, int y)
	{
		if (!ExploredMap[x + y * Width])
		{
			return false;
		}
		if (FakeUnexploredMap != null && FakeUnexploredMap[x + y * Width])
		{
			return false;
		}
		return true;
	}

	public void SetExplored(int x, int y, bool state)
	{
		ExploredMap[x + y * Width] = state;
	}

	public bool GetReallyExplored(int x, int y)
	{
		return ExploredMap[x + y * Width];
	}

	public void SetFakeUnexplored(int x, int y, bool state)
	{
		if (FakeUnexploredMap == null)
		{
			FakeUnexploredMap = new bool[Width * Height];
		}
		FakeUnexploredMap[x + y * Width] = state;
	}

	[Obsolete("This is dangerous to use because the queued events will be overwritten. We'll probably remove or refactor this. -BB 7-16-22")]
	public void QueueEvent(Event E)
	{
		if (QueuedEvents == null)
		{
			QueuedEvents = new List<IEvent>();
		}
		QueuedEvents.Add(E);
	}

	private void QueueEvent(MinEvent E)
	{
		if (QueuedEvents == null)
		{
			QueuedEvents = new List<IEvent>();
		}
		QueuedEvents.Add(E);
	}

	public void QueueEvent(string ID)
	{
		QueueEvent(new Event(ID));
	}

	private void QueueEventOnce(Event E)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is Event @event && @event.ID == E.ID)
				{
					return;
				}
			}
		}
		QueueEvent(E);
	}

	public void QueueEventOnce(string ID)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is Event @event && @event.ID == ID)
				{
					return;
				}
			}
		}
		QueueEvent(ID);
	}

	private void QueueEventOnce(MinEvent E)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is MinEvent minEvent && minEvent.ID == E.ID)
				{
					return;
				}
			}
		}
		QueueEvent(E);
	}

	public void CheckEventQueue()
	{
		if (QueuedEvents == null || QueuedEvents.Count <= 0)
		{
			return;
		}
		int num = 0;
		while (QueuedEvents.Count > 0)
		{
			if (++num >= 100)
			{
				MetricsManager.LogError("cyclic event queue on " + QueuedEvents[0].GetType().Name + ", clearing");
				QueuedEvents.Clear();
				break;
			}
			if (QueuedEventsToFire == null)
			{
				QueuedEventsToFire = new List<IEvent>(QueuedEvents.Count);
			}
			QueuedEventsToFire.Clear();
			QueuedEventsToFire.AddRange(QueuedEvents);
			QueuedEvents.Clear();
			foreach (IEvent item in QueuedEventsToFire)
			{
				try
				{
					if (item is Event e)
					{
						FireEvent(e);
					}
					else if (item is MinEvent minEvent && WantEvent(minEvent.GetID(), minEvent.GetCascadeLevel()))
					{
						HandleEvent(minEvent);
					}
				}
				catch (Exception ex)
				{
					XRLCore.LogError(ex.ToString(), "Queued event error");
				}
			}
		}
	}

	public bool IsAdjacentTo(Zone Z)
	{
		if (Math.Abs(Z.wX - wX) <= 1)
		{
			return Math.Abs(Z.wY - wY) <= 1;
		}
		return false;
	}

	public static int GetSuspendabilityTurns()
	{
		if (!Options.CacheEarly)
		{
			return 40;
		}
		return 5;
	}

	public Suspendability GetSuspendability(int Turns)
	{
		if (The.ZoneManager.ActiveZone == this)
		{
			return Suspendability.Active;
		}
		if (The.ZoneManager.PinnedZones != null)
		{
			if (The.ZoneManager.PinnedZones.Count > 3)
			{
				MetricsManager.LogException("ZonePins", new Exception("ZonePins"));
				The.ZoneManager.PinnedZones.Clear();
			}
			if (The.ZoneManager.PinnedZones.Contains(ZoneID))
			{
				return Suspendability.Pinned;
			}
		}
		if (XRLCore.CurrentTurn - LastActive <= Turns)
		{
			return Suspendability.TooRecentlyActive;
		}
		if (HasTryingToJoinPartyLeaderForZoneUncaching())
		{
			return Suspendability.CompanionTryingToJoinPartyLeader;
		}
		if (The.Player != null && The.Player.OnWorldMap() && HasPlayerLed())
		{
			return Suspendability.CompanionWhilePlayerIsOnWorldMap;
		}
		if (!FireEvent("CheckZoneSuspend"))
		{
			return Suspendability.CheckZoneSuspendEventFailed;
		}
		return Suspendability.Suspendable;
	}

	public static int GetFreezabilityTurns()
	{
		if (!Options.CacheEarly)
		{
			return 80;
		}
		return 5;
	}

	public Freezability GetFreezability(int Turns)
	{
		if (XRLCore.CurrentTurn - LastActive <= Turns)
		{
			return Freezability.TooRecentlyActive;
		}
		if (IsWorldMap() && ZoneWorld == The.ActiveZone.ZoneWorld)
		{
			return Freezability.IsWorldMap;
		}
		if (HasLeftBehindByPlayer())
		{
			return Freezability.FormerPlayerObject;
		}
		if (HasTryingToJoinPartyLeaderForZoneUncaching())
		{
			return Freezability.CompanionTryingToJoinPartyLeader;
		}
		if (The.Player != null && The.Player.OnWorldMap() && HasPlayerLed())
		{
			return Freezability.CompanionWhilePlayerIsOnWorldMap;
		}
		return Freezability.Freezable;
	}

	public Freezability GetFreezability()
	{
		return GetFreezability(GetFreezabilityTurns());
	}

	public bool GetFreezable(int Turns)
	{
		return GetFreezability(Turns) == Freezability.Freezable;
	}

	public bool GetFreezable()
	{
		return GetFreezable(GetFreezabilityTurns());
	}

	public bool WantEvent(int ID, int cascade)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].WantEvent(ID, cascade))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (!Map[j][i].HandleEvent(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool HandleEvent<T>(T E, IEvent ParentEvent) where T : MinEvent
	{
		bool result = HandleEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public List<GameObject> GetObjectsThatWantEvent(int ID, int cascade)
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				List<GameObject> objects = Map[j][i].Objects;
				int k = 0;
				for (int count = objects.Count; k < count; k++)
				{
					if (objects[k].WantEvent(ID, cascade))
					{
						list.Add(objects[k]);
					}
				}
			}
		}
		return list;
	}

	public bool AnyObjectsWantEvent(int ID, int cascade)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				List<GameObject> objects = Map[j][i].Objects;
				int k = 0;
				for (int count = objects.Count; k < count; k++)
				{
					if (objects[k].WantEvent(ID, cascade))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void Thawed()
	{
		eZoneThawed.Zone = this;
		HandleEvent(eZoneThawed);
		eZoneThawed.Reset();
	}

	public void Activated()
	{
		eZoneActivated.Zone = this;
		HandleEvent(eZoneActivated);
		The.Game.Systems.ForEach(delegate(IGameSystem s)
		{
			s.ZoneActivated(this);
		});
		eZoneActivated.Reset();
	}

	public void Deactivated()
	{
		eZoneDeactivated.Zone = this;
		HandleEvent(eZoneDeactivated);
		eZoneDeactivated.Reset();
	}

	public void WantSynchronizeExistence()
	{
		QueueEventOnce(eSynchronizeExistence);
	}

	public void CheckWeather(long TurnNumber)
	{
		if (HasWeather && TurnNumber > NextWindChange)
		{
			WindChange(TurnNumber);
		}
	}

	public void CheckWeather()
	{
		CheckWeather(The.Game.TimeTicks);
	}

	public void WindChange(long TurnNumber)
	{
		int currentWindSpeed = CurrentWindSpeed;
		string currentWindDirection = CurrentWindDirection;
		string windSpeedDescription = GetWindSpeedDescription(currentWindSpeed);
		int num2 = (CurrentWindSpeed = WindSpeed.RollCached());
		int num3 = num2;
		string text = (CurrentWindDirection = WindDirections.CachedCommaExpansion().GetRandomElement());
		string d = text;
		NextWindChange = TurnNumber + WindDuration.RollCached();
		if (!IsActive() || !The.Player.HasSkill("Survival"))
		{
			return;
		}
		string windSpeedDescription2 = GetWindSpeedDescription(num3);
		if (windSpeedDescription == windSpeedDescription2)
		{
			if (currentWindDirection != CurrentWindDirection)
			{
				if (The.Player.HasSkill("Survival_Trailblazer"))
				{
					MessageQueue.AddPlayerMessage("The wind changes direction from the " + Directions.GetExpandedDirection(currentWindDirection) + " to the " + Directions.GetExpandedDirection(d) + ".");
				}
				else
				{
					MessageQueue.AddPlayerMessage("The wind changes direction.");
				}
			}
		}
		else if (The.Player.HasSkill("Survival_Trailblazer"))
		{
			if (windSpeedDescription2 == null)
			{
				MessageQueue.AddPlayerMessage("The wind becomes still.");
			}
			else if (windSpeedDescription == null)
			{
				MessageQueue.AddPlayerMessage("The wind begins blowing at " + windSpeedDescription2 + " from the " + Directions.GetExpandedDirection(d) + ".");
			}
			else if (num3 > currentWindSpeed)
			{
				MessageQueue.AddPlayerMessage("The wind intensifies to " + windSpeedDescription2 + ", blowing from the " + Directions.GetExpandedDirection(d) + ".");
			}
			else if (num3 < currentWindSpeed)
			{
				MessageQueue.AddPlayerMessage("The wind calms to " + windSpeedDescription2 + ", blowing from the " + Directions.GetExpandedDirection(d) + ".");
			}
		}
		else if (windSpeedDescription2 == null)
		{
			MessageQueue.AddPlayerMessage("The wind becomes still.");
		}
		else if (windSpeedDescription == null)
		{
			MessageQueue.AddPlayerMessage("The wind begins blowing at " + windSpeedDescription2 + ".");
		}
		else if (num3 > currentWindSpeed)
		{
			MessageQueue.AddPlayerMessage("The wind intensifies to " + windSpeedDescription2 + ".");
		}
		else if (num3 < currentWindSpeed)
		{
			MessageQueue.AddPlayerMessage("The wind calms to " + windSpeedDescription2 + ".");
		}
	}

	public void WindChange()
	{
		CheckWeather(The.Game.TimeTicks);
	}

	public static string GetWindSpeedDescription(int kph)
	{
		if (kph <= 0)
		{
			return null;
		}
		if (kph <= 10)
		{
			return "a very gentle breeze";
		}
		if (kph <= 20)
		{
			return "a gentle breeze";
		}
		if (kph <= 30)
		{
			return "a moderate breeze";
		}
		if (kph <= 40)
		{
			return "a fresh breeze";
		}
		if (kph <= 50)
		{
			return "a strong breeze";
		}
		if (kph <= 60)
		{
			return "near gale intensity";
		}
		if (kph <= 70)
		{
			return "gale intensity";
		}
		if (kph <= 90)
		{
			return "strong gale intensity";
		}
		if (kph <= 100)
		{
			return "storm intensity";
		}
		if (kph <= 120)
		{
			return "violent storm intensity";
		}
		return "hurricane intensity";
	}

	public void ClearNavigationCaches()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Map[i][j].ClearNavigationCache();
			}
		}
	}

	public string GetDirectionFromZone(Zone Target)
	{
		if (Target == null || Target == this)
		{
			return ".";
		}
		if (resolvedX == Target.resolvedX)
		{
			if (resolvedY == Target.resolvedY)
			{
				return ".";
			}
			if (resolvedY < Target.resolvedY)
			{
				return "S";
			}
			return "N";
		}
		if (resolvedX < Target.resolvedX)
		{
			if (resolvedY == Target.resolvedY)
			{
				return "E";
			}
			if (resolvedY < Target.resolvedY)
			{
				return "SE";
			}
			return "NE";
		}
		if (resolvedY == Target.resolvedY)
		{
			return "W";
		}
		if (resolvedY < Target.resolvedY)
		{
			return "SW";
		}
		return "NW";
	}

	public void Constrain(ref int x, ref int y)
	{
		if (x < 0)
		{
			x = 0;
		}
		if (y < 0)
		{
			y = 0;
		}
		if (x >= Width)
		{
			x = Width - 1;
		}
		if (y >= Height)
		{
			y = Height - 1;
		}
	}

	public void Constrain(ref int x1, ref int y1, ref int x2, ref int y2)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (x2 < 0)
		{
			x2 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (y2 < 0)
		{
			y2 = 0;
		}
		if (x1 >= Width)
		{
			x1 = Width - 1;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y1 >= Height)
		{
			y1 = Height - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
	}

	public Cell GetPullDownLocation(GameObject who)
	{
		try
		{
			string zoneProperty = GetZoneProperty("pulldownLocation");
			if (!string.IsNullOrEmpty(zoneProperty) && zoneProperty.IndexOf(',') != -1)
			{
				string[] array = zoneProperty.Split(',');
				Cell cell = GetCell(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]));
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception processing pulldown location", x);
		}
		for (int num = Height - 1; num >= 0; num--)
		{
			for (int i = Width / 2; i < Width - 1; i++)
			{
				Cell cell = GetCell(i, num);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who) && !cell.HasSwimmingDepthLiquid())
				{
					return cell;
				}
				cell = GetCell(Width - 1 - i, num);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who) && !cell.HasSwimmingDepthLiquid())
				{
					return cell;
				}
			}
		}
		for (int num2 = Height - 1; num2 >= 0; num2--)
		{
			for (int j = Width / 2; j < Width - 1; j++)
			{
				Cell cell = GetCell(j, num2);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who))
				{
					return cell;
				}
				cell = GetCell(Width - 1 - j, num2);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		for (int num3 = Height - 1; num3 >= 0; num3--)
		{
			for (int k = Width / 2; k < Width - 1; k++)
			{
				Cell cell = GetCell(k, num3);
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
				cell = GetCell(Width - 1 - k, num3);
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		return GetCell(Width / 2, Height - 1);
	}
}

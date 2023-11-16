using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

public class Cell
{
	public const int DISTANCE_INDEFINITE = 9999999;

	public string PaintRenderString;

	public string PaintTileColor;

	public string PaintColorString;

	public string PaintTile;

	public string PaintDetailColor;

	private static StringBuilder addressBuilder = new StringBuilder();

	public int X;

	public int Y;

	public Zone ParentZone;

	public List<GameObject> Objects = new List<GameObject>();

	public List<string> SemanticTags;

	public string _GroundLiquid;

	public const int DEFAULT_ALPHA = 230;

	[NonSerialized]
	public Color32 minimapColor = INVALID_CACHE;

	[NonSerialized]
	public bool lastMinimapVisibile;

	[NonSerialized]
	public bool lastMinimapLit;

	[NonSerialized]
	public bool lastMinimapExplored;

	[NonSerialized]
	private static Color32 WhiteAlphaD = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 230);

	[NonSerialized]
	private static Color32 BlackAlpha32 = new Color32(0, 0, 0, 32);

	[NonSerialized]
	private static Color32 BlackAlpha128 = new Color32(0, 0, 0, 128);

	[NonSerialized]
	private static Color32 BlackAlpha164 = new Color32(0, 0, 0, 164);

	[NonSerialized]
	private static Color32 VioletAlphaD = new Color32(byte.MaxValue, 0, byte.MaxValue, 230);

	[NonSerialized]
	private static Color32 RedAlphaD = new Color32(byte.MaxValue, 0, 0, 230);

	[NonSerialized]
	private static Color32 GreenAlphaD = new Color32(0, byte.MaxValue, 0, 230);

	[NonSerialized]
	private static Color32 DarkYellowAlphaD = new Color32(128, 128, 0, 230);

	[NonSerialized]
	private static Color32 DarkBlueAlphaD = new Color32(0, 0, 128, 230);

	[NonSerialized]
	private static Color32 GrayAlphaD = new Color32(128, 128, 128, 230);

	[NonSerialized]
	private static Color32 CanaryAlphaD = new Color32(byte.MaxValue, byte.MaxValue, 128, 230);

	[NonSerialized]
	public Dictionary<int, int> NavigationWeightCache = new Dictionary<int, int>();

	public static readonly Color32 INVALID_CACHE = new Color32(1, 2, 3, 4);

	[NonSerialized]
	private static Event eBeforePhysicsRejectObjectEntringCell = new Event("BeforePhysicsRejectObjectEntringCell", "Object", null);

	public int OccludeCache = -1;

	private static List<GameObject> EventList = new List<GameObject>(8);

	private static bool EventListInUse = false;

	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListCardinalFirst = new string[8] { "N", "E", "S", "W", "NW", "SE", "SW", "NE" };

	public static string[] DirectionListCardinalOnly = new string[4] { "N", "E", "S", "W" };

	[NonSerialized]
	private List<Cell> _LocalAdjacentCellCache;

	[NonSerialized]
	private List<Cell> _LocalCardinalAdjacentCellCache;

	[NonSerialized]
	private static StringBuilder RenderBuilder = new StringBuilder(1024);

	[NonSerialized]
	private static RenderEvent eRender = new RenderEvent();

	[NonSerialized]
	private RenderEvent eLastRender = new RenderEvent();

	[NonSerialized]
	private static Color ColorBlack = ConsoleLib.Console.ColorUtility.ColorMap['k'];

	[NonSerialized]
	private static Color ColorBrightBlue = new Color(0f, 0f, 1f);

	[NonSerialized]
	private static Color ColorBrightCyan = new Color(0f, 1f, 1f);

	[NonSerialized]
	private static Color ColorBrightGreen = new Color(0f, 0f, 1f);

	[NonSerialized]
	private static Color ColorBrightRed = new Color(1f, 0f, 0f);

	[NonSerialized]
	private static Color ColorDarkBlue = new Color(0f, 0f, 0.5f);

	[NonSerialized]
	private static Color ColorDarkCyan = new Color(0f, 0.5f, 0.5f);

	[NonSerialized]
	private static Color ColorDarkGreen = new Color(0f, 0.5f, 0f);

	[NonSerialized]
	private static Color ColorDarkRed = new Color(0.5f, 0f, 0f);

	[NonSerialized]
	private static Color ColorGray = ConsoleLib.Console.ColorUtility.ColorMap['y'];

	public Point2D Pos2D => new Point2D(X, Y);

	public Location2D location => Location2D.get(X, Y);

	public int LocalCoordKey => (X << 16) | Y;

	public string DebugName => X + ", " + Y;

	public LightLevel CurrentLightLevel => ParentZone.GetLight(X, Y);

	public int RenderedObjectsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < Objects.Count; i++)
			{
				if (Objects[i].pRender != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public string GroundLiquid
	{
		get
		{
			if (_GroundLiquid != null)
			{
				return _GroundLiquid;
			}
			if (ParentZone != null)
			{
				return ParentZone.GroundLiquid;
			}
			return null;
		}
		set
		{
			_GroundLiquid = value;
		}
	}

	public bool Explored => ParentZone.GetExplored(X, Y);

	public bool InActiveZone
	{
		get
		{
			if (ParentZone != null)
			{
				return ParentZone.IsActive();
			}
			return false;
		}
	}

	public bool juiceEnabled => Options.UseOverlayCombatEffects;

	public Cell(Zone ParentZone)
	{
		this.ParentZone = ParentZone;
	}

	public bool AllInDirections(IEnumerable<string> directions, int distance, Predicate<Cell> test)
	{
		foreach (string direction in directions)
		{
			if (!AllInDirection(direction, distance, test))
			{
				return false;
			}
		}
		return true;
	}

	public GlobalLocation GetGlobalLocation()
	{
		return new GlobalLocation(this);
	}

	public bool AnyInDirection(string direction, int distance, Predicate<Cell> test)
	{
		Cell cell = this;
		for (int i = 0; i < distance; i++)
		{
			cell = cell.GetCellFromDirection(direction);
			if (test(cell))
			{
				return true;
			}
			if (cell == null)
			{
				return false;
			}
		}
		return false;
	}

	public bool AllInDirection(string direction, int distance, Predicate<Cell> test)
	{
		Cell cell = this;
		for (int i = 0; i < distance; i++)
		{
			cell = cell.GetCellFromDirection(direction);
			if (!test(cell))
			{
				return false;
			}
			if (cell == null)
			{
				break;
			}
		}
		return true;
	}

	public string GetAddress()
	{
		addressBuilder.Length = 0;
		addressBuilder.Append(ParentZone.ZoneID).Append('@').Append(X)
			.Append(',')
			.Append(Y);
		return addressBuilder.ToString();
	}

	public void walk(Func<Cell, Cell> next, Predicate<Cell> walker)
	{
		if (walker(this))
		{
			next(this)?.walk(next, walker);
		}
	}

	public static Cell FromAddress(string CellAddress)
	{
		try
		{
			string[] array = CellAddress.Split('@');
			string[] array2 = array[1].Split(',');
			return XRLCore.Core.Game.ZoneManager.GetZone(array[0]).GetCell(int.Parse(array2[0]), int.Parse(array2[1]));
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
			return null;
		}
	}

	public bool ConsideredOutside()
	{
		if (ParentZone.IsOutside())
		{
			return true;
		}
		if (HasObject("FlyingWhitelistArea"))
		{
			return true;
		}
		return false;
	}

	public bool HasSemanticTag(string tag)
	{
		if (SemanticTags != null)
		{
			return SemanticTags.Any((string t) => t.EqualsNoCase(tag));
		}
		return false;
	}

	public void AddSemanticTag(string tag)
	{
		if (SemanticTags == null)
		{
			SemanticTags = new List<string>();
		}
		if (!HasSemanticTag(tag))
		{
			SemanticTags.Add(tag);
		}
	}

	public void RemoveSemanticTag(string tag)
	{
		if (SemanticTags != null && HasSemanticTag(tag))
		{
			SemanticTags.Remove(tag);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(X);
		stringBuilder.Append(",");
		stringBuilder.Append(Y);
		stringBuilder.Append(" Objects:");
		foreach (GameObject @object in Objects)
		{
			stringBuilder.Append(@object.ToString() + " ");
		}
		return stringBuilder.ToString();
	}

	public Color32 GetMinimapColor()
	{
		if (!minimapColor.Equals(INVALID_CACHE))
		{
			return minimapColor;
		}
		if (this == XRLCore.Core.Game.Player.Body.GetCurrentCell())
		{
			return WhiteAlphaD;
		}
		if (!IsExplored())
		{
			return BlackAlpha32;
		}
		if (lastMinimapVisibile != IsVisible())
		{
			lastMinimapVisibile = IsVisible();
			minimapColor = INVALID_CACHE;
		}
		if (lastMinimapLit != IsLit())
		{
			lastMinimapVisibile = IsVisible();
			minimapColor = INVALID_CACHE;
		}
		if (lastMinimapExplored != IsExplored())
		{
			lastMinimapVisibile = IsVisible();
			minimapColor = INVALID_CACHE;
		}
		if (minimapColor.a == INVALID_CACHE.a && minimapColor.r == INVALID_CACHE.r && minimapColor.g == INVALID_CACHE.g && minimapColor.b == INVALID_CACHE.b)
		{
			if (IsLit())
			{
				minimapColor = BlackAlpha164;
			}
			else
			{
				minimapColor = BlackAlpha128;
			}
			if (ParentZone.IsWorldMap())
			{
				minimapColor = BlackAlpha32;
			}
			else if ((HasObjectWithPart("StairsUp") || HasObjectWithPart("StairsDown")) && !HasObjectWithPart("HiddenRender"))
			{
				minimapColor = VioletAlphaD;
			}
			else if (IsVisible() && IsLit() && HasObjectWithPart("Combat"))
			{
				GameObject firstObjectWithPart = GetFirstObjectWithPart("Combat");
				if (firstObjectWithPart.pBrain != null)
				{
					if (firstObjectWithPart.pBrain.IsHostileTowards(The.Player))
					{
						minimapColor = RedAlphaD;
					}
					else
					{
						minimapColor = GreenAlphaD;
					}
				}
				else
				{
					minimapColor = RedAlphaD;
				}
			}
			else if (HasObjectWithTag("Chest"))
			{
				minimapColor = DarkYellowAlphaD;
			}
			else if (HasObjectWithTag("LiquidVolume"))
			{
				minimapColor = DarkBlueAlphaD;
			}
			else if (HasObjectWithTag("Wall"))
			{
				minimapColor = GrayAlphaD;
			}
			else if (HasObjectWithTag("Door"))
			{
				minimapColor = CanaryAlphaD;
			}
		}
		return minimapColor;
	}

	public List<GameObject> FastFloodVisibility(string SearchPart, int Radius)
	{
		return ParentZone.FastFloodVisibility(X, Y, Radius, SearchPart, null);
	}

	public static Cell Load(SerializationReader Reader, int x, int y, Zone ParentZone)
	{
		Cell cell = new Cell(ParentZone);
		cell.X = x;
		cell.Y = y;
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			cell.Objects.Add(Reader.ReadGameObject());
		}
		num = Reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			cell.AddObjectWithoutEvents(GameObject.createUnmodified(Reader.ReadString()));
		}
		cell.PaintTile = Reader.ReadString();
		cell.PaintTileColor = Reader.ReadString();
		cell.PaintRenderString = Reader.ReadString();
		cell.PaintColorString = Reader.ReadString();
		cell.PaintDetailColor = Reader.ReadString();
		int num2 = Reader.ReadInt32();
		if (num2 > 0)
		{
			cell.SemanticTags = new List<string>(num2);
			for (int k = 0; k < num2; k++)
			{
				cell.SemanticTags.Add(Reader.ReadString());
			}
		}
		return cell;
	}

	public bool ShouldWrite(GameObject Object)
	{
		if (Object.HasIntProperty("ForceMutableSave"))
		{
			return true;
		}
		if (Object.HasTag("Immutable"))
		{
			return false;
		}
		if (Object.HasTag("ImmutableWhenUnexplored") && !IsExplored())
		{
			return false;
		}
		return true;
	}

	public void Save(SerializationWriter Writer)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Objects.Count; i++)
		{
			if (!ShouldWrite(Objects[i]))
			{
				num2++;
			}
			else
			{
				num++;
			}
		}
		Writer.Write(num);
		int j = 0;
		for (int count = Objects.Count; j < count; j++)
		{
			GameObject gameObject = Objects[j];
			if (ShouldWrite(gameObject))
			{
				Writer.WriteGameObject(gameObject);
			}
		}
		Writer.Write(num2);
		int k = 0;
		for (int count2 = Objects.Count; k < count2; k++)
		{
			GameObject gameObject2 = Objects[k];
			if (!ShouldWrite(gameObject2))
			{
				Writer.Write(gameObject2.Blueprint);
			}
		}
		Writer.Write(PaintTile);
		Writer.Write(PaintTileColor);
		Writer.Write(PaintRenderString);
		Writer.Write(PaintColorString);
		Writer.Write(PaintDetailColor);
		if (SemanticTags == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(SemanticTags.Count);
		foreach (string semanticTag in SemanticTags)
		{
			Writer.Write(semanticTag);
		}
	}

	public void ClearNavigationCache()
	{
		NavigationWeightCache.Clear();
	}

	public int GetNavigationWeightFor(GameObject Looker, bool Autoexploring = false, bool Juggernaut = false)
	{
		return NavigationWeight(Looker, Zone.CalculateNav(Looker, Autoexploring, Juggernaut));
	}

	public int NavigationWeight(GameObject Looker = null, bool Smart = false, bool Burrower = false, bool Autoexploring = false, bool Flying = false, bool WallWalker = false, bool IgnoresWalls = false, bool Swimming = false, bool Slimewalking = false, bool Aquatic = false, bool Polypwalking = false, bool Strutwalking = false, bool Juggernaut = false, bool Reefer = false)
	{
		return NavigationWeight(Looker, Zone.CalculateNav(Smart, Burrower, Autoexploring, Flying, WallWalker, IgnoresWalls, Swimming, Slimewalking, Aquatic, Polypwalking, Strutwalking, Juggernaut, Reefer));
	}

	public int NavigationWeight(GameObject Looker, int Nav)
	{
		if (((uint)Nav & 0x800u) != 0)
		{
			return 1;
		}
		if (NavigationWeightCache.TryGetValue(Nav, out var value))
		{
			return value;
		}
		bool Uncacheable = false;
		if (((uint)Nav & 0x10u) != 0)
		{
			bool flag = false;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].IsWalkableWall(Looker, ref Uncacheable))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				value = 101;
			}
			else if (HasObjectWithPart("Combat"))
			{
				value = 99;
			}
		}
		if (((uint)Nav & 0x100u) != 0 && value < 100 && !HasAquaticSupportFor(Looker))
		{
			value = 101;
		}
		if (value < 3)
		{
			switch (Nav & 0x600)
			{
			case 1536:
				if (!HasObjectWithTag("Reef"))
				{
					value = 3;
				}
				break;
			case 512:
				if (!HasObjectWithTag("NavigationPolyp"))
				{
					value = 3;
				}
				break;
			case 1024:
				if (!HasObjectWithTag("NavigationStrut"))
				{
					value = 3;
				}
				break;
			}
		}
		if (value < 25 && ((uint)Nav & 0x1000u) != 0 && HasObjectWithTag("Reef"))
		{
			value = 25;
		}
		value = GetNavigationWeightEvent.GetFor(this, Looker, ref Uncacheable, value, Nav);
		value = GetAdjacentNavigationWeightEvent.GetFor(this, Looker, ref Uncacheable, value, Nav);
		value = ActorGetNavigationWeightEvent.GetFor(this, Looker, ref Uncacheable, value, Nav);
		if (!Uncacheable)
		{
			NavigationWeightCache[Nav] = value;
		}
		return value;
	}

	public int NavigationWeight(GameObject Looker, ref int Nav)
	{
		if (((uint)Nav & 0x10000000u) != 0)
		{
			Nav = Zone.CalculateNav(Looker) | (Nav & -268435457);
		}
		return NavigationWeight(Looker, Nav);
	}

	public bool IsGraveyard()
	{
		return this == XRLCore.Core.Game.Graveyard;
	}

	public GameObject FastFloodVisibilityFirstBlueprint(string blueprint, GameObject looker)
	{
		return ParentZone.FastSquareVisibilityFirstBlueprint(X, Y, 10, blueprint, looker);
	}

	public bool HasObjectNearby(string blueprint, GameObject looker = null, bool visibleOnly = false, bool exploredOnly = true)
	{
		return ParentZone.FastFloodFindAnyBlueprint(X, Y, 24, blueprint, looker ?? The.Player, visibleOnly, exploredOnly);
	}

	public GameObject FindObjectNearby(string blueprint, GameObject looker = null, bool visibleOnly = false, bool exploredOnly = true)
	{
		return ParentZone.FastFloodFindFirstBlueprint(X, Y, 24, blueprint, looker ?? The.Player, visibleOnly, exploredOnly);
	}

	public bool HasObjectWithBlueprint(string[] Blueprints)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			int j = 0;
			for (int num = Blueprints.Length; j < num; j++)
			{
				if (Objects[i].Blueprint == Blueprints[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectInDirection(string Direction, string Blueprint)
	{
		return GetCellFromDirection(Direction)?.HasObject(Blueprint) ?? false;
	}

	public bool HasStairs()
	{
		return HasObjectWithTagOrProperty("Stairs");
	}

	public bool HasObjectWithTag(string Tag)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTag(string Tag, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag) && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTagOrProperty(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTagOrProperty(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTagEqualToValue(string Name, string Value)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return string.Equals(Objects[i].GetPropertyOrTag(Name), Value);
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTagOtherThan(string Name, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPropertyOrTag(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTagAdjacent(string Tag)
	{
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells())
		{
			if (localAdjacentCell.HasObjectWithTag(Tag))
			{
				return true;
			}
		}
		return false;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> pFilter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && pFilter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> pFilter)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> pFilter, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && pFilter(Objects[i]))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> pFilter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && pFilter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2, Name3);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> pFilter)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2, Name3);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> pFilter, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && pFilter(Objects[i]))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public int CountObjectsWithTag(string Tag)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag))
			{
				num++;
			}
		}
		return num;
	}

	public int CountObjectWithTagCardinalAdjacent(string Tag)
	{
		int i = 0;
		ForeachCardinalAdjacentLocalCell(delegate(Cell c)
		{
			if (c.HasObjectWithTag(Tag))
			{
				i++;
			}
		});
		return i;
	}

	public GameObject GetFirstObjectWithTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTag(string Name, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagExcept(string Name, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPropertyOrTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagAndNotTag(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name1) && !Objects[i].HasTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagAndNotPropertyOrTag(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) && !Objects[i].HasPropertyOrTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagAndNotPropertyOrTag(string Name1, string Name2, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) && !Objects[i].HasPropertyOrTag(Name2) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject()
	{
		if (Objects.Count <= 0)
		{
			return null;
		}
		return Objects[0];
	}

	public GameObject GetFirstVisibleObject()
	{
		if (!IsVisible())
		{
			return null;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectExcept(string Blueprint, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectExcept(string Blueprint, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].Blueprint == Blueprint && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual GameObject GetFirstObjectWithPart(string Part)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part))
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(List<string> Parts)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = Parts.Count; j < count2; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetFirstObjectWithPart(Part);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, Predicate<GameObject> Filter1, Predicate<GameObject> Filter2)
	{
		if (Filter1 == null)
		{
			if (Filter2 == null)
			{
				return GetFirstObjectWithPart(Part);
			}
			return GetFirstObjectWithPart(Part, Filter2);
		}
		if (Filter2 == null)
		{
			return GetFirstObjectWithPart(Part, Filter1);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && Filter1(Objects[i]) && Filter2(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(Part) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject GetFirstRealObject(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics != null && gameObject.pPhysics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithAnyPart(List<string> Parts)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithAnyPart(List<string> Parts, Predicate<GameObject> filter)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]) && filter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetHighestRenderLayerObject()
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObject(Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithPart(string Part)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Objects[i].HasPart(Part))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithPart(string Part, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithAnyPart(List<string> Parts)
	{
		GameObject gameObject = null;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.pRender == null || (gameObject != null && gameObject2.pRender.RenderLayer <= gameObject.pRender.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithAnyPart(List<string> Parts, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.pRender == null || (gameObject != null && gameObject2.pRender.RenderLayer <= gameObject.pRender.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]) && Filter(gameObject2))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectFor(GameObject who)
	{
		GameObject gameObject = null;
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectFor(GameObject who, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithPartFor(GameObject who, string Part)
	{
		GameObject gameObject = null;
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Objects[i].HasPart(Part))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithPartFor(GameObject who, string Part, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (Objects[i].pRender != null && (gameObject == null || Objects[i].pRender.RenderLayer > gameObject.pRender.RenderLayer) && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithAnyPartFor(GameObject who, List<string> Parts)
	{
		GameObject gameObject = null;
		bool flag = false;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (gameObject2.pRender == null || (gameObject != null && gameObject2.pRender.RenderLayer <= gameObject.pRender.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithAnyPartFor(GameObject who, List<string> Parts, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = false;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.ConsiderSolidFor(who))
			{
				if (!flag)
				{
					flag = true;
					gameObject = null;
				}
			}
			else if (flag)
			{
				continue;
			}
			if (gameObject2.pRender == null || (gameObject != null && gameObject2.pRender.RenderLayer <= gameObject.pRender.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]) && Filter(gameObject2))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public bool HasObjectWithEffect(string EffectType)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(Type EffectType)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(string EffectType, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType) && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(Type EffectType, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType) && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetFirstObjectWithEffect(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithEffect(string Name)
	{
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(Name))
			{
				if (list == null)
				{
					list = new List<GameObject> { Objects[i] };
				}
				else
				{
					list.Add(Objects[i]);
				}
			}
		}
		return list;
	}

	public bool HasFurniture()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].isFurniture())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCombatObject()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsCombatObject())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRealObject()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsReal)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObject()
	{
		return Objects.Count > 0;
	}

	public bool HasObject(GameObject obj)
	{
		return Objects.Contains(obj);
	}

	public bool HasObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObject(Predicate<GameObject> filter)
	{
		if (filter == null)
		{
			return Objects.Count > 0;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectOtherThan(Predicate<GameObject> pFilter, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool HasObjectWithPart(string Part)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVisibleObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPart(string Part, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartOtherThan(string Part, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithAnyPart(List<string> Parts)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithAnyPart(List<string> Parts, Predicate<GameObject> pFilter)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]) && pFilter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprint(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprintEndsWith(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint != null && Objects[i].Blueprint.EndsWith(Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprintStartsWith(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint != null && Objects[i].Blueprint.StartsWith(Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public virtual List<GameObject> GetObjectsInCell()
	{
		return new List<GameObject>(Objects);
	}

	public int GetAdjacentObjectTestCount(Predicate<GameObject> test, int Radius = 1)
	{
		int num = 0;
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells(Radius))
		{
			num += localAdjacentCell.Objects.Where((GameObject o) => test(o)).Count();
		}
		return num;
	}

	public int GetAdjacentObjectCount(string Blueprint, int Radius = 1)
	{
		int num = 0;
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells(Radius))
		{
			num += localAdjacentCell.GetObjectCount(Blueprint);
		}
		return num;
	}

	public int GetObjectCount()
	{
		return Objects.Count;
	}

	public int GetObjectCount(string Blueprint)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCount(Predicate<GameObject> pFilter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public bool AnyObject()
	{
		return Objects.Count > 0;
	}

	public bool AnyObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyObject(Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetObjectInCell(int n)
	{
		if (Objects.Count > n)
		{
			return Objects[n];
		}
		return null;
	}

	public GameObject FindObject(Predicate<GameObject> test)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && test(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectExcept(Predicate<GameObject> test, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && Objects[i] != skip && test(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectExcept(string Blueprint, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && Objects[i] != skip)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public void FindObjects(List<GameObject> List, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && Filter(Objects[i]))
			{
				List.Add(Objects[i]);
			}
		}
	}

	public Cell ClearWalls()
	{
		ClearObjectsWithTag("Wall");
		return this;
	}

	public GameObject ClearAndAddObject(string Blueprint)
	{
		Clear();
		return AddObject(Blueprint);
	}

	public GameObject AddTableObject(string Table)
	{
		GameObject gameObject = GameObjectFactory.create(PopulationManager.RollOneFrom(Table).Blueprint);
		if (gameObject == null)
		{
			throw new Exception("failed to roll a population result from " + Table);
		}
		return AddObject(gameObject);
	}

	public List<GameObject> AddPopulation(string Table)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (PopulationResult item in PopulationManager.Generate(Table, new Dictionary<string, string> { 
		{
			"zonetier",
			ZoneManager.zoneGenerationContextTier.ToString()
		} }))
		{
			list.Add(AddObject(item.Blueprint));
		}
		return list;
	}

	public void AddObject(string Blueprint, int n, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		for (int i = 0; i < n; i++)
		{
			AddObject(Blueprint, null, Tracking, beforeObjectCreated, afterObjectCreated);
		}
	}

	public GameObject ClearAndAddObject(GameObject obj, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		return AddObject(obj, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, Tracking);
	}

	public GameObject ClearAndAddObject(string Blueprint, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		return AddObject(Blueprint, null, Tracking, null, null);
	}

	public void ClearAndAddObject(string Blueprint, int n, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		AddObject(Blueprint, n, Tracking);
	}

	public GameObject RequireObject(string Blueprint, string Context = null, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		if (HasObject(Blueprint))
		{
			return FindObject(Blueprint);
		}
		GameObject gameObject = GameObject.create(Blueprint, 0, 0, Context, beforeObjectCreated, afterObjectCreated);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, Tracking);
	}

	public GameObject AddObject(string Blueprint, string Context = null, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		GameObject gameObject = GameObject.create(Blueprint, 0, 0, Context, beforeObjectCreated, afterObjectCreated);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, Tracking);
	}

	public GameObject AddObject(string Blueprint, Action<GameObject> beforeObjectCreated, List<GameObject> Tracking = null)
	{
		GameObject gameObject = GameObject.create(Blueprint, 0, 0, null, beforeObjectCreated);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, Tracking);
	}

	public virtual GameObject AddObject(GameObject GO, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, bool Repaint = true, string Direction = null, string Type = null, GameObject Dragging = null, List<GameObject> Tracking = null, IEvent ParentEvent = null)
	{
		minimapColor = INVALID_CACHE;
		OccludeCache = -1;
		ClearNavigationCache();
		Objects.Add(GO);
		Tracking?.Add(GO);
		if (GO.pPhysics != null)
		{
			GO.pPhysics.EnterCell(this);
		}
		GO.ProcessEnterCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, ParentEvent);
		GO.ProcessEnteredCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, ParentEvent);
		GO.ProcessObjectEnteredCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, ParentEvent);
		if (GO.IsPlayer())
		{
			Zone.SoundMapDirty = true;
		}
		if (Repaint && ParentZone.Built)
		{
			CheckPaintWallsAround(GO);
			CheckPaintLiquidsAround(GO);
		}
		return GO;
	}

	protected virtual GameObject AddObjectWithoutEvents(GameObject GO, List<GameObject> Tracking = null)
	{
		Objects.Add(GO);
		Tracking?.Add(GO);
		if (GO.pPhysics != null)
		{
			GO.pPhysics.EnterCell(this);
		}
		return GO;
	}

	public void SetReachable(bool State)
	{
		if (ParentZone.ReachableMap == null)
		{
			ParentZone.ClearReachableMap();
		}
		ParentZone.ReachableMap[X, Y] = State;
	}

	public void SetExplored(bool State)
	{
		ParentZone.SetExplored(X, Y, State);
	}

	public void SetExplored()
	{
		ParentZone.SetExplored(X, Y, state: true);
	}

	public void SetFakeUnexplored(bool State)
	{
		ParentZone.SetFakeUnexplored(X, Y, State);
	}

	public void MakeFakeUnexplored()
	{
		ParentZone.SetFakeUnexplored(X, Y, state: true);
	}

	public virtual bool IsExplored()
	{
		return ParentZone.GetExplored(X, Y);
	}

	public virtual bool IsReallyExplored()
	{
		return ParentZone.GetReallyExplored(X, Y);
	}

	public virtual bool IsExploredFor(GameObject obj)
	{
		if (obj != null && obj.IsPlayer())
		{
			return IsExplored();
		}
		return IsReallyExplored();
	}

	public LightLevel GetLight()
	{
		return ParentZone.GetLight(X, Y);
	}

	public bool IsLit()
	{
		if (IsGraveyard())
		{
			return false;
		}
		return (int)ParentZone.GetLight(X, Y) > 0;
	}

	public bool IsVisible()
	{
		if (IsGraveyard())
		{
			return false;
		}
		if (ParentZone == null || !ParentZone.IsActive())
		{
			return false;
		}
		return ParentZone.GetVisibility(X, Y);
	}

	public Cell getClosestCellFromList(List<Cell> cells)
	{
		cells.Sort((Cell a, Cell b) => a.ManhattanDistanceTo(this).CompareTo(b.ManhattanDistanceTo(this)));
		return cells.FirstOrDefault();
	}

	public void PaintWallsAround()
	{
		ZoneManager.PaintWalls(ParentZone, X - 1, Y - 1, X + 1, Y + 1);
	}

	public void CheckPaintWallsAround(GameObject GO)
	{
		if (GO.HasTagOrProperty("PaintedWall") || GO.HasTagOrProperty("PaintedFence") || GO.HasTagOrProperty("ForceRepaintSolid"))
		{
			PaintWallsAround();
		}
	}

	public void PaintLiquidsAround()
	{
		ZoneManager.PaintWater(ParentZone, X - 1, Y - 1, X + 1, Y + 1);
	}

	public void CheckPaintLiquidsAround(GameObject GO)
	{
		if (GO.HasTagOrProperty("PaintedLiquidAtlas") || GO.HasTagOrProperty("ForceRepaintLiquid"))
		{
			PaintLiquidsAround();
		}
	}

	public bool RemoveObject(GameObject GO, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, bool Repaint = true, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		GO.ProcessLeavingCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, ParentEvent);
		minimapColor = INVALID_CACHE;
		OccludeCache = -1;
		ClearNavigationCache();
		Objects.Remove(GO);
		GO.ProcessLeftCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, ParentEvent);
		if (Repaint && ParentZone.Built)
		{
			CheckPaintWallsAround(GO);
			CheckPaintLiquidsAround(GO);
		}
		return true;
	}

	public Cell getClosestPassableCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCell(Predicate<Cell> filter)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable() && filter(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCell(Cell alsoClosestTo)
	{
		if (alsoClosestTo == null)
		{
			return getClosestPassableCell();
		}
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => (c1.location.Distance(location) + c1.location.Distance(alsoClosestTo.location)).CompareTo(c2.location.Distance(location) + c2.location.Distance(alsoClosestTo.location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCellExcept(List<Cell> except)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable() && !except.Contains(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCellFor(GameObject who)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable(who));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCellForExcept(GameObject who, List<Cell> except)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable(who) && !except.Contains(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCell(Cell alsoClosestTo)
	{
		if (alsoClosestTo == null)
		{
			return getClosestEmptyCell();
		}
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => (c1.location.Distance(location) + c1.location.Distance(alsoClosestTo.location)).CompareTo(c2.location.Distance(location) + c2.location.Distance(alsoClosestTo.location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCellExcept(List<Cell> except)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty() && !except.Contains(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestReachableCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty() && c.IsReachable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public Cell getClosestReachableCellFor(GameObject who)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmptyFor(who) && c.IsReachable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.location.Distance(location).CompareTo(c2.location.Distance(location)));
		}
		return cells[0];
	}

	public bool IsReachable()
	{
		return ParentZone.IsReachable(X, Y);
	}

	public int DistanceToEdge()
	{
		int val = Math.Min(ParentZone.Width - X, X);
		int val2 = Math.Min(ParentZone.Height - Y, Y);
		return Math.Min(val, val2);
	}

	public int DistanceTo(GameObject GO)
	{
		if (GO == null)
		{
			return 9999999;
		}
		if (GO.pPhysics == null)
		{
			return 9999999;
		}
		return PathDistanceTo(GO.pPhysics.CurrentCell);
	}

	public int CosmeticDistanceTo(Point2D location)
	{
		return CosmeticDistanceTo(location.x, location.y);
	}

	public int CosmeticDistanceto(Location2D location)
	{
		return CosmeticDistanceTo(location.x, location.y);
	}

	public int CosmeticDistanceTo(int x, int y)
	{
		return (int)Math.Sqrt((float)(x - X) * 0.6666f * ((float)(x - X) * 0.6666f) + (float)((y - Y) * (y - Y)));
	}

	public int DistanceTo(int x, int y)
	{
		return Math.Max(Math.Abs(X - x), Math.Abs(Y - y));
	}

	public int ManhattanDistanceTo(GameObject obj)
	{
		if (obj == null)
		{
			return 9999999;
		}
		return ManhattanDistanceTo(obj.CurrentCell);
	}

	public int ManhattanDistanceTo(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.IsGraveyard())
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Abs(num3 - num) + Math.Abs(num4 - num2) + Math.Abs(zoneZ2 - zoneZ);
		}
		return Math.Abs(C.X - X) + Math.Abs(C.Y - Y);
	}

	public int PathDistanceTo(Location2D L)
	{
		if (L == null)
		{
			return 9999999;
		}
		return Math.Max(Math.Abs(L.x - X), Math.Abs(L.y - Y));
	}

	public int PathDistanceTo(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.IsGraveyard())
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Max(Math.Max(Math.Abs(num3 - num), Math.Abs(num4 - num2)), Math.Abs(zoneZ2 - zoneZ)) + 1;
		}
		return Math.Max(Math.Abs(C.X - X), Math.Abs(C.Y - Y));
	}

	public Point2D PathDifferenceTo(Cell C)
	{
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			return new Point2D(num3 - num, num4 - num2);
		}
		return new Point2D(X - C.X, Y - C.Y);
	}

	public double RealDistanceTo(GameObject GO)
	{
		if (GO == null)
		{
			return 9999999.0;
		}
		if (GO.pPhysics == null)
		{
			return 9999999.0;
		}
		return RealDistanceTo(GO.CurrentCell);
	}

	public double RealDistanceTo(Cell C, bool indefiniteWorld = true)
	{
		if (C == null)
		{
			return 9999999.0;
		}
		if (indefiniteWorld)
		{
			if (C.ParentZone.IsWorldMap())
			{
				return 9999999.0;
			}
			if (ParentZone.IsWorldMap())
			{
				return 9999999.0;
			}
		}
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			return Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4) + (zoneZ - zoneZ2) * (zoneZ - zoneZ2));
		}
		return Math.Sqrt((C.X - X) * (C.X - X) + (C.Y - Y) * (C.Y - Y));
	}

	public int DistanceToRespectStairs(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (zoneZ != zoneZ2)
			{
				if (num != num3 || num2 != num4)
				{
					return int.MaxValue;
				}
				if (Math.Abs(zoneZ2 - zoneZ) == 1)
				{
					if (zoneZ < zoneZ2)
					{
						if (!HasObjectWithPart("StairsUp"))
						{
							return int.MaxValue;
						}
					}
					else if (!HasObjectWithPart("StairsDown"))
					{
						return int.MaxValue;
					}
				}
			}
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Max(Math.Max(Math.Abs(num3 - num), Math.Abs(num4 - num2)), Math.Abs(zoneZ2 - zoneZ));
		}
		return Math.Max(Math.Abs(C.X - X), Math.Abs(C.Y - Y));
	}

	public virtual List<GameObject> GetObjectsWithRegisteredEvent(string EventName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual List<GameObject> GetObjectsWithRegisteredEvent(string EventName, Predicate<GameObject> pFilter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual GameObject GetFirstObjectWithRegisteredEvent(string EventName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual GameObject GetFirstObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual bool HasObjectWithRegisteredEvent(string EventName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool HasObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ForeachObjectWithRegisteredEvent(string EventName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				aProc(Objects[i]);
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
	}

	public virtual void GetObjectsWithTagOrProperty(List<GameObject> List, string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasTagOrProperty(Name))
			{
				List.Add(gameObject);
			}
		}
	}

	public virtual List<GameObject> GetObjectsWithTagOrProperty(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetObjectsWithTagOrProperty(list, Name);
		return list;
	}

	public virtual bool ForeachObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				if (!pProc(Objects[i]))
				{
					return false;
				}
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
		return true;
	}

	public virtual List<GameObject> GetObjectsWithProperty(string PropertyName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual void GetObjectsWithProperty(string PropertyName, List<GameObject> Return)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				Return.Add(Objects[i]);
			}
		}
	}

	public virtual int GetObjectCountWithProperty(string PropertyName)
	{
		int num = 0;
		for (int i = 0; i < Objects.Count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				num++;
			}
		}
		return num;
	}

	public virtual List<GameObject> GetObjectsWithIntProperty(string PropertyName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual List<GameObject> GetObjectsWithIntProperty(string PropertyName, Predicate<GameObject> pFilter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0 && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual int GetObjectCountWithIntProperty(string PropertyName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				num++;
			}
		}
		return num;
	}

	public virtual bool HasObjectWithIntProperty(string PropertyName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool HasObjectWithIntProperty(string PropertyName, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0 && pFilter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual GameObject GetObjectWithTagOrProperty(string TagName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTagOrProperty(TagName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual GameObject GetObjectWithTag(string TagName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual List<GameObject> GetObjectsWithTag(string TagName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual List<GameObject> GetObjectsWithTag(string TagName, Predicate<GameObject> pFilter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName) && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual void ForeachObjectWithTagOrProperty(string Name, Action<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) || Objects[i].HasProperty(Name))
			{
				aProc(Objects[i]);
			}
		}
	}

	public virtual void ForeachObjectWithTag(string TagName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				aProc(Objects[i]);
			}
		}
	}

	public virtual void ForeachObjectWithTag(string TagName, Predicate<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count && (!Objects[i].HasTag(TagName) || aProc(Objects[i])); i++)
		{
		}
	}

	public virtual int GetObjectCountWithTag(string TagName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				num++;
			}
		}
		return num;
	}

	public virtual void ForeachObjectWithPart(string PartName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName))
			{
				aProc(gameObject);
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
	}

	public virtual bool ForeachObjectWithPart(string PartName, Predicate<GameObject> pProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName))
			{
				if (!pProc(gameObject))
				{
					return false;
				}
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		switch (Objects.Count)
		{
		case 1:
			aProc(Objects[0]);
			break;
		case 2:
		{
			GameObject obj4 = Objects[0];
			GameObject obj5 = Objects[1];
			aProc(obj4);
			aProc(obj5);
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			GameObject obj3 = Objects[2];
			aProc(obj);
			aProc(obj2);
			aProc(obj3);
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				aProc(list[i]);
			}
			break;
		}
		case 0:
			break;
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc)
	{
		switch (Objects.Count)
		{
		case 1:
			if (!pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (!pProc(obj))
			{
				return false;
			}
			if (!pProc(obj2))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (!pProc(obj3))
			{
				return false;
			}
			if (!pProc(obj4))
			{
				return false;
			}
			if (!pProc(obj5))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (pFilter(Objects[0]))
			{
				aProc(Objects[0]);
			}
			return;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (pFilter(obj))
			{
				aProc(obj);
			}
			if (pFilter(obj2))
			{
				aProc(obj2);
			}
			return;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (pFilter(obj3))
			{
				aProc(obj3);
			}
			if (pFilter(obj4))
			{
				aProc(obj4);
			}
			if (pFilter(obj5))
			{
				aProc(obj5);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (pFilter(list[i]))
			{
				aProc(list[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (pFilter(Objects[0]))
			{
				pProc(Objects[0]);
				return false;
			}
			break;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (pFilter(obj) && !pProc(obj))
			{
				return false;
			}
			if (pFilter(obj2))
			{
				pProc(obj2);
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (pFilter(obj3) && !pProc(obj3))
			{
				return false;
			}
			if (pFilter(obj4) && !pProc(obj4))
			{
				return false;
			}
			if (pFilter(obj5))
			{
				pProc(obj5);
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (pFilter(list[i]) && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return false;
	}

	public void SafeForeachObject(Action<GameObject> aProc)
	{
		switch (Objects.Count)
		{
		case 1:
			aProc(Objects[0]);
			return;
		case 2:
		{
			GameObject obj2 = Objects[0];
			GameObject gameObject3 = Objects[1];
			aProc(obj2);
			if (gameObject3.IsValid())
			{
				aProc(gameObject3);
			}
			return;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject gameObject = Objects[1];
			GameObject gameObject2 = Objects[2];
			aProc(obj);
			if (gameObject.IsValid())
			{
				aProc(gameObject);
			}
			if (gameObject2.IsValid())
			{
				aProc(gameObject2);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		aProc(list[0]);
		int i = 1;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].IsValid())
			{
				aProc(list[i]);
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc)
	{
		switch (Objects.Count)
		{
		case 1:
			if (!pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj2 = Objects[0];
			GameObject gameObject3 = Objects[1];
			if (!pProc(obj2))
			{
				return false;
			}
			if (gameObject3.IsValid() && !pProc(gameObject3))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject gameObject = Objects[1];
			GameObject gameObject2 = Objects[2];
			if (!pProc(obj))
			{
				return false;
			}
			if (gameObject.IsValid() && !pProc(gameObject))
			{
				return false;
			}
			if (gameObject2.IsValid() && !pProc(gameObject2))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			if (!pProc(list[0]))
			{
				return false;
			}
			int i = 1;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].IsValid() && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (pFilter(Objects[0]))
			{
				aProc(Objects[0]);
			}
			return;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject gameObject = Objects[1];
			if (pFilter(obj))
			{
				aProc(obj);
			}
			if (gameObject.IsValid() && pFilter(gameObject))
			{
				aProc(gameObject);
			}
			return;
		}
		case 3:
		{
			GameObject obj2 = Objects[0];
			GameObject gameObject2 = Objects[1];
			GameObject gameObject3 = Objects[2];
			if (pFilter(obj2))
			{
				aProc(obj2);
			}
			if (gameObject2.IsValid() && pFilter(gameObject2))
			{
				aProc(gameObject2);
			}
			if (gameObject3.IsValid() && pFilter(gameObject3))
			{
				aProc(gameObject3);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		aProc(list[0]);
		int i = 1;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].IsValid() && pFilter(list[i]))
			{
				aProc(list[i]);
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (pFilter(Objects[0]) && !pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj2 = Objects[0];
			GameObject gameObject3 = Objects[1];
			if (pFilter(obj2) && !pProc(obj2))
			{
				return false;
			}
			if (gameObject3.IsValid() && pFilter(gameObject3) && !pProc(gameObject3))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject gameObject = Objects[1];
			GameObject gameObject2 = Objects[2];
			if (pFilter(obj) && !pProc(obj))
			{
				return false;
			}
			if (gameObject.IsValid() && pFilter(gameObject) && !pProc(gameObject))
			{
				return false;
			}
			if (gameObject2.IsValid() && pFilter(gameObject2) && !pProc(gameObject2))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			if (pFilter(list[0]) && !pProc(list[0]))
			{
				return false;
			}
			int i = 1;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].IsValid() && pFilter(list[i]) && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public virtual List<GameObject> GetObjectsViaEventList()
	{
		return Event.NewGameObjectList(Objects);
	}

	public virtual List<GameObject> GetObjects()
	{
		return new List<GameObject>(Objects);
	}

	public virtual List<GameObject> GetObjects(string Blueprint)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			int j = 0;
			for (int count2 = Objects.Count; j < count2; j++)
			{
				if (Objects[j].Blueprint == Blueprint)
				{
					list.Add(Objects[j]);
					if (list.Count >= num)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public virtual void GetObjects(List<GameObject> List, string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				List.Add(Objects[i]);
			}
		}
	}

	public virtual List<GameObject> GetObjects(Predicate<GameObject> pFilter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			int j = 0;
			for (int count2 = Objects.Count; j < count2; j++)
			{
				if (pFilter(Objects[j]))
				{
					list.Add(Objects[j]);
					if (list.Count >= num)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public virtual List<GameObject> GetObjectsThatInheritFrom(string Blueprint)
	{
		List<GameObject> list = new List<GameObject>();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetBlueprint().InheritsFrom(Blueprint))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual GameObject GetFirstObjectThatInheritsFrom(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetBlueprint().InheritsFrom(Blueprint))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual List<GameObject> GetObjectsWithPartReadonly(string PartName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual List<GameObject> GetObjectsWithPart(string PartName)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual void GetObjectsWithPart(string PartName, List<GameObject> Return)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				Return.Add(Objects[i]);
			}
		}
	}

	public virtual IEnumerable<GameObject> LoopObjects()
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			yield return Objects[i];
		}
	}

	public virtual IEnumerable<GameObject> LoopObjectsWithPart(string PartName)
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				yield return Objects[i];
			}
		}
	}

	public virtual List<GameObject> GetObjectsWithPart(List<string> PartNames)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartNames));
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					list.Add(Objects[i]);
					break;
				}
			}
		}
		return list;
	}

	public virtual IEnumerable<GameObject> LoopObjectsWithPart(List<string> PartNames)
	{
		int k = PartNames.Count;
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			for (int l = 0; l < k; l++)
			{
				if (Objects[i].HasPart(PartNames[l]))
				{
					yield return Objects[i];
					break;
				}
			}
		}
	}

	public virtual void GetObjectsWithPart(List<string> PartNames, List<GameObject> Return)
	{
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					Return.Add(Objects[i]);
					break;
				}
			}
		}
	}

	public virtual List<GameObject> GetObjectsWithPart(string PartName, Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName, pFilter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName) && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public virtual IEnumerable<GameObject> LoopObjectsWithPart(string PartName, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			if (Objects[i].HasPart(PartName) && pFilter(Objects[i]))
			{
				yield return Objects[i];
			}
		}
	}

	public virtual List<GameObject> GetObjectsWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName, Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, CheckFlight, CheckAttackable, CheckSolidity));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public virtual List<GameObject> GetRealObjects(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		List<GameObject> list = new List<GameObject>(GetRealObjectCount(Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, CheckFlight, CheckAttackable, CheckSolidity));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics != null && gameObject.pPhysics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public virtual void GetObjectsWithPart(string PartName, List<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public virtual void GetRealObjects(List<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics != null && gameObject.pPhysics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public virtual int GetObjectCountWithPart(string PartName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				num++;
			}
		}
		return num;
	}

	public virtual int GetObjectCountWithPart(List<string> PartNames)
	{
		int num = 0;
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	public virtual int GetObjectCountWithPart(string PartName, Predicate<GameObject> pFilter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName) && pFilter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public virtual int GetObjectCountWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				num++;
			}
		}
		return num;
	}

	public virtual int GetObjectCountAndFirstObjectWithPart(out GameObject FirstObject, string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public virtual int GetRealObjectCount(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics pPhysics = gameObject.pPhysics;
			if (pPhysics != null && pPhysics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				num++;
			}
		}
		return num;
	}

	public virtual int GetRealObjectCountAndFirstObject(out GameObject FirstObject, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics pPhysics = gameObject.pPhysics;
			if (pPhysics != null && pPhysics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidFor(Projectile, AttackPOV)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public virtual bool IsEmptyExcludingCombat()
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null && !gameObject.HasPart("Combat") && !gameObject.HasPart("Brain") && gameObject.pRender.RenderLayer > 5 && (!gameObject.HasPart("Door") || (gameObject.GetPart("Door") as Door).bLocked) && !gameObject.HasPart("StairsDown") && !gameObject.HasPart("StairsUp"))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyAtRenderLayer(int Layer)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < Objects.Count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null && gameObject.pRender.Visible && gameObject.pRender.RenderLayer >= Layer)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsOpenForPlacement()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null)
			{
				if (gameObject.pPhysics.Solid)
				{
					return false;
				}
				if (gameObject.HasPart("StairsDown"))
				{
					return false;
				}
				if (gameObject.HasPart("StairsUp"))
				{
					return false;
				}
				if (gameObject.HasPart("Combat"))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual bool IsEmptyForPopulation()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics.Solid)
			{
				return false;
			}
			if (gameObject.IsCombatObject())
			{
				return false;
			}
			if (gameObject.HasTag("Furniture"))
			{
				return false;
			}
			if (gameObject.HasTag("Door"))
			{
				return false;
			}
			if (gameObject.HasTag("Wall"))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsSpawnable()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics.Solid)
			{
				return false;
			}
			if (gameObject.IsCombatObject())
			{
				return false;
			}
			if (gameObject.IsSwimmingDepthLiquid())
			{
				return false;
			}
			if (gameObject.GetPart("StairsDown") is StairsDown stairsDown && stairsDown.PullDown)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsCorner()
	{
		if (X == 0 || X == 79 || Y == 0 || Y == 24)
		{
			return false;
		}
		if (GetCellFromDirection("N").HasObjectWithTag("Wall") && GetCellFromDirection("NE").HasObjectWithTag("Wall") && GetCellFromDirection("E").HasObjectWithTag("Wall"))
		{
			return true;
		}
		if (GetCellFromDirection("E").HasObjectWithTag("Wall") && GetCellFromDirection("SE").HasObjectWithTag("Wall") && GetCellFromDirection("S").HasObjectWithTag("Wall"))
		{
			return true;
		}
		if (GetCellFromDirection("S").HasObjectWithTag("Wall") && GetCellFromDirection("SW").HasObjectWithTag("Wall") && GetCellFromDirection("W").HasObjectWithTag("Wall"))
		{
			return true;
		}
		if (GetCellFromDirection("W").HasObjectWithTag("Wall") && GetCellFromDirection("NW").HasObjectWithTag("Wall") && GetCellFromDirection("N").HasObjectWithTag("Wall"))
		{
			return true;
		}
		return false;
	}

	public virtual bool IsEmpty()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null && gameObject.pRender.RenderLayer > 5 && (!(gameObject.GetPart("Door") is Door door) || door.bLocked) && !gameObject.HasPart("StairsDown") && !gameObject.HasPart("StairsUp"))
			{
				return false;
			}
			if (gameObject.HasPart("Combat"))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsEmptyFor(GameObject who)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null && gameObject.pRender.RenderLayer > 5 && (!(gameObject.GetPart("Door") is Door door) || door.bLocked) && !gameObject.HasPart("StairsDown") && !gameObject.HasPart("StairsUp") && gameObject.PhaseMatches(who))
			{
				return false;
			}
			if (gameObject.HasPart("Combat") && gameObject.FlightMatches(who))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsEmptyIgnoring(Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (!pFilter(gameObject))
			{
				if (gameObject.pRender != null && gameObject.pRender.RenderLayer > 5 && (!(gameObject.GetPart("Door") is Door door) || door.bLocked) && !gameObject.HasPart("StairsDown") && !gameObject.HasPart("StairsUp"))
				{
					return false;
				}
				if (gameObject.HasPart("Combat"))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual bool ClearImpassableObjects(GameObject obj, bool includeCombatObjects = false)
	{
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics != null && ((obj == null) ? gameObject.ConsiderSolid() : gameObject.ConsiderSolidFor(obj)) && (!(gameObject.GetPart("Door") is Door door) || door.bLocked))
			{
				StairsDown stairsDown = gameObject.GetPart("StairsDown") as StairsDown;
				if (stairsDown != null && stairsDown.PullDown && (obj == null || stairsDown.IsValidForPullDown(obj)))
				{
					if (list == null)
					{
						list = Event.NewGameObjectList();
					}
					list.Add(Objects[i]);
					continue;
				}
				if (stairsDown == null && !gameObject.HasPart("StairsUp"))
				{
					if (obj == null)
					{
						if (list == null)
						{
							list = Event.NewGameObjectList();
						}
						list.Add(Objects[i]);
						continue;
					}
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", obj);
					bool num = gameObject.FireEvent(eBeforePhysicsRejectObjectEntringCell);
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", null);
					if (num)
					{
						if (list == null)
						{
							list = Event.NewGameObjectList();
						}
						list.Add(Objects[i]);
						continue;
					}
				}
			}
			if (includeCombatObjects && gameObject.IsCombatObject() && gameObject != obj)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
		}
		list?.ForEach(delegate(GameObject o)
		{
			Objects.Remove(o);
		});
		return true;
	}

	public virtual bool IsPassable(GameObject obj = null, bool includeCombatObjects = true)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pPhysics != null && ((obj == null) ? gameObject.ConsiderSolid() : gameObject.ConsiderSolidFor(obj)) && (!(gameObject.GetPart("Door") is Door door) || door.bLocked))
			{
				StairsDown stairsDown = gameObject.GetPart("StairsDown") as StairsDown;
				if (stairsDown != null && stairsDown.PullDown && (obj == null || stairsDown.IsValidForPullDown(obj)))
				{
					return false;
				}
				if (stairsDown == null && !gameObject.HasPart("StairsUp"))
				{
					if (obj == null)
					{
						return false;
					}
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", obj);
					bool num = gameObject.FireEvent(eBeforePhysicsRejectObjectEntringCell);
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", null);
					if (num)
					{
						return false;
					}
				}
			}
			if (includeCombatObjects && gameObject.IsCombatObject() && gameObject != obj)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsEmptyOfSolid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.pRender != null && gameObject.pRender.RenderLayer > 5 && (!gameObject.HasPart("Door") || gameObject.GetPart<Door>().bLocked) && !gameObject.HasPart("StairsDown") && !gameObject.HasPart("StairsUp") && gameObject.pPhysics != null && gameObject.pPhysics.Solid)
			{
				return false;
			}
			if (gameObject.HasPart("Combat"))
			{
				return false;
			}
		}
		return true;
	}

	public void ClearOccludeCache()
	{
		OccludeCache = -1;
	}

	public bool IsEdge()
	{
		if (X != 0 && X != ParentZone.Width - 1 && Y != 0)
		{
			return Y == ParentZone.Height - 1;
		}
		return true;
	}

	public virtual bool HasExternalWall(Predicate<Cell> test = null)
	{
		if (HasWall())
		{
			string[] directionList = Directions.DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirection = GetCellFromDirection(direction);
				if (cellFromDirection != null && !cellFromDirection.HasWall() && (test == null || test(cellFromDirection)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasWallInDirection(string dir)
	{
		return GetCellFromDirection(dir)?.HasWall() ?? true;
	}

	public virtual bool HasWall()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty("Wall") > 0)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsOccluding()
	{
		if (OccludeCache != -1)
		{
			return OccludeCache == 1;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i]?.pRender != null && Objects[i].pRender.Occluding)
			{
				OccludeCache = 1;
				return true;
			}
		}
		OccludeCache = 0;
		return false;
	}

	public virtual bool IsOccludingFor(GameObject What)
	{
		if (OccludeCache == 0)
		{
			return false;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i]?.pRender != null && Objects[i].pRender.Occluding && Objects[i].PhaseAndFlightMatches(What))
			{
				OccludeCache = 1;
				return true;
			}
		}
		return false;
	}

	public virtual bool IsOccludingOtherThan(GameObject skip)
	{
		if (OccludeCache == 0)
		{
			return false;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i]?.pRender != null && Objects[i].pRender.Occluding)
			{
				OccludeCache = 1;
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid())
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolid(bool ForFluid)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid(ForFluid))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolid(bool ForFluid, int Phase)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid(ForFluid, Phase))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolidOtherThan(GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].ConsiderSolid())
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolidFor(GameObject Attacker)
	{
		if (Attacker == null)
		{
			return IsSolid();
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(Attacker))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsSolidFor(GameObject Projectile, GameObject Attacker)
	{
		if (Projectile == null)
		{
			return IsSolidFor(Attacker);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(Projectile, Attacker))
			{
				return true;
			}
		}
		return false;
	}

	public void FindSolidObjectForMissile(GameObject Attacker, GameObject Projectile, out GameObject SolidObject, out bool IsSolid)
	{
		SolidObject = null;
		IsSolid = false;
		TreatAsSolid treatAsSolid = Projectile?.GetPart("TreatAsSolid") as TreatAsSolid;
		bool flag = Projectile?.GetPart("Projectile") is Projectile projectile && projectile.PenetrateWalls;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(Projectile, Attacker) && (!flag || !Objects[i].IsWall()))
			{
				IsSolid = true;
				SolidObject = Objects[i];
				break;
			}
			if (treatAsSolid != null && treatAsSolid.Match(Objects[i]) && Objects[i].PhaseMatches(Projectile ?? Attacker))
			{
				IsSolid = true;
				if (treatAsSolid.Hits)
				{
					SolidObject = Objects[i];
				}
				break;
			}
		}
	}

	public bool HasSolidObjectForMissile(GameObject Attacker, GameObject Projectile = null)
	{
		FindSolidObjectForMissile(Attacker, Projectile, out var _, out var IsSolid);
		return IsSolid;
	}

	public GameObject FindSolidObjectForMissile(GameObject Attacker, GameObject Projectile = null)
	{
		FindSolidObjectForMissile(Attacker, Projectile, out var SolidObject, out var _);
		return SolidObject;
	}

	public bool BroadcastEvent(Event E)
	{
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].BroadcastEvent(E);
		case 2:
		{
			GameObject gameObject = Objects[1];
			if (!Objects[0].BroadcastEvent(E))
			{
				return false;
			}
			if (!gameObject.BroadcastEvent(E))
			{
				return false;
			}
			return true;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!list[i].BroadcastEvent(E))
				{
					return false;
				}
			}
			return true;
		}
		}
	}

	public bool BroadcastEvent(Event E, IEvent PE)
	{
		bool result = BroadcastEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool BroadcastEvent(Event E, Event PE)
	{
		bool result = BroadcastEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEvent(string ID)
	{
		if (ID.IndexOf(',') != -1)
		{
			bool result = true;
			{
				foreach (string item in ID.CachedCommaExpansion())
				{
					if (!FireEvent(item))
					{
						result = false;
					}
				}
				return result;
			}
		}
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].FireEvent(ID);
		case 2:
		{
			GameObject gameObject4 = Objects[0];
			GameObject gameObject5 = Objects[1];
			if (!gameObject4.FireEvent(ID))
			{
				return false;
			}
			if (gameObject5 != null && gameObject5.IsValid() && !gameObject5.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		case 3:
		{
			GameObject gameObject = Objects[0];
			GameObject gameObject2 = Objects[1];
			GameObject gameObject3 = Objects[2];
			if (!gameObject.FireEvent(ID))
			{
				return false;
			}
			if (gameObject2 != null && gameObject2.IsValid() && !gameObject2.FireEvent(ID))
			{
				return false;
			}
			if (gameObject3 != null && gameObject3.IsValid() && !gameObject3.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		case 4:
		{
			GameObject gameObject6 = Objects[0];
			GameObject gameObject7 = Objects[1];
			GameObject gameObject8 = Objects[2];
			GameObject gameObject9 = Objects[3];
			if (!gameObject6.FireEvent(ID))
			{
				return false;
			}
			if (gameObject7 != null && gameObject7.IsValid() && !gameObject7.FireEvent(ID))
			{
				return false;
			}
			if (gameObject8 != null && gameObject8.IsValid() && !gameObject8.FireEvent(ID))
			{
				return false;
			}
			if (gameObject9 != null && gameObject9.IsValid() && !gameObject9.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		default:
			if (EventListInUse)
			{
				List<GameObject> list = new List<GameObject>(Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i].IsValid() && !list[i].FireEvent(ID))
					{
						return false;
					}
				}
			}
			else
			{
				EventListInUse = true;
				try
				{
					EventList.Clear();
					EventList.AddRange(Objects);
					int j = 0;
					for (int count2 = EventList.Count; j < count2; j++)
					{
						if (EventList[j].IsValid() && !EventList[j].FireEvent(ID))
						{
							return false;
						}
					}
				}
				finally
				{
					EventList.Clear();
					EventListInUse = false;
				}
			}
			return true;
		}
	}

	public bool FireEvent(Event E)
	{
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].FireEvent(E);
		case 2:
		{
			GameObject gameObject4 = Objects[0];
			GameObject gameObject5 = Objects[1];
			if (!gameObject4.FireEvent(E))
			{
				return false;
			}
			if (gameObject5 != null && gameObject5.IsValid() && !gameObject5.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		case 3:
		{
			GameObject gameObject = Objects[0];
			GameObject gameObject2 = Objects[1];
			GameObject gameObject3 = Objects[2];
			if (!gameObject.FireEvent(E))
			{
				return false;
			}
			if (gameObject2 != null && gameObject2.IsValid() && !gameObject2.FireEvent(E))
			{
				return false;
			}
			if (gameObject3 != null && gameObject3.IsValid() && !gameObject3.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		case 4:
		{
			GameObject gameObject6 = Objects[0];
			GameObject gameObject7 = Objects[1];
			GameObject gameObject8 = Objects[2];
			GameObject gameObject9 = Objects[3];
			if (!gameObject6.FireEvent(E))
			{
				return false;
			}
			if (gameObject7 != null && gameObject7.IsValid() && !gameObject7.FireEvent(E))
			{
				return false;
			}
			if (gameObject8 != null && gameObject8.IsValid() && !gameObject8.FireEvent(E))
			{
				return false;
			}
			if (gameObject9 != null && gameObject9.IsValid() && !gameObject9.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		default:
			if (EventListInUse)
			{
				List<GameObject> list = new List<GameObject>(Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i].IsValid() && !list[i].FireEvent(E))
					{
						return false;
					}
				}
			}
			else
			{
				EventListInUse = true;
				try
				{
					EventList.Clear();
					EventList.AddRange(Objects);
					int j = 0;
					for (int count2 = EventList.Count; j < count2; j++)
					{
						if (EventList[j].IsValid() && !EventList[j].FireEvent(E))
						{
							return false;
						}
					}
				}
				finally
				{
					EventList.Clear();
					EventListInUse = false;
				}
			}
			return true;
		}
	}

	public bool FireEvent(Event E, IEvent PE)
	{
		bool result = FireEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEvent(Event E, Event PE)
	{
		bool result = FireEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEventDirect(Event E)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			if (!Objects[i].FireEvent(E))
			{
				return false;
			}
		}
		return true;
	}

	public void QuickGetAdjacentCells(List<Cell> Return, bool bLocalOnly)
	{
		Return.Add(this);
		for (int i = 0; i < DirectionList.Length; i++)
		{
			string direction = DirectionList[i];
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				Return.Add(localCellFromDirection);
			}
		}
	}

	public IEnumerable<Cell> YieldAdjacentCells(int Radius, bool LocalOnly = false, bool BuiltOnly = true)
	{
		Radius = Radius * 2 + 1;
		int x = X;
		int y = Y;
		int x2 = 1;
		int y2 = 0;
		int j = 1;
		int i = 0;
		int p = 1;
		int c = Radius * Radius - 1;
		while (i < c)
		{
			x += x2;
			y += y2;
			if (p >= j)
			{
				p = 0;
				int num = x2;
				x2 = -y2;
				y2 = num;
				if (y2 == 0)
				{
					j++;
				}
			}
			Cell cellGlobal = ParentZone.GetCellGlobal(x, y, LocalOnly, BuiltOnly);
			if (cellGlobal != null)
			{
				yield return cellGlobal;
			}
			i++;
			p++;
		}
	}

	public void GetAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true, bool BuiltOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		Return.Add(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly, BuiltOnly);
				if (cellFromDirectionGlobal != null && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetAdjacentCells(int Radius, bool LocalOnly = true, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2) + 1);
		GetAdjacentCells(Radius, list, LocalOnly, BuiltOnly);
		return list;
	}

	public void GetRealAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		Return.Add(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.RealDistanceTo(this) <= (double)Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetRealAdjacentCells(int Radius, bool LocalOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		GetRealAdjacentCells(Radius, list, LocalOnly);
		return list;
	}

	public bool IsAdjacentTo(Cell C, bool BuiltOnly = true)
	{
		if (C == null)
		{
			return false;
		}
		return GetCellFromDirectionOfCell(C, BuiltOnly) == C;
	}

	public Cell GetCellOrFirstConnectedSpawnLocation(bool bLocalOnly = true)
	{
		if (IsEmpty())
		{
			return this;
		}
		return GetConnectedSpawnLocation(bLocalOnly);
	}

	public Cell GetConnectedSpawnLocation(bool bLocalOnly = true)
	{
		List<Cell> list = new List<Cell>();
		GetConnectedSpawnLocations(1, list, bLocalOnly);
		if (list.Count <= 0)
		{
			return GetFirstEmptyAdjacentCell();
		}
		return list[0];
	}

	public List<Cell> GetConnectedSpawnLocations(int HowMany)
	{
		List<Cell> list = new List<Cell>();
		GetConnectedSpawnLocations(HowMany, list);
		return list;
	}

	public void GetConnectedSpawnLocations(int HowMany, List<Cell> Return, bool bLocalOnly = true)
	{
		List<Cell> list = new List<Cell>();
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0 && Return.Count < HowMany)
		{
			Cell cell = cleanQueue.Dequeue();
			list.Add(cell);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, bLocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !list.Contains(cellFromDirectionGlobal))
				{
					if (cellFromDirectionGlobal.IsPassable())
					{
						Return.Add(cellFromDirectionGlobal);
					}
					if (cellFromDirectionGlobal.IsPassable(null, includeCombatObjects: false))
					{
						cleanQueue.Enqueue(cellFromDirectionGlobal);
					}
				}
			}
		}
	}

	public void GetPassableConnectedAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsPassable(Object, IncludeCombatObjects) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public Cell GetFirstPassableConnectedAdjacentCell(int Radius, bool LocalOnly = true, Predicate<Cell> Filter = null, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		List<string> list = new List<string>(DirectionList);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			list.ShuffleInPlace();
			foreach (string item in list)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(item, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && cellFromDirectionGlobal.IsPassable(Object, IncludeCombatObjects) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius && (Filter == null || Filter(cellFromDirectionGlobal)))
				{
					return cellFromDirectionGlobal;
				}
			}
		}
		return null;
	}

	public List<Cell> GetPassableConnectedAdjacentCells(int Radius, bool LocalOnly = true, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetPassableConnectedAdjacentCells(Radius, list, LocalOnly, Object, IncludeCombatObjects);
		return list;
	}

	public void GetEmptyConnectedAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsEmpty() && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public Cell GetFirstEmptyConnectedAdjacentCell(int Radius, bool LocalOnly = true, Predicate<Cell> Filter = null)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		List<string> list = new List<string>(DirectionList);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			list.ShuffleInPlace();
			foreach (string item in list)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(item, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && cellFromDirectionGlobal.IsEmpty() && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius && (Filter == null || Filter(cellFromDirectionGlobal)))
				{
					return cellFromDirectionGlobal;
				}
			}
		}
		return null;
	}

	public List<Cell> GetEmptyConnectedAdjacentCells(int Radius, bool LocalOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetEmptyConnectedAdjacentCells(Radius, list, LocalOnly);
		return list;
	}

	public void GetEmptyConnectedAdjacentCellsIgnoring(int Radius, List<Cell> Return, Predicate<GameObject> Filter, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsEmptyIgnoring(Filter) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetEmptyConnectedAdjacentCellsIgnoring(int Radius, Predicate<GameObject> Filter, bool LocalOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetEmptyConnectedAdjacentCellsIgnoring(Radius, list, Filter, LocalOnly);
		return list;
	}

	public void GetPassableConnectedAdjacentCellsFor(List<Cell> Return, GameObject For, int Radius, Predicate<Cell> Filter = null, bool LocalOnly = true, bool ExcludeWalls = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsPassable(For) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					if (Filter == null || Filter(cellFromDirectionGlobal))
					{
						Return.Add(cellFromDirectionGlobal);
					}
				}
			}
		}
	}

	public List<Cell> GetPassableConnectedAdjacentCellsFor(GameObject For, int Radius, Predicate<Cell> Filter = null, bool LocalOnly = true, bool ExcludeWalls = true)
	{
		List<Cell> list = new List<Cell>();
		GetPassableConnectedAdjacentCellsFor(list, For, Radius, Filter, LocalOnly, ExcludeWalls);
		return list;
	}

	public List<Cell> GetCardinalAdjacentCells(bool bLocalOnly = false, bool BuiltOnly = true, bool IncludeThis = false)
	{
		List<Cell> list = new List<Cell>(DirectionListCardinalOnly.Length);
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cell = (bLocalOnly ? GetLocalCellFromDirection(direction) : GetCellFromDirection(direction));
			if (cell != null)
			{
				list.Add(cell);
			}
		}
		if (IncludeThis)
		{
			list.Add(this);
		}
		return list;
	}

	public void ForeachCardinalAdjacentCell(Action<Cell> aProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null)
			{
				aProc(cellFromDirection);
			}
		}
	}

	public bool ForeachCardinalAdjacentCell(Predicate<Cell> pProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && !pProc(cellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public void ForeachCardinalAdjacentLocalCell(Action<Cell> aProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachCardinalAdjacentLocalCell(Predicate<Cell> pProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasLOSTo(GameObject go)
	{
		if (ParentZone == null)
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		Cell currentCell = go.GetCurrentCell();
		if (currentCell == null)
		{
			return false;
		}
		return ParentZone.CalculateLOS(X, Y, currentCell.X, currentCell.Y);
	}

	public bool HasAdjacentAquaticSupportFor(GameObject obj)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.HasAquaticSupportFor(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentNonAquaticSupportFor(GameObject obj)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !localCellFromDirection.HasAquaticSupportFor(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentLocalNonwallCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !localCellFromDirection.HasWall())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentLocalWallCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.HasWall())
			{
				return true;
			}
		}
		return false;
	}

	public List<Cell> GetEmptyAdjacentCells(int MinRadius, int MaxRadius)
	{
		List<Cell> list = new List<Cell>();
		for (int i = X - MaxRadius; i <= X + MaxRadius; i++)
		{
			for (int j = Y - MaxRadius; j <= Y + MaxRadius; j++)
			{
				Cell cell = ParentZone.GetCell(i, j);
				if (cell != null && PathDistanceTo(cell) >= MinRadius && PathDistanceTo(cell) <= MaxRadius && cell.IsEmpty())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyAdjacentCells()
	{
		List<Cell> list = new List<Cell>();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && cellFromDirection.IsEmpty())
			{
				list.Add(cellFromDirection);
			}
		}
		return list;
	}

	public Cell GetFirstEmptyAdjacentCell(int MinRadius, int MaxRadius)
	{
		for (int i = X - MaxRadius; i <= X + MaxRadius; i++)
		{
			for (int j = Y - MaxRadius; j <= Y + MaxRadius; j++)
			{
				Cell cell = ParentZone.GetCell(i, j);
				if (cell != null && PathDistanceTo(cell) >= MinRadius && PathDistanceTo(cell) <= MaxRadius && cell.IsEmpty())
				{
					return cell;
				}
			}
		}
		return null;
	}

	public Cell GetFirstEmptyAdjacentCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && cellFromDirection.IsEmpty())
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	public List<Cell> GetLocalEmptyAdjacentCells()
	{
		List<Cell> list = new List<Cell>();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.IsEmpty())
			{
				list.Add(localCellFromDirection);
			}
		}
		return list;
	}

	public List<Cell> GetNavigableAdjacentCells(GameObject who, int MaxWeight = 5, bool builtOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		if (ParentZone != null)
		{
			ParentZone.CalculateNavigationMap(who);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirection = GetCellFromDirection(direction, builtOnly);
				if (cellFromDirection != null && ParentZone.NavigationMap[cellFromDirection.X, cellFromDirection.Y].Weight <= MaxWeight)
				{
					list.Add(cellFromDirection);
				}
			}
		}
		return list;
	}

	public List<Cell> GetLocalNavigableAdjacentCells(GameObject who, int MaxWeight = 5)
	{
		List<Cell> list = Event.NewCellList();
		if (ParentZone != null)
		{
			ParentZone.CalculateNavigationMap(who);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null && ParentZone.NavigationMap[localCellFromDirection.X, localCellFromDirection.Y].Weight <= MaxWeight)
				{
					list.Add(localCellFromDirection);
				}
			}
		}
		return list;
	}

	public List<Cell> GetAdjacentCells(bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>(8);
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null)
			{
				list.Add(cellFromDirection);
			}
		}
		return list;
	}

	public bool AnyAdjacentCell(Predicate<Cell> pFilter, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && pFilter(cellFromDirection))
			{
				return true;
			}
		}
		return false;
	}

	public Cell GetFirstAdjacentCell(Predicate<Cell> pFilter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && pFilter(cellFromDirection))
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	public void ForeachAdjacentCell(Action<Cell> aProc, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null)
			{
				aProc(cellFromDirection);
			}
		}
	}

	public bool ForeachAdjacentCell(Predicate<Cell> pProc, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !pProc(cellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public Cell GetRandomLocalAdjacentCell()
	{
		List<Cell> localAdjacentCells = GetLocalAdjacentCells(1);
		if (localAdjacentCells == null || localAdjacentCells.Count == 0)
		{
			return this;
		}
		return localAdjacentCells.GetRandomElement();
	}

	public Cell GetRandomLocalAdjacentCell(Predicate<Cell> filter)
	{
		List<Cell> localAdjacentCells = GetLocalAdjacentCells(1);
		if (localAdjacentCells == null || localAdjacentCells.Count == 0)
		{
			return this;
		}
		return localAdjacentCells.GetRandomElement(filter) ?? this;
	}

	public bool HasUnexploredAdjacentCell(bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !cellFromDirection.IsExplored())
			{
				return true;
			}
		}
		return false;
	}

	public Cell GetFirstNonOccludingAdjacentCell(bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !cellFromDirection.IsOccluding())
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	public List<Cell> GetLocalAdjacentCells(int Radius, bool includeSelf = false)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		if (includeSelf)
		{
			list.Add(this);
		}
		for (int i = Math.Max(X - Radius, 0); i <= X + Radius && i <= ParentZone.Width - 1; i++)
		{
			for (int j = Math.Max(Y - Radius, 0); j <= Y + Radius && j <= ParentZone.Height - 1; j++)
			{
				Cell cell = ParentZone.GetCell(i, j);
				if (cell != this)
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetLocalAdjacentCellsCircular(int Radius)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		for (int i = Math.Max(X - Radius, 0); i <= X + Radius && i <= ParentZone.Width - 1; i++)
		{
			for (int j = Math.Max(Y - Radius, 0); j <= Y + Radius && j <= ParentZone.Height - 1; j++)
			{
				Cell cell = ParentZone.GetCell(i, j);
				if (cell != this && cell.location.Distance(location) <= Radius)
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetLocalAdjacentCells()
	{
		if (_LocalAdjacentCellCache == null)
		{
			_LocalAdjacentCellCache = new List<Cell>();
			for (int i = 0; i < DirectionList.Length; i++)
			{
				string direction = DirectionList[i];
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null)
				{
					_LocalAdjacentCellCache.Add(localCellFromDirection);
				}
			}
		}
		return _LocalAdjacentCellCache;
	}

	public void ForeachLocalAdjacentCell(Action<Cell> aProc)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachLocalAdjacentCell(Predicate<Cell> pProc)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public void ForeachLocalAdjacentCellAndSelf(Action<Cell> aProc)
	{
		aProc(this);
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachLocalAdjacentCellAndSelf(Predicate<Cell> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public Cell FindLocalAdjacentCell(Predicate<Cell> pFilter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && pFilter(localCellFromDirection))
			{
				return localCellFromDirection;
			}
		}
		return null;
	}

	public bool AnyLocalAdjacentCell(Predicate<Cell> pFilter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && pFilter(localCellFromDirection))
			{
				return true;
			}
		}
		return false;
	}

	public List<Cell> GetLocalCardinalAdjacentCells()
	{
		if (_LocalCardinalAdjacentCellCache == null)
		{
			_LocalCardinalAdjacentCellCache = new List<Cell>(DirectionListCardinalOnly.Length);
			for (int i = 0; i < DirectionListCardinalOnly.Length; i++)
			{
				string direction = DirectionListCardinalOnly[i];
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null)
				{
					_LocalCardinalAdjacentCellCache.Add(localCellFromDirection);
				}
			}
		}
		return _LocalCardinalAdjacentCellCache;
	}

	public Cell GetRandomLocalCardinalAdjacentCell()
	{
		List<Cell> localCardinalAdjacentCells = GetLocalCardinalAdjacentCells();
		if (localCardinalAdjacentCells == null || localCardinalAdjacentCells.Count == 0)
		{
			return this;
		}
		return localCardinalAdjacentCells.GetRandomElement();
	}

	public Cell GetRandomLocalCardinalAdjacentCell(Predicate<Cell> filter)
	{
		List<Cell> localAdjacentCells = GetLocalAdjacentCells();
		if (localAdjacentCells == null || localAdjacentCells.Count == 0)
		{
			return this;
		}
		return localAdjacentCells.GetRandomElement(filter) ?? this;
	}

	public string GetDirectionFrom(Location2D Target)
	{
		if (Target == null || (Target.x == X && Target.y == Y))
		{
			return ".";
		}
		int x = X;
		int y = Y;
		int x2 = Target.x;
		int y2 = Target.y;
		if (x == x2)
		{
			if (y == y2)
			{
				return ".";
			}
			if (y < y2)
			{
				return "S";
			}
			return "N";
		}
		if (x < x2)
		{
			if (y == y2)
			{
				return "E";
			}
			if (y < y2)
			{
				return "SE";
			}
			return "NE";
		}
		if (y == y2)
		{
			return "W";
		}
		if (y < y2)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetGeneralDirectionFrom(Location2D Target)
	{
		if (Target == null)
		{
			return ".";
		}
		int x = X;
		int y = Y;
		int x2 = Target.x;
		int y2 = Target.y;
		bool num = x == x2 || Math.Abs(x - x2) < Math.Abs(y - y2) / 2;
		bool flag = y == y2 || Math.Abs(y - y2) < Math.Abs(x - x2) / 2;
		if (num)
		{
			if (flag)
			{
				return ".";
			}
			if (y < y2)
			{
				return "S";
			}
			return "N";
		}
		if (x < x2)
		{
			if (flag)
			{
				return "E";
			}
			if (y < y2)
			{
				return "SE";
			}
			return "NE";
		}
		if (flag)
		{
			return "W";
		}
		if (y < y2)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetDirectionFromCell(Cell Target)
	{
		if (Target == null || Target == this)
		{
			return ".";
		}
		Zone parentZone = Target.ParentZone;
		int num;
		int num2;
		int num3;
		int num4;
		if (parentZone != ParentZone)
		{
			num = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num2 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num3 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num4 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num = X;
			num2 = Y;
			num3 = Target.X;
			num4 = Target.Y;
		}
		if (num == num3)
		{
			if (num2 == num4)
			{
				return ".";
			}
			if (num2 < num4)
			{
				return "S";
			}
			return "N";
		}
		if (num < num3)
		{
			if (num2 == num4)
			{
				return "E";
			}
			if (num2 < num4)
			{
				return "SE";
			}
			return "NE";
		}
		if (num2 == num4)
		{
			return "W";
		}
		if (num2 < num4)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetGeneralDirectionFromCell(Cell Target)
	{
		if (Target == null)
		{
			return ".";
		}
		Zone parentZone = Target.ParentZone;
		int num;
		int num2;
		int num3;
		int num4;
		if (parentZone != ParentZone)
		{
			num = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num2 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num3 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num4 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num = X;
			num2 = Y;
			num3 = Target.X;
			num4 = Target.Y;
		}
		bool num5 = num == num3 || Math.Abs(num - num3) < Math.Abs(num2 - num4) / 2;
		bool flag = num2 == num4 || Math.Abs(num2 - num4) < Math.Abs(num - num3) / 2;
		if (num5)
		{
			if (flag)
			{
				return ".";
			}
			if (num2 < num4)
			{
				return "S";
			}
			return "N";
		}
		if (num < num3)
		{
			if (flag)
			{
				return "E";
			}
			if (num2 < num4)
			{
				return "SE";
			}
			return "NE";
		}
		if (flag)
		{
			return "W";
		}
		if (num2 < num4)
		{
			return "SW";
		}
		return "NW";
	}

	public Cell GetCellFromDelta(ref float xp, ref float yp, float xd, float yd, bool global = false)
	{
		if (xp == 0f && yp == 0f)
		{
			return this;
		}
		while ((int)xp == X && (int)yp == Y)
		{
			xp += xd;
			yp += yd;
		}
		int num = (int)xp;
		int num2 = (int)yp;
		if (num < 0)
		{
			return null;
		}
		if (num > 79)
		{
			return null;
		}
		if (num2 < 0)
		{
			return null;
		}
		if (num2 > 24)
		{
			return null;
		}
		return ParentZone.GetCell(num, num2);
	}

	public Cell GetCellFromOffset(int xd, int yd)
	{
		return ParentZone.GetCell(X + xd, Y + yd);
	}

	public IEnumerable<Cell> GetCellsInACosmeticCircle(int radius)
	{
		int yradius = (int)Math.Max(1.0, (double)radius * 0.66);
		float radius_squared = radius * radius;
		for (int x = X - radius; x <= X + radius; x++)
		{
			for (int y = Y - yradius; y <= Y + yradius; y++)
			{
				float num = Math.Abs(x - X);
				float num2 = (float)Math.Abs(y - Y) * 1.3333f;
				float num3 = num * num + num2 * num2;
				Debug.Log("xd: " + num + " yd:" + num2 + " d=" + num3);
				if (num3 <= radius_squared && ParentZone.GetCell(x, y) != null)
				{
					yield return ParentZone.GetCell(x, y);
				}
			}
		}
	}

	public IEnumerable<Cell> GetCellsInABox(int width, int height)
	{
		for (int x = X; x <= X + width; x++)
		{
			for (int y = Y; y <= Y + height; y++)
			{
				yield return ParentZone.GetCell(x, y);
			}
		}
	}

	public Cell GetCellFromDirection(string Direction, bool BuiltOnly = true)
	{
		return GetCellFromDirectionGlobal(Direction, bLocalOnly: false, BuiltOnly);
	}

	public List<Cell> GetDirectionAndAdjacentCells(string Direction, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell cellFromDirection = GetCellFromDirection(Direction, BuiltOnly);
		if (cellFromDirection != null)
		{
			list.Add(cellFromDirection);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string direction in adjacentDirections)
		{
			Cell cellFromDirection2 = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection2 != null)
			{
				list.Add(cellFromDirection2);
			}
		}
		return list;
	}

	public Dictionary<string, Cell> GetAdjacentDirectionCellMap(string Direction, bool BuiltOnly = true)
	{
		Dictionary<string, Cell> dictionary = new Dictionary<string, Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell cellFromDirection = GetCellFromDirection(Direction, BuiltOnly);
		if (cellFromDirection != null)
		{
			dictionary.Add(Direction, cellFromDirection);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string text in adjacentDirections)
		{
			Cell cellFromDirection2 = GetCellFromDirection(text, BuiltOnly);
			if (cellFromDirection2 != null)
			{
				dictionary.Add(text, cellFromDirection2);
			}
		}
		return dictionary;
	}

	public Cell GetLocalCellFromDirection(string Direction, bool BuiltOnly = true)
	{
		return GetCellFromDirectionGlobal(Direction, bLocalOnly: true, BuiltOnly);
	}

	public List<Cell> GetLocalDirectionAndAdjacentCells(string Direction, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell localCellFromDirection = GetLocalCellFromDirection(Direction, BuiltOnly);
		if (localCellFromDirection != null)
		{
			list.Add(localCellFromDirection);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string direction in adjacentDirections)
		{
			if (GetLocalCellFromDirection(direction, BuiltOnly) != null)
			{
				list.Add(localCellFromDirection);
			}
		}
		return list;
	}

	public Cell GetCellFromDirectionOfCell(Cell C, bool BuiltOnly = true)
	{
		string directionFromCell = GetDirectionFromCell(C);
		if (directionFromCell == "." || directionFromCell == "?")
		{
			return null;
		}
		return GetCellFromDirection(directionFromCell, BuiltOnly);
	}

	public Cell GetLocalCellFromDirectionOfCell(Cell C, bool BuiltOnly = true)
	{
		string directionFromCell = GetDirectionFromCell(C);
		if (directionFromCell == "." || directionFromCell == "?")
		{
			return null;
		}
		return GetLocalCellFromDirection(directionFromCell, BuiltOnly);
	}

	public Cell GetCellFromDirectionFiltered(string Direction, List<string> ZoneFilter)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap())
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			string text = ZoneID.Assemble(zoneWorld, num, num2, num3, num4, zoneZ);
			if (!ZoneFilter.Contains(text))
			{
				return null;
			}
			zone = zoneManager.GetZone(text);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public Cell GetCellFromDirectionGlobalIfBuilt(string Direction)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap())
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			string zoneID = ZoneID.Assemble(zoneWorld, num, num2, num3, num4, zoneZ);
			if (!XRLCore.Core.Game.ZoneManager.IsZoneBuilt(zoneID))
			{
				return null;
			}
			zone = zoneManager.GetZone(zoneID);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public Cell GetCellFromDirectionGlobal(string Direction, bool bLocalOnly = true, bool bLiveZonesOnly = true)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap() || bLocalOnly)
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			if (bLiveZonesOnly && (zoneManager == null || !zoneManager.IsZoneLive(zoneWorld, num, num2, num3, num4, zoneZ)))
			{
				return null;
			}
			zone = zoneManager.GetZone(zoneWorld, num, num2, num3, num4, zoneZ);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public string Render(ConsoleChar Char, bool Visible, LightLevel Lit, bool Explored, bool bAlt, bool bSkulk = false, bool bDisableColorEffects = false, List<GameObject> wantsToPaint = null)
	{
		if (GameManager.bDraw == 13)
		{
			return "?";
		}
		bool flag = Globals.RenderMode == RenderModeType.Tiles;
		eRender.ColorString = "&y";
		eRender.BackgroundString = "";
		eRender.HighestLayer = -1;
		eRender.CustomDraw = false;
		eRender.Tile = null;
		eRender.RenderString = (Explored ? "." : " ");
		eRender.DetailColor = null;
		eRender.Lit = Lit;
		eRender.HFlip = false;
		eRender.VFlip = false;
		if (GameManager.bDraw == 14)
		{
			return "4";
		}
		GameObject body = XRLCore.Core.Game.Player.Body;
		bool flag2 = false;
		GameObject gameObject2;
		Render pRender2;
		if (Explored)
		{
			if (bAlt)
			{
				eRender.DetailColor = "k";
				eRender.ColorString = "&k";
				eRender.BackgroundString = "^k";
			}
			else if (XRLCore.RenderFloorTextures)
			{
				if (flag && !string.IsNullOrEmpty(PaintTile))
				{
					Char.Tile = PaintTile;
					eRender.Tile = PaintTile;
				}
				if (!string.IsNullOrEmpty(PaintRenderString))
				{
					eRender.RenderString = PaintRenderString;
				}
				if (Visible)
				{
					if (flag && !string.IsNullOrEmpty(PaintTileColor))
					{
						eRender.ColorString = PaintTileColor;
					}
					else
					{
						eRender.ColorString = PaintColorString;
					}
				}
				if (string.IsNullOrEmpty(PaintDetailColor))
				{
					Char.Detail = ColorBlack;
				}
				else
				{
					Char.Detail = ConsoleLib.Console.ColorUtility.ColorMap[PaintDetailColor[0]];
				}
			}
			bool flag3 = false;
			bool flag4 = false;
			if (bAlt && body != null)
			{
				if (body.HasPart("CookingAndGathering_Harvestry"))
				{
					flag3 = true;
				}
				if (body.HasPart("TrashRifling"))
				{
					flag4 = true;
				}
			}
			bool flag5 = Lit != LightLevel.None;
			bool flag6 = flag5 && Visible;
			int num = ((body != null) ? DistanceTo(body) : 9999999);
			int num2 = -1;
			for (int i = 0; i < Objects.Count; i++)
			{
				GameObject gameObject = Objects[i];
				if (!flag2 || gameObject.IsPlayer() || ((num <= 1) ? gameObject.ConsiderSolidInRenderingContextFor(body) : gameObject.ConsiderSolidInRenderingContext()))
				{
					Render pRender = gameObject.pRender;
					if (pRender != null)
					{
						if (pRender.CustomRender && gameObject.HasRegisteredEvent("CustomRender"))
						{
							gameObject.FireEvent(Event.New("CustomRender", "RenderEvent", eRender));
						}
						if (gameObject.IsPlayer() || (pRender.Visible && (flag6 || pRender.RenderIfDark)))
						{
							if (pRender.RenderLayer >= eRender.HighestLayer)
							{
								num2 = i;
								eRender.HighestLayer = pRender.RenderLayer;
							}
							gameObject.Seen();
							if (gameObject == Sidebar.CurrentTarget)
							{
								XRLCore.CludgeTargetRendered = true;
							}
							if (!flag2 && ((num <= 1) ? gameObject.ConsiderSolidInRenderingContextFor(body) : gameObject.ConsiderSolidInRenderingContext()))
							{
								flag2 = true;
							}
						}
					}
					if (wantsToPaint != null && gameObject.HasTag("AlwaysPaint"))
					{
						wantsToPaint.Add(gameObject);
					}
				}
				if (bAlt && gameObject.HasTag("ImportantOverlayObject"))
				{
					break;
				}
			}
			if (num2 > -1)
			{
				gameObject2 = Objects[num2];
				pRender2 = gameObject2.pRender;
				ParentZone.RenderedObjects++;
				if (XRLCore.RenderFloorTextures || pRender2.RenderLayer > 0)
				{
					if (!flag5 && gameObject2.IsPlayer())
					{
						eRender.RenderString = pRender2.RenderString;
						eRender.HFlip = gameObject2.pRender.getHFlip();
						eRender.VFlip = gameObject2.pRender.getVFlip();
						if (flag && !string.IsNullOrEmpty(pRender2.TileColor))
						{
							eRender.ColorString = pRender2.TileColor;
						}
						else
						{
							eRender.ColorString = pRender2.ColorString;
						}
						eRender.HighestLayer = pRender2.RenderLayer;
						if (flag)
						{
							eRender.Tile = gameObject2.pRender.Tile;
						}
						eRender.WantsToPaint = false;
						gameObject2.Render(eRender);
						if (wantsToPaint != null && eRender.WantsToPaint)
						{
							wantsToPaint.Add(gameObject2);
						}
						if (flag)
						{
							Char.Tile = gameObject2.pRender.Tile;
							Char.Detail = ColorGray;
						}
					}
					else if (pRender2.RenderIfDark || flag6)
					{
						eRender.HFlip = gameObject2.pRender.getHFlip();
						eRender.VFlip = gameObject2.pRender.getVFlip();
						if (flag)
						{
							eRender.Tile = gameObject2.GetTile();
						}
						if (!bAlt)
						{
							eRender.RenderString = pRender2.RenderString;
							if (flag && !string.IsNullOrEmpty(pRender2.TileColor))
							{
								eRender.ColorString = pRender2.TileColor;
							}
							else
							{
								eRender.ColorString = pRender2.ColorString;
							}
						}
						else
						{
							eRender.RenderString = "Û";
							string propertyOrTag = gameObject2.GetPropertyOrTag("OverlayColor");
							if (propertyOrTag != null)
							{
								eRender.RenderString = pRender2.RenderString;
								eRender.ColorString = propertyOrTag;
								eRender.BackgroundString = "^k";
								if (eRender.ColorString.Length > 1)
								{
									eRender.DetailColor = eRender.ColorString.Substring(1, 1);
								}
							}
							else
							{
								eRender.ColorString = "&k";
								eRender.BackgroundString = "^k";
								eRender.DetailColor = "k";
							}
							string stringProperty = gameObject2.GetStringProperty("OverlayDetailColor");
							if (stringProperty != null)
							{
								eRender.DetailColor = stringProperty;
							}
							string stringProperty2 = gameObject2.GetStringProperty("OverlayRenderString");
							if (stringProperty2 != null)
							{
								eRender.RenderString = stringProperty2;
							}
							string stringProperty3 = gameObject2.GetStringProperty("OverlayTile");
							if (stringProperty3 != null)
							{
								eRender.Tile = stringProperty3;
							}
							if (gameObject2.pBrain != null && body != null)
							{
								Brain pBrain = gameObject2.pBrain;
								if (gameObject2.IsPlayer())
								{
									eRender.BackgroundString = "^k";
									eRender.ColorString = "&B";
									eRender.DetailColor = "B";
								}
								else
								{
									if (pBrain != null)
									{
										GameObject partyLeader = pBrain.PartyLeader;
										if (partyLeader != null && partyLeader.IsPlayer())
										{
											eRender.BackgroundString = "^k";
											eRender.ColorString = "&b";
											eRender.DetailColor = "b";
											goto IL_08e4;
										}
									}
									if (gameObject2.IsHostileTowards(body))
									{
										eRender.RenderString = pRender2.RenderString;
										eRender.ColorString = "&R";
										eRender.BackgroundString = "^k";
										eRender.DetailColor = "R";
									}
									else
									{
										eRender.RenderString = pRender2.RenderString;
										eRender.ColorString = "&G";
										eRender.BackgroundString = "^k";
										eRender.DetailColor = "G";
									}
								}
							}
							else if (gameObject2.HasPart("Tinkering_Mine"))
							{
								Tinkering_Mine tinkering_Mine = gameObject2.GetPart("Tinkering_Mine") as Tinkering_Mine;
								if (tinkering_Mine.Timer != -1 || tinkering_Mine.ConsiderHostile(body))
								{
									eRender.RenderString = pRender2.RenderString;
									eRender.ColorString = "&R";
									eRender.BackgroundString = "^k";
									eRender.DetailColor = "R";
								}
								else
								{
									eRender.RenderString = pRender2.RenderString;
									eRender.ColorString = "&G";
									eRender.BackgroundString = "^k";
									eRender.DetailColor = "G";
								}
							}
							else if (flag4 && gameObject2.HasPart("Garbage"))
							{
								eRender.RenderString = pRender2.RenderString;
								eRender.DetailColor = "w";
								eRender.ColorString = "&w";
								eRender.BackgroundString = "^k";
							}
							else if (flag3 && gameObject2.GetPart("Harvestable") is Harvestable harvestable && harvestable.Ripe)
							{
								eRender.RenderString = pRender2.RenderString;
								eRender.DetailColor = "w";
								eRender.ColorString = "&w";
								eRender.BackgroundString = "^k";
							}
						}
						goto IL_08e4;
					}
				}
			}
		}
		goto IL_0b14;
		IL_08e4:
		if (gameObject2 == Sidebar.CurrentTarget && !bAlt)
		{
			Brain pBrain2 = gameObject2.pBrain;
			if (gameObject2.IsPlayer())
			{
				eRender.BackgroundString = "^k";
				eRender.ColorString = "&B";
				eRender.DetailColor = "B";
			}
			else
			{
				if (pBrain2 != null)
				{
					GameObject partyLeader2 = pBrain2.PartyLeader;
					if (partyLeader2 != null && partyLeader2.IsPlayer())
					{
						eRender.BackgroundString = "^k";
						eRender.ColorString = "&b";
						eRender.DetailColor = "b";
						goto IL_0a0c;
					}
				}
				if (pBrain2 != null && pBrain2.IsHostileTowards(body))
				{
					if (XRLCore.CurrentFrame < 15)
					{
						eRender.BackgroundString = "^r";
					}
					else if (XRLCore.CurrentFrame > 30 && XRLCore.CurrentFrame < 45)
					{
						eRender.BackgroundString = "^r";
					}
				}
				else if (XRLCore.CurrentFrame < 15)
				{
					eRender.BackgroundString = "^g";
				}
				else if (XRLCore.CurrentFrame > 30 && XRLCore.CurrentFrame < 45)
				{
					eRender.BackgroundString = "^g";
				}
			}
		}
		goto IL_0a0c;
		IL_0a0c:
		eRender.HighestLayer = pRender2.RenderLayer;
		eRender.WantsToPaint = false;
		if (bAlt)
		{
			gameObject2.OverlayRender(eRender);
		}
		else
		{
			gameObject2.Render(eRender);
		}
		if (wantsToPaint != null && eRender.WantsToPaint && !wantsToPaint.Contains(gameObject2))
		{
			wantsToPaint.Add(gameObject2);
		}
		if (flag)
		{
			Char.Tile = eRender.Tile;
		}
		Color value;
		if (string.IsNullOrEmpty(gameObject2.pRender.DetailColor))
		{
			Char.Detail = ColorBlack;
		}
		else if (string.IsNullOrEmpty(gameObject2.pRender.DetailColor))
		{
			Char.Detail = Color.black;
		}
		else if (!ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(gameObject2.pRender.DetailColor[0], out value))
		{
			Char.Detail = Color.black;
		}
		else
		{
			Char.Detail = value;
		}
		Char.HFlip = eRender.HFlip;
		Char.VFlip = eRender.VFlip;
		goto IL_0b14;
		IL_0b14:
		int j = 0;
		for (int count = Objects.Count; j < count; j++)
		{
			eRender.WantsToPaint = false;
			Objects[j].FinalRender(eRender, bAlt);
			if (wantsToPaint != null && eRender.WantsToPaint && !wantsToPaint.Contains(Objects[j]))
			{
				wantsToPaint.Add(Objects[j]);
			}
		}
		if (!string.IsNullOrEmpty(eRender.DetailColor))
		{
			Char.Detail = ConsoleLib.Console.ColorUtility.ColorMap[eRender.DetailColor[0]];
		}
		if (eRender.RenderString == "^")
		{
			eRender.RenderString = "^^";
		}
		else if (eRender.RenderString == "&")
		{
			eRender.RenderString = "&&";
		}
		if (GameManager.bDraw == 19)
		{
			return "?";
		}
		if (eRender.CustomDraw)
		{
			Char.Tile = eRender.Tile;
			return RenderBuilder.Clear().Append(eRender.ColorString).Append(eRender.BackgroundString)
				.Append(eRender.RenderString)
				.ToString();
		}
		if (!bAlt)
		{
			if (bSkulk)
			{
				eRender.BackgroundString = "^k";
				if (Visible)
				{
					eRender.ColorString = "&B";
					if (flag)
					{
						Char.TileForeground = ColorBrightBlue;
						Char.Detail = ColorDarkBlue;
					}
				}
				else
				{
					eRender.ColorString = "&b";
					if (flag)
					{
						Char.TileForeground = ColorDarkBlue;
						Char.Detail = ColorDarkBlue;
					}
				}
			}
			else if (!Visible)
			{
				eRender.ColorString = "&K";
				if (flag)
				{
					Char.Detail = ColorBlack;
				}
			}
			else
			{
				switch (Lit)
				{
				case LightLevel.Darkvision:
					if (!bDisableColorEffects)
					{
						eRender.ColorString = "&G";
						eRender.DetailColor = "g";
						if (flag)
						{
							Char.TileForeground = ColorBrightGreen;
							Char.Detail = ColorDarkGreen;
						}
					}
					break;
				case LightLevel.Safelight:
					if (!bDisableColorEffects)
					{
						eRender.ColorString = "&r";
						eRender.DetailColor = "R";
						if (flag)
						{
							Char.TileForeground = ColorDarkRed;
							Char.Detail = ColorBrightRed;
						}
					}
					break;
				case LightLevel.Radar:
				case LightLevel.LitRadar:
				{
					if (!bDisableColorEffects)
					{
						int currentFrame = XRLCore.CurrentFrame;
						if (currentFrame >= 27 && currentFrame <= 44 && BlocksRadar())
						{
							eRender.ColorString = "&R";
							eRender.DetailColor = "r";
							if (flag)
							{
								Char.TileForeground = ColorBrightRed;
								Char.Detail = ColorDarkRed;
							}
						}
						else if (Lit == LightLevel.Radar)
						{
							eRender.ColorString = "&C";
							eRender.DetailColor = "c";
							if (flag)
							{
								Char.TileForeground = ColorBrightCyan;
								Char.Detail = ColorDarkCyan;
							}
						}
					}
					if (!flag2 || XRLCore.CurrentFrame10 % 125 < 95)
					{
						break;
					}
					GameObject gameObject3 = null;
					int num3 = -1;
					int k = 0;
					for (int count2 = Objects.Count; k < count2; k++)
					{
						GameObject gameObject4 = Objects[k];
						if (gameObject4.pPhysics != null && !gameObject4.pPhysics.Solid && gameObject4.IsReal && gameObject4.pRender != null && gameObject4.pRender.Visible && gameObject4.pRender.RenderLayer > num3 && gameObject4.pRender.RenderLayer > 0)
						{
							num3 = gameObject4.pRender.RenderLayer;
							gameObject3 = gameObject4;
						}
					}
					if (gameObject3 != null)
					{
						gameObject3.Render(eRender);
						if (flag)
						{
							Char.Tile = eRender.Tile;
						}
					}
					break;
				}
				default:
					eRender.ColorString = "&K";
					if (flag)
					{
						Char.Detail = ColorBlack;
					}
					break;
				case LightLevel.Dimvision:
				case LightLevel.Light:
				case LightLevel.Interpolight:
				case LightLevel.Omniscient:
					break;
				}
			}
		}
		if (GameManager.bDraw == 20)
		{
			return "?";
		}
		if (eRender.ColorString != eLastRender.ColorString || eRender.BackgroundString != eLastRender.BackgroundString || eRender.RenderString != eLastRender.RenderString)
		{
			eLastRender.ColorString = eRender.ColorString;
			eLastRender.BackgroundString = eRender.BackgroundString;
			eLastRender.RenderString = eRender.RenderString;
			eLastRender.Final = RenderBuilder.Clear().Append(eRender.ColorString).Append(eRender.BackgroundString)
				.Append(eRender.RenderString)
				.ToString();
		}
		_ = Char.HFlip;
		return eLastRender.Final;
	}

	public void ClearObjectsWithPart(string Part)
	{
		GameObject gameObject = null;
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasPart(Part) || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			RemoveObject(item);
		}
	}

	public void ClearObjectsWithProperty(string Property)
	{
		GameObject gameObject = null;
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasStringProperty(Property) || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list != null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				RemoveObject(list[j]);
			}
		}
	}

	public void ClearObjectsWithIntProperty(string Property)
	{
		List<GameObject> list = null;
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(Property) <= 0 || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list != null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				RemoveObject(list[j]);
			}
		}
	}

	public void ClearObjectsWithTag(string Tag)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag) && Objects[i].CanClear())
			{
				list.Add(Objects[i]);
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			RemoveObject(list[j]);
		}
	}

	public Cell ClearTerrain()
	{
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWall() && Objects[i].CanClear())
			{
				list.Add(Objects[i]);
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			GameObject gameObject = list[j];
			if (gameObject != null && gameObject.CanClear())
			{
				RemoveObject(list[j]);
			}
		}
		return this;
	}

	/// <summary>
	///             Clear this cell's <see cref="F:XRL.World.Cell.Objects" />.
	///             </summary><param name="Blueprint">A replacement blueprint to place in this cell after clearing.</param><param name="Important"><c>true</c> if important objects should be cleared; otherwise, <c>false</c>.</param><param name="Combat"><c>true</c> if combat objects should be cleared; otherwise, <c>false</c>.</param><param name="alsoExclude">A predicate which will prevent objects from being cleared if it returns true.</param><seealso cref="M:XRL.World.GameObject.IsImportant" /><seealso cref="M:XRL.World.GameObject.IsCombatObject(System.Boolean)" />
	public Cell Clear(string Blueprint = null, bool Important = false, bool Combat = false, Func<GameObject, bool> alsoExclude = null)
	{
		if (Objects.Count == 0)
		{
			return this;
		}
		if (Objects.Count == 1)
		{
			GameObject gameObject = Objects[0];
			if (gameObject != null && gameObject.CanClear(Important, Combat) && (alsoExclude == null || !alsoExclude(Objects[0])))
			{
				RemoveObject(Objects[0]);
			}
			return this;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject2 = list[i];
			if (gameObject2 != null && gameObject2.CanClear(Important, Combat) && (alsoExclude == null || !alsoExclude(Objects[0])))
			{
				RemoveObject(list[i]);
			}
		}
		if (Blueprint != null)
		{
			AddObject(Blueprint);
		}
		return this;
	}

	public void DustPuff()
	{
		if (!InActiveZone)
		{
			return;
		}
		for (int i = 0; i < 15; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 4f;
			num2 = (float)Math.Cos(num3) / 4f;
			if (Stat.Random(1, 4) <= 3)
			{
				The.ParticleManager.Add("&y.", X, Y, num, num2, 15, 0f, 0f);
			}
			else
			{
				The.ParticleManager.Add("&y±", X, Y, num, num2, 15, 0f, 0f);
			}
		}
	}

	public void PsychicPulse()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				ParticleText("&B" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			for (int k = 0; k < 5; k++)
			{
				ParticleText("&b" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			for (int l = 0; l < 5; l++)
			{
				ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
		}
	}

	public void LargeFireblast()
	{
		for (int i = 2; i < 5; i++)
		{
			ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
		for (int j = 2; j < 5; j++)
		{
			ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
		for (int k = 2; k < 5; k++)
		{
			ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
	}

	public void SmallFireblast()
	{
		for (int i = 0; i < 3; i++)
		{
			ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
		for (int j = 0; j < 3; j++)
		{
			ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
		for (int k = 0; k < 3; k++)
		{
			ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
	}

	public void ParticleBlip(string Text, int Duration)
	{
		if (InActiveZone)
		{
			The.ParticleManager.Add(Text, X, Y, 0f, 0f, Duration, 0f, 0f);
		}
	}

	public void ParticleBlip(string Text)
	{
		if (InActiveZone)
		{
			The.ParticleManager.Add(Text, X, Y, 0f, 0f, 10, 0f, 0f);
		}
	}

	public void ParticleText(string Text, float Velocity, int Life)
	{
		if (InActiveZone)
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float num2 = (float)Math.Sin(num) / 4f;
			float num3 = (float)Math.Cos(num) / 4f;
			num2 *= Velocity;
			num3 *= Velocity;
			The.ParticleManager.Add(Markup.Transform(Text), X, Y, num2, num3, Life, 0f, 0f);
		}
	}

	public void ParticleText(string Text, bool IgnoreVisibility = true)
	{
		if (IgnoreVisibility || IsVisible())
		{
			ParticleText(Text, 1f, 999);
		}
	}

	public void ParticleText(string Text, int angleMin = 0, int angleMax = 359, bool IgnoreVisibility = true)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float xDel = (float)Math.Sin(num) / 4f;
			float yDel = (float)Math.Cos(num) / 4f;
			The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f);
		}
	}

	public void ParticleText(string Text, float xVel, float yVel, char color = ' ', bool IgnoreVisibility = true)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			if (color != ' ')
			{
				Text = "{{" + color + "|" + Text + "}}";
			}
			The.ParticleManager.Add(Text, X, Y, xVel, yVel, 999, 0f, 0f);
		}
	}

	public void ParticleText(string Text, char color, bool IgnoreVisibility = true, float juiceDuration = 1.5f, float floatLength = -8f, GameObject emitting = null)
	{
		if (!IgnoreVisibility && !IsVisible())
		{
			return;
		}
		if (juiceEnabled)
		{
			if (color == ' ')
			{
				Text = Markup.Transform(Text);
				color = ConsoleLib.Console.ColorUtility.ParseForegroundColor(Text);
				Text = ConsoleLib.Console.ColorUtility.StripFormatting(Text);
			}
			CombatJuice.floatingText(this, Text, ConsoleLib.Console.ColorUtility.ColorMap[color], juiceDuration, floatLength, 1f, ignoreVisibility: true, emitting);
			return;
		}
		float num = (float)Stat.Random(0, 359) / 58f;
		float xDel = (float)Math.Sin(num) / 4f;
		float yDel = (float)Math.Cos(num) / 4f;
		if (color != ' ')
		{
			Text = "{{" + color + "|" + Text + "}}";
		}
		The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f);
	}

	public void ParticleText(string Text)
	{
		if (InActiveZone)
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float xDel = (float)Math.Sin(num) / 4f;
			float yDel = (float)Math.Cos(num) / 4f;
			The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f);
		}
	}

	public virtual GameObject GetCombatObject()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsCombatObject())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public virtual GameObject GetCombatTarget(GameObject Attacker = null, bool IgnoreFlight = false, bool IgnoreAttackable = false, bool IgnorePhase = false, int Phase = 0, GameObject Projectile = null, GameObject CheckPhaseAgainst = null, bool AllowInanimate = true, bool InanimateSolidOnly = false)
	{
		if (IgnorePhase)
		{
			Phase = 5;
		}
		else if (Phase == 0)
		{
			if (CheckPhaseAgainst == null)
			{
				CheckPhaseAgainst = Projectile ?? Attacker;
			}
			Phase = CheckPhaseAgainst?.GetPhase() ?? 5;
		}
		bool checkFlight = !IgnoreFlight;
		bool checkAttackable = !IgnoreAttackable;
		bool checkSolidity = false;
		GameObject solidityPOV = Projectile ?? Attacker;
		if (GetObjectCountAndFirstObjectWithPart(out var FirstObject, "Combat", Phase, Attacker, Attacker, solidityPOV, Projectile, checkFlight, checkAttackable, checkSolidity) > 1)
		{
			List<GameObject> list = Event.NewGameObjectList();
			GetObjectsWithPart("Combat", list, Phase, Attacker, Attacker, solidityPOV, Projectile, checkFlight, checkAttackable, checkSolidity);
			list.Sort(new CombatSorter(Attacker));
			FirstObject = list[0];
		}
		if (FirstObject == null && AllowInanimate)
		{
			if (InanimateSolidOnly || IsSolidFor(Projectile, Attacker))
			{
				checkSolidity = true;
			}
			if (GetRealObjectCountAndFirstObject(out FirstObject, Phase, Attacker, Attacker, solidityPOV, Projectile, checkFlight, checkAttackable, checkSolidity) > 1)
			{
				List<GameObject> list2 = Event.NewGameObjectList();
				GetRealObjects(list2, Phase, Attacker, Attacker, solidityPOV, Projectile, checkFlight, checkAttackable, checkSolidity);
				list2.Sort(new CombatSorter(Attacker));
				FirstObject = list2[0];
			}
		}
		return FirstObject;
	}

	public int GetTotalHostileDifficultyLevel(GameObject who)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsHostileTowards(who))
			{
				num += Objects[i].Con(who).GetValueOrDefault();
			}
		}
		return num;
	}

	public int GetTotalAdjacentHostileDifficultyLevel(GameObject who)
	{
		int num = 0;
		foreach (Cell adjacentCell in GetAdjacentCells())
		{
			num += adjacentCell.GetTotalHostileDifficultyLevel(who);
		}
		return num;
	}

	public GameObject GetOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsOpenLiquidVolume())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetDangerousOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsDangerousOpenLiquidVolume())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetWadingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWadingDepthLiquid())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetSwimmingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmingDepthLiquid())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool HasSwimmingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmingDepthLiquid())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsOpenLiquidVolume())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWadingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWadingDepthLiquid())
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetAquaticSupportFor(GameObject who)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmableFor(who))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool HasAquaticSupportFor(GameObject who)
	{
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsBridge)
			{
				return false;
			}
			if (!flag && Objects[i].IsSwimmableFor(who))
			{
				flag = true;
			}
		}
		return flag;
	}

	public bool HasWalkableWallFor(GameObject who)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWalkableWall(who))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHealingPool()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsHealingPool())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeader()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsTryingToJoinPartyLeader())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeaderForZoneUncaching()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsTryingToJoinPartyLeaderForZoneUncaching())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPlayerLed()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsPlayerLed())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWasPlayer()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WasPlayer())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLeftBehindByPlayer()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].LeftBehindByPlayer())
			{
				return true;
			}
		}
		return false;
	}

	public int GetHealingLocationValue(GameObject Actor)
	{
		return PollForHealingLocationEvent.GetFor(Actor, this);
	}

	public bool IsHealingLocation(GameObject Actor)
	{
		return PollForHealingLocationEvent.GetFor(Actor, this, First: true) > 0;
	}

	public void UseHealingLocation(GameObject Actor)
	{
		UseHealingLocationEvent.Send(Actor, this);
	}

	public GameObject findObjectById(string id)
	{
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = FindObjectByIdEvent.Find(Objects[num], id);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public bool OnWorldMap()
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.IsWorldMap();
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (ID == GetContentsEvent.ID)
		{
			return true;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantEvent(ID, cascade))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		if (E is GetContentsEvent && E is GetContentsEvent getContentsEvent)
		{
			getContentsEvent.Objects.AddRange(Objects);
		}
		int num = -1;
		int cascadeLevel = E.GetCascadeLevel();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantEvent(E.ID, cascadeLevel))
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(Objects);
			int j = num;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (!list[j].HandleEvent(E))
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
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantEvent(ID, cascade))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public int TemperatureChange(int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, int Phase = 0)
	{
		if (Phase == 0 && Actor != null)
		{
			Phase = Actor.GetPhase();
		}
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.TemperatureChange(Amount, Actor, Radiant, MinAmbient, MaxAmbient, Phase))
			{
				num++;
			}
			if (count != Objects.Count)
			{
				count = Objects.Count;
				if (i < count && Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
		return num;
	}

	public void Splash(string Particle)
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 3; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
				num = (float)Math.Sin(num3) / 3f;
				num2 = (float)Math.Cos(num3) / 3f;
				The.ParticleManager.Add(Particle, X, Y, num, num2, 5, 0f, 0f);
			}
		}
	}

	public void LiquidSplash(string Color)
	{
		if (!InActiveZone)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 3f;
			num2 = (float)Math.Cos(num3) / 3f;
			char c = '.';
			switch (Stat.Random(0, 5))
			{
			case 1:
				c = 'ú';
				break;
			case 2:
				c = 'ø';
				break;
			case 3:
				c = '~';
				break;
			case 4:
				c = 'ù';
				break;
			case 5:
				c = '\a';
				break;
			}
			The.ParticleManager.Add("&" + Color + c, X, Y, num, num2, 5, 0f, 0f);
			Thread.Sleep(Stat.Random(5, 15));
		}
	}

	public void LiquidSplash(List<string> Colors)
	{
		if (!ParentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 3f;
			num2 = (float)Math.Cos(num3) / 3f;
			char c = '.';
			switch (Stat.Random(0, 5))
			{
			case 1:
				c = 'ú';
				break;
			case 2:
				c = 'ø';
				break;
			case 3:
				c = '~';
				break;
			case 4:
				c = 'ù';
				break;
			case 5:
				c = '\a';
				break;
			}
			The.ParticleManager.Add("&" + Colors.GetRandomElement() + c, X, Y, num, num2, 5, 0f, 0f);
			Thread.Sleep(Stat.Random(5, 15));
		}
	}

	public void LiquidSplash(BaseLiquid Liquid)
	{
		if (Liquid != null)
		{
			LiquidSplash(Liquid.GetColors());
		}
	}

	public void TelekinesisBlip()
	{
		if (InActiveZone)
		{
			int i = 0;
			for (int num = Stat.Random(2, 4); i < num; i++)
			{
				int num2 = Stat.Random(0, 359);
				float num3 = (float)Stat.RandomCosmetic(4, 14) / 5f;
				The.ParticleManager.Add("@", X, Y, (float)Math.Sin((double)num2 * 0.017) / num3, (float)Math.Cos((double)num2 * 0.017) / num3, Stat.Random(3, 20));
			}
		}
	}

	public void DilationSplat()
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 360; i++)
			{
				float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
				The.ParticleManager.Add("@", X, Y, (float)Math.Sin((double)i * 0.017) / num, (float)Math.Cos((double)i * 0.017) / num);
			}
		}
	}

	public void ImplosionSplat(int Radius = 12)
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 360; i++)
			{
				float num = (float)Stat.RandomCosmetic(1, 5) / ((float)Radius / 4f);
				The.ParticleManager.Add("@", (int)Math.Round((double)X + (double)Radius * Math.Sin((double)i * 0.017)), (int)Math.Round((double)Y + (double)Radius * Math.Cos((double)i * 0.017)), (float)(0.0 - Math.Sin((double)i * 0.017)) / num, (float)(0.0 - Math.Cos((double)i * 0.017)) / num, (int)Math.Round((float)Radius * num));
			}
		}
	}

	public bool HasBridge()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsBridge)
			{
				return true;
			}
		}
		return false;
	}

	public List<GameObject> GetSolidObjects()
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid())
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetSolidObjectsFor(GameObject who)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(who))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public bool IsAudible(GameObject By, int Volume = 20)
	{
		if (ParentZone == null || ParentZone.IsWorldMap())
		{
			return false;
		}
		return ParentZone.FastFloodAudibilityAny(X, Y, Volume, (GameObject GO) => GO == By, By);
	}

	public bool IsSmellable(GameObject By, int Intensity = 5)
	{
		if (ParentZone == null || ParentZone.IsWorldMap())
		{
			return false;
		}
		return ParentZone.FastFloodOlfactionAny(X, Y, Intensity, (GameObject GO) => GO == By, By);
	}

	public Cell GetReversibleAccessUpCell()
	{
		if (HasObjectWithPart("StairsUp"))
		{
			Cell cellFromDirectionGlobalIfBuilt = GetCellFromDirectionGlobalIfBuilt("U");
			if (cellFromDirectionGlobalIfBuilt != null && cellFromDirectionGlobalIfBuilt.HasObjectWithPart("StairsDown"))
			{
				return cellFromDirectionGlobalIfBuilt;
			}
		}
		return null;
	}

	public Cell GetReversibleAccessDownCell()
	{
		if (HasObjectWithPart("StairsDown"))
		{
			Cell cellFromDirectionGlobalIfBuilt = GetCellFromDirectionGlobalIfBuilt("D");
			if (cellFromDirectionGlobalIfBuilt != null && cellFromDirectionGlobalIfBuilt.HasObjectWithPart("StairsUp"))
			{
				return cellFromDirectionGlobalIfBuilt;
			}
		}
		return null;
	}

	public bool BlocksRadar()
	{
		return BlocksRadarEvent.Check(this);
	}

	public bool IsSameOrAdjacent(Cell C, bool BuiltOnly = true)
	{
		if (C != this)
		{
			return IsAdjacentTo(C, BuiltOnly);
		}
		return true;
	}

	public bool Is(GlobalLocation loc)
	{
		if (loc.ZoneID != ParentZone?.ZoneID)
		{
			return false;
		}
		if (loc.CellX != X)
		{
			return false;
		}
		if (loc.CellY != Y)
		{
			return false;
		}
		return true;
	}

	public void PlayWorldSound(string Clip, float Volume = 0.5f, float PitchVariance = 0f, bool Combat = false, float Delay = 0f)
	{
		if (!Options.Sound || (Combat && !Options.UseCombatSounds) || string.IsNullOrEmpty(Clip) || !ParentZone.IsActive())
		{
			return;
		}
		Cell playerCell = The.PlayerCell;
		if (playerCell != null && (playerCell.ParentZone.IsWorldMap() || (float)PathDistanceTo(playerCell) <= 40f * Volume))
		{
			if (Zone.SoundMapDirty)
			{
				ParentZone.UpdateSoundMap();
			}
			int num = Zone.SoundMap.GetCostAtPoint(Pos2D);
			if (num == int.MaxValue)
			{
				num = Zone.SoundMap.GetCostFromPointDirection(Pos2D.location, Zone.SoundMap.GetLowestCostDirectionFrom(Pos2D));
			}
			SoundManager.PlayWorldSound(Clip, num, !IsVisible(), Volume, PitchVariance, Delay);
		}
	}

	public bool FastFloodVisibilityAny(int Radius, string SearchPart, GameObject Looker)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodVisibilityAny(X, Y, Radius, SearchPart, Looker);
	}

	public bool FastFloodVisibilityAny(int Radius, Predicate<GameObject> Filter, GameObject Looker)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodVisibilityAny(X, Y, Radius, Filter, Looker);
	}

	public bool FastFloodAudibilityAny(int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodAudibilityAny(X, Y, Radius, Filter, Hearer);
	}

	public bool FastFloodOlfactionAny(int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodOlfactionAny(X, Y, Radius, Filter, Smeller);
	}

	public List<Point> LineFromAngle(int degrees)
	{
		if (ParentZone == null)
		{
			return new List<Point>();
		}
		return ParentZone.LineFromAngle(X, Y, degrees);
	}

	public void Indicate()
	{
		if (juiceEnabled)
		{
			ParticleText("v", 'W');
		}
		else
		{
			ParticleBlip("&WX");
		}
	}

	public bool IsSolidGround()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetPart("StairsDown") is StairsDown stairsDown && stairsDown.PullDown)
			{
				return false;
			}
		}
		return true;
	}
}

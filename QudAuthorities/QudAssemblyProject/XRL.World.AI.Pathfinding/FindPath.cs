using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.UI;

namespace XRL.World.AI.Pathfinding;

public class FindPath
{
	public const int MAX_CELL_NAV = 4000;

	public static CellNavigationValue[] CellNavs = new CellNavigationValue[4000];

	public static int nCellNav = -1;

	[NonSerialized]
	private static List<string> ZoneFilter = new List<string>();

	[NonSerialized]
	private static Dictionary<Cell, CellNavigationValue> OpenList = new Dictionary<Cell, CellNavigationValue>();

	[NonSerialized]
	private static Dictionary<Cell, CellNavigationValue> CloseList = new Dictionary<Cell, CellNavigationValue>();

	[NonSerialized]
	private static OrderedBag<CellNavigationValue> OrderedNavigationValues = new OrderedBag<CellNavigationValue>();

	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListSorted = new string[8] { "N", "E", "S", "W", "NW", "NE", "SE", "SW" };

	public static string[] DirectionListU = new string[9] { "U", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListUSorted = new string[9] { "U", "N", "E", "S", "W", "NW", "NE", "SE", "SW" };

	public static string[] DirectionListD = new string[9] { "D", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListDSorted = new string[9] { "D", "N", "E", "S", "W", "NW", "NE", "SE", "SW" };

	public static string[] DirectionListCardinalOnly = new string[4] { "E", "S", "N", "W" };

	public bool bFound;

	public List<string> Directions = new List<string>();

	public List<Cell> Steps = new List<Cell>();

	public static void Initalize()
	{
		for (int i = 0; i < 4000; i++)
		{
			CellNavs[i] = new CellNavigationValue(0.0, 0.0, null, null, null);
		}
	}

	public CellNavigationValue NewCellNav(double Cost = 0.0, double Estimate = 0.0, Cell C = null, Cell Parent = null, string Direction = null)
	{
		if (nCellNav >= 3999)
		{
			return null;
		}
		nCellNav++;
		return CellNavs[nCellNav].Set(Cost, Estimate, C, Parent, Direction);
	}

	public FindPath()
	{
	}

	public FindPath(string StartZoneID, int X1, int Y1, string EndZoneID, int X2, int Y2, bool PathGlobal = false, bool PathUnlimited = false, GameObject Looker = null, bool Juggernaut = false)
	{
		if (StartZoneID != null && EndZoneID != null)
		{
			Zone zone = The.ZoneManager.GetZone(StartZoneID);
			Zone zone2 = The.ZoneManager.GetZone(EndZoneID);
			PerformPathfind(zone, X1, Y1, zone2, X2, Y2, PathGlobal, Looker, PathUnlimited, AddNoise: false, CardinalDirectionsOnly: false, null, 100, ExploredOnly: false, Juggernaut);
		}
	}

	public FindPath(Zone StartZone, int X1, int Y1, Zone EndZone, int X2, int Y2, bool PathGlobal, bool PathUnlimited, GameObject Looker, bool AddNoise)
	{
		if (StartZone != null && EndZone != null)
		{
			PerformPathfind(StartZone, X1, Y1, EndZone, X2, Y2, PathGlobal, Looker, PathUnlimited, AddNoise);
		}
	}

	public FindPath(Zone StartZone, int X1, int Y1, Zone EndZone, int X2, int Y2, bool PathGlobal = false, bool PathUnlimited = false, GameObject Looker = null, bool AddNoise = false, bool CardinalOnly = false, bool Juggernaut = false)
	{
		if (StartZone != null && EndZone != null)
		{
			PerformPathfind(StartZone, X1, Y1, EndZone, X2, Y2, PathGlobal, Looker, PathUnlimited, AddNoise, CardinalOnly, null, 100, ExploredOnly: false, Juggernaut);
		}
	}

	public FindPath(Zone StartZone, int X1, int Y1, Zone EndZone, int X2, int Y2, bool PathGlobal = false, bool PathUnlimited = false, bool Juggernaut = false, GameObject Looker = null, int MaxWeight = 100)
	{
		if (StartZone != null && EndZone != null)
		{
			bool juggernaut = Juggernaut;
			PerformPathfind(StartZone, X1, Y1, EndZone, X2, Y2, PathGlobal, Looker, PathUnlimited, AddNoise: false, CardinalDirectionsOnly: false, null, MaxWeight, ExploredOnly: false, juggernaut);
		}
	}

	public FindPath(Cell C1, Cell C2, bool PathGlobal = false, bool PathUnlimited = true, GameObject Looker = null, int MaxWeight = 100, bool ExploredOnly = false, bool Juggernaut = false)
	{
		if (C1 != null && C2 != null)
		{
			Zone parentZone = C1.ParentZone;
			Zone parentZone2 = C2.ParentZone;
			if (parentZone != null && parentZone2 != null)
			{
				PerformPathfind(parentZone, C1.X, C1.Y, parentZone2, C2.X, C2.Y, PathGlobal, Looker, Unlimited: true, AddNoise: false, CardinalDirectionsOnly: false, null, MaxWeight, ExploredOnly, Juggernaut);
			}
		}
	}

	public FindPath(Zone StartZone, int X1, int Y1, Zone EndZone, int X2, int Y2, bool PathGlobal, bool PathUnlimited, GameObject Looker, CleanQueue<XRLCore.SortPoint> Avoid, bool ExploredOnly = false)
	{
		if (StartZone != null && EndZone != null)
		{
			PerformPathfind(StartZone, X1, Y1, EndZone, X2, Y2, PathGlobal, Looker, PathUnlimited, AddNoise: false, CardinalDirectionsOnly: false, Avoid, 100, ExploredOnly);
		}
	}

	public void PerformPathfind(Zone StartZone, int X1, int Y1, Zone EndZone, int X2, int Y2, bool PathGlobal = false, GameObject Looker = null, bool Unlimited = false, bool AddNoise = false, bool CardinalDirectionsOnly = false, CleanQueue<XRLCore.SortPoint> Avoid = null, int MaxWeight = 100, bool ExploredOnly = false, bool Juggernaut = false)
	{
		if (StartZone == null || EndZone == null || (StartZone == EndZone && X1 == X2 && Y1 == Y2))
		{
			return;
		}
		nCellNav = 0;
		bool drawPathfinder = Options.DrawPathfinder;
		Cell cell = StartZone.GetCell(X1, Y1);
		Cell cell2 = EndZone.GetCell(X2, Y2);
		StartZone.CalculateNavigationMap(Looker, AddNoise, ExploredOnly, Juggernaut);
		if (StartZone != EndZone)
		{
			EndZone.CalculateNavigationMap(Looker, AddNoise, ExploredOnly, Juggernaut);
		}
		List<Zone> list = null;
		OpenList.Clear();
		CloseList.Clear();
		OrderedNavigationValues.Clear();
		OpenList.Add(StartZone.GetCell(X1, Y1), NewCellNav(0.0, 0.0, null, null, "."));
		try
		{
			ScreenBuffer screenBuffer = Popup._ScreenBuffer;
			ZoneFilter.Clear();
			ZoneFilter.Add(StartZone.ZoneID);
			ZoneFilter.Add(EndZone.ZoneID);
			int num = (Unlimited ? 9999 : Math.Max(80, cell.ManhattanDistanceTo(cell2) * 5 / 4));
			if (num > 1 && !cell.IsAdjacentTo(cell2))
			{
				if (StartZone.NavigationMap[X1, Y1].Weight <= MaxWeight && !cell.AnyLocalAdjacentCell((Cell C) => C.ParentZone.NavigationMap[C.X, C.Y].Weight <= MaxWeight))
				{
					bFound = false;
					return;
				}
				if (!cell2.AnyLocalAdjacentCell((Cell C) => C.ParentZone.NavigationMap[C.X, C.Y].Weight <= MaxWeight))
				{
					bFound = false;
					return;
				}
			}
			int num2 = 0;
			while (OpenList.Count > 0)
			{
				Cell cell3 = null;
				double num3 = double.MaxValue;
				foreach (Cell key in OpenList.Keys)
				{
					if (OpenList[key].Total < num3)
					{
						num3 = OpenList[key].Total;
						cell3 = key;
					}
				}
				if (drawPathfinder)
				{
					screenBuffer.WriteAt(cell3, "&Y!").Draw();
				}
				CellNavigationValue cellNavigationValue = OpenList[cell3];
				if (drawPathfinder)
				{
					screenBuffer.WriteAt(cell, "&GS").WriteAt(cell2, "&RE").Draw();
				}
				string[] array = DirectionListSorted;
				if (CardinalDirectionsOnly)
				{
					array = DirectionListCardinalOnly;
				}
				else if (PathGlobal && StartZone != EndZone)
				{
					if (cell3.HasObjectWithPart("StairsUp"))
					{
						array = DirectionListUSorted;
					}
					else if (cell3.HasObjectWithPart("StairsDown"))
					{
						array = DirectionListDSorted;
					}
				}
				int i = 0;
				for (int num4 = array.Length; i < num4; i++)
				{
					string text = array[i];
					num2++;
					if (num2 > 2000 && !Unlimited)
					{
						bFound = false;
						return;
					}
					Cell cell4 = null;
					cell4 = cell3.GetCellFromDirectionFiltered(text, ZoneFilter);
					NavigationWeight[,] navigationMap = StartZone.NavigationMap;
					if (cell4 != null && cell4.ParentZone != StartZone)
					{
						if (cell4.ParentZone == cell2.ParentZone)
						{
							navigationMap = EndZone.NavigationMap;
						}
						else
						{
							if (list == null)
							{
								list = new List<Zone>();
							}
							if (!list.Contains(cell4.ParentZone))
							{
								cell4.ParentZone.CalculateNavigationMap(Looker, AddNoise, ExploredOnly, Juggernaut);
							}
							navigationMap = cell4.ParentZone.NavigationMap;
						}
					}
					if (cell4 != cell2 && (cell4 == null || CloseList.ContainsKey(cell4) || navigationMap[cell4.X, cell4.Y].Weight > MaxWeight))
					{
						continue;
					}
					if (drawPathfinder)
					{
						screenBuffer.WriteAt(cell4, "&K?").Draw();
					}
					double num5 = cell2.RealDistanceTo(cell4, indefiniteWorld: false);
					if (nCellNav >= 4000)
					{
						bFound = false;
						return;
					}
					if (OpenList.TryGetValue(cell4, out var value))
					{
						double num6 = cellNavigationValue.Cost + (double)navigationMap[cell4.X, cell4.Y].Weight;
						if (Avoid != null)
						{
							for (int j = 0; j < Avoid.Items.Count; j++)
							{
								XRLCore.SortPoint sortPoint = Avoid.Items[j];
								if (sortPoint.X == cell4.X && sortPoint.Y == cell4.Y)
								{
									num6 = 99999.0;
								}
							}
						}
						if (text.Length > 1)
						{
							num6 += 0.001;
						}
						double num7 = num5;
						if (cell4.ParentZone.Z != cell2.ParentZone.Z)
						{
							num7 *= 10.0;
						}
						if (text != cellNavigationValue.Direction)
						{
							num7 += 60.0;
						}
						if (num6 + num7 < value.Total)
						{
							value.Set(num6, num7, null, cell3, text);
						}
					}
					else if (CloseList.TryGetValue(cell4, out value))
					{
						double num8 = cellNavigationValue.Cost + (double)navigationMap[cell4.X, cell4.Y].Weight;
						if (Avoid != null)
						{
							for (int k = 0; k < Avoid.Items.Count; k++)
							{
								XRLCore.SortPoint sortPoint2 = Avoid.Items[k];
								if (sortPoint2.X == cell4.X && sortPoint2.Y == cell4.Y)
								{
									num8 = 99999.0;
								}
							}
						}
						if (text.Length > 1)
						{
							num8 += 0.001;
						}
						double num9 = num5;
						if (cell4.ParentZone.Z != cell2.ParentZone.Z)
						{
							num9 *= 10.0;
						}
						if (num8 + num9 < value.Total)
						{
							value.Set(num8, num9, null, cell3, text);
							OpenList.Add(cell4, value);
							CloseList.Remove(cell4);
						}
					}
					else if (num5 <= (double)num)
					{
						double num10 = cellNavigationValue.Cost + (double)navigationMap[cell4.X, cell4.Y].Weight;
						if (Avoid != null)
						{
							for (int l = 0; l < Avoid.Items.Count; l++)
							{
								XRLCore.SortPoint sortPoint3 = Avoid.Items[l];
								if (sortPoint3.X == cell4.X && sortPoint3.Y == cell4.Y)
								{
									num10 = 99999.0;
								}
							}
						}
						if (text.Length > 1)
						{
							num10 += 0.001;
						}
						double num11 = num5;
						if (cell4.ParentZone.Z != cell2.ParentZone.Z)
						{
							num11 *= 10.0;
						}
						OpenList.Add(cell4, NewCellNav(num10, num11, null, cell3, text));
					}
					if (cell4 != cell2)
					{
						continue;
					}
					bFound = true;
					if (drawPathfinder)
					{
						screenBuffer.WriteAt(cell, "S").WriteAt(cell2, "E").Draw();
					}
					if (text != ".")
					{
						Steps.Add(cell4);
						Directions.Add(text);
					}
					Cell cell5 = cell3;
					while (cell5 != null)
					{
						Steps.Add(cell5);
						if (drawPathfinder)
						{
							screenBuffer.WriteAt(cell5, ".").Draw();
						}
						if (OpenList.ContainsKey(cell5))
						{
							if (OpenList[cell5].Direction != ".")
							{
								Directions.Add(OpenList[cell5].Direction);
							}
							cell5 = OpenList[cell5].Parent;
						}
						else
						{
							if (CloseList[cell5].Direction != ".")
							{
								Directions.Add(CloseList[cell5].Direction);
							}
							cell5 = CloseList[cell5].Parent;
						}
					}
					Directions.Reverse();
					Steps.Reverse();
					if (drawPathfinder && Options.DrawPathfinderHalt)
					{
						screenBuffer.Draw();
						Keyboard.getch();
					}
					return;
				}
				if (!CloseList.ContainsKey(cell3))
				{
					CloseList.Add(cell3, cellNavigationValue);
					OpenList.Remove(cell3);
				}
			}
			bFound = false;
			if (drawPathfinder && Options.DrawPathfinderHalt)
			{
				screenBuffer.Draw();
				Keyboard.getch();
			}
		}
		finally
		{
			OpenList.Clear();
			CloseList.Clear();
			OrderedNavigationValues.Clear();
			for (int m = 0; m <= nCellNav; m++)
			{
				CellNavs[m].Parent = null;
				CellNavs[m].C = null;
			}
		}
	}
}

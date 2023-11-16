using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace Genkit;

public class Pathfinder
{
	public List<PathfinderNode> addNodes = new List<PathfinderNode>();

	public Dictionary<Location2D, PathfinderNode> nodesByPosition = new Dictionary<Location2D, PathfinderNode>();

	public static CellNavigationValue[] CellNavs = null;

	public static int nCellNav = -1;

	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListSorted = new string[8] { "N", "E", "S", "W", "NW", "NE", "SE", "SW" };

	public static string[] DirectionListU = new string[9] { "U", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListD = new string[9] { "D", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListOrdinalOnly = new string[4] { "E", "S", "N", "W" };

	public int[,] CurrentNavigationMap;

	public const int MAXWEIGHT = 9999;

	public bool bFound;

	public List<string> Directions = new List<string>();

	public List<PathfinderNode> Steps = new List<PathfinderNode>(2000);

	public Pathfinder(int width, int height)
	{
		CurrentNavigationMap = new int[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Location2D location2D = Location2D.get(i, j);
				PathfinderNode pathfinderNode = new PathfinderNode
				{
					pos = location2D
				};
				nodesByPosition.Add(location2D, pathfinderNode);
				addNodes.Add(pathfinderNode);
			}
		}
		for (int k = 0; k < width; k++)
		{
			for (int l = 0; l < height; l++)
			{
				if (k > 0)
				{
					nodesByPosition[Location2D.get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.get(k - 1, l)]);
					nodesByPosition[Location2D.get(k, l)].nodesByDirection.Add("W", nodesByPosition[Location2D.get(k - 1, l)]);
				}
				if (k < width - 1)
				{
					nodesByPosition[Location2D.get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.get(k + 1, l)]);
					nodesByPosition[Location2D.get(k, l)].nodesByDirection.Add("E", nodesByPosition[Location2D.get(k + 1, l)]);
				}
				if (l > 0)
				{
					nodesByPosition[Location2D.get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.get(k, l - 1)]);
					nodesByPosition[Location2D.get(k, l)].nodesByDirection.Add("N", nodesByPosition[Location2D.get(k, l - 1)]);
				}
				if (l < height - 1)
				{
					nodesByPosition[Location2D.get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.get(k, l + 1)]);
					nodesByPosition[Location2D.get(k, l)].nodesByDirection.Add("S", nodesByPosition[Location2D.get(k, l + 1)]);
				}
			}
		}
	}

	public static void Initalize()
	{
		CellNavs = new CellNavigationValue[16800];
		for (int i = 0; i < 16800; i++)
		{
			CellNavs[i] = new CellNavigationValue(0, 0, null, null, null);
		}
	}

	public CellNavigationValue NewCellNav(int g, int h, PathfinderNode cCell, PathfinderNode pParent, string pDirection)
	{
		if (nCellNav >= 16799)
		{
			return null;
		}
		nCellNav++;
		return CellNavs[nCellNav].Set(g, h, cCell, pParent, pDirection);
	}

	public void setWeightsFromGrid<T>(Grid<T> grid, Func<int, int, T, int> weightFunc)
	{
		grid.forEach(delegate(int x, int y, T c)
		{
			CurrentNavigationMap[x, y] = weightFunc(x, y, c);
		});
	}

	private int CellDistance(PathfinderNode c1, PathfinderNode c2)
	{
		int val = Math.Abs(c1.X - c2.X);
		int val2 = Math.Abs(c1.Y - c2.Y);
		return Math.Max(val, val2);
	}

	public bool FindPath(Location2D start, Location2D end, bool bDisplay = false, bool bOrdinalDirectionsOnly = false, int MaxDistance = 9999, bool shuffleDirections = false)
	{
		return FindPath(nodesByPosition[start], nodesByPosition[end], bDisplay, bOrdinalDirectionsOnly, MaxDistance, shuffleDirections);
	}

	public bool FindPath(PathfinderNode start, PathfinderNode finish, bool bDisplay = true, bool bOrdinalDirectionsOnly = false, int MaxDistance = 9999, bool shuffleDirections = false)
	{
		if (CellNavs == null)
		{
			Initalize();
		}
		nCellNav = 0;
		ScreenBuffer screenBuffer = Popup._ScreenBuffer;
		Dictionary<PathfinderNode, CellNavigationValue> dictionary = new Dictionary<PathfinderNode, CellNavigationValue>();
		Dictionary<PathfinderNode, CellNavigationValue> dictionary2 = new Dictionary<PathfinderNode, CellNavigationValue>();
		Steps.Clear();
		Directions.Clear();
		dictionary.Add(start, new CellNavigationValue(0, 0, null, null, "."));
		List<string> list = ((!bOrdinalDirectionsOnly) ? new List<string>(DirectionListSorted) : new List<string>(DirectionListOrdinalOnly));
		while (dictionary.Count > 0)
		{
			PathfinderNode pathfinderNode = null;
			int num = 999999999;
			foreach (PathfinderNode key in dictionary.Keys)
			{
				if (dictionary[key].EstimatedTotalCost < num)
				{
					num = dictionary[key].EstimatedTotalCost;
					pathfinderNode = key;
				}
			}
			if (bDisplay)
			{
				screenBuffer.Goto(pathfinderNode.X, pathfinderNode.Y);
				screenBuffer.Write("&Y!");
				XRLCore._Console.DrawBuffer(screenBuffer);
			}
			CellNavigationValue cellNavigationValue = dictionary[pathfinderNode];
			if (bDisplay)
			{
				screenBuffer.Goto(start.X, start.Y);
				screenBuffer.Write("&GS");
				screenBuffer.Goto(finish.X, finish.Y);
				screenBuffer.Write("&RE");
				XRLCore._Console.DrawBuffer(screenBuffer);
			}
			if (shuffleDirections)
			{
				list.ShuffleInPlace();
			}
			foreach (string item in list)
			{
				PathfinderNode pathfinderNode2 = null;
				pathfinderNode2 = pathfinderNode.GetNodeFromDirection(item);
				if (pathfinderNode2 != finish && (pathfinderNode2 == null || dictionary2.ContainsKey(pathfinderNode2) || CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y] >= 9999))
				{
					continue;
				}
				if (bDisplay)
				{
					screenBuffer.Goto(pathfinderNode2.X, pathfinderNode2.Y);
					screenBuffer.Write("&K?");
					XRLCore._Console.DrawBuffer(screenBuffer);
				}
				int num2 = CellDistance(finish, pathfinderNode2);
				if (nCellNav >= 4000)
				{
					bFound = false;
					for (int i = 0; i < 4000; i++)
					{
						CellNavs[i].cCell = null;
						CellNavs[i].Parent = null;
					}
					return bFound;
				}
				if (dictionary.ContainsKey(pathfinderNode2))
				{
					int num3 = cellNavigationValue.EstimatedTotalCost + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y] + num2;
					int num4 = num2;
					if (dictionary[pathfinderNode2].EstimatedTotalCost < num3)
					{
						dictionary[pathfinderNode2].Set(cellNavigationValue.EstimatedNodeToGoal + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], num4 + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], null, pathfinderNode, item);
					}
				}
				else if (num2 <= MaxDistance)
				{
					int num5 = num2;
					dictionary.Add(pathfinderNode2, NewCellNav(cellNavigationValue.EstimatedNodeToGoal + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], num5 + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], null, pathfinderNode, item));
				}
				if (pathfinderNode2 != finish)
				{
					continue;
				}
				bFound = true;
				if (bDisplay)
				{
					screenBuffer.Goto(start.X, start.Y);
					screenBuffer.Write("S");
					screenBuffer.Goto(finish.X, finish.Y);
					screenBuffer.Write("E");
				}
				if (item != ".")
				{
					Steps.Add(pathfinderNode2);
					Directions.Add(item);
				}
				PathfinderNode pathfinderNode3 = pathfinderNode;
				while (pathfinderNode3 != null && !Steps.Contains(pathfinderNode3))
				{
					Steps.Add(pathfinderNode3);
					if (bDisplay)
					{
						screenBuffer.Goto(pathfinderNode3.X, pathfinderNode3.Y);
						screenBuffer.Write(".");
						XRLCore._Console.DrawBuffer(screenBuffer);
					}
					if (dictionary.ContainsKey(pathfinderNode3))
					{
						if (dictionary[pathfinderNode3].Direction != ".")
						{
							Directions.Add(dictionary[pathfinderNode3].Direction);
						}
						pathfinderNode3 = dictionary[pathfinderNode3].Parent;
					}
					else
					{
						if (dictionary2[pathfinderNode3].Direction != ".")
						{
							Directions.Add(dictionary2[pathfinderNode3].Direction);
						}
						pathfinderNode3 = dictionary2[pathfinderNode3].Parent;
					}
				}
				Directions.Reverse();
				Steps.Reverse();
				for (int j = 0; j <= nCellNav; j++)
				{
					CellNavs[j].Parent = null;
					CellNavs[j].cCell = null;
				}
				if (bDisplay)
				{
					XRLCore._Console.DrawBuffer(screenBuffer);
					Keyboard.getch();
				}
				return bFound;
			}
			if (!dictionary2.ContainsKey(pathfinderNode))
			{
				dictionary2.Add(pathfinderNode, cellNavigationValue);
				dictionary.Remove(pathfinderNode);
			}
		}
		bFound = false;
		for (int k = 0; k <= nCellNav; k++)
		{
			CellNavs[k].Parent = null;
			CellNavs[k].cCell = null;
		}
		if (bDisplay)
		{
			XRLCore._Console.DrawBuffer(screenBuffer);
			Keyboard.getch();
		}
		return bFound;
	}
}

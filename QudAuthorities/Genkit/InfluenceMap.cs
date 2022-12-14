using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace Genkit;

[Serializable]
public class InfluenceMap
{
	private int Width;

	private int Height;

	public int[,] Walls;

	public int[,] Weights;

	public int[,] WeightedCosts;

	public List<Location2D> Seeds;

	public List<int> SeedGrowthProbability;

	public bool bDraw;

	public List<InfluenceMapRegion> Regions = new List<InfluenceMapRegion>();

	public Dictionary<int, InfluenceMapRegion> SeedToRegionMap = new Dictionary<int, InfluenceMapRegion>();

	private List<Location2D> N = new List<Location2D>();

	public int[,] CostMap;

	public Dictionary<Location2D, int> SeedMap = new Dictionary<Location2D, int>();

	public Dictionary<int, int> SizeMap = new Dictionary<int, int>();

	[NonSerialized]
	private static Queue<Location2D> Frontier = new Queue<Location2D>();

	[NonSerialized]
	private static List<Location2D> SkipRegion = new List<Location2D>();

	public InfluenceMap(int _Width, int _Height)
	{
		Width = _Width;
		Height = _Height;
		Walls = new int[Width, Height];
		CostMap = new int[Width, Height];
		Seeds = new List<Location2D>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				CostMap[i, j] = int.MinValue;
			}
		}
	}

	public InfluenceMap copy()
	{
		InfluenceMap influenceMap = new InfluenceMap(Width, Height);
		if (Walls != null)
		{
			Array.Copy(Walls, influenceMap.Walls, Width * Height);
		}
		if (CostMap != null)
		{
			Array.Copy(CostMap, influenceMap.CostMap, Width * Height);
		}
		if (Weights != null)
		{
			influenceMap.UsingWeights();
			Array.Copy(Weights, influenceMap.Weights, Width * Height);
			Array.Copy(WeightedCosts, influenceMap.WeightedCosts, Width * Height);
		}
		influenceMap.Seeds = new List<Location2D>(Seeds);
		if (SeedGrowthProbability != null)
		{
			influenceMap.SeedGrowthProbability = new List<int>(SeedGrowthProbability);
		}
		influenceMap.bDraw = bDraw;
		return influenceMap;
	}

	public void UsingWeights()
	{
		if (Weights == null)
		{
			Weights = new int[Width, Height];
			WeightedCosts = new int[Width, Height];
		}
	}

	public int AddGlobalRegion()
	{
		InfluenceMapRegion influenceMapRegion = new InfluenceMapRegion(Regions.Count, this);
		for (int i = 0; i < Regions.Count; i++)
		{
			influenceMapRegion.Cells.AddRange(Regions[i].Cells);
			influenceMapRegion.BorderCells.AddRange(Regions[i].BorderCells);
			influenceMapRegion.NonBorderCells.AddRange(Regions[i].NonBorderCells);
			influenceMapRegion.AdjacentRegions = new List<InfluenceMapRegion>();
			if (Regions[i].BoundingBox.x1 < influenceMapRegion.BoundingBox.x1)
			{
				influenceMapRegion.BoundingBox.x1 = Regions[i].BoundingBox.x1;
			}
			if (Regions[i].BoundingBox.x2 > influenceMapRegion.BoundingBox.x2)
			{
				influenceMapRegion.BoundingBox.x2 = Regions[i].BoundingBox.x2;
			}
			if (Regions[i].BoundingBox.y1 < influenceMapRegion.BoundingBox.y1)
			{
				influenceMapRegion.BoundingBox.y1 = Regions[i].BoundingBox.y1;
			}
			if (Regions[i].BoundingBox.y2 > influenceMapRegion.BoundingBox.y2)
			{
				influenceMapRegion.BoundingBox.y2 = Regions[i].BoundingBox.y2;
			}
			influenceMapRegion.Seed = Regions.Count;
		}
		Regions.Add(influenceMapRegion);
		return Regions.Count - 1;
	}

	public int FindClosestSeedToInList(int seed, List<int> targetSeeds)
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < targetSeeds.Count; i++)
		{
			if (num > Seeds[seed].SquareDistance(Seeds[targetSeeds[i]]))
			{
				num = Seeds[seed].SquareDistance(Seeds[targetSeeds[i]]);
				result = targetSeeds[i];
			}
		}
		return result;
	}

	public int FindClosestSeedTo(Location2D where, Func<InfluenceMapRegion, bool> regionFilter = null)
	{
		int num = int.MaxValue;
		int result = 0;
		for (int i = 0; i < Seeds.Count; i++)
		{
			if (Seeds[i].Distance(where) < num && (regionFilter == null || regionFilter(Regions[i])))
			{
				result = i;
				num = Seeds[i].Distance(where);
			}
		}
		return result;
	}

	public int FindClosestSeedToInList(Location2D location, List<int> targetSeeds)
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < targetSeeds.Count; i++)
		{
			if (num > location.SquareDistance(Seeds[targetSeeds[i]]))
			{
				num = location.SquareDistance(Seeds[targetSeeds[i]]);
				result = targetSeeds[i];
			}
		}
		return result;
	}

	public void ClearSeeds()
	{
		Seeds.Clear();
		Regions.Clear();
		SeedToRegionMap.Clear();
		N.Clear();
		Weights = null;
		WeightedCosts = null;
	}

	private List<Location2D> LoadNeighbors(Location2D P)
	{
		N.Clear();
		if (P.x > 0 && Walls[P.x - 1, P.y] == 0)
		{
			N.Add(Location2D.get(P.x - 1, P.y));
		}
		if (P.x < Width - 1 && Walls[P.x + 1, P.y] == 0)
		{
			N.Add(Location2D.get(P.x + 1, P.y));
		}
		if (P.y > 0 && Walls[P.x, P.y - 1] == 0)
		{
			N.Add(Location2D.get(P.x, P.y - 1));
		}
		if (P.y < Height - 1 && Walls[P.x, P.y + 1] == 0)
		{
			N.Add(Location2D.get(P.x, P.y + 1));
		}
		if (P.x > 0 && P.y > 0 && Walls[P.x - 1, P.y - 1] == 0)
		{
			N.Add(Location2D.get(P.x - 1, P.y - 1));
		}
		if (P.x > 0 && P.y < Height - 1 && Walls[P.x - 1, P.y + 1] == 0)
		{
			N.Add(Location2D.get(P.x - 1, P.y + 1));
		}
		if (P.x < Width - 1 && P.y > 0 && Walls[P.x + 1, P.y - 1] == 0)
		{
			N.Add(Location2D.get(P.x + 1, P.y - 1));
		}
		if (P.x < Width - 1 && P.y < Height - 1 && Walls[P.x + 1, P.y + 1] == 0)
		{
			N.Add(Location2D.get(P.x + 1, P.y + 1));
		}
		return N;
	}

	public int GetSeedAt(Location2D P)
	{
		if (SeedMap.ContainsKey(P))
		{
			return SeedMap[P];
		}
		return -1;
	}

	public int GetSeedAt(Point2D P)
	{
		return GetSeedAt(P.location);
	}

	public int LargestSeed()
	{
		int num = -1;
		int result = 0;
		foreach (int key in SizeMap.Keys)
		{
			if (SizeMap[key] > num)
			{
				result = key;
				num = SizeMap[key];
			}
		}
		return result;
	}

	public int LargestSize()
	{
		return SizeMap[LargestSeed()];
	}

	public bool HasUnreached()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (Walls[i, j] == 0 && CostMap[i, j] == int.MinValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddSeedAtMaxima()
	{
		int num = 0;
		Location2D location2D = Location2D.get(0, 0);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (CostMap[i, j] != int.MinValue && CostMap[i, j] > num)
				{
					num = CostMap[i, j];
					location2D = Location2D.get(i, j);
				}
			}
		}
		AddSeed(location2D.x, location2D.y);
	}

	public void MoveSeedsToCenters(bool bDraw = false)
	{
		int[] array = new int[Seeds.Count];
		int[] array2 = new int[Seeds.Count];
		int[] array3 = new int[Seeds.Count];
		for (int i = 0; i < Seeds.Count; i++)
		{
			array[i] = 0;
			array2[i] = 0;
			array3[i] = 0;
		}
		for (int j = 0; j < Width; j++)
		{
			for (int k = 0; k < Height; k++)
			{
				if (SeedMap.ContainsKey(Location2D.get(j, k)))
				{
					int num = SeedMap[Location2D.get(j, k)];
					array[num] += j;
					array2[num] += k;
					array3[num]++;
				}
			}
		}
		for (int l = 0; l < Seeds.Count; l++)
		{
			if (array3[l] > 0)
			{
				array[l] /= array3[l];
				array2[l] /= array3[l];
				array3[l] = int.MaxValue;
			}
		}
		for (int m = 0; m < Width; m++)
		{
			for (int n = 0; n < Height; n++)
			{
				if (SeedMap.ContainsKey(Location2D.get(m, n)))
				{
					int num2 = SeedMap[Location2D.get(m, n)];
					int num3 = (m - array[num2]) * (m - array[num2]) + (n - array2[num2]) * (n - array2[num2]);
					if (num3 < array3[num2])
					{
						Seeds[num2] = Location2D.get(m, n);
						array3[num2] = num3;
					}
				}
			}
		}
		Recalculate();
	}

	public void SeedAllUnseeded(bool bDraw = false, bool bRecalculate = true)
	{
		RecalculateCostOnly(bDraw);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (Walls[i, j] == 0 && CostMap[i, j] == int.MinValue)
				{
					AddSeed(Location2D.get(i, j));
					RecalculateCostOnly(bDraw);
				}
			}
		}
		if (Seeds.Count > 0 && bRecalculate)
		{
			Recalculate();
		}
	}

	public void AddSeedAtTrueRandom(int MinimumDistance = 4)
	{
		List<Location2D> list = new List<Location2D>();
		list.Clear();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (Walls[i, j] == 0 && !SeedMap.ContainsKey(Location2D.get(i, j)))
				{
					list.Add(Location2D.get(i, j));
				}
			}
		}
		if (list.Count != 0)
		{
			AddSeed(Calc.Random(list));
			return;
		}
		List<Location2D> list2 = new List<Location2D>();
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				if (CostMap[k, l] != int.MinValue)
				{
					list2.Add(Location2D.get(k, l));
				}
			}
		}
		Algorithms.RandomShuffleInPlace(list2);
		foreach (Location2D item in list2)
		{
			if (CostMap[item.x, item.y] > MinimumDistance)
			{
				AddSeed(item.x, item.y);
				return;
			}
		}
		if (Seeds.Count > 0)
		{
			AddSeedAtMaximaInLargestSeed(Break: true);
		}
	}

	public void AddSeedAtRandom(int MinimumDistance = 4)
	{
		List<Location2D> list = new List<Location2D>();
		while (true)
		{
			list.Clear();
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					if (Walls[i, j] == 0 && !SeedMap.ContainsKey(Location2D.get(i, j)))
					{
						list.Add(Location2D.get(i, j));
					}
				}
			}
			if (list.Count == 0)
			{
				break;
			}
			AddSeed(Calc.Random(list));
		}
		List<Location2D> list2 = new List<Location2D>();
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				if (CostMap[k, l] != int.MinValue)
				{
					list2.Add(Location2D.get(k, l));
				}
			}
		}
		Algorithms.RandomShuffleInPlace(list2);
		foreach (Location2D item in list2)
		{
			if (CostMap[item.x, item.y] > MinimumDistance)
			{
				AddSeed(item.x, item.y);
				return;
			}
		}
		if (Seeds.Count > 0)
		{
			AddSeedAtMaximaInLargestSeed(Break: true);
		}
	}

	public void AddSeedAtMaximaInLargestSeed(bool Break = false)
	{
		int num = 0;
		Location2D location2D = Location2D.get(0, 0);
		int num2 = LargestSeed();
		if (Seeds.Count == 0)
		{
			if (!Break)
			{
				AddSeedAtRandom();
			}
			return;
		}
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Location2D location2D2 = Location2D.get(i, j);
				if (CostMap[location2D2.x, location2D2.y] != int.MinValue && GetSeedAt(location2D2) == num2 && CostMap[location2D2.x, location2D2.y] > num)
				{
					num = CostMap[location2D2.x, location2D2.y];
					location2D = location2D2;
				}
			}
		}
		AddSeed(location2D.x, location2D.y);
	}

	public void Recalculate(bool bDraw = false)
	{
		if (Seeds.Count == 0)
		{
			throw new Exception("No seeds were defined");
		}
		if (Seeds.Count != Seeds.Distinct().Count())
		{
			Debug.LogWarning("Seeds list contained dupes");
			Seeds = Seeds.Distinct().ToList();
		}
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				CostMap[i, j] = int.MinValue;
			}
		}
		if (WeightedCosts != null)
		{
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					WeightedCosts[k, l] = int.MaxValue;
				}
			}
		}
		SeedMap.Clear();
		SizeMap.Clear();
		Frontier.Clear();
		SkipRegion.Clear();
		SeedToRegionMap.Clear();
		Regions.Clear();
		int num = 0;
		foreach (Location2D seed in Seeds)
		{
			Frontier.Enqueue(seed);
			CostMap[seed.x, seed.y] = 0;
			if (WeightedCosts != null)
			{
				WeightedCosts[seed.x, seed.y] = 0;
			}
			SeedMap.Add(seed, num);
			SizeMap.Add(num, 0);
			Regions.Add(new InfluenceMapRegion(num, this));
			Regions[num].AddCell(seed);
			SeedToRegionMap.Add(num, Regions[num]);
			num++;
		}
		while (Frontier.Count > 0)
		{
			Location2D location2D;
			while (true)
			{
				location2D = Frontier.Dequeue();
				SeedToRegionMap[SeedMap[location2D]].Size++;
				LoadNeighbors(location2D);
				if (SeedGrowthProbability == null || Stat.Random(1, 1000) <= SeedGrowthProbability[SeedMap[location2D]])
				{
					break;
				}
				Frontier.Enqueue(location2D);
			}
			for (int m = 0; m < N.Count; m++)
			{
				Location2D location2D2 = N[m];
				bool flag = false;
				if (CostMap[location2D2.x, location2D2.y] == int.MinValue)
				{
					CostMap[location2D2.x, location2D2.y] = CostMap[location2D.x, location2D.y] + 1;
					if (WeightedCosts != null)
					{
						WeightedCosts[location2D2.x, location2D2.y] = WeightedCosts[location2D.x, location2D.y] + Math.Max(Weights[location2D2.x, location2D2.y], 1);
					}
					SeedMap.Add(location2D2, SeedMap[location2D]);
					Regions[SeedMap[location2D]].AddCell(location2D2);
					Regions[SeedMap[location2D]].NonBorderCells.Add(location2D2);
					SizeMap[SeedMap[location2D]] = SizeMap[SeedMap[location2D]] + 1;
					Frontier.Enqueue(location2D2);
					flag = true;
				}
				else if (!SkipRegion.Contains(location2D2) && SeedToRegionMap[SeedMap[location2D2]] != SeedToRegionMap[SeedMap[location2D]])
				{
					InfluenceMapRegion item = SeedToRegionMap[SeedMap[location2D2]];
					if (!SeedToRegionMap[SeedMap[location2D]].AdjacentRegions.Contains(item))
					{
						SeedToRegionMap[SeedMap[location2D]].AdjacentRegions.Add(item);
					}
					if (!SeedToRegionMap[SeedMap[location2D]].BorderCells.Contains(location2D))
					{
						SeedToRegionMap[SeedMap[location2D]].BorderCells.Add(location2D);
					}
				}
				if (WeightedCosts == null)
				{
					continue;
				}
				int num2 = WeightedCosts[location2D.x, location2D.y] + Math.Max(Weights[location2D2.x, location2D2.y], 1);
				if (num2 < WeightedCosts[location2D2.x, location2D2.y])
				{
					WeightedCosts[location2D2.x, location2D2.y] = num2;
					if (!flag)
					{
						Frontier.Enqueue(location2D2);
						SkipRegion.Add(location2D2);
					}
				}
			}
			if (bDraw)
			{
				Draw(bDrawConnectionMap: false, bWait: false);
			}
		}
		if (bDraw)
		{
			Draw(bDrawConnectionMap: false);
		}
		SkipRegion.Clear();
	}

	public int GetCostAtPoint(Point2D P)
	{
		return GetCostAtPoint(P.location);
	}

	public int GetCostAtPoint(Location2D P)
	{
		int result = int.MaxValue;
		if (P == null)
		{
			return result;
		}
		if (P.x < 0 || P.x >= Width || P.y < 0 || P.y >= Height)
		{
			return int.MaxValue;
		}
		if (CostMap[P.x, P.y] == int.MinValue)
		{
			return int.MaxValue;
		}
		return CostMap[P.x, P.y];
	}

	public int GetCostFromPointDirection(Location2D P, string Dir)
	{
		return GetCostAtPoint(P.FromDirection(Dir));
	}

	public int GetWeightedCostAtPoint(Point2D P)
	{
		return GetWeightedCostAtPoint(P.location);
	}

	public int GetWeightedCostAtPoint(Location2D P)
	{
		if (P == null || WeightedCosts == null)
		{
			return int.MaxValue;
		}
		if (P.x < 0 || P.x >= Width || P.y < 0 || P.y >= Height)
		{
			return int.MaxValue;
		}
		return WeightedCosts[P.x, P.y];
	}

	public int GetWeightedCostFromPointDirection(Location2D P, string Dir)
	{
		return GetWeightedCostAtPoint(P.FromDirection(Dir));
	}

	public string GetLowestCostDirectionFrom(Point2D P)
	{
		return GetLowestCostDirectionFrom(P.location);
	}

	public string GetLowestCostDirectionFrom(Location2D P)
	{
		if (P == null)
		{
			return ".";
		}
		string result = ".";
		int num = int.MaxValue;
		for (int i = 0; i < Location2D.Directions.Length; i++)
		{
			int costFromPointDirection = GetCostFromPointDirection(P, Location2D.Directions[i]);
			if (costFromPointDirection < num)
			{
				result = Location2D.Directions[i];
				num = costFromPointDirection;
			}
		}
		return result;
	}

	public string GetLowestWeightedCostDirectionFrom(Point2D P)
	{
		return GetLowestWeightedCostDirectionFrom(P.location);
	}

	public string GetLowestWeightedCostDirectionFrom(Location2D P)
	{
		if (P == null)
		{
			return ".";
		}
		string result = ".";
		int num = int.MaxValue;
		for (int i = 0; i < Location2D.Directions.Length; i++)
		{
			int weightedCostFromPointDirection = GetWeightedCostFromPointDirection(P, Location2D.Directions[i]);
			if (weightedCostFromPointDirection < num)
			{
				result = Location2D.Directions[i];
				num = weightedCostFromPointDirection;
			}
		}
		return result;
	}

	public void RecalculateCostOnly(bool bDraw = false)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				CostMap[i, j] = int.MinValue;
			}
		}
		if (WeightedCosts != null)
		{
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					WeightedCosts[k, l] = int.MaxValue;
				}
			}
		}
		if (Seeds.Count == 0)
		{
			return;
		}
		foreach (Location2D seed in Seeds)
		{
			Frontier.Enqueue(seed);
			CostMap[seed.x, seed.y] = 0;
			if (WeightedCosts != null)
			{
				WeightedCosts[seed.x, seed.y] = 0;
			}
		}
		while (Frontier.Count > 0)
		{
			Location2D location2D = Frontier.Dequeue();
			LoadNeighbors(location2D);
			for (int m = 0; m < N.Count; m++)
			{
				Location2D location2D2 = N[m];
				bool flag = false;
				if (CostMap[location2D2.x, location2D2.y] == int.MinValue)
				{
					CostMap[location2D2.x, location2D2.y] = CostMap[location2D.x, location2D.y] + 1;
					Frontier.Enqueue(location2D2);
					flag = true;
				}
				if (WeightedCosts == null)
				{
					continue;
				}
				int num = WeightedCosts[location2D.x, location2D.y] + Math.Max(Weights[location2D2.x, location2D2.y], 1);
				if (num < WeightedCosts[location2D2.x, location2D2.y])
				{
					WeightedCosts[location2D2.x, location2D2.y] = num;
					if (!flag)
					{
						Frontier.Enqueue(location2D2);
					}
				}
			}
		}
		if (bDraw || Options.DrawInfluenceMaps)
		{
			DrawCostsOnly();
		}
	}

	public bool DrawLine(int x0, int y0, int x1, int y1, ScreenBuffer SB)
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
				if (flag)
				{
					SB.Goto(num5, num7);
					SB.Write("#");
				}
				else
				{
					SB.Goto(num7, num5);
					SB.Write("#");
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
				if (flag)
				{
					SB.Goto(num5, i);
					SB.Write("#");
				}
				else
				{
					SB.Goto(i, num5);
					SB.Write("#");
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

	public void DrawProperty(List<string> properties, Dictionary<InfluenceMapRegion, string> propertyMap, bool bDrawConnectionMap = true)
	{
		ScreenBuffer scrapBuffer = Popup.ScrapBuffer;
		List<string> list = new List<string>();
		list.Add("&G");
		list.Add("&R");
		list.Add("&B");
		list.Add("&M");
		list.Add("&W");
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				scrapBuffer.Goto(i, j);
				if (CostMap[i, j] != int.MinValue)
				{
					int num = CostMap[i, j];
					int key = SeedMap[Location2D.get(i, j)];
					string text = ((!propertyMap.ContainsKey(SeedToRegionMap[key])) ? "&K" : list[properties.IndexOf(propertyMap[SeedToRegionMap[key]])]);
					string text2 = num.ToString("X");
					scrapBuffer.Write(text + text2);
				}
				else
				{
					scrapBuffer.Write("&K.");
				}
			}
		}
		if (bDrawConnectionMap)
		{
			for (int k = 0; k < Seeds.Count; k++)
			{
				foreach (InfluenceMapRegion adjacentRegion in Regions[k].AdjacentRegions)
				{
					DrawLine(Seeds[k].x, Seeds[k].y, Seeds[adjacentRegion.Seed].x, Seeds[adjacentRegion.Seed].y, scrapBuffer);
				}
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		Keyboard.getch();
	}

	public void Draw(bool bDrawConnectionMap = true, bool bWait = true)
	{
		ScreenBuffer scrapBuffer = Popup.ScrapBuffer;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				scrapBuffer.Goto(i, j);
				if (CostMap[i, j] != int.MinValue)
				{
					int num = CostMap[i, j];
					int num2 = SeedMap[Location2D.get(i, j)];
					string text = "&y";
					int num3 = num2 % 15;
					if (num3 == 0)
					{
						text = "&y";
					}
					if (num3 == 1)
					{
						text = "&R";
					}
					if (num3 == 2)
					{
						text = "&G";
					}
					if (num3 == 3)
					{
						text = "&B";
					}
					if (num3 == 4)
					{
						text = "&M";
					}
					if (num3 == 5)
					{
						text = "&C";
					}
					if (num3 == 6)
					{
						text = "&W";
					}
					if (num3 == 7)
					{
						text = "&r";
					}
					if (num3 == 8)
					{
						text = "&g";
					}
					if (num3 == 9)
					{
						text = "&b";
					}
					if (num3 == 10)
					{
						text = "&m";
					}
					if (num3 == 11)
					{
						text = "&c";
					}
					if (num3 == 12)
					{
						text = "&w";
					}
					if (num3 == 13)
					{
						text = "&Y";
					}
					if (num3 == 14)
					{
						text = "&K";
					}
					string text2 = num.ToString("X");
					scrapBuffer.Write(text + text2);
				}
				else if (Walls[i, j] > 0)
				{
					scrapBuffer.Write("&r.");
				}
				else
				{
					scrapBuffer.Write("&K.");
				}
			}
		}
		if (bDrawConnectionMap)
		{
			for (int k = 0; k < Seeds.Count; k++)
			{
				foreach (InfluenceMapRegion adjacentRegion in Regions[k].AdjacentRegions)
				{
					DrawLine(Seeds[k].x, Seeds[k].y, Seeds[adjacentRegion.Seed].x, Seeds[adjacentRegion.Seed].y, scrapBuffer);
				}
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		if (bWait)
		{
			Keyboard.getch();
		}
	}

	public void DrawCostsOnly(bool wait = false)
	{
		ScreenBuffer scrapBuffer = Popup.ScrapBuffer;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				scrapBuffer.Goto(i, j);
				if (CostMap[i, j] != int.MinValue)
				{
					int num = CostMap[i, j];
					string text = "&B";
					string text2 = num.ToString("X");
					scrapBuffer.Write(text + text2);
				}
				else
				{
					scrapBuffer.Write("&K.");
				}
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		if (wait)
		{
			Keyboard.getch();
		}
	}

	public void ClearWalls()
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Walls[i, j] = 0;
			}
		}
	}

	public void AddSeed(Location2D P, bool bRecalculate = true)
	{
		AddSeed(P.x, P.y, bRecalculate);
	}

	public void AddSeed(int x, int y, bool bRecalculate = true)
	{
		for (int i = 0; i < Seeds.Count; i++)
		{
			if (Seeds[i].x == x && Seeds[i].y == y)
			{
				if (bRecalculate)
				{
					Recalculate();
				}
				return;
			}
		}
		Seeds.Add(Location2D.get(x, y));
		if (bRecalculate)
		{
			Recalculate();
		}
	}
}

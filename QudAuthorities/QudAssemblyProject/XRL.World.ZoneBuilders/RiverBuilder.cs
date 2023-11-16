using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class RiverBuilder
{
	public string Puddle = "SaltyWaterDeepPool";

	public bool Pairs;

	public bool HardClear;

	public RiverBuilder()
	{
	}

	public RiverBuilder(bool hardClear, string Puddle = "SaltyWaterDeepPool")
	{
		this.Puddle = Puddle;
		HardClear = hardClear;
	}

	public bool BuildZone(Zone Z)
	{
		if (Z.GetTerrainObject().Blueprint == "TerrainSaltdunes" || Z.GetTerrainObject().Blueprint == "TerrainSaltdunes2" || Z.GetTerrainObject().Blueprint == "TerrainTremblezone")
		{
			Puddle = "SaltDeepPool";
		}
		if (Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainFungalOuter"))
		{
			Puddle = "ProteanDeepPool";
		}
		if (Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainPalladiumReef") || Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainLakeHinnom"))
		{
			Puddle = "AlgalWaterDeepPool";
		}
		List<CachedZoneConnection> list = new List<CachedZoneConnection>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.Contains("River"))
			{
				list.Add(item);
			}
		}
		int num = 40;
		int num2 = 20;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("River"))
			{
				if (zoneConnection.Type.Contains("Start"))
				{
					num = zoneConnection.X;
					num2 = zoneConnection.Y;
				}
				list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, zoneConnection.Type, null));
			}
		}
		bool num3 = list.Count == 1;
		if (list.Count <= 1)
		{
			num = Stat.Random(5, 75);
			num2 = Stat.Random(5, 20);
		}
		else
		{
			num = list[0].X;
			num2 = list[0].Y;
		}
		if (Z.BuildTries > 5 || Pairs)
		{
			GameObjectFactory.Factory.CreateObject("Drillbot");
		}
		if (num3)
		{
			CellularGrid cellularGrid = new CellularGrid();
			cellularGrid.SeedBorders = false;
			cellularGrid.Passes = 4;
			cellularGrid.SeedChance = 40;
			cellularGrid.Generate(Stat.Rand, 80, 30);
			for (int i = 1; i < Z.Width - 1; i++)
			{
				for (int j = 1; j < Z.Height - 1; j++)
				{
					if (cellularGrid.cells[i, j] == 1)
					{
						Z.ReachableMap[i, j] = true;
						if (HardClear)
						{
							Z.GetCell(i, j).Clear();
						}
						else
						{
							Z.GetCell(i, j).ClearObjectsWithTag("Wall");
						}
						Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
					}
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			if (num == list[k].X && num2 == list[k].Y)
			{
				continue;
			}
			FastNoise pathNoise = new FastNoise();
			pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
			pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
			pathNoise.SetFractalOctaves(4);
			pathNoise.SetFrequency(0.1f);
			Pathfinder pathfinder = Z.getPathfinder(delegate(int x, int y, Cell c)
			{
				int num4 = 0;
				num4 = (int)(Math.Abs(pathNoise.GetNoise((x + Z.wX * 80) / 3, y + Z.wY * 25)) * 190f);
				return Z.GetCell(x, y).HasWall() ? (20 + num4) : num4;
			});
			if (pathfinder.FindPath(Location2D.get(num, num2), Location2D.get(list[k].X, list[k].Y), bDisplay: false, bOrdinalDirectionsOnly: true, 24300, shuffleDirections: true))
			{
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell = Z.GetCell(step.X, step.Y);
					Z.ReachableMap[step.X, step.Y] = true;
					if (HardClear)
					{
						cell.Clear();
					}
					else
					{
						cell.ClearTerrain();
					}
					cell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
				}
			}
			foreach (PathfinderNode step2 in pathfinder.Steps)
			{
				Cell cell2 = Z.GetCell(step2.X, step2.Y);
				foreach (Cell localAdjacentCell in cell2.GetLocalAdjacentCells(2))
				{
					if (localAdjacentCell.CosmeticDistanceTo(cell2.Pos2D) > 1)
					{
						continue;
					}
					Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
					if (HardClear)
					{
						localAdjacentCell.Clear();
						localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
						continue;
					}
					localAdjacentCell.ClearTerrain();
					if (localAdjacentCell.IsEmpty())
					{
						localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
					}
				}
			}
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class JoppaOutskirtsRuins : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(30, 60), Stat.Random(50, 70), Stat.Random(125, 135), 0, 10, 0, 1, list, 5);
		if (Watervine.WatervineNoise == null)
		{
			List<PerlinNoise2Df> list2 = null;
			Random seededRand = GetSeededRand("WatervineNoise");
			list2 = new List<PerlinNoise2Df>();
			list2.Add(new PerlinNoise2Df(8, 0.86f, seededRand));
			list2.Add(new PerlinNoise2Df(32, 0.72f, seededRand));
			list2.Add(new PerlinNoise2Df(64, 0.6f, seededRand));
			list2.Add(new PerlinNoise2Df(128, 0.48f, seededRand));
			list2.Add(new PerlinNoise2Df(300, 0.36f, seededRand));
			list2.Add(new PerlinNoise2Df(600, 0.12f, seededRand));
			list2.Add(new PerlinNoise2Df(1200, 0.06f, seededRand));
			Watervine.WatervineNoise = PerlinNoise2Df.sumNoiseFunctions(1200, 375, 0, 0, list2, 0.5f);
			Watervine.WatervineNoise = PerlinNoise2Df.Smooth(Watervine.WatervineNoise, 1200, 375, 1);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		double[,] array = new double[Z.Width, Z.Height];
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				array[i, j] = 0.0;
			}
		}
		if (Z.X == 0 && Z.wX > 0 && !ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld()).Contains("TerrainSaltmarsh"))
		{
			for (int k = 0; k < Z.Height; k++)
			{
				for (int m = 0; m < 10; m++)
				{
					array[m, k] = -1.0 + 0.05 * (double)m;
				}
			}
		}
		if (Z.X == 2 && Z.wX < 79)
		{
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(Z.wX + 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone.Contains("TerrainSaltmarsh") && !objectTypeForZone.Contains("TerrainWatervine"))
			{
				for (int n = 0; n < Z.Height; n++)
				{
					for (int num3 = Z.Width - 10; num3 < Z.Width; num3++)
					{
						array[num3, n] = -1.0 + 0.05 * (double)(Z.Width - num3);
					}
				}
			}
		}
		if (Z.Y == 0 && Z.wY > 0)
		{
			string objectTypeForZone2 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY - 1, Z.GetZoneWorld());
			if (!objectTypeForZone2.Contains("TerrainSaltmarsh") && !objectTypeForZone2.Contains("TerrainWatervine"))
			{
				for (int num4 = 0; num4 < 10; num4++)
				{
					for (int num5 = 0; num5 < Z.Width; num5++)
					{
						array[num5, num4] = -1.0 + 0.05 * (double)num4;
					}
				}
			}
		}
		if (Z.Y == 2 && Z.wY < 24)
		{
			string objectTypeForZone3 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY + 1, Z.GetZoneWorld());
			if (!objectTypeForZone3.Contains("TerrainSaltmarsh") && !objectTypeForZone3.Contains("TerrainWatervine"))
			{
				for (int num6 = Z.Height - 10; num6 < Z.Height; num6++)
				{
					for (int num7 = 0; num7 < Z.Width; num7++)
					{
						array[num7, num6] = -1.0 + 0.05 * (double)(Z.Height - num6);
					}
				}
			}
		}
		if (Z.X == 0 && Z.wX > 0)
		{
			string objectTypeForZone4 = ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone4.Contains("TerrainSaltmarsh") && !objectTypeForZone4.Contains("TerrainWatervine"))
			{
				for (int num8 = 0; num8 < Z.Height; num8++)
				{
					for (int num9 = 0; num9 < 10; num9++)
					{
						array[num9, num8] = -1.0 + 0.1 * (double)num9;
					}
				}
			}
		}
		for (int num10 = 0; num10 < Z.Width; num10++)
		{
			for (int num11 = 0; num11 < Z.Height; num11++)
			{
				double num12 = (double)Watervine.WatervineNoise[num10 + num, num11 + num2] + array[num10, num11];
				if (noiseMap.Noise[num10, num11] >= 5)
				{
					Z.GetCell(num10, num11).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if ((double)noiseMap.Noise[num10, num11] >= 3.5)
				{
					Z.GetCell(num10, num11).AddObject(GameObjectFactory.Factory.CreateObject("Watervine"));
				}
				else if (num12 >= 0.8)
				{
					Z.GetCell(num10, num11).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num12 >= 0.7)
				{
					if (Stat.Random(1, 100) <= 98)
					{
						Z.GetCell(num10, num11).AddObject(GameObjectFactory.Factory.CreateObject("Watervine"));
					}
				}
				else
				{
					Z.GetCell(num10, num11).SetReachable(State: true);
				}
			}
		}
		if (Z.X == 1 && Z.Y == 0)
		{
			InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.RandomPointFurtherThan4, 600);
			if (influenceMap.Regions.Count > 0)
			{
				InfluenceMapRegion randomElement = influenceMap.Regions.GetRandomElement();
				Rect2D rect2D = GridTools.MaxRectByArea(randomElement.GetGrid()).Translate(randomElement.BoundingBox.UpperLeft);
				if (rect2D.x1 == 0)
				{
					rect2D = new Rect2D(1, rect2D.y1, rect2D.x2, rect2D.y2);
				}
				if (rect2D.x2 == Z.Width - 1)
				{
					rect2D = new Rect2D(rect2D.x1, rect2D.y1, Z.Width - 2, rect2D.y2);
				}
				if (rect2D.y1 == 0)
				{
					rect2D = new Rect2D(rect2D.x1, 1, rect2D.x2, rect2D.y2);
				}
				if (rect2D.y2 == Z.Height - 1)
				{
					rect2D = new Rect2D(rect2D.x1, rect2D.y1, rect2D.x2, Z.Height - 2);
				}
				string wall = "?";
				if (randomElement.Center.x <= 40)
				{
					wall = "E";
				}
				if (randomElement.Center.x >= 40)
				{
					wall = "W";
				}
				if (rect2D.y2 > 16)
				{
					wall = "N";
				}
				if (rect2D.y1 < 8)
				{
					wall = "S";
				}
				if (rect2D.y1 <= 0)
				{
					wall = "S";
				}
				if (rect2D.y2 >= 24)
				{
					wall = "N";
				}
				if (rect2D.y1 <= 0 && rect2D.y2 >= 24)
				{
					if (randomElement.Center.x <= 40)
					{
						wall = "E";
					}
					if (randomElement.Center.x >= 40)
					{
						wall = "W";
					}
				}
				Point2D randomDoorCell = rect2D.GetRandomDoorCell(wall);
				string cellSide = rect2D.GetCellSide(randomDoorCell);
				Rect2D r = rect2D.ReduceBy(0, 0);
				int num13 = 0;
				if (cellSide == "N")
				{
					num13 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
				}
				if (cellSide == "S")
				{
					num13 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
				}
				if (cellSide == "E")
				{
					num13 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
				}
				if (cellSide == "W")
				{
					num13 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
				}
				if (num13 == 0 || num13 == 1)
				{
					r.y2 = r.y1 + 3;
				}
				else
				{
					r.y1 = r.y2 - 3;
				}
				if (num13 == 0 || num13 == 3)
				{
					r.x2 = r.x1 + 3;
				}
				else
				{
					r.x1 = r.x2 - 3;
				}
				ClearRect(Z, r);
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "WoodWall", r);
				Point2D randomDoorCell2 = r.GetRandomDoorCell(cellSide, 1);
				Z.GetCell(randomDoorCell2).Clear();
				Z.GetCell(randomDoorCell2).AddObject("Door");
				int num14 = Stat.Random(3, 10);
				for (int num15 = 0; num15 < num14; num15++)
				{
					ZoneBuilderSandbox.PlaceObjectInRect(Z, rect2D.ReduceBy(1, 1), "Tombstone");
				}
				r.ForEachLocation(delegate(Location2D l)
				{
					if (Stat.Random(1, 100) <= 90)
					{
						Z.GetCell(l).ClearObjectsWithTag("Wall");
					}
				});
				int prefabWidth = 5;
				int prefabHeight = 3;
				List<Cell> cells = Z.GetCells(delegate(Cell c)
				{
					if (c.X > 75)
					{
						return false;
					}
					if (c.Y > 20)
					{
						return false;
					}
					if (!c.HasObjectWithPart("LiquidVolume"))
					{
						return false;
					}
					for (int num18 = 0; num18 < prefabWidth; num18++)
					{
						for (int num19 = 0; num19 < prefabHeight; num19++)
						{
							Cell cellFromOffset = c.GetCellFromOffset(num18, num19);
							if (cellFromOffset == null || !cellFromOffset.HasObjectWithPart("LiquidVolume"))
							{
								return false;
							}
						}
					}
					return true;
				});
				if (cells.Count <= 0)
				{
					cells.Add(Z.GetRandomCell(Math.Max(prefabWidth, prefabHeight)));
				}
				Cell randomElement2 = cells.GetRandomElement();
				for (int num16 = 0; num16 < prefabWidth; num16++)
				{
					for (int num17 = 0; num17 < prefabHeight; num17++)
					{
						randomElement2.GetCellFromOffset(num16, num17).Clear();
					}
				}
				randomElement2.AddObject("JoppaSultanShrine_5x3_ruined");
			}
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using Genkit;
using HistoryKit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class BananaGrove : ZoneBuilderSandbox
{
	private static List<PerlinNoise2D> NoiseFunctions;

	private static double[,] BananaGroveNoise;

	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	private const int DACCA_CHANCE = 5;

	public bool Underground;

	public static void Save(SerializationWriter Writer)
	{
		if (BananaGroveNoise == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(1);
		for (int i = 0; i < 1200; i++)
		{
			for (int j = 0; j < 375; j++)
			{
				Writer.Write(BananaGroveNoise[i, j]);
			}
		}
	}

	public static void Load(SerializationReader Reader)
	{
		if (Reader.ReadInt32() == 0)
		{
			BananaGroveNoise = null;
			return;
		}
		BananaGroveNoise = new double[1200, 375];
		for (int i = 0; i < 1200; i++)
		{
			for (int j = 0; j < 375; j++)
			{
				BananaGroveNoise[i, j] = Reader.ReadDouble();
			}
		}
	}

	public bool BuildZone(Zone Z)
	{
		if (BananaGroveNoise == null)
		{
			NoiseFunctions = new List<PerlinNoise2D>();
			NoiseFunctions.Add(new PerlinNoise2D(1, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(4, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(8, 1f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(16, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(32, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(128, 4f, Stat.Rand));
			BananaGroveNoise = PerlinNoise2D.sumNoiseFunctions(1200, 375, 0, 0, NoiseFunctions);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (!Z.GetCell(i, j).IsPassable())
				{
					continue;
				}
				double num3 = BananaGroveNoise[i + num, j + num2];
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				if (num3 >= 0.8)
				{
					num4 = 20;
					num5 = 80;
					num6 = 5;
				}
				else if (num3 >= 0.7)
				{
					num4 = 3;
					num5 = 60;
					num6 = 3;
				}
				else if (num3 >= 0.5)
				{
					num4 = 1;
					num5 = 1;
					num6 = 30;
				}
				if (Stat.Random(1, 100) <= num4)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Starapple Tree"));
				}
				else if (Stat.Random(1, 100) <= num5)
				{
					if (If.Chance(5))
					{
						Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Red Death Dacca"));
					}
					else
					{
						Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Banana Tree"));
					}
				}
				else if (Stat.Random(1, 100) <= num6)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Dicalyptus Tree"));
				}
			}
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Grassy"));
		if (Z.GetTerrainNameFromDirection(".") == "TerrainTheSpindle" && Z.Z >= 12 && Z.Z <= 15)
		{
			int num7 = 10;
			int num8 = 10;
			Box b = new Box(20 - num7 / 2, 12 - num8 / 2, 20 - num7 / 2 + num7, 12 - num8 / 2 + num8);
			Box b2 = new Box(60 - num7 / 2, 12 - num8 / 2, 60 - num7 / 2 + num7, 12 - num8 / 2 + num8);
			Z.ClearBox(b);
			Z.FillBox(b, "EbonFulcrete");
			Z.ClearBox(b2);
			Z.FillBox(b2, "EbonFulcrete");
			Z.ProcessHollowBox(new Box(0, 0, 79, 24), delegate(Cell c)
			{
				if (c.HasWall())
				{
					c.Clear();
					c.AddObject("EbonFulcrete");
				}
			});
			ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		}
		List<Location2D> ruinWalkerStarts = new List<Location2D>();
		int CHANCE_RUIN_WALKER = 1;
		int MAX_RUIN_WALKER = 100;
		Action<string, Cell> afterPlacement = delegate(string o, Cell c)
		{
			if (Stat.Random(1, MAX_RUIN_WALKER) <= CHANCE_RUIN_WALKER && !c.HasObject("EbonFulcrete"))
			{
				ruinWalkerStarts.Add(c.location);
			}
		};
		if (Z.Y == 2 && Z.GetTerrainNameFromDirection("S") == "TerrainTheSpindle")
		{
			if (Z.X == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNW.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.X == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_N.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.X == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNE.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.Y == 0 && Z.GetTerrainNameFromDirection("N") == "TerrainTheSpindle")
		{
			if (Z.X == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSW.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				});
			}
			if (Z.X == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_S.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				});
			}
			if (Z.X == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSE.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				});
			}
			if (Z.X == 2 && Z.Z == 10)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombRuinedMuralCistern.rpm", Z.GetCell(31, 7), 0, null, delegate(Cell c)
				{
					c.Clear();
				});
				Z.GetCell(0, 0).AddObject("SultanMuralControllerRandom2AlwaysRuined");
			}
			if (Z.Z == 11)
			{
				int num9 = 0;
				int num10 = 0;
				List<Location2D> list = new List<Location2D>();
				List<GameObject> list2 = new List<GameObject>();
				if (Z.X == 1)
				{
					num9 = 0;
					num10 = 17;
					list.AddRange(new Box(num9, 2, num10, 2).contents());
				}
				else if (Z.X == 0)
				{
					num9 = 44;
					num10 = 79;
					list.AddRange(new Box(num9, 2, num10, 2).contents());
					if (Z.Z == 11)
					{
						list.AddRange(new Box(num9, 0, num9, 2).contents());
						list.AddRange(new Box(num9 - 1, 0, num9 - 1, 1).contents());
					}
					if (Z.Z == 11)
					{
						Z.GetCell(num9 - 1, 1).AddObject("MediumBoulder");
					}
				}
				Z.Clear(list);
				foreach (Location2D item in list)
				{
					Cell cell = Z.GetCell(item);
					foreach (Cell adjacentCell in cell.GetAdjacentCells())
					{
						foreach (GameObject item2 in adjacentCell.GetObjectsWithTag("Wall"))
						{
							if (!item2.HasPropertyOrTag("HasGraffiti") && !list2.Contains(item2))
							{
								list2.Add(item2);
							}
						}
					}
					foreach (PopulationResult item3 in PopulationManager.Generate("RobbersCutContents"))
					{
						for (int k = 0; k < item3.Number; k++)
						{
							cell.AddObject(item3.Blueprint);
						}
					}
				}
				int num11 = 30;
				foreach (GameObject item4 in list2)
				{
					if (Stat.Random(1, 100) <= num11 && !item4.HasPropertyOrTag("HasGraffiti") && !item4.HasPart("Graffiti"))
					{
						Graffitied graffitied = new Graffitied();
						item4.AddPart(graffitied);
						graffitied.Graffiti(item4);
					}
				}
			}
		}
		if (Z.X == 0 && Z.GetTerrainNameFromDirection("W") == "TerrainTheSpindle")
		{
			if (Z.Y == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ENE.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_E.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ESE.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.X == 2 && Z.GetTerrainNameFromDirection("E") == "TerrainTheSpindle")
		{
			if (Z.Y == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WNW.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_W.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WSW.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.X == 0 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NW") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SE.rpm", PlacePrefabAlign.NW, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 2 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NE") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SW.rpm", PlacePrefabAlign.NE, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 0 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SW") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NE.rpm", PlacePrefabAlign.SW, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 2 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SE") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NW.rpm", PlacePrefabAlign.SE, null, null, null, null, (string o, Cell c) => (Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o, delegate(string o, Cell c)
			{
				c.Clear();
			}, afterPlacement);
		}
		foreach (Location2D item5 in ruinWalkerStarts)
		{
			Location2D location2D = item5;
			int num12 = 100;
			while ((location2D.x < 6 || location2D.x > 73 || location2D.y < 6 || location2D.y > 19) && location2D.x >= 0 && location2D.x <= 79 && location2D.y >= 0 && location2D.y <= 24 && location2D != null)
			{
				Cell cell2;
				while (true)
				{
					num12--;
					if (num12 < 1)
					{
						break;
					}
					int num13 = 60;
					if (Stat.Random(1, 100) <= num13)
					{
						Location2D location2D2 = location2D.FromDirection(location2D.CardinalDirectionToCenter());
						if (location2D2 == null || Z.GetCell(location2D2) == null || Z.GetCell(location2D2).HasObject("EbonFulcrete"))
						{
							continue;
						}
						location2D = location2D2;
					}
					else
					{
						Location2D location2D3 = location2D.FromDirection(Directions.GetRandomCardinalDirection());
						if (location2D3 == null || Z.GetCell(location2D3) == null || Z.GetCell(location2D3).HasObject("EbonFulcrete"))
						{
							continue;
						}
						location2D = location2D3;
					}
					cell2 = Z.GetCell(location2D);
					if (cell2 == null)
					{
						break;
					}
					goto IL_0eb0;
				}
				break;
				IL_0eb0:
				if (cell2.HasObject("EbonFulcrete"))
				{
					break;
				}
				if (!(location2D != null))
				{
					continue;
				}
				foreach (Cell cardinalAdjacentCell in Z.GetCell(location2D).GetCardinalAdjacentCells(bLocalOnly: true, BuiltOnly: true, IncludeThis: true))
				{
					int num14 = 50;
					if (cardinalAdjacentCell.HasWall() && !cardinalAdjacentCell.HasObject("EbonFulcrete") && Stat.Random(1, 100) <= num14)
					{
						cardinalAdjacentCell.ClearWalls();
						int num15 = 35;
						if (Stat.Random(1, 100) <= num15)
						{
							cardinalAdjacentCell.AddObject(PopulationManager.RollOneFrom("TombWallRuinRemains").Blueprint);
						}
					}
				}
			}
		}
		if (!Underground)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
			Z.ClearReachableMap();
			Z.BuildReachableMap(Z.Width / 2, Z.Height / 2);
		}
		return true;
	}
}

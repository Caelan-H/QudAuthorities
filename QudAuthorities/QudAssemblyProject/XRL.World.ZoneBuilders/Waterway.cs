using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Waterway
{
	public bool BuildZone(Zone Z)
	{
		MazeCell mazeCell = XRLCore.Core.Game.WorldMazes["QudWaterwayMaze"].Cell[Z.wX, Z.wY];
		if ((Z.X == 1 && Z.Y == 0 && mazeCell.N) || (Z.X == 1 && Z.Y == 2 && mazeCell.S) || (Z.X == 1 && Z.Y == 1 && !mazeCell.E && !mazeCell.W && mazeCell.N && mazeCell.S))
		{
			for (int i = 30; i <= 50; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					Z.GetCell(i, j).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(i, j).ClearObjectsWithPart("Door");
				}
			}
			for (int k = 0; k < Z.Height; k++)
			{
			}
			for (int l = 0; l < Z.Height; l++)
			{
				foreach (GameObject item in Z.GetCell(29, l).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(29, l).RemoveObject(item);
					Z.GetCell(29, l).AddObject("Fulcrete");
				}
				foreach (GameObject item2 in Z.GetCell(51, l).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(51, l).RemoveObject(item2);
					Z.GetCell(51, l).AddObject("Fulcrete");
				}
			}
			Z.CacheZoneConnection("-", Z.Width / 2, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2, Z.Height - 1, "RiverSouthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 - 5, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 + 5, Z.Height - 1, "RiverSouthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 + 5, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 - 5, Z.Height - 1, "RiverSouthMouth", null);
			if (!new RiverBuilder
			{
				Pairs = true
			}.BuildZone(Z))
			{
				_ = Z.BuildTries;
				_ = 15;
				return true;
			}
		}
		else if ((Z.X == 0 && Z.Y == 1 && mazeCell.W) || (Z.X == 2 && Z.Y == 1 && mazeCell.E) || (Z.X == 1 && Z.Y == 1 && mazeCell.E && mazeCell.W && !mazeCell.N && !mazeCell.S))
		{
			for (int m = 0; m < Z.Width; m++)
			{
				for (int n = 7; n <= 18; n++)
				{
					Z.GetCell(m, n).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(m, n).ClearObjectsWithPart("Door");
				}
			}
			for (int num = 0; num < Z.Width; num++)
			{
				foreach (Cell localAdjacentCell in Z.GetCell(num, 12).GetLocalAdjacentCells(Stat.Random(1, 4)))
				{
					localAdjacentCell.AddObject("SaltyWaterDeepPool");
				}
			}
			for (int num2 = 0; num2 < Z.Width; num2++)
			{
				foreach (GameObject item3 in Z.GetCell(num2, 6).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(num2, 6).RemoveObject(item3);
					Z.GetCell(num2, 6).AddObject("Fulcrete");
				}
				foreach (GameObject item4 in Z.GetCell(num2, 19).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(num2, 19).RemoveObject(item4);
					Z.GetCell(num2, 19).AddObject("Fulcrete");
				}
			}
		}
		else if (Z.X == 1 && Z.Y == 1)
		{
			if (mazeCell.N)
			{
				for (int num3 = 30; num3 <= 50; num3++)
				{
					for (int num4 = 0; num4 < 18; num4++)
					{
						Z.GetCell(num3, num4).ClearObjectsWithIntProperty("Wall");
					}
				}
				for (int num5 = 0; num5 < 18; num5++)
				{
					foreach (GameObject item5 in Z.GetCell(29, num5).GetObjectsWithIntProperty("Wall"))
					{
						Z.GetCell(29, num5).RemoveObject(item5);
						Z.GetCell(29, num5).AddObject("Fulcrete");
					}
					foreach (GameObject item6 in Z.GetCell(51, num5).GetObjectsWithIntProperty("Wall"))
					{
						Z.GetCell(51, num5).RemoveObject(item6);
						Z.GetCell(51, num5).AddObject("Fulcrete");
					}
				}
			}
			if (mazeCell.S)
			{
				for (int num6 = 30; num6 <= 50; num6++)
				{
					for (int num7 = 7; num7 < Z.Height; num7++)
					{
						Z.GetCell(num6, num7).ClearObjectsWithIntProperty("Wall");
					}
				}
				for (int num8 = 7; num8 < Z.Height; num8++)
				{
					foreach (GameObject item7 in Z.GetCell(29, num8).GetObjectsWithIntProperty("Wall"))
					{
						Z.GetCell(29, num8).RemoveObject(item7);
						Z.GetCell(29, num8).AddObject("Fulcrete");
					}
					foreach (GameObject item8 in Z.GetCell(51, num8).GetObjectsWithIntProperty("Wall"))
					{
						Z.GetCell(51, num8).RemoveObject(item8);
						Z.GetCell(51, num8).AddObject("Fulcrete");
					}
				}
			}
			if (mazeCell.W)
			{
				for (int num9 = 0; num9 < 40; num9++)
				{
					for (int num10 = 7; num10 <= 18; num10++)
					{
						Z.GetCell(num9, num10).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			if (mazeCell.E)
			{
				for (int num11 = 30; num11 < Z.Width; num11++)
				{
					for (int num12 = 7; num12 <= 18; num12++)
					{
						Z.GetCell(num11, num12).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			for (int num13 = 0; num13 < Z.Height; num13++)
			{
				foreach (GameObject item9 in Z.GetCell(29, num13).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(29, num13).RemoveObject(item9);
					Z.GetCell(29, num13).AddObject("Fulcrete");
				}
				foreach (GameObject item10 in Z.GetCell(51, num13).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(51, num13).RemoveObject(item10);
					Z.GetCell(51, num13).AddObject("Fulcrete");
				}
			}
			for (int num14 = 0; num14 < Z.Width; num14++)
			{
				foreach (GameObject item11 in Z.GetCell(num14, 6).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(num14, 6).RemoveObject(item11);
					Z.GetCell(num14, 6).AddObject("Fulcrete");
				}
				foreach (GameObject item12 in Z.GetCell(num14, 19).GetObjectsWithIntProperty("Wall"))
				{
					Z.GetCell(num14, 19).RemoveObject(item12);
					Z.GetCell(num14, 19).AddObject("Fulcrete");
				}
			}
		}
		return true;
	}
}

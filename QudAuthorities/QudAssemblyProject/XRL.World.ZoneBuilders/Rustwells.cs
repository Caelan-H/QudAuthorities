using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Rustwells : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z == 10)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		}
		Z.ForeachObjectWithTagOrProperty("Stairs", delegate(GameObject O)
		{
			MetricsManager.LogError("Pre-existing stair in " + O.CurrentCell);
		});
		Pitted pitted = new Pitted();
		pitted.MaxWells = 1;
		pitted.MinWells = 1;
		pitted.MinRadius = 8;
		pitted.MaxRadius = 10;
		pitted.XMargin = 20;
		List<Location2D> list = new List<Location2D>();
		List<Location2D> list2 = new List<Location2D>();
		pitted.BuildZone(Z, list, list2);
		Rect2D pitBounds = new Rect2D(list).ExpandBy(1, clampToScreenSize: true);
		int num = 10;
		if (25.in100())
		{
			num += Stat.Random(1, 6);
		}
		int depth = ZoneBuilderSandbox.GetOracleIntColumn(Z, 1, 4);
		float anglechunks = ZoneBuilderSandbox.GetOracleIntColumn(Z, 1, num);
		int divs = ZoneBuilderSandbox.GetOracleIntColumn(Z, 2, 4);
		Z.ForeachCell(delegate(Cell C)
		{
			int num2 = C.CosmeticDistanceTo(pitBounds.Center);
			int num3 = ((int)(Math.Atan2(C.Pos2D.x - pitBounds.Center.x, C.Pos2D.y - pitBounds.Center.y) / (Math.PI / (double)anglechunks)) + num2 / (divs * depth)) % divs;
			if (!pitBounds.Contains(C.Pos2D) && num2 / depth % 2 == 0 && num3 == 0)
			{
				C.AddObject("Fulcrete");
			}
		});
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		List<NoiseMapNode> list3 = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list3.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		if (Z.Z > 10)
		{
			NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					if (i > 0 && j > 0 && i < 79 && j < 24 && noiseMap.Noise[i, j] > 2 && !Z.GetCell(i, j).HasObject("Fulcrete"))
					{
						Z.GetCell(i, j).ClearWalls();
					}
				}
			}
		}
		Z.GetCell(10 + (Z.Z - 1) / 3, Z.Height - 1).AddObject("Fulcrete");
		if (Z.Z > 10)
		{
			StairsUp stairsUp = new StairsUp();
			stairsUp.Reachable = false;
			stairsUp.BuildZone(Z);
		}
		if (list.Count > 9)
		{
			StairsDown stairsDown = new StairsDown();
			stairsDown.Reachable = false;
			stairsDown.EmptyOnly = false;
			stairsDown.x = "1-78";
			stairsDown.y = "1-23";
			stairsDown.BuildZone(Z);
		}
		Z.FireEvent("FirmPitEdges");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		if (Z.Z == 10)
		{
			for (int k = 0; k < Stat.Random(0, 4); k++)
			{
				Pathfinder pathfinder = new Pathfinder(80, 25);
				Cell randomElement = (from c in Z.GetEmptyCells()
					where c.X == 0 || c.Y == 0 || c.X == 79 || c.Y == 24
					select c).GetRandomElement();
				if (randomElement == null || !pathfinder.FindPath(Z.GetCell(randomElement.X, randomElement.Y).location, Z.GetCell(40, 15).location, bDisplay: false, bOrdinalDirectionsOnly: true, 24300))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell = Z.GetCell(step.X, step.Y);
					GameObject gO = GameObject.create("SaltyWaterDeepPool");
					cell.AddObject(gO);
				}
			}
		}
		Z.ForeachCell((Action<Cell>)SanityCheck);
		if (Z.Z == 13)
		{
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2, "RustwellsBottomPool");
		}
		return true;
	}

	public void SanityCheck(Cell C)
	{
		if (!C.HasWall())
		{
			C.SetReachable(State: true);
		}
		else if (C.HasStairs())
		{
			MetricsManager.LogError("Stairs generated blocked by wall, clearing.");
			C.ClearWalls();
			C.SetReachable(State: true);
		}
	}
}

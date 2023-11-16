using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Redrock : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z > 10)
		{
			new StairsUp().BuildZone(Z);
		}
		if (Z.Z < 14)
		{
			new StairsDown().BuildZone(Z);
		}
		List<NoiseMapNode> extraNodes = new List<NoiseMapNode>();
		Z.GetCells().ForEach(delegate(Cell c)
		{
			c.AddObject("Shale");
		});
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			extraNodes.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		Z.GetCells().ForEach(delegate(Cell c)
		{
			if (c.HasObjectWithTag("Stairs"))
			{
				extraNodes.Add(new NoiseMapNode(c.X, c.Y));
			}
		});
		string seedsPerSector = "0-7";
		if (Z.Z == 11)
		{
			seedsPerSector = "1-3";
		}
		if (Z.Z == 12)
		{
			seedsPerSector = "0-5";
		}
		if (Z.Z == 13)
		{
			seedsPerSector = "0-6";
		}
		if (Z.Z == 14)
		{
			seedsPerSector = "0-7";
		}
		int maximumSeeds = int.MaxValue;
		if (Z.Z == 11)
		{
			maximumSeeds = 3;
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 2, seedsPerSector, 80, 240, 0, 4, -3, 1, extraNodes, -1, maximumSeeds);
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (i > 0 && j > 0 && i < 79 && j < 24 && noiseMap.Noise[i, j] >= 1 && !Z.GetCell(i, j).HasObject("Fulcrete"))
				{
					Z.GetCell(i, j).ClearWalls();
				}
			}
		}
		string dice = "0";
		if (Z.Z == 11)
		{
			dice = "0";
		}
		if (Z.Z == 12)
		{
			dice = "1";
		}
		if (Z.Z == 13)
		{
			dice = "2-3";
		}
		if (Z.Z == 14)
		{
			dice = "3-4";
		}
		int num = Stat.Roll(dice);
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		for (int k = 0; k < num; k++)
		{
			Location2D randomElement = noiseMap.PlacedSeeds.GetRandomElement();
			list.Add(new NoiseMapNode(randomElement.x, randomElement.y));
		}
		NoiseMap noiseMap2 = new NoiseMap(80, 25, 10, 1, 1, "0", 80, 240, 0, 3, -3, 1, list, -1, num * 3);
		for (int m = 0; m < 80; m++)
		{
			for (int n = 0; n < 25; n++)
			{
				if (m > 0 && n > 0 && m < 79 && n < 24 && noiseMap2.Noise[m, n] >= 1)
				{
					Z.GetCell(m, n).AddObject("DeepBrackishPool");
				}
			}
		}
		foreach (NoiseMapNode featureLocation in list)
		{
			Cell randomElement2 = (from c in Z.GetCells()
				where c.HasWall() && c.DistanceTo(featureLocation.x, featureLocation.y) < 30
				select c).GetRandomElement();
			ZoneBuilderSandbox.TunnelTo(Z, Location2D.get(featureLocation.x, featureLocation.y), Location2D.get(randomElement2.X, randomElement2.Y), pathWithNoise: true, 0.2f, 200, delegate(Cell c)
			{
				c.Clear();
				c.AddObject("DeepBrackishPool");
			});
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, 400);
		List<GameObject> stairs = Z.FindObjects((GameObject o) => o.HasTag("Stairs"));
		List<InfluenceMapRegion> list2 = influenceMap.Regions.Where((InfluenceMapRegion r) => !r.Cells.Any((Location2D l) => Z.GetCell(l).HasObjectWithTag("Stairs"))).ToList();
		if (stairs.Count > 0)
		{
			list2.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => stairs.Sum((GameObject s) => b.Center.Distance(s.CurrentCell.location)) - stairs.Sum((GameObject s) => a.Center.Distance(s.CurrentCell.location)));
		}
		int num2 = 3;
		if (Z.Z == 11)
		{
			num2 = Stat.Random(0, 1);
		}
		if (Z.Z == 12)
		{
			num2 = Stat.Random(1, 2);
		}
		if (Z.Z == 13)
		{
			num2 = Stat.Random(2, 3);
		}
		if (Z.Z == 14)
		{
			num2 = Stat.Random(3, 4);
		}
		for (int num3 = 0; num3 < num2; num3++)
		{
			Location2D start = noiseMap.PlacedSeeds.GetRandomElement();
			noiseMap.PlacedSeeds.Sort((Location2D a, Location2D b) => b.Distance(start) - a.Distance(start));
			ZoneBuilderSandbox.TunnelTo(Z, start, noiseMap.PlacedSeeds.First(), pathWithNoise: true, 0.2f, 200);
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		foreach (Cell item in from c in Z.GetCells()
			where !c.HasWall()
			select c)
		{
			item.SetReachable(State: true);
		}
		if (Z.Z == 12)
		{
			int num4 = 1;
			if (75.in100())
			{
				num4++;
			}
			if (num4 == 2 && 30.in100())
			{
				num4++;
			}
			for (int num5 = 0; num5 < list2.Count && num5 < num4; num5++)
			{
				list2[num5].Cells.ForEach(delegate(Location2D c)
				{
					Z.GetCell(c).AddObject("InfluenceMapBlocker");
				});
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2[num5], "RedrockLevel2Encounters");
			}
		}
		if (Z.Z == 13)
		{
			string blueprint = PopulationManager.RollOneFrom("RedrockSnapjawFortStyle").Blueprint;
			if (blueprint == "Stockade")
			{
				new RedrockStockadeMaker().BuildZone(Z, ClearCombatObjectsFirst: true, "BrinestalkStakes", "SnapjawParty1", null, "SnapjawStockadeRoom with Snapjaws", "SnapjawStockadeRoom with Bear", "SnapjawStockadeRoom Small with Snapjaws", "SnapjawStockadeRoom Small with Bear", "16-35", "12-20", SpecialRedrockBuilder: true, "SnapjawStockadeOuterArea");
			}
			if (blueprint == "City")
			{
				InfluenceMap iF = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, 100);
				new CaveCity().BuildZone(Z, iF, "Brinestalk Gate", "BrinestalkStakes", 2, 9, 3, 3, "3-5", 100, "RedrockSnapjawCaveRoomDecoration", "RedrockSnapjawCaveRoomCreatures", "SnapjawBoss", Z.FindObject("StairsUp")?.CurrentCell?.location);
			}
			if (blueprint == "Fort")
			{
				new SnapjawFortMaker().BuildZone(Z);
			}
		}
		if (Z.Z == 14)
		{
			new RiverStartMouth().BuildZone(Z);
			new RiverSouthMouth().BuildZone(Z);
			new RiverBuilder().BuildZone(Z);
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		return true;
	}
}

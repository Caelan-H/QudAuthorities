using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Weald
{
	private static List<PerlinNoise2D> NoiseFunctions;

	private static double[,] WealdNoise;

	public bool BuildZone(Zone Z)
	{
		if (WealdNoise == null)
		{
			NoiseFunctions = new List<PerlinNoise2D>();
			NoiseFunctions.Add(new PerlinNoise2D(1, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(4, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(8, 1f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(16, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(32, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(128, 4f, Stat.Rand));
			WealdNoise = PerlinNoise2D.sumNoiseFunctions(1200, 375, 0, 0, NoiseFunctions);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				double num3 = WealdNoise[i + num, j + num2];
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
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Witchwood Tree"));
				}
				else if (Stat.Random(1, 100) <= num6)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Yuckwheat"));
				}
			}
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Grassy"));
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}

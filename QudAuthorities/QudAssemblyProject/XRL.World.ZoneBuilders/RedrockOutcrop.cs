using System.Collections.Generic;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class RedrockOutcrop : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).RequireObject("DaylightWidget");
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		for (int i = 0; i < 20; i++)
		{
			list.Add(new NoiseMapNode(10, i, -10));
			list.Add(new NoiseMapNode(i, 10, -10));
		}
		NoiseMap noiseMap = new NoiseMap(20, 20, 10, 3, 3, 2, 80, 80, 4, 3, 0, 1, list);
		NoiseMap noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 3, 3, 2, 20, 20, 4, 3, 0, 1, list);
		int num = Stat.Random(1, 59);
		int num2 = Stat.Random(1, 3);
		bool flag = false;
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "StairsDown")
			{
				num = zoneConnection.X;
				num2 = zoneConnection.Y;
				flag = true;
				break;
			}
		}
		for (int j = 0; j < 20; j++)
		{
			for (int k = 0; k < 20; k++)
			{
				if (noiseMap.Noise[j, k] > 1)
				{
					Z.GetCell(j + num, k + num2)?.ClearAndAddObject("Shale");
				}
			}
		}
		for (int l = 0; l < Z.Width; l++)
		{
			for (int m = 0; m < Z.Height; m++)
			{
				if (noiseMap2.Noise[l, m] > 1)
				{
					Z.GetCell(l, m).ClearAndAddObject("Shale");
				}
			}
		}
		if (flag)
		{
			Cell cell = Z.GetCell(num, num2);
			cell.Clear();
			cell.RequireObject("StairsDown");
			EnsureCellReachable(Z, cell);
			BuildReachableMap(Z, num, num2);
			return true;
		}
		BuildReachableMap(Z, num, num2);
		for (int n = 0; n < 11; n++)
		{
			for (int num3 = 10 - n; num3 <= 10 + n; num3++)
			{
				for (int num4 = 10 - n; num4 <= 10 + n; num4++)
				{
					Cell cell2 = Z.GetCell(num3 + num, num4 + num2);
					if (cell2.IsReachable() && cell2.IsEmpty())
					{
						cell2.AddObject("StairsDown");
						return true;
					}
				}
			}
		}
		MetricsManager.LogError("Failed placing stairs down for Oboroqoru's lair, placing anywhere in zone");
		ZoneBuilderSandbox.PlaceObject("StairsDown", Z);
		return true;
	}

	public void BuildReachableMap(Zone Z, int xp, int yp)
	{
		for (int i = 0; i < 11; i++)
		{
			for (int j = 10 - i; j <= 10 + i; j++)
			{
				for (int k = 10 - i; k <= 10 + i; k++)
				{
					Cell cell = Z.GetCell(j + xp, k + yp);
					if (cell != null && cell.IsEmpty())
					{
						Z.ClearReachableMap(bValue: true);
						if (Z.BuildReachableMap(j, k) > 400)
						{
							return;
						}
					}
				}
			}
		}
		for (int l = 0; l < Z.Width; l++)
		{
			for (int m = 0; m < Z.Height; m++)
			{
				if (Z.GetCell(l, m).IsEmpty())
				{
					Z.ClearReachableMap(bValue: true);
					if (Z.BuildReachableMap(l, m) > 400)
					{
						return;
					}
				}
			}
		}
	}
}

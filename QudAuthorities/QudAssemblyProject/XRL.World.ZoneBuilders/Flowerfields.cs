using XRL.Rules;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Flowerfields
{
	public bool Underground;

	public bool BuildZone(Zone Z)
	{
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 6, 80, 80, 4, 3, 0, 1, null);
		NoiseMap noiseMap2 = new NoiseMap(80, 25, 10, 3, 3, 6, 80, 80, 4, 3, 0, 1, null);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (noiseMap.Noise[i, j] > 1 && (!Underground || Z.GetCell(i, j).IsPassable()))
				{
					if (Stat.Rnd.Next(100) < 30)
					{
						if (!Z.GetCell(i, j).IsOccluding())
						{
							Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Dogthorn Tree"));
						}
					}
					else if (Stat.Rnd.Next(100) < 30)
					{
						if (!Z.GetCell(i, j).IsOccluding())
						{
							Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Witchwood Tree"));
						}
					}
					else if (Stat.Rnd.Next(100) < 1)
					{
						if (!Z.GetCell(i, j).IsOccluding())
						{
							Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Lahbloom"));
						}
					}
					else
					{
						if (Stat.Rnd.Next(1000) >= 5)
						{
							continue;
						}
						if (Stat.Rnd.Next(100) < 50)
						{
							if (!Z.GetCell(i, j).IsOccluding())
							{
								Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Starapple Tree"));
							}
						}
						else if (!Z.GetCell(i, j).IsOccluding())
						{
							Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Feral Lah"));
						}
					}
				}
				else if (noiseMap2.Noise[i, j] > 1 && Stat.Rnd.Next(100) < 85 && Z.GetCell(i, j).RenderedObjectsCount == 0)
				{
					Grassy.PaintCell(Z.GetCell(i, j));
				}
			}
		}
		if (!Underground)
		{
			Z.ClearReachableMap();
			if (Z.BuildReachableMap(0, 0) < 400)
			{
				return false;
			}
		}
		return true;
	}
}

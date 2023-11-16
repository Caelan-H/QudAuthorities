using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Plains
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (Stat.Rnd.Next(100) < 2)
				{
					if (!Z.GetCell(i, j).IsOccluding())
					{
						Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Dogthorn Tree"));
					}
				}
				else if (Stat.Rnd.Next(100) < 2)
				{
					if (!Z.GetCell(i, j).IsOccluding())
					{
						Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Witchwood Tree"));
					}
				}
				else if (Stat.Rnd.Next(3000) < 2 && !Z.GetCell(i, j).IsOccluding())
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Starapple Tree"));
				}
			}
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Grassy"));
		for (int k = 0; k < Z.Width; k++)
		{
			for (int l = 0; l < Z.Height; l++)
			{
				if (Z.GetCell(k, l).IsEmpty())
				{
					Z.ClearReachableMap(bValue: true);
					if (Z.BuildReachableMap(k, l) > 400)
					{
						return true;
					}
				}
			}
		}
		return true;
	}
}

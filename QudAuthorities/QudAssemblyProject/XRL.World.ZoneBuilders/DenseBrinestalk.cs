using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class DenseBrinestalk
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (Z.GetCell(i, j).IsEmpty() && Stat.Random(1, 100) < 25)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Brinestalk"));
				}
			}
		}
		return true;
	}
}

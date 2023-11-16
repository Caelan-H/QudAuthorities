using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Test
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (Stat.Rnd.Next(100) < 5)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Wall"));
				}
				else if (Stat.Rnd.Next(100) < 5)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Battle Axe"));
				}
				else if (Stat.Rnd.Next(100) < 5)
				{
					XRLCore.Core.Game.ActionManager.AddActiveObject(Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Wof")));
				}
			}
		}
		return true;
	}
}

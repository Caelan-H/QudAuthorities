using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class CollapseAtLevel
{
	public int Level = 5;

	public bool BuildZone(Zone Z)
	{
		if (XRLCore.Core.Game.Player.Body.Statistics["Level"].Value < Level)
		{
			return true;
		}
		Z.ClearReachableMap(bValue: true);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				Z.GetCell(i, j).Clear();
				Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Halite"));
			}
		}
		return true;
	}
}

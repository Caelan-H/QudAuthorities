namespace XRL.World.ZoneBuilders;

public class GenericSolid
{
	public string Material = "Shale";

	public bool ClearFirst = true;

	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (ClearFirst)
				{
					Z.GetCell(i, j).Clear();
				}
				Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject(Material));
			}
		}
		return true;
	}
}

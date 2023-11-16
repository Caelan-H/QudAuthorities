using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Tarland
{
	public bool BuildZone(Zone Z)
	{
		CellularGrid cellularGrid = new CellularGrid();
		cellularGrid.Passes = 3;
		cellularGrid.SeedChance = 55;
		cellularGrid.SeedBorders = true;
		cellularGrid.Generate(Stat.Rand, Z.Width, Z.Height);
		CellularGrid cellularGrid2 = new CellularGrid();
		cellularGrid2.Passes = 3;
		cellularGrid2.SeedChance = 45;
		cellularGrid2.SeedBorders = true;
		cellularGrid2.Generate(Stat.Rand, Z.Width, Z.Height);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (cellularGrid.cells[i, j] == 0)
				{
					Z.GetCell(i, j).Clear();
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("AsphaltPuddle"));
				}
				else if (cellularGrid2.cells[i, j] == 0)
				{
					Z.GetCell(i, j).Clear();
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Shale"));
				}
			}
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		return true;
	}
}

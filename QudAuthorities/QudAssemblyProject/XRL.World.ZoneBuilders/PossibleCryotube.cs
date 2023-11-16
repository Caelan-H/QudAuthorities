using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class PossibleCryotube : ZoneBuilderSandbox
{
	public int Chance = 20;

	public int ExtraChancePerLevel = 10;

	public int ChanceOfAdditionalTubes = 10;

	public bool BuildZone(Zone Z)
	{
		if (Stat.Random(1, 10000) > Chance + ExtraChancePerLevel * (Z.Z - 10))
		{
			return true;
		}
		int num = 1;
		while (Stat.Random(1, 10000) <= ChanceOfAdditionalTubes)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			Cell cellWithEmptyBorder = Z.GetCellWithEmptyBorder(2);
			if (cellWithEmptyBorder == null)
			{
				continue;
			}
			string blueprint = PopulationManager.RollOneFrom("CryotubeContents").Blueprint;
			Cryobarrio2.MakeCryochamber(cellWithEmptyBorder.X, cellWithEmptyBorder.Y, Z, blueprint);
			if (!(blueprint == "*Destroyed"))
			{
				continue;
			}
			Physics.LegacyApplyExplosion(cellWithEmptyBorder, new List<Cell>(), new List<GameObject>(), 35000, Local: true, Show: false);
			int x = cellWithEmptyBorder.X;
			int y = cellWithEmptyBorder.Y;
			for (int j = y - 1; j <= y + 1; j++)
			{
				for (int k = x - 1; k <= x + 1; k++)
				{
					if (5.in100())
					{
						Z.GetCell(k, j).AddObject("ConvalessencePuddle");
					}
				}
			}
			Z.GetCell(x - 2, y - 2).Clear();
			Z.GetCell(x + 2, y - 2).Clear();
			Z.GetCell(x - 2, y + 2).Clear();
			Z.GetCell(x + 2, y + 2).Clear();
			Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallBroken");
		}
		return true;
	}
}

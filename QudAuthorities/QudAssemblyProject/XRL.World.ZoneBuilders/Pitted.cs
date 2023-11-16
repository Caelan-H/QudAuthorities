using System.Collections.Generic;
using Genkit;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

/// <summary>
///             Should typically be added as a builder to zones in a single column on Z level 10 through 13 of an area that will be pitted.
///             </summary>
public class Pitted : ZoneBuilderSandbox
{
	public int MinWells = 3;

	public int MaxWells = 6;

	public int MinRadius = 3;

	public int MaxRadius = 10;

	public int XMargin = 4;

	public int YMargin = 2;

	public bool BuildZone(Zone Z)
	{
		return BuildZone(Z, new List<Location2D>(), new List<Location2D>());
	}

	public bool BuildZone(Zone Z, List<Location2D> pitCellsOut, List<Location2D> centerCellsOut)
	{
		return BuildPits(Z, MinWells, MaxWells, MinRadius, MaxRadius, XMargin, YMargin, pitCellsOut, centerCellsOut);
	}

	public static bool BuildPits(Zone Z, int MinWells, int MaxWells, int MinRadius, int MaxRadius, int XMargin, int YMargin, List<Location2D> pitCellsOut, List<Location2D> centerCellsOut)
	{
		if (Z.Z == 10)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		}
		int oracleIntColumn = ZoneBuilderSandbox.GetOracleIntColumn(Z, "nwells", MinWells, MaxWells);
		for (int i = 0; i < oracleIntColumn; i++)
		{
			int oracleIntColumn2 = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_r", MinRadius, MaxRadius);
			int oracleIntColumn3 = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_x", XMargin + oracleIntColumn2 * 2, 80 - XMargin - oracleIntColumn2 * 2);
			int oracleIntColumn4 = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_y", YMargin + oracleIntColumn2, 24 - YMargin - oracleIntColumn2);
			Cell cell = Z.GetCell(oracleIntColumn3, oracleIntColumn4);
			List<Cell> list = new List<Cell>();
			for (int j = -(oracleIntColumn2 * 2); j <= oracleIntColumn2 * 2; j++)
			{
				for (int k = -oracleIntColumn2; k <= oracleIntColumn2; k++)
				{
					Cell cell2 = Z.GetCell(oracleIntColumn3 + j, oracleIntColumn4 + k);
					if (cell2.CosmeticDistanceTo(cell.X, cell.Y) <= oracleIntColumn2 - (Z.Z - 10))
					{
						centerCellsOut?.Add(cell2.location);
						cell2.Clear();
						cell2.AddObject("FlyingWhitelistArea");
						list.Add(cell2);
						if (Z.Z <= 12)
						{
							GameObject gameObject = GameObjectFactory.Factory.CreateObject("Pit");
							gameObject.GetPart<XRL.World.Parts.StairsDown>().ConnectLanding = false;
							cell2.AddObject(gameObject);
							pitCellsOut.Add(cell2.location);
						}
						else
						{
							cell2.AddObject("SaltyWaterExtraDeepPool");
						}
					}
				}
			}
			for (int l = -(MaxRadius * 2); l <= MaxRadius * 2; l++)
			{
				for (int m = -MaxRadius; m <= MaxRadius; m++)
				{
					Cell cell3 = Z.GetCell(oracleIntColumn3 + l, oracleIntColumn4 + m);
					if (cell3.CosmeticDistanceTo(cell.X, cell.Y) <= MaxRadius)
					{
						cell3.AddObject("StairBlocker");
						cell3.AddObject("InfluenceMapBlocker");
					}
				}
			}
			if (pitCellsOut.Count == 0)
			{
				pitCellsOut.Add(cell.location);
			}
		}
		return true;
	}
}

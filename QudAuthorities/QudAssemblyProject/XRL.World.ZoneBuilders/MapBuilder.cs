using XRL.EditorFormats.Map;

namespace XRL.World.ZoneBuilders;

public class MapBuilder
{
	public string FileName;

	public bool ClearBeforePlacingObjectsIfObjectsExist;

	public MapBuilder()
	{
	}

	public MapBuilder(string mapFileName)
		: this()
	{
		FileName = mapFileName;
	}

	public void Clear(Cell C)
	{
		C.Clear(null, Important: false, Combat: true);
	}

	public static bool BuildFromFile(Zone Z, string FileName, bool Clear = false)
	{
		return new MapBuilder
		{
			FileName = FileName,
			ClearBeforePlacingObjectsIfObjectsExist = Clear
		}.BuildZone(Z);
	}

	public bool BuildZone(Zone Z)
	{
		MapFile mapFile = MapFile.LoadWithMods(FileName);
		if (mapFile.width == 0)
		{
			MetricsManager.LogError("Couldn't find the map: " + FileName);
			return false;
		}
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				Cell cell = Z.GetCell(i, j);
				if (cell != null)
				{
					MapFileCell mapFileCell = mapFile.Cells[i, j];
					if (!ClearBeforePlacingObjectsIfObjectsExist)
					{
						mapFileCell.ApplyTo(cell);
					}
					else
					{
						mapFileCell.ApplyTo(cell, CheckEmpty: true, Clear);
					}
				}
			}
		}
		return true;
	}
}

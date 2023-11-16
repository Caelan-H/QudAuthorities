using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class BethesdaColdZone
{
	public bool BuildZone(Zone Z)
	{
		Z.BaseTemperature = 25 - (Z.Z - 10) * 9;
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				Cell cell = Z.Map[i][j];
				BlueTile.PaintCell(cell);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.pPhysics != null)
					{
						if (gameObject.HasProperty("StartFrozen"))
						{
							gameObject.pPhysics.Temperature = gameObject.pPhysics.BrittleTemperature - 30;
						}
						else if (gameObject.Stat("ColdResistance") < 100)
						{
							gameObject.pPhysics.Temperature = gameObject.pPhysics.AmbientTemperature;
						}
					}
				}
			}
		}
		return true;
	}
}

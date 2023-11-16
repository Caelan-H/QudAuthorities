namespace XRL.World;

public class CellBlueprint
{
	public string Name;

	public string Inherits;

	public string ApplyTo;

	public string LandingZone;

	public bool Mutable = true;

	public ZoneBlueprint[,,] LevelBlueprint = new ZoneBlueprint[Definitions.Width, Definitions.Height, Definitions.Layers];

	public void CopyFrom(CellBlueprint ParentBlueprint)
	{
		for (int i = 0; i < Definitions.Width; i++)
		{
			for (int j = 0; j < Definitions.Height; j++)
			{
				for (int k = 0; k < Definitions.Layers; k++)
				{
					LevelBlueprint[i, j, k] = ParentBlueprint.LevelBlueprint[i, j, k];
				}
			}
		}
	}
}

namespace XRL.World.ZoneBuilders;

public class CaveSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "Cave", "South");
	}
}

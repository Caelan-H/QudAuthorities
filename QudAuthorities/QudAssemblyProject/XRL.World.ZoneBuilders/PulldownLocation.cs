namespace XRL.World.ZoneBuilders;

public class PulldownLocation
{
	public int x;

	public int y;

	public bool BuildZone(Zone Z)
	{
		ZoneManager.instance.SetZoneProperty(Z.ZoneID, "pulldownLocation", x + "," + y);
		return true;
	}
}

namespace XRL.World.ZoneBuilders;

public class FlagInside
{
	public bool BuildZone(Zone Z)
	{
		Z.SetZoneProperty("inside", "1");
		foreach (GameObject @object in Z.GetObjects("DaylightWidget"))
		{
			@object.Destroy();
		}
		return true;
	}
}

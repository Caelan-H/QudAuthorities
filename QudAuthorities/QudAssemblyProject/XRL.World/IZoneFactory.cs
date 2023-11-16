namespace XRL.World;

public abstract class IZoneFactory
{
	public virtual bool CanBuildZone(string zoneid)
	{
		return true;
	}

	public abstract Zone BuildZone(string zoneid);

	public virtual void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
	}
}

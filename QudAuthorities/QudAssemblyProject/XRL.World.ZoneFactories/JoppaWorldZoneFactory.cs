using XRL.World.Parts;

namespace XRL.World.ZoneFactories;

public class JoppaWorldZoneFactory : IZoneFactory
{
	public override bool CanBuildZone(string zoneid)
	{
		if (zoneid.Contains("."))
		{
			return zoneid.LastIndexOf("-") > zoneid.LastIndexOf(".");
		}
		return false;
	}

	public override Zone BuildZone(string zoneid)
	{
		return new Zone();
	}

	public override void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
		zone.GetObjectsWithPart("Physics").ForEach(delegate(GameObject o)
		{
			if (o.HasTagOrProperty("Wall") || o.HasPropertyOrTag("PaintedWall"))
			{
				o.AddPart(new HologramWallMaterial());
			}
			else
			{
				o.AddPart(new HologramMaterial());
			}
		});
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
	}
}

using System.Collections.Generic;
using System.Linq;
using XRL.World.Parts;

namespace XRL.World.ZoneFactories;

public class ThinWorldZoneFactory : IZoneFactory
{
	public override Zone BuildZone(string zoneid)
	{
		if (zoneid == "ThinWorld")
		{
			Zone zone = new Zone(80, 25);
			zone.GetCells().ForEach(delegate(Cell c)
			{
				c.AddObject("TerrainBrightsheol");
			});
			return zone;
		}
		if (ZoneManager.instance.CachedZones.Any((KeyValuePair<string, Zone> z) => z.Key.StartsWith("ThinWorld.")))
		{
			return ZoneManager.instance.CachedZones.First((KeyValuePair<string, Zone> z) => z.Key.StartsWith("ThinWorld.")).Value;
		}
		Zone zone2 = new Zone(80, 25);
		zone2.ZoneID = zoneid;
		zone2.loadMap("Brightsheol.rpm");
		zone2.GetCell(0, 0).AddObject("ThinWorldWidget");
		zone2.DisplayName = "Garden at the Gate to Brightsheol";
		return zone2;
	}

	public override void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
		zone.GetObjectsWithPart("Physics").ForEach(delegate(GameObject o)
		{
			if (o.HasTagOrProperty("Wall") || o.HasPropertyOrTag("PaintedWall"))
			{
				o.AddPart(new HologramWallMaterial());
			}
			else if (!o.HasTagOrProperty("NoThinWorldHologram"))
			{
				o.AddPart(new HologramMaterial());
			}
			if (o.HasPart("UnityPrefabImposter"))
			{
				UnityPrefabImposter part = o.GetPart<UnityPrefabImposter>();
				if (part.prefabID == "Prefabs/Imposters/StatueBase")
				{
					part.prefabID = "Prefabs/Imposters/BlueStatueBase";
				}
			}
		});
		zone.SetZoneProperty("inside", "1");
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
	}
}

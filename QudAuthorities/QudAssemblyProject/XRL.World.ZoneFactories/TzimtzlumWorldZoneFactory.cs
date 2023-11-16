using System.Collections.Generic;

namespace XRL.World.ZoneFactories;

public class TzimtzlumWorldZoneFactory : IZoneFactory
{
	public override Zone BuildZone(string ZoneID)
	{
		if (ZoneID == "Tzimtzlum")
		{
			Zone zone = new Zone(80, 25);
			zone.GetCells().ForEach(delegate(Cell c)
			{
				c.AddObject("TerrainTzimtzlum");
			});
			return zone;
		}
		ClamSystem clamSystem = The.Game.RequireSystem(() => new ClamSystem());
		if (ZoneID != clamSystem.ClamWorldId)
		{
			return clamSystem.GetClamZone();
		}
		Zone zone2 = new Zone(80, 25);
		zone2.ZoneID = clamSystem.ClamWorldId;
		zone2.loadMap("Tzimtzlum.rpm");
		zone2.DisplayName = "Tzimtzlum";
		zone2.IncludeContextInZoneDisplay = false;
		zone2.IncludeStratumInZoneDisplay = false;
		zone2.GetCell(0, 0).RequireObject("ZoneMusic").SetStringProperty("Track", "Clam Dimension");
		zone2.Built = true;
		List<GameObject> objects = zone2.GetObjects("Giant Clam");
		if (objects.Count == 0)
		{
			MetricsManager.LogError("Tzimtzlum generated without clams");
		}
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].SetIntProperty("ClamId", i);
		}
		The.Game.ZoneManager.SetZoneProperty(ZoneID, "SpecialUpMessage", "You’re in a pocket dimension with no worldmap.");
		return zone2;
	}

	public override void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
		zone.SetZoneProperty("inside", "1");
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
	}
}

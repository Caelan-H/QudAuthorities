using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class Spire
{
	public int RuinLevel;

	public string ZonesWide = "1";

	public string ZonesHigh = "1";

	public bool BuildZone(Zone Z)
	{
		int num = 0;
		SpireZoneTemplate spireZoneTemplate;
		Ruiner ruiner;
		do
		{
			num++;
			ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
			if (zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.SpireZoneTemplate") != null)
			{
				spireZoneTemplate = ((SpireZoneTemplate)zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.SpireZoneTemplate")).Copy();
			}
			else
			{
				spireZoneTemplate = new SpireZoneTemplate();
				spireZoneTemplate.New(Z.Width, Z.Height, ZonesWide.RollCached(), ZonesHigh.RollCached());
				zoneManager.SetZoneColumnProperty(Z.ZoneID, "Builder.Ruins.SpireZoneTemplate", spireZoneTemplate);
				spireZoneTemplate = spireZoneTemplate.Copy();
			}
			ruiner = new Ruiner();
			Z.ClearReachableMap();
			spireZoneTemplate.GenerateRooms(Z.GetZoneZ() - 10);
		}
		while (!spireZoneTemplate.EnsureConnections(Z) && num < 20);
		spireZoneTemplate.BuildZone(Z, Z.GetZoneZ() > 10);
		RuinLevel = 50 - Z.GetZoneZ();
		if (RuinLevel > 0)
		{
			ruiner.RuinZone(Z, 50 - Z.GetZoneZ(), Z.GetZoneZ() > 10);
		}
		bool flag = false;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "StairsUp" || zoneConnection.Type == "StairsDown")
			{
				flag = true;
				Z.BuildReachableMap(zoneConnection.X, zoneConnection.Y);
				break;
			}
		}
		if (!flag)
		{
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					if (!Z.GetCell(i, j).IsOccluding())
					{
						Z.BuildReachableMap(i, j);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XRL.World;

[Serializable]
public class WorldBlueprint
{
	public string DisplayName;

	public string Name;

	public string Map;

	public string ZoneFactory;

	public string ZoneFactoryRegex;

	public string Plane;

	public string Protocol;

	public List<ZoneBuilderBlueprint> Builders = new List<ZoneBuilderBlueprint>();

	public Dictionary<string, CellBlueprint> CellBlueprintsByApplication = new Dictionary<string, CellBlueprint>();

	public Dictionary<string, CellBlueprint> CellBlueprintsByName = new Dictionary<string, CellBlueprint>();

	public bool testZoneFactoryRegex(string zoneid)
	{
		if (string.IsNullOrEmpty(ZoneFactoryRegex) || string.IsNullOrEmpty(zoneid))
		{
			return false;
		}
		try
		{
			return Regex.IsMatch(zoneid, ZoneFactoryRegex);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			return false;
		}
	}

	public Zone FactoryBuild(string zoneid)
	{
		return (Activator.CreateInstance(ModManager.ResolveType("XRL.World.ZoneFactories." + ZoneFactory)) as IZoneFactory).BuildZone(zoneid);
	}
}

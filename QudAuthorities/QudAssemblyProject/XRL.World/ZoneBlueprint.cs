using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World;

public class ZoneBlueprint
{
	public string Name;

	public string Level;

	public int Tier;

	public string x;

	public string y;

	public string GroundLiquid;

	public bool disableForcedConnections;

	public bool ProperName;

	public string NameContext;

	public string IndefiniteArticle;

	public string DefiniteArticle;

	public bool IncludeContextInZoneDisplay = true;

	public bool IncludeStratumInZoneDisplay = true;

	public bool HasWeather;

	public string WindSpeed;

	public string WindDirections;

	public string WindDuration;

	public List<ZoneMapBlueprint> Maps = new List<ZoneMapBlueprint>();

	public List<ZoneBuilderBlueprint> Builders = new List<ZoneBuilderBlueprint>();

	public List<ZoneBuilderBlueprint> PostBuilders = new List<ZoneBuilderBlueprint>();

	public List<ZoneEncounterBlueprint> Encounters = new List<ZoneEncounterBlueprint>();

	public ZoneBlueprint(ZoneBlueprint Parent)
	{
		if (Parent == null)
		{
			return;
		}
		Level = "-";
		Tier = Parent.Tier;
		x = "-";
		y = "-";
		if (!string.IsNullOrEmpty(Parent.Name))
		{
			Name = Parent.Name;
		}
		foreach (ZoneMapBlueprint map in Parent.Maps)
		{
			ZoneMapBlueprint item = new ZoneMapBlueprint
			{
				File = map.File
			};
			Maps.Add(item);
		}
		foreach (ZoneEncounterBlueprint encounter in Parent.Encounters)
		{
			ZoneEncounterBlueprint zoneEncounterBlueprint = new ZoneEncounterBlueprint
			{
				Table = encounter.Table,
				Amount = encounter.Amount
			};
			foreach (string key in encounter.Parameters.Keys)
			{
				zoneEncounterBlueprint.Parameters.Add(key, encounter.Parameters[key]);
			}
			Encounters.Add(zoneEncounterBlueprint);
		}
		foreach (ZoneBuilderBlueprint builder in Parent.Builders)
		{
			ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint
			{
				Class = builder.Class
			};
			foreach (string key2 in builder.Parameters.Keys)
			{
				if (zoneBuilderBlueprint.Parameters == null)
				{
					zoneBuilderBlueprint.Parameters = new Dictionary<string, object>();
				}
				zoneBuilderBlueprint.Parameters.Add(key2, builder.Parameters[key2]);
			}
			Builders.Add(zoneBuilderBlueprint);
		}
		foreach (ZoneBuilderBlueprint postBuilder in Parent.PostBuilders)
		{
			ZoneBuilderBlueprint zoneBuilderBlueprint2 = new ZoneBuilderBlueprint
			{
				Class = postBuilder.Class
			};
			foreach (string key3 in postBuilder.Parameters.Keys)
			{
				if (zoneBuilderBlueprint2.Parameters == null)
				{
					zoneBuilderBlueprint2.Parameters = new Dictionary<string, object>();
				}
				zoneBuilderBlueprint2.Parameters.Add(key3, postBuilder.Parameters[key3]);
			}
			PostBuilders.Add(zoneBuilderBlueprint2);
		}
	}

	public bool AnyBuilder(Func<ZoneBuilderBlueprint, bool> p)
	{
		if (!Builders.Any(p))
		{
			return PostBuilders.Any(p);
		}
		return true;
	}
}

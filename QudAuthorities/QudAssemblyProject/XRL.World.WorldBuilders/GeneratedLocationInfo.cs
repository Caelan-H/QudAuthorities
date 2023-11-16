using System;
using Genkit;
using Qud.API;

namespace XRL.World.WorldBuilders;

[Serializable]
public class GeneratedLocationInfo
{
	public string name;

	public string ownerID;

	public string secretID;

	public string attribute;

	public Location2D zoneLocation;

	public string targetZone;

	public int distanceTo(Location2D location)
	{
		return zoneLocation.Distance(location);
	}

	public bool isUndiscovered()
	{
		if (string.IsNullOrEmpty(secretID))
		{
			return false;
		}
		return !JournalAPI.IsMapOrVillageNoteRevealed(secretID);
	}
}

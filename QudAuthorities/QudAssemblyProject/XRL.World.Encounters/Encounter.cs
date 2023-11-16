using System.Collections.Generic;

namespace XRL.World.Encounters;

public class Encounter
{
	public string Density = "minimum";

	public List<GameObject> Objects = new List<GameObject>();

	public List<Encounter> SubEncounters = new List<Encounter>();

	public List<string> ZoneBuilders = new List<string>();
}

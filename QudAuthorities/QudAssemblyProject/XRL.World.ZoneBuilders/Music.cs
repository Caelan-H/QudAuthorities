namespace XRL.World.ZoneBuilders;

public class Music
{
	public string Track = "";

	public string Chance = "100";

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).RequireObject("ZoneMusic").SetStringProperty("Track", Track);
		return true;
	}
}

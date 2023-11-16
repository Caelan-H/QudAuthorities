namespace XRL.World.ZoneBuilders;

public class AddEncounterBuilder
{
	public string Table;

	public bool BuildZone(Zone Z)
	{
		ZoneManager.ApplyEncounterBlueprintToZone(new ZoneEncounterBlueprint(Table)
		{
			Amount = "medium"
		}, Z);
		return true;
	}
}

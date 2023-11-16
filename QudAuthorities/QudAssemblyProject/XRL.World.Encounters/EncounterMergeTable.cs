namespace XRL.World.Encounters;

public class EncounterMergeTable
{
	public string Table;

	public int Weight;

	public EncounterMergeTable MakeCopy()
	{
		return new EncounterMergeTable
		{
			Table = Table,
			Weight = Weight
		};
	}
}

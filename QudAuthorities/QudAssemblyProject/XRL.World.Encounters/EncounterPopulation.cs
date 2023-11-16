namespace XRL.World.Encounters;

public class EncounterPopulation : EncounterObjectBase
{
	public string _Table;

	public string Number;

	public string Table
	{
		get
		{
			if (!string.IsNullOrEmpty(_Table) && _Table.Contains("{zonetier}"))
			{
				return _Table.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
			}
			return _Table;
		}
		set
		{
			_Table = value;
		}
	}
}

using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class AddLocationToRegion : HistoricEvent
{
	private string locationName;

	public AddLocationToRegion(string _locationName)
	{
		locationName = _locationName;
	}

	public override void Generate()
	{
		addEntityListItem("locations", locationName);
		duration = 0L;
	}
}

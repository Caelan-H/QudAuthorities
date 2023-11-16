using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class DiscoveredLocation : HistoricEvent
{
	public override void Generate()
	{
		duration = Random(5, 8);
		HistoricEntity newEntity = history.GetNewEntity(year - Random(0, 1000));
		newEntity.ApplyEvent(new LocationConstructed());
		setEventProperty("location", newEntity.id);
		setEventProperty("gospel", "discovered " + newEntity.GetSnapshotAtYear(year).GetProperty("name"));
	}
}

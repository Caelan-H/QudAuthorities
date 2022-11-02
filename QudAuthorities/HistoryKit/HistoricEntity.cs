using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Annals;
using XRL.Core;
using XRL.Rules;
using XRL.World;

namespace HistoryKit;

[Serializable]
public class HistoricEntity
{
	public delegate bool eventMatchingDelegate(HistoricEvent ev);

	public const long INVALID_DATE = long.MinValue;

	public const long LAST_ACTIVE_YEAR = long.MinValue;

	public string id;

	[NonSerialized]
	public History _history;

	public List<HistoricEvent> events = new List<HistoricEvent>();

	public History history
	{
		get
		{
			if (_history == null)
			{
				return XRLCore.Core.Game.sultanHistory;
			}
			return _history;
		}
		set
		{
			_history = value;
		}
	}

	public long firstYear
	{
		get
		{
			if (events == null || events.Count == 0)
			{
				return long.MinValue;
			}
			return events[0].year;
		}
	}

	public long lastYear
	{
		get
		{
			if (events == null || events.Count == 0)
			{
				return long.MinValue;
			}
			return events[events.Count - 1].year + events[events.Count - 1].duration;
		}
	}

	public HistoricEntity(History history)
	{
		this.history = history;
	}

	public static HistoricEntity Load(SerializationReader reader, History history)
	{
		HistoricEntity historicEntity = new HistoricEntity(history);
		historicEntity.id = reader.ReadString();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			HistoricEvent historicEvent = HistoricEvent.Load(reader);
			historicEvent.entity = historicEntity;
			historicEvent.history = history;
			historicEntity.events.Add(historicEvent);
		}
		return historicEntity;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(id);
		writer.Write(events.Count);
		foreach (HistoricEvent @event in events)
		{
			@event.Save(writer);
		}
	}

	public HistoricEvent GetRandomEventWhereDelegate(eventMatchingDelegate matcher, Random R = null)
	{
		if (R == null)
		{
			R = Stat.Rand;
		}
		List<HistoricEvent> list = new List<HistoricEvent>();
		for (int i = 0; i < events.Count; i++)
		{
			if (matcher(events[i]))
			{
				list.Add(events[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[R.Next(0, list.Count)];
	}

	public void SetPropertyAtCurrentYear(string name, string value)
	{
		ApplyEvent(new SetEntityProperty(name, value));
	}

	public void MutateListPropertyAtCurrentYear(string name, Func<string, string> mutation)
	{
		ApplyEvent(new MutateListProperty(name, mutation, GetCurrentSnapshot()));
	}

	public void ApplyEvent(HistoricEvent newEvent, long year = long.MinValue)
	{
		if (year == long.MinValue)
		{
			newEvent.year = lastYear;
		}
		else
		{
			newEvent.year = year;
		}
		newEvent.SetEntityHistory(this, history);
		history.SetupEvent(newEvent);
		newEvent.Generate();
		events.Add(newEvent);
		history.AddEvent(newEvent);
	}

	public HistoricEntitySnapshot GetCurrentSnapshot()
	{
		return GetSnapshotAtYear(history.currentYear);
	}

	public HistoricEntitySnapshot GetSnapshotAtYear(long year)
	{
		HistoricEntitySnapshot historicEntitySnapshot = new HistoricEntitySnapshot(this);
		if (events == null || events.Count == 0)
		{
			return historicEntitySnapshot;
		}
		foreach (HistoricEvent item in events.OrderBy((HistoricEvent ev) => ev.year))
		{
			if (item.year <= year)
			{
				item.applyToSnapshot(historicEntitySnapshot);
			}
		}
		return historicEntitySnapshot;
	}
}

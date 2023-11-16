using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;

namespace HistoryKit;

[Serializable]
public class HistoricEntityList : IEnumerable<HistoricEntity>, IEnumerable
{
	public delegate bool entityMatchingDelegate(HistoricEntity entity);

	public delegate void entityForeachDelegate(HistoricEntity entity);

	public List<HistoricEntity> entities = new List<HistoricEntity>();

	public int Count => entities.Count;

	public IEnumerator<HistoricEntity> GetEnumerator()
	{
		return entities.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return entities.GetEnumerator();
	}

	public HistoricEntity GetRandomElement(Random R = null)
	{
		if (entities.Count == 0)
		{
			return null;
		}
		if (R == null)
		{
			R = Stat.Rand;
		}
		return entities[R.Next(0, entities.Count)];
	}

	public HistoricEntity GetEntity(string id)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].id.EqualsNoCase(id))
			{
				return entities[i];
			}
		}
		return null;
	}

	public HistoricEntityList ForEach(entityForeachDelegate matchingDelegate)
	{
		HistoricEntityList historicEntityList = new HistoricEntityList();
		for (int i = 0; i < entities.Count; i++)
		{
			matchingDelegate(entities[i]);
			historicEntityList.entities.Add(entities[i]);
		}
		return historicEntityList;
	}

	public HistoricEntityList GetEntitiesByDelegate(entityMatchingDelegate matchingDelegate)
	{
		HistoricEntityList historicEntityList = new HistoricEntityList();
		for (int i = 0; i < entities.Count; i++)
		{
			if (matchingDelegate(entities[i]))
			{
				historicEntityList.entities.Add(entities[i]);
			}
		}
		return historicEntityList;
	}

	public HistoricEntityList GetEntitiesWithProperty(string property)
	{
		return GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).properties.ContainsKey(property));
	}

	public HistoricEntityList GetEntitiesWherePropertyEquals(string property, string value)
	{
		return GetEntitiesByDelegate(delegate(HistoricEntity entity)
		{
			HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
			return snapshotAtYear.properties.ContainsKey(property) && snapshotAtYear.properties[property] == value;
		});
	}

	public HistoricEntityList GetEntitiesWithListProperty(string property)
	{
		return GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).listProperties.ContainsKey(property));
	}

	public HistoricEntityList GetEntitiesWithListPropertyThatContains(string property, string value)
	{
		return GetEntitiesByDelegate(delegate(HistoricEntity entity)
		{
			HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
			return snapshotAtYear.listProperties.ContainsKey(property) && snapshotAtYear.listProperties[property].Contains(value);
		});
	}

	public string Dump()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < entities.Count; i++)
		{
			HistoricEntitySnapshot snapshotAtYear = entities[i].GetSnapshotAtYear(entities[i].lastYear);
			stringBuilder.AppendLine("*** " + snapshotAtYear.properties["name"] + " ***");
			for (int j = 0; j < entities[i].events.Count; j++)
			{
				if (entities[i].events[j].hasEventProperty("gospel"))
				{
					stringBuilder.AppendLine("  @" + entities[i].events[j].year + "  " + entities[i].events[j].getEventProperty("gospel"));
				}
			}
		}
		return stringBuilder.ToString();
	}
}

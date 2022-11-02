using System;
using System.Collections.Generic;
using Qud.API;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace HistoryKit;

[Serializable]
public class HistoricEvent
{
	public long id;

	public long year;

	public long duration = 1L;

	[NonSerialized]
	public History history;

	[NonSerialized]
	public HistoricEntity entity;

	public Dictionary<string, string> eventProperties;

	public Dictionary<string, string> entityProperties;

	public Dictionary<string, List<string>> addedListProperties;

	public Dictionary<string, List<string>> removedListProperties;

	public Dictionary<string, HistoricPerspective> perspectives;

	public static HistoricEvent Load(SerializationReader reader)
	{
		HistoricEvent historicEvent = new HistoricEvent();
		historicEvent.id = reader.ReadInt64();
		historicEvent.year = reader.ReadInt64();
		historicEvent.duration = reader.ReadInt64();
		if (reader.ReadBoolean())
		{
			historicEvent.eventProperties = reader.ReadDictionary<string, string>();
		}
		if (reader.ReadBoolean())
		{
			historicEvent.entityProperties = reader.ReadDictionary<string, string>();
		}
		if (reader.ReadBoolean())
		{
			int num = reader.ReadInt32();
			historicEvent.addedListProperties = new Dictionary<string, List<string>>(num);
			for (int i = 0; i < num; i++)
			{
				string key = reader.ReadString();
				List<string> value = reader.ReadStringList();
				historicEvent.addedListProperties.Add(key, value);
			}
		}
		if (reader.ReadBoolean())
		{
			int num2 = reader.ReadInt32();
			historicEvent.removedListProperties = new Dictionary<string, List<string>>(num2);
			for (int j = 0; j < num2; j++)
			{
				string key2 = reader.ReadString();
				List<string> value2 = reader.ReadStringList();
				historicEvent.removedListProperties.Add(key2, value2);
			}
		}
		int num3 = reader.ReadInt32();
		if (num3 > 0)
		{
			historicEvent.perspectives = new Dictionary<string, HistoricPerspective>(num3);
			for (int k = 0; k < num3; k++)
			{
				HistoricPerspective historicPerspective = HistoricPerspective.Load(reader);
				if (historicPerspective.eventId != historicEvent.id)
				{
					throw new Exception("eventId mismatch: " + historicEvent.id + " vs. " + historicPerspective.eventId);
				}
				historicEvent.perspectives.Add(historicPerspective.entityId, historicPerspective);
			}
		}
		return historicEvent;
	}

	public virtual void Save(SerializationWriter writer)
	{
		writer.Write(id);
		writer.Write(year);
		writer.Write(duration);
		writer.Write(eventProperties != null);
		if (eventProperties != null)
		{
			writer.Write(eventProperties);
		}
		writer.Write(entityProperties != null);
		if (entityProperties != null)
		{
			writer.Write(entityProperties);
		}
		writer.Write(addedListProperties != null);
		if (addedListProperties != null)
		{
			writer.Write(addedListProperties.Count);
			foreach (KeyValuePair<string, List<string>> addedListProperty in addedListProperties)
			{
				writer.Write(addedListProperty.Key);
				writer.Write(addedListProperty.Value);
			}
		}
		writer.Write(removedListProperties != null);
		if (removedListProperties != null)
		{
			writer.Write(removedListProperties.Count);
			foreach (KeyValuePair<string, List<string>> removedListProperty in removedListProperties)
			{
				writer.Write(removedListProperty.Key);
				writer.Write(removedListProperty.Value);
			}
		}
		if (perspectives == null)
		{
			writer.Write(0);
			return;
		}
		writer.Write(perspectives.Count);
		foreach (string key in perspectives.Keys)
		{
			perspectives[key].Save(writer);
		}
	}

	public void addListProperty(string name, List<string> value)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		if (addedListProperties.ContainsKey(name))
		{
			addedListProperties[name].AddRange(value);
		}
		else
		{
			addedListProperties[name] = value;
		}
	}

	public void changeListProperty(string name, List<string> oldValues, List<string> newValues)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		if (removedListProperties == null)
		{
			removedListProperties = new Dictionary<string, List<string>>();
		}
		addedListProperties[name] = newValues;
		removedListProperties[name] = oldValues;
	}

	public bool hasEventProperty(string key)
	{
		if (eventProperties == null)
		{
			return false;
		}
		return eventProperties.ContainsKey(key);
	}

	public string getEventProperty(string name, string defaultValue = null)
	{
		if (eventProperties == null)
		{
			return defaultValue;
		}
		if (eventProperties.TryGetValue(name, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public void setEventProperty(string key, string value)
	{
		if (eventProperties == null)
		{
			eventProperties = new Dictionary<string, string>();
		}
		value = ExpandString(value);
		if (eventProperties.ContainsKey(key))
		{
			eventProperties[key] = value;
		}
		else
		{
			eventProperties.Add(key, value);
		}
	}

	public string GetEventProperty(string name, string _default = null)
	{
		if (eventProperties == null)
		{
			return _default;
		}
		if (!eventProperties.ContainsKey(name))
		{
			return _default;
		}
		return eventProperties[name];
	}

	public string ExpandString(string s)
	{
		return HistoricStringExpander.ExpandString(s, entity.GetSnapshotAtYear(year + duration), history);
	}

	public string ExpandString(string s, Dictionary<string, string> vars)
	{
		return HistoricStringExpander.ExpandString(s, entity.GetSnapshotAtYear(year + duration), history, vars);
	}

	public int Random(int low, int high)
	{
		return history.Random(low, high);
	}

	public virtual void SetEntityHistory(HistoricEntity entity, History history)
	{
		this.entity = entity;
		this.history = history;
	}

	public virtual void Generate()
	{
	}

	public void addEntityListItem(string list, string value, bool force = false)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		value = ExpandString(value);
		if (!addedListProperties.ContainsKey(list))
		{
			addedListProperties.Add(list, new List<string>());
		}
		if (force || !addedListProperties[list].Contains(value))
		{
			addedListProperties[list].Add(value);
		}
	}

	public void removeEntityListItem(string list, string value)
	{
		if (removedListProperties == null)
		{
			removedListProperties = new Dictionary<string, List<string>>();
		}
		value = ExpandString(value);
		if (!removedListProperties.ContainsKey(list))
		{
			removedListProperties.Add(list, new List<string>());
		}
		removedListProperties[list].Add(value);
	}

	public void setEntityProperty(string name, string value)
	{
		if (entityProperties == null)
		{
			entityProperties = new Dictionary<string, string>();
		}
		value = ExpandString(value);
		if (entityProperties.ContainsKey(name))
		{
			entityProperties[name] = value;
		}
		else
		{
			entityProperties.Add(name, value);
		}
	}

	public virtual void applyToSnapshot(HistoricEntitySnapshot snapshot)
	{
		if (entityProperties != null)
		{
			foreach (string key in entityProperties.Keys)
			{
				if (snapshot.properties.ContainsKey(key))
				{
					snapshot.properties[key] = entityProperties[key];
				}
				else
				{
					snapshot.properties.Add(key, entityProperties[key]);
				}
			}
		}
		if (removedListProperties != null)
		{
			foreach (string key2 in removedListProperties.Keys)
			{
				foreach (string item in removedListProperties[key2])
				{
					if (snapshot.listProperties.ContainsKey(key2) && snapshot.listProperties[key2].Contains(item))
					{
						snapshot.listProperties[key2].Remove(item);
					}
				}
			}
		}
		if (addedListProperties == null)
		{
			return;
		}
		foreach (string key3 in addedListProperties.Keys)
		{
			foreach (string item2 in addedListProperties[key3])
			{
				if (!snapshot.listProperties.ContainsKey(key3))
				{
					snapshot.listProperties.Add(key3, new List<string>());
				}
				snapshot.listProperties[key3].Add(item2);
			}
		}
	}

	public virtual HistoricPerspective getPerspective(HistoricEntity entity)
	{
		if (perspectives == null)
		{
			return null;
		}
		if (!perspectives.ContainsKey(entity.id))
		{
			return null;
		}
		return perspectives[entity.id];
	}

	public virtual HistoricPerspective getPerspective(HistoricEntitySnapshot snapshot)
	{
		return getPerspective(snapshot.entity);
	}

	public virtual HistoricPerspective requirePerspective(HistoricEntitySnapshot snapshot, object useFeeling = null)
	{
		if (perspectives == null)
		{
			perspectives = new Dictionary<string, HistoricPerspective>(1);
		}
		if (perspectives.ContainsKey(snapshot.entity.id))
		{
			return perspectives[snapshot.entity.id];
		}
		HistoricPerspective historicPerspective = new HistoricPerspective();
		historicPerspective.eventId = id;
		historicPerspective.entityId = snapshot.entity.id;
		snapshot.supplyPerspectiveColors(historicPerspective);
		if (useFeeling == null)
		{
			historicPerspective.randomizeFeeling();
		}
		else
		{
			historicPerspective.feeling = (int)useFeeling;
		}
		perspectives[snapshot.entity.id] = historicPerspective;
		return historicPerspective;
	}

	public bool HasUnrevealedSecret()
	{
		if (HasUnrevealedRegion())
		{
			return true;
		}
		if (HasUnrevealedRelicQuest())
		{
			return true;
		}
		if (JournalAPI.HasUnrevealedSultanEvent(id))
		{
			return true;
		}
		return false;
	}

	public JournalSultanNote Reveal()
	{
		PerformRegionReveal();
		PerformRelicQuestReveal();
		return JournalAPI.RevealSultanEvent(id);
	}

	public bool HasUnrevealedRegion()
	{
		if (hasEventProperty("revealsRegion") && getEventProperty("revealsRegion") != null && getEventProperty("revealsRegion") != "unknown")
		{
			string eventProperty = getEventProperty("revealsRegion");
			if (!XRLCore.Core.Game.HasIntGameState("sultanRegionReveal_" + eventProperty))
			{
				return true;
			}
		}
		return false;
	}

	public void PerformRegionReveal()
	{
		if (!hasEventProperty("revealsRegion") || getEventProperty("revealsRegion") == null || !(getEventProperty("revealsRegion") != "unknown"))
		{
			return;
		}
		string revealedRegion = getEventProperty("revealsRegion");
		if (XRLCore.Core.Game.HasIntGameState("sultanRegionReveal_" + revealedRegion))
		{
			return;
		}
		XRLCore.Core.Game.SetIntGameState("sultanRegionReveal_" + revealedRegion, 1);
		Vector2i vector2i = XRLCore.Core.Game.GetObjectGameState("sultanRegionPosition_" + revealedRegion) as Vector2i;
		if (vector2i != null)
		{
			Zone zone = XRLCore.Core.Game.ZoneManager.GetZone("JoppaWorld");
			Popup.Show("You discover the location of " + revealedRegion + ".");
			The.Game.Systems.ForEach(delegate(IGameSystem s)
			{
				s.LocationDiscovered(revealedRegion);
			});
			zone.GetCell(vector2i.x, vector2i.y).FireEvent("SultanReveal");
			int zoneTier = XRLCore.Core.Game.ZoneManager.GetZoneTier("JoppaWorld." + vector2i.x + "." + vector2i.y + ".1.1.10");
			AddQuestsForRegion(revealedRegion, zoneTier);
			JournalAPI.AddAccomplishment("You discovered the location of " + revealedRegion + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered the location of " + revealedRegion + ", once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
	}

	public static void AddQuestsForRegion(string regionName, int zoneTier)
	{
		QuestStep questStep = new QuestStep();
		questStep.ID = Guid.NewGuid().ToString();
		questStep.Name = "Travel to the historical site of " + regionName;
		questStep.Text = "";
		questStep.XP = 250 * zoneTier;
		Quest quest = new Quest();
		quest.ID = "Visit " + regionName;
		quest.Name = "Visit " + regionName;
		quest.StepsByID = new Dictionary<string, QuestStep>();
		quest.StepsByID.Add(questStep.ID, questStep);
		quest.Level = 1;
		quest._Manager = new VisitSultanDungeonQuestManager();
		(quest._Manager as VisitSultanDungeonQuestManager).Region = regionName;
		XRLCore.Core.Game.StartQuest(quest);
	}

	public bool HasUnrevealedRelicQuest()
	{
		if (hasEventProperty("revealsItem"))
		{
			string eventProperty = GetEventProperty("revealsItem");
			if (!XRLCore.Core.Game.HasIntGameState("sultanItemReveal_" + eventProperty))
			{
				return true;
			}
		}
		return false;
	}

	public void PerformRelicQuestReveal()
	{
		if (hasEventProperty("revealsItem"))
		{
			string eventProperty = GetEventProperty("revealsItem");
			if (!XRLCore.Core.Game.HasIntGameState("sultanItemReveal_" + eventProperty))
			{
				XRLCore.Core.Game.SetIntGameState("sultanItemReveal_" + eventProperty, 1);
				string eventProperty2 = GetEventProperty("revealsItemLocation");
				AddQuestsForRelic(eventProperty, eventProperty2);
			}
		}
	}

	public void AddQuestsForRelic(string relicName, string locationName)
	{
		QuestStep questStep = new QuestStep();
		questStep.ID = Guid.NewGuid().ToString();
		questStep.Name = "Locate the historical relic " + relicName + " at " + locationName + " and recover it";
		questStep.Text = "";
		questStep.XP = 1000;
		Quest quest = new Quest();
		quest.ID = "Recover " + relicName;
		quest.Name = "Recover " + relicName;
		quest.StepsByID = new Dictionary<string, QuestStep>();
		quest.StepsByID.Add(questStep.ID, questStep);
		quest.Level = 1;
		LocateRelicQuestManager locateRelicQuestManager = new LocateRelicQuestManager();
		locateRelicQuestManager.QuestID = quest.ID;
		locateRelicQuestManager.Relic = relicName;
		quest._Manager = locateRelicQuestManager;
		XRLCore.Core.Game.StartQuest(quest);
	}
}

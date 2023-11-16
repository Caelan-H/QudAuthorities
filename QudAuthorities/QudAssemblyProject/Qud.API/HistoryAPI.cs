using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HistoryKit;
using XRL;
using XRL.Core;
using XRL.World;

namespace Qud.API;

public static class HistoryAPI
{
	public static string GetEntityName(string id)
	{
		if (string.Equals(id, "The Yd Freehold"))
		{
			return "the Yd Freehold";
		}
		return XRLCore.Core?.Game?.sultanHistory?.GetEntity(id)?.GetCurrentSnapshot()?.GetProperty("name");
	}

	public static HistoricEvent GetEvent(long id)
	{
		return XRLCore.Core?.Game?.sultanHistory?.GetEvent(id);
	}

	public static List<HistoricEntity> GetSultans()
	{
		History history = XRLCore.Core?.Game?.sultanHistory;
		if (history == null)
		{
			return new List<HistoricEntity>();
		}
		HistoricEntityList entitiesWithProperty = history.GetEntitiesWithProperty("isCandidate");
		if (entitiesWithProperty == null)
		{
			return new List<HistoricEntity>();
		}
		return entitiesWithProperty.entities;
	}

	public static List<string> GetSultanHatedFactions(HistoricEntity sultan)
	{
		return sultan.GetCurrentSnapshot().GetList("hatedFactions");
	}

	public static List<string> GetSultanLikedFactions(HistoricEntity sultan)
	{
		return sultan.GetCurrentSnapshot().GetList("likedFactions");
	}

	public static List<HistoricEntity> GetVillages()
	{
		return new List<HistoricEntity>(XRLCore.Core.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "village").entities);
	}

	public static List<HistoricEntity> GetKnownVillages()
	{
		List<HistoricEntity> list = new List<HistoricEntity>();
		foreach (HistoricEntity entity in GetVillages())
		{
			if (JournalAPI.VillageNotes.Any((JournalVillageNote note) => note.revealed && note.villageID == entity.id))
			{
				list.Add(entity);
			}
		}
		return list;
	}

	public static HistoricEntitySnapshot GetVillageSnapshot(string Faction)
	{
		foreach (HistoricEntity entity in The.Game.sultanHistory.entities)
		{
			foreach (HistoricEvent @event in entity.events)
			{
				if (@event.entityProperties != null && @event.entityProperties.TryGetValue("type", out var value) && !(value != "village") && @event.entityProperties.TryGetValue("name", out var value2) && Faction.EndsWith(value2))
				{
					return entity.GetCurrentSnapshot();
				}
			}
		}
		return null;
	}

	public static string ExpandVillageText(string Text, string Faction = null, HistoricEntitySnapshot Snapshot = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Text);
		ExpandVillageText(stringBuilder, Faction, Snapshot);
		return stringBuilder.ToString();
	}

	public static void ExpandVillageText(StringBuilder Text, string Faction = null, HistoricEntitySnapshot Snapshot = null)
	{
		if (Snapshot == null && !Faction.IsNullOrEmpty())
		{
			Snapshot = GetVillageSnapshot(Faction);
		}
		string newValue = "a village";
		string newValue2 = "the act of procreating";
		string newValue3 = "those who oppose arable land and the act of procreating";
		string newValue4 = "roaming around idly";
		if (Snapshot != null)
		{
			Random r = new Random(Snapshot.Name.GetStableHashCode());
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.GetFactionMembers(Snapshot.GetProperty("baseFaction")).GetRandomElement(r);
			newValue = Snapshot.Name;
			newValue2 = Snapshot.GetRandomElementFromListProperty("sacredThings", null, r) ?? Snapshot.GetProperty("defaultSacredThing");
			newValue3 = Snapshot.GetRandomElementFromListProperty("profaneThings", null, r) ?? Snapshot.GetProperty("defaultProfaneThing");
			newValue4 = randomElement.GetxTag_CommaDelimited("TextFragments", "Activity", null, r);
		}
		Text.Replace("=village.name=", newValue).Replace("=village.sacred=", newValue2).Replace("=village.profane=", newValue3)
			.Replace("=village.activity=", newValue4);
	}

	public static HistoricEntitySnapshot GetSultanForPeriod(int period)
	{
		List<HistoricEntity> sultans = GetSultans();
		if (sultans == null || sultans.Count == 0)
		{
			return null;
		}
		return sultans.Where((HistoricEntity s) => s != null && s.GetCurrentSnapshot().GetProperty("period") == period.ToString()).Single().GetCurrentSnapshot();
	}

	public static List<string> GetLikedFactionsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("likedFactions"));
	}

	public static List<string> GetDislikedFactionsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("hatedFactions"));
	}

	public static List<string> GetDomainsForSultan(int period)
	{
		HistoricEntitySnapshot sultanForPeriod = GetSultanForPeriod(period);
		if (sultanForPeriod == null)
		{
			return new List<string>();
		}
		return new List<string>(sultanForPeriod.GetList("elements"));
	}

	public static List<HistoricEntity> GetKnownSultans()
	{
		History sultanHistory = XRLCore.Core.Game.sultanHistory;
		List<HistoricEntity> list = new List<HistoricEntity>();
		foreach (HistoricEntity entity in sultanHistory.GetEntitiesWithProperty("isCandidate").entities)
		{
			if (AnyKnownSultanEventsWithGospelsOrTombPropaganda(entity.id))
			{
				list.Add(entity);
			}
		}
		return list;
	}

	public static List<HistoricEvent> GetKnownSultanEventsWithGospels(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.hasEventProperty("gospel") && JournalAPI.KnowsSultanEvent(@event.id))
			{
				list.Add(@event);
			}
		}
		return list;
	}

	public static bool AnyKnownSultanEventsWithGospels(string sultanId)
	{
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.hasEventProperty("gospel") && JournalAPI.KnowsSultanEvent(@event.id))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyKnownSultanEventsWithGospelsOrTombPropaganda(string sultanId)
	{
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if ((@event.hasEventProperty("gospel") || @event.hasEventProperty("tombInscription")) && JournalAPI.KnowsSultanEvent(@event.id))
			{
				return true;
			}
		}
		return false;
	}

	public static HistoricEntity GetResheph()
	{
		return XRLCore.Core.Game.sultanHistory.GetEntitiesWithProperty("Resheph").entities[0];
	}

	public static List<HistoricEvent> GetSultanEventsWithGospels(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.hasEventProperty("gospel"))
			{
				list.Add(@event);
			}
		}
		return list;
	}

	public static List<HistoricEvent> GetSultanEventsWithTombInscriptions(string sultanId)
	{
		List<HistoricEvent> list = new List<HistoricEvent>();
		foreach (HistoricEvent @event in XRLCore.Core.Game.sultanHistory.GetEntity(sultanId).events)
		{
			if (@event.hasEventProperty("tombInscription"))
			{
				list.Add(@event);
			}
		}
		return list;
	}
}

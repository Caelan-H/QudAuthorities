using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.WorldBuilders;

[Serializable]
public class WorldInfo
{
	public Dictionary<string, List<Location2D>> terrainLocations = new Dictionary<string, List<Location2D>>();

	public Dictionary<int, List<Location2D>> tierLocations = new Dictionary<int, List<Location2D>>();

	public List<GeneratedLocationInfo> lairs = new List<GeneratedLocationInfo>();

	public List<GeneratedLocationInfo> ruins = new List<GeneratedLocationInfo>();

	public List<GeneratedLocationInfo> villages = new List<GeneratedLocationInfo>();

	public List<GeneratedLocationInfo> enemySettlements = new List<GeneratedLocationInfo>();

	public List<GeneratedLocationInfo> friendlySettlements = new List<GeneratedLocationInfo>();

	public static uint ROAD_NORTH = 1u;

	public static uint ROAD_SOUTH = 2u;

	public static uint ROAD_EAST = 4u;

	public static uint ROAD_WEST = 8u;

	public static uint ROAD_NONE = 16u;

	public static uint ROAD_START = 32u;

	public uint[,] RoadSystem;

	public uint[,] RiverSystem;

	private static Stack<string> OriginalDirectionStack = new Stack<string>();

	private static Stack<Location2D> LocationsToVisit = new Stack<Location2D>();

	private static Stack<string> LastStepDirectionStack = new Stack<string>();

	private static HashSet<Location2D> VisitedLocations = new HashSet<Location2D>();

	[NonSerialized]
	public Dictionary<Location2D, List<GeneratedLocationInfo>> locationTolocationMap;

	public List<List<GeneratedLocationInfo>> allLocationTypes => new List<List<GeneratedLocationInfo>> { lairs, ruins, villages, enemySettlements, friendlySettlements };

	public void ForeachGeneratedLocation(Action<GeneratedLocationInfo> action)
	{
		foreach (List<GeneratedLocationInfo> allLocationType in allLocationTypes)
		{
			foreach (GeneratedLocationInfo item in allLocationType)
			{
				action(item);
			}
		}
	}

	public bool AnyGeneratedLocation(Predicate<GeneratedLocationInfo> check)
	{
		foreach (List<GeneratedLocationInfo> allLocationType in allLocationTypes)
		{
			foreach (GeneratedLocationInfo item in allLocationType)
			{
				if (check(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RequireLocationToLocationMap()
	{
		if (locationTolocationMap != null)
		{
			return;
		}
		locationTolocationMap = new Dictionary<Location2D, List<GeneratedLocationInfo>>();
		foreach (List<GeneratedLocationInfo> allLocationType in allLocationTypes)
		{
			foreach (GeneratedLocationInfo item in allLocationType)
			{
				if (!locationTolocationMap.TryGetValue(item.zoneLocation, out var value))
				{
					value = new List<GeneratedLocationInfo>();
					locationTolocationMap.Add(item.zoneLocation, value);
				}
				value.Add(item);
			}
		}
	}

	public GeneratedLocationInfo GetGeneratedLocationAt(Location2D location)
	{
		if (location == null)
		{
			return null;
		}
		RequireLocationToLocationMap();
		if (locationTolocationMap.TryGetValue(location, out var value))
		{
			return value.GetRandomElement();
		}
		return null;
	}

	public List<GeneratedLocationInfo> GetGeneratedLocationsAt(Location2D location)
	{
		if (location == null)
		{
			return null;
		}
		RequireLocationToLocationMap();
		if (locationTolocationMap.TryGetValue(location, out var value))
		{
			return value;
		}
		return null;
	}

	public GeneratedLocationInfo FindLocationAlongPathFromLandmark(Location2D landmarkLocation, out string pathType, out string directionToDestination, out string directionFromLandmark, Predicate<Location2D> isValid = null)
	{
		List<uint[,]> list = new List<uint[,]> { RoadSystem, RiverSystem };
		List<string> list2 = new List<string> { "road", "river" };
		GeneratedLocationInfo generatedLocationInfo = null;
		pathType = null;
		directionToDestination = null;
		directionFromLandmark = null;
		string text = null;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i][landmarkLocation.x, landmarkLocation.y] == 0)
			{
				continue;
			}
			LocationsToVisit.Clear();
			VisitedLocations.Clear();
			VisitedLocations.Add(landmarkLocation);
			LocationsToVisit.Push(landmarkLocation);
			LastStepDirectionStack.Push(".");
			string d;
			while (LocationsToVisit.Count > 0)
			{
				Location2D location2D = LocationsToVisit.Pop();
				d = LastStepDirectionStack.Pop();
				if (OriginalDirectionStack.Count > 0)
				{
					text = OriginalDirectionStack.Pop();
				}
				if (location2D == null)
				{
					continue;
				}
				if (location2D != landmarkLocation)
				{
					generatedLocationInfo = GetGeneratedLocationAt(location2D);
					if (generatedLocationInfo != null && (isValid == null || isValid(generatedLocationInfo.zoneLocation)))
					{
						goto IL_0113;
					}
				}
				if (!(location2D != null) || location2D.x < 0 || location2D.y < 0 || location2D.x >= 240 || location2D.y >= 75)
				{
					continue;
				}
				if ((list[i][location2D.x, location2D.y] & ROAD_NORTH) != 0 && !VisitedLocations.Contains(location2D.FromDirection("N")))
				{
					LocationsToVisit.Push(location2D.FromDirection("N"));
					VisitedLocations.Add(location2D.FromDirection("N"));
					if (text == null)
					{
						OriginalDirectionStack.Push("N");
					}
					else
					{
						OriginalDirectionStack.Push(text);
					}
					LastStepDirectionStack.Push("N");
				}
				if ((list[i][location2D.x, location2D.y] & ROAD_SOUTH) != 0 && !VisitedLocations.Contains(location2D.FromDirection("S")))
				{
					LocationsToVisit.Push(location2D.FromDirection("S"));
					VisitedLocations.Add(location2D.FromDirection("S"));
					if (text == null)
					{
						OriginalDirectionStack.Push("S");
					}
					else
					{
						OriginalDirectionStack.Push(text);
					}
					LastStepDirectionStack.Push("S");
				}
				if ((list[i][location2D.x, location2D.y] & ROAD_EAST) != 0 && !VisitedLocations.Contains(location2D.FromDirection("E")))
				{
					LocationsToVisit.Push(location2D.FromDirection("E"));
					VisitedLocations.Add(location2D.FromDirection("E"));
					if (text == null)
					{
						OriginalDirectionStack.Push("E");
					}
					else
					{
						OriginalDirectionStack.Push(text);
					}
					LastStepDirectionStack.Push("E");
				}
				if ((list[i][location2D.x, location2D.y] & ROAD_WEST) != 0 && !VisitedLocations.Contains(location2D.FromDirection("W")))
				{
					LocationsToVisit.Push(location2D.FromDirection("W"));
					VisitedLocations.Add(location2D.FromDirection("W"));
					if (text == null)
					{
						OriginalDirectionStack.Push("W");
					}
					else
					{
						OriginalDirectionStack.Push(text);
					}
					LastStepDirectionStack.Push("W");
				}
			}
			continue;
			IL_0113:
			directionToDestination = Directions.GetExpandedDirection(text);
			directionFromLandmark = Directions.GetExpandedDirection(Directions.GetOppositeDirection(d));
			pathType = list2[i];
			break;
		}
		OriginalDirectionStack.Clear();
		LocationsToVisit.Clear();
		VisitedLocations.Clear();
		LastStepDirectionStack.Clear();
		return generatedLocationInfo;
	}

	public DynamicQuestDeliveryTarget ResolveDeliveryTargetForVillage(HistoricEntitySnapshot village, int minDistanceInclusive = 12, int maxDistanceInclusive = 18, int tierBroaden = 0, Predicate<Location2D> isLocationVaild = null)
	{
		DynamicQuestDeliveryTarget dynamicQuestDeliveryTarget = new DynamicQuestDeliveryTarget();
		dynamicQuestDeliveryTarget.type = PopulationManager.RollOneFrom("Delivery Quest Locations").Blueprint;
		Location2D villageLocation = Zone.zoneIDTo240x72Location(village.GetProperty("zoneID"));
		if (dynamicQuestDeliveryTarget.type == "Lair")
		{
			GeneratedLocationInfo randomElement = lairs.Where((GeneratedLocationInfo l) => isLocationVaild(l.zoneLocation) && villageLocation.ManhattanDistance(l.zoneLocation) >= minDistanceInclusive && villageLocation.ManhattanDistance(l.zoneLocation) <= maxDistanceInclusive).GetRandomElement();
			if (randomElement == null)
			{
				Debug.LogWarning("Couldn't find lair for village: " + village.GetProperty("name") + " at " + minDistanceInclusive + " to " + maxDistanceInclusive);
				return null;
			}
			dynamicQuestDeliveryTarget.displayName = randomElement.name;
			dynamicQuestDeliveryTarget.zoneId = randomElement.targetZone;
			dynamicQuestDeliveryTarget.secretId = randomElement.secretID;
			dynamicQuestDeliveryTarget.location = randomElement.zoneLocation;
		}
		else if (dynamicQuestDeliveryTarget.type == "Ruins")
		{
			GeneratedLocationInfo randomElement2 = ruins.Where((GeneratedLocationInfo l) => isLocationVaild(l.zoneLocation) && villageLocation.ManhattanDistance(l.zoneLocation) >= minDistanceInclusive && villageLocation.ManhattanDistance(l.zoneLocation) <= maxDistanceInclusive).GetRandomElement();
			if (randomElement2 == null)
			{
				Debug.LogWarning("Couldn't find ruin for village: " + village.GetProperty("name") + " at " + minDistanceInclusive + " to " + maxDistanceInclusive);
				return null;
			}
			dynamicQuestDeliveryTarget.displayName = randomElement2.name;
			dynamicQuestDeliveryTarget.zoneId = randomElement2.targetZone;
			dynamicQuestDeliveryTarget.secretId = randomElement2.secretID;
			dynamicQuestDeliveryTarget.location = randomElement2.zoneLocation;
		}
		else if (dynamicQuestDeliveryTarget.type == "Village")
		{
			GeneratedLocationInfo randomElement3 = villages.Where((GeneratedLocationInfo l) => isLocationVaild(l.zoneLocation) && villageLocation.ManhattanDistance(l.zoneLocation) >= minDistanceInclusive && villageLocation.ManhattanDistance(l.zoneLocation) <= maxDistanceInclusive).GetRandomElement();
			if (randomElement3 == null)
			{
				Debug.LogWarning("Couldn't find village for village: " + village.GetProperty("name") + " at " + minDistanceInclusive + " to " + maxDistanceInclusive);
				return null;
			}
			dynamicQuestDeliveryTarget.displayName = randomElement3.name;
			dynamicQuestDeliveryTarget.zoneId = randomElement3.targetZone;
			dynamicQuestDeliveryTarget.secretId = randomElement3.secretID;
			dynamicQuestDeliveryTarget.location = randomElement3.zoneLocation;
		}
		else if (dynamicQuestDeliveryTarget.type == "EnemySettlement")
		{
			GeneratedLocationInfo randomElement4 = enemySettlements.Where((GeneratedLocationInfo l) => isLocationVaild(l.zoneLocation) && villageLocation.ManhattanDistance(l.zoneLocation) >= minDistanceInclusive && villageLocation.ManhattanDistance(l.zoneLocation) <= maxDistanceInclusive).GetRandomElement();
			if (randomElement4 == null)
			{
				Debug.LogWarning("Couldn't find enemy settlement for village: " + village.GetProperty("name") + " at " + minDistanceInclusive + " to " + maxDistanceInclusive);
				return null;
			}
			dynamicQuestDeliveryTarget.displayName = randomElement4.name;
			dynamicQuestDeliveryTarget.zoneId = randomElement4.targetZone;
			dynamicQuestDeliveryTarget.secretId = randomElement4.secretID;
			dynamicQuestDeliveryTarget.location = randomElement4.zoneLocation;
		}
		else if (dynamicQuestDeliveryTarget.type == "FriendlySettlement")
		{
			GeneratedLocationInfo randomElement5 = friendlySettlements.Where((GeneratedLocationInfo l) => isLocationVaild(l.zoneLocation) && villageLocation.ManhattanDistance(l.zoneLocation) >= minDistanceInclusive && villageLocation.ManhattanDistance(l.zoneLocation) <= maxDistanceInclusive).GetRandomElement();
			if (randomElement5 == null)
			{
				Debug.LogWarning("Couldn't find friendly settlement for village: " + village.GetProperty("name") + " at " + minDistanceInclusive + " to " + maxDistanceInclusive);
				return null;
			}
			dynamicQuestDeliveryTarget.displayName = randomElement5.name;
			dynamicQuestDeliveryTarget.zoneId = randomElement5.targetZone;
			dynamicQuestDeliveryTarget.secretId = randomElement5.secretID;
			dynamicQuestDeliveryTarget.location = randomElement5.zoneLocation;
		}
		else if (dynamicQuestDeliveryTarget.type == "Wilds")
		{
			throw new NotImplementedException();
		}
		return dynamicQuestDeliveryTarget;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.Wish;
using XRL.World.Encounters;
using XRL.World.Loaders;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World;

[HasWishCommand]
[HasModSensitiveStaticCache]
public class GameObjectFactory
{
	[ModSensitiveStaticCache(false)]
	private static GameObjectFactory _Factory = null;

	private bool LoadBlueprintsHappened;

	public Dictionary<string, GameObjectBlueprint> Blueprints = new Dictionary<string, GameObjectBlueprint>();

	public List<GameObjectBlueprint> BlueprintList = new List<GameObjectBlueprint>();

	public Queue<GameObject> gameObjectPool = new Queue<GameObject>();

	public Queue<XRL.World.Parts.Physics> physicsPool = new Queue<XRL.World.Parts.Physics>();

	public Queue<Render> renderPool = new Queue<Render>();

	private Dictionary<int, List<GameObjectBlueprint>> Tiers = new Dictionary<int, List<GameObjectBlueprint>>();

	private Dictionary<int, uint> TierDeltaWeights;

	private Dictionary<int, uint> TechTierDeltaWeights;

	private Dictionary<string, double> RoleWeightMultipliers;

	public long ObjectsCreated;

	[NonSerialized]
	private static Event eObjectCreated = new Event("ObjectCreated", "Context", null);

	[NonSerialized]
	private static Event eCommandTakeObject = new Event("CommandTakeObject", "Object", (object)null, "EnergyCost", 0).SetSilent(Silent: true);

	[NonSerialized]
	public static Dictionary<string, string> populationContext = new Dictionary<string, string>();

	public static GameObjectFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				Loading.LoadTask("Loading ObjectBlueprints.xml", LoadFactory);
			}
			return _Factory;
		}
	}

	private static void LoadFactory()
	{
		_Factory = new GameObjectFactory();
		_Factory.LoadBlueprints();
	}

	[PreGameCacheInit]
	private static void PreGameInit()
	{
		Factory.DispatchLoadBlueprints();
	}

	public void DispatchLoadBlueprints()
	{
		if (!LoadBlueprintsHappened)
		{
			LoadBlueprintsHappened = true;
			Loading.LoadTask("Dispatch LoadBlueprint", CallLoadBlueprint);
		}
	}

	public void Hotload()
	{
		Blueprints = new Dictionary<string, GameObjectBlueprint>();
		BlueprintList = new List<GameObjectBlueprint>();
		LoadBlueprints();
	}

	public static GameObject create(string blueprint)
	{
		return Factory.CreateObject(blueprint);
	}

	public void Pool(GameObject objectToPool, bool allowGameObjectPool = false)
	{
		objectToPool.Blueprint = "*PooledObject";
		if (objectToPool._pRender != null)
		{
			if (objectToPool._pRender.PoolReset())
			{
				renderPool.Enqueue(objectToPool.pRender);
			}
			objectToPool._pRender = null;
		}
		if (objectToPool._pPhysics != null)
		{
			if (objectToPool._pPhysics.PoolReset())
			{
				physicsPool.Enqueue(objectToPool.pPhysics);
			}
			objectToPool._pPhysics = null;
		}
		objectToPool.Blueprint = "*PooledObject";
		objectToPool._clearCaches();
		if (objectToPool._IntProperty != null)
		{
			objectToPool._IntProperty.Clear();
		}
		if (objectToPool._Property != null)
		{
			objectToPool._Property.Clear();
		}
		objectToPool.Statistics.Clear();
		objectToPool._Energy = null;
		The.ActionManager?.RemoveActiveObject(objectToPool);
		if (objectToPool._Effects != null)
		{
			objectToPool._Effects.Clear();
		}
		objectToPool.ResetNameCache();
		if (objectToPool.RegisteredEffectEvents != null)
		{
			objectToPool.RegisteredEffectEvents.Clear();
		}
		if (objectToPool.RegisteredPartEvents != null)
		{
			objectToPool.RegisteredPartEvents.Clear();
		}
		objectToPool.PartsList.Clear();
		objectToPool._isCombatObject = byte.MaxValue;
		objectToPool._CachedStrippedName = null;
		objectToPool.PronounSetName = null;
		objectToPool.PronounSetKnown = false;
		objectToPool.GenderName = null;
		objectToPool.DeepCopyInventoryObjectMap = null;
		if (allowGameObjectPool && !objectToPool.IsCombatObject())
		{
			gameObjectPool.Enqueue(objectToPool);
		}
	}

	public void Pool(List<GameObject> objectListToPool)
	{
		try
		{
			foreach (GameObject item in objectListToPool)
			{
				Pool(item);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GameObjectFactory::Pool", x);
		}
		objectListToPool.Clear();
	}

	public List<GameObjectBlueprint> GetFactionMembers(string Faction)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			if (blueprint.Tags.ContainsKey("ExcludeFromDynamicEncounters") || blueprint.Tags.ContainsKey("BaseObject"))
			{
				continue;
			}
			GamePartBlueprint part = blueprint.GetPart("Brain");
			if (part == null || !part.Parameters.TryGetValue("Factions", out var value) || !value.Contains(Faction))
			{
				continue;
			}
			foreach (string item in value.CachedCommaExpansion())
			{
				if (item.StartsWith(Faction) && item[Faction.Length] == '-')
				{
					list.Add(blueprint);
					break;
				}
			}
		}
		return list;
	}

	public void InitWeights()
	{
		if (TierDeltaWeights == null)
		{
			TierDeltaWeights = new Dictionary<int, uint>();
			TierDeltaWeights.Add(-7, 10u);
			TierDeltaWeights.Add(-6, 100u);
			TierDeltaWeights.Add(-5, 1000u);
			TierDeltaWeights.Add(-4, 10000u);
			TierDeltaWeights.Add(-3, 100000u);
			TierDeltaWeights.Add(-2, 1000000u);
			TierDeltaWeights.Add(-1, 10000000u);
			TierDeltaWeights.Add(0, 100000000u);
			TierDeltaWeights.Add(1, 10000000u);
			TierDeltaWeights.Add(2, 1000000u);
			TierDeltaWeights.Add(3, 100000u);
			TierDeltaWeights.Add(4, 10000u);
			TierDeltaWeights.Add(5, 1000u);
			TierDeltaWeights.Add(6, 100u);
			TierDeltaWeights.Add(7, 10u);
		}
		if (TechTierDeltaWeights == null)
		{
			TechTierDeltaWeights = new Dictionary<int, uint>();
			TechTierDeltaWeights.Add(-7, 10u);
			TechTierDeltaWeights.Add(-6, 100u);
			TechTierDeltaWeights.Add(-5, 1000u);
			TechTierDeltaWeights.Add(-4, 10000u);
			TechTierDeltaWeights.Add(-3, 100000u);
			TechTierDeltaWeights.Add(-2, 1000000u);
			TechTierDeltaWeights.Add(-1, 10000000u);
			TechTierDeltaWeights.Add(0, 100000000u);
			TechTierDeltaWeights.Add(1, 10000000u);
			TechTierDeltaWeights.Add(2, 1000000u);
			TechTierDeltaWeights.Add(3, 100000u);
			TechTierDeltaWeights.Add(4, 10000u);
			TechTierDeltaWeights.Add(5, 1000u);
			TechTierDeltaWeights.Add(6, 100u);
			TechTierDeltaWeights.Add(7, 10u);
		}
		if (RoleWeightMultipliers == null)
		{
			RoleWeightMultipliers = new Dictionary<string, double>();
			RoleWeightMultipliers.Add("Common", 4.0);
			RoleWeightMultipliers.Add("Minion", 4.0);
			RoleWeightMultipliers.Add("Artillery", 0.25);
			RoleWeightMultipliers.Add("Skirmisher", 1.0);
			RoleWeightMultipliers.Add("Uncommon", 0.25);
			RoleWeightMultipliers.Add("Brute", 0.25);
			RoleWeightMultipliers.Add("Tank", 0.25);
			RoleWeightMultipliers.Add("Rare", 0.01);
			RoleWeightMultipliers.Add("Leader", 0.1);
			RoleWeightMultipliers.Add("Hero", 0.1);
			RoleWeightMultipliers.Add("Epic", 0.01);
		}
	}

	public void FabricateDynamicInheritsTable(string baseName)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].DescendsFrom(baseName))
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicInheritsTable:" + baseName, list);
	}

	public void FabricateDynamicArtifactsTable()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].HasPart("Examiner") && Factory.BlueprintList[i].GetPart("Examiner").Parameters.ContainsKey("Complexity") && Factory.BlueprintList[i].GetPart("Examiner").Parameters["Complexity"] != "0")
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicArtifactsTable:", list);
	}

	public void FabricateDynamicHasPartTable(string baseName)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		for (int i = 0; i < Factory.BlueprintList.Count; i++)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(Factory.BlueprintList[i]) && Factory.BlueprintList[i].HasPart(baseName))
			{
				list.Add(Factory.BlueprintList[i]);
			}
		}
		FabricateDynamicPopulationTable("DynamicHasPartTable:" + baseName, list);
	}

	public void FabricateDynamicSemanticTable(string tableName)
	{
		string[] array = tableName.Split(':');
		string[] array2 = array[1].Split(',');
		int num = -1;
		int num2 = -1;
		if (array.Count() > 2 && !string.IsNullOrEmpty(array[2]))
		{
			num = Convert.ToInt32(array[2]);
		}
		if (array.Count() > 3 && !string.IsNullOrEmpty(array[3]))
		{
			num2 = Convert.ToInt32(array[3]);
		}
		Dictionary<GameObjectBlueprint, uint> dictionary = new Dictionary<GameObjectBlueprint, uint>();
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint value6 in Blueprints.Values)
		{
			if (!value6.HasTag("Semantic" + array2[0]) || !EncountersAPI.IsEligibleForDynamicEncounters(value6))
			{
				continue;
			}
			uint num3 = 0u;
			string[] array3 = array2;
			foreach (string text in array3)
			{
				if (value6.HasTag("Semantic" + text))
				{
					num3++;
				}
			}
			if (num3 != 0)
			{
				list.Add(value6);
				dictionary.Add(value6, num3);
			}
		}
		Tiers.Clear();
		uint num4 = 1u;
		InitWeights();
		MetricsManager.LogEditorInfo("=== building " + tableName + " ===");
		PopulationInfo populationInfo = new PopulationInfo(tableName);
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary2 = new Dictionary<string, PopulationGroup>();
		for (int j = 0; j < list.Count; j++)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(list[j]))
			{
				continue;
			}
			int num5 = int.MinValue;
			int num6 = int.MinValue;
			if (num != -1)
			{
				num5 = num - list[j].Tier;
			}
			if (num2 != -1)
			{
				num6 = num2 - list[j].TechTier;
			}
			uint num7 = num4;
			if (num5 != int.MinValue && TierDeltaWeights.TryGetValue(num5, out var value))
			{
				num7 += value;
			}
			if (num6 != int.MinValue && TechTierDeltaWeights.TryGetValue(num6, out value))
			{
				num7 += value;
			}
			if (list[j].Props.TryGetValue("Role", out var value2) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				num7 = (uint)Math.Ceiling((double)num7 * value3);
			}
			if (list[j].Tags.ContainsKey(key3))
			{
				try
				{
					double num8 = Convert.ToDouble(list[j].Tags[key3]);
					num7 = (uint)Math.Ceiling((double)num7 * num8);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + j, x);
				}
			}
			num7 *= dictionary[list[j]];
			if (num7 == 0)
			{
				continue;
			}
			if (!list[j].Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!list[j].Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = list[j].GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary2.ContainsKey(tag))
				{
					PopulationGroup populationGroup2 = new PopulationGroup();
					populationGroup2.Name = "aggregate:" + tag;
					populationGroup2.Weight = 1u;
					populationGroup2.Style = "pickone";
					dictionary2.Add(tag, populationGroup2);
					populationGroup.Items.Add(populationGroup2);
				}
				PopulationGroup populationGroup3 = dictionary2[tag];
				if (populationGroup3.Weight < num7)
				{
					populationGroup3.Weight = num7;
				}
				populationGroup3.Items.Add(new PopulationObject(list[j].Name, value4, num7, value5));
				MetricsManager.LogEditorInfo("aggregate element: " + list[j].Name + " weight=" + num7 + " aggregateGroup=" + tag);
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(list[j].Name, value4, num7, value5));
				MetricsManager.LogEditorInfo("element: " + list[j].Name + " weight=" + num7);
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			Debug.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public void FabricateDynamicObjectsTable(string tableName)
	{
		int num = -1;
		int num2 = -1;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint value6 in Blueprints.Values)
		{
			if (value6.HasTag(tableName))
			{
				list.Add(value6);
			}
		}
		Tiers.Clear();
		uint num3 = 1u;
		InitWeights();
		MetricsManager.LogEditorInfo("=== building " + tableName + " ===");
		PopulationInfo populationInfo = new PopulationInfo(tableName);
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		for (int i = 0; i < list.Count; i++)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(list[i]))
			{
				continue;
			}
			int num4 = int.MinValue;
			int num5 = int.MinValue;
			if (num != -1)
			{
				num4 = num - list[i].Tier;
			}
			if (num2 != -1)
			{
				num5 = num2 - list[i].TechTier;
			}
			uint num6 = num3;
			if (num4 != int.MinValue && TierDeltaWeights.TryGetValue(num4, out var value))
			{
				num6 += value;
			}
			if (num5 != int.MinValue && TechTierDeltaWeights.TryGetValue(num5, out value))
			{
				num6 += value;
			}
			if (list[i].Props.TryGetValue("Role", out var value2) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				num6 = (uint)Math.Ceiling((double)num6 * value3);
			}
			if (list[i].Tags.ContainsKey(key3))
			{
				try
				{
					double num7 = Convert.ToDouble(list[i].Tags[key3]);
					num6 = (uint)Math.Ceiling((double)num6 * num7);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + i, x);
				}
			}
			if (num6 == 0)
			{
				continue;
			}
			if (!list[i].Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!list[i].Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = list[i].GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary.ContainsKey(tag))
				{
					PopulationGroup populationGroup2 = new PopulationGroup();
					populationGroup2.Name = "aggregate:" + tag;
					populationGroup2.Weight = 1u;
					populationGroup2.Style = "pickone";
					dictionary.Add(tag, populationGroup2);
					populationGroup.Items.Add(populationGroup2);
				}
				PopulationGroup populationGroup3 = dictionary[tag];
				if (populationGroup3.Weight < num6)
				{
					populationGroup3.Weight = num6;
				}
				populationGroup3.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
				MetricsManager.LogEditorInfo("aggregate element: " + list[i].Name + " weight=" + num6 + " aggregateGroup=" + tag);
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(list[i].Name, value4, num6, value5));
				MetricsManager.LogEditorInfo("element: " + list[i].Name + " weight=" + num6);
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			Debug.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public void FabricateDynamicPopulationTable(string tableName, List<GameObjectBlueprint> dynamicTableObjects)
	{
		Tiers.Clear();
		uint num = 1u;
		InitWeights();
		for (int i = 0; i <= 8; i++)
		{
			PopulationInfo populationInfo = new PopulationInfo();
			if (i == 0)
			{
				populationInfo.Name = tableName;
			}
			else
			{
				populationInfo.Name = tableName + ":Tier" + i;
			}
			PopulationGroup populationGroup = new PopulationGroup();
			populationGroup.Chance = "100";
			populationGroup.Number = "1";
			populationGroup.Style = "pickone";
			string key = tableName + ":Number";
			string key2 = tableName + ":Builder";
			string key3 = tableName + ":Weight";
			Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
			for (int j = 0; j < dynamicTableObjects.Count; j++)
			{
				if (!EncountersAPI.IsEligibleForDynamicEncounters(dynamicTableObjects[j]))
				{
					continue;
				}
				int key4 = i - dynamicTableObjects[j].Tier;
				if (i == 0)
				{
					key4 = 0;
				}
				if (!TierDeltaWeights.TryGetValue(key4, out var value))
				{
					value = num;
				}
				if (dynamicTableObjects[j].Props.TryGetValue("Role", out var value2) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
				{
					value = (uint)Math.Ceiling((double)value * value3);
				}
				if (dynamicTableObjects[j].Tags.ContainsKey(key3))
				{
					try
					{
						double num2 = Convert.ToDouble(dynamicTableObjects[j].Tags[key3]);
						value = (uint)Math.Ceiling((double)value * num2);
					}
					catch (Exception x)
					{
						MetricsManager.LogException("Invalid table weight tag on: " + j, x);
					}
				}
				if (value == 0)
				{
					continue;
				}
				if (!dynamicTableObjects[j].Tags.TryGetValue(key, out var value4))
				{
					value4 = "1";
				}
				if (!dynamicTableObjects[j].Tags.TryGetValue(key2, out var value5))
				{
					value5 = "1";
				}
				string tag = dynamicTableObjects[j].GetTag("AggregateWith", null);
				if (tag != null)
				{
					if (!dictionary.ContainsKey(tag))
					{
						PopulationGroup populationGroup2 = new PopulationGroup();
						populationGroup2.Name = "aggregate:" + tag;
						populationGroup2.Weight = 1u;
						populationGroup2.Style = "pickone";
						dictionary.Add(tag, populationGroup2);
						populationGroup.Items.Add(populationGroup2);
					}
					PopulationGroup populationGroup3 = dictionary[tag];
					if (populationGroup3.Weight < value)
					{
						populationGroup3.Weight = value;
					}
					populationGroup3.Items.Add(new PopulationObject(dynamicTableObjects[j].Name, value4, value, value5));
					MetricsManager.LogEditorInfo("aggregate element: " + dynamicTableObjects[j].Name + " weight=" + value + " aggregateGroup=" + tag);
				}
				else
				{
					populationGroup.Items.Add(new PopulationObject(dynamicTableObjects[j].Name, value4, value, value5));
					MetricsManager.LogEditorInfo("element: " + dynamicTableObjects[j].Name + " weight=" + value);
				}
			}
			populationInfo.Items.Add(populationGroup);
			if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
			{
				Debug.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
				continue;
			}
			if (populationInfo.Name == "DynamicInheritsTable:Armor:Tier8")
			{
				populationInfo.Name = populationInfo.Name;
			}
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public void FabricateMultitierDynamicPopulationTable(string tableName, IEnumerable<GameObjectBlueprint> dynamicTableObjects, int minTier, int maxTier)
	{
		Tiers.Clear();
		uint num = 1u;
		InitWeights();
		PopulationInfo populationInfo = new PopulationInfo();
		populationInfo.Name = tableName;
		PopulationGroup populationGroup = new PopulationGroup();
		populationGroup.Chance = "100";
		populationGroup.Number = "1";
		populationGroup.Style = "pickone";
		string key = tableName + ":Number";
		string key2 = tableName + ":Builder";
		string key3 = tableName + ":Weight";
		Dictionary<string, PopulationGroup> dictionary = new Dictionary<string, PopulationGroup>();
		foreach (GameObjectBlueprint dynamicTableObject in dynamicTableObjects)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(dynamicTableObject))
			{
				continue;
			}
			int num2 = 0;
			if (dynamicTableObject.Tier < minTier || dynamicTableObject.Tier > maxTier)
			{
				num2 = Math.Min(Math.Abs(minTier - dynamicTableObject.Tier), Math.Abs(minTier - dynamicTableObject.Tier));
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (!TierDeltaWeights.TryGetValue(num2, out var value))
			{
				value = num;
			}
			if (dynamicTableObject.Props.TryGetValue("Role", out var value2) && RoleWeightMultipliers.TryGetValue(value2, out var value3))
			{
				value = (uint)Math.Ceiling((double)value * value3);
			}
			if (dynamicTableObject.Tags.ContainsKey(key3))
			{
				try
				{
					double num3 = Convert.ToDouble(dynamicTableObject.Tags[key3]);
					value = (uint)Math.Ceiling((double)value * num3);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Invalid table weight tag on: " + dynamicTableObject, x);
				}
			}
			if (value == 0)
			{
				continue;
			}
			if (!dynamicTableObject.Tags.TryGetValue(key, out var value4))
			{
				value4 = "1";
			}
			if (!dynamicTableObject.Tags.TryGetValue(key2, out var value5))
			{
				value5 = "1";
			}
			string tag = dynamicTableObject.GetTag("AggregateWith", null);
			if (tag != null)
			{
				if (!dictionary.ContainsKey(tag))
				{
					PopulationGroup populationGroup2 = new PopulationGroup();
					populationGroup2.Name = "aggregate:" + tag;
					populationGroup2.Weight = 1u;
					populationGroup2.Style = "pickone";
					dictionary.Add(tag, populationGroup2);
					populationGroup.Items.Add(populationGroup2);
				}
				PopulationGroup populationGroup3 = dictionary[tag];
				if (populationGroup3.Weight < value)
				{
					populationGroup3.Weight = value;
				}
				populationGroup3.Items.Add(new PopulationObject(dynamicTableObject.Name, value4, value, value5));
				MetricsManager.LogEditorInfo("aggregate element: " + dynamicTableObject.Name + " weight=" + value + " aggregateGroup=" + tag);
			}
			else
			{
				populationGroup.Items.Add(new PopulationObject(dynamicTableObject.Name, value4, value, value5));
				MetricsManager.LogEditorInfo("element: " + dynamicTableObject.Name + " weight=" + value);
			}
		}
		populationInfo.Items.Add(populationGroup);
		if (PopulationManager.Populations.ContainsKey(populationInfo.Name))
		{
			Debug.LogWarning("Double entry during table fabrication for populationInfo: " + populationInfo.Name);
		}
		else
		{
			PopulationManager.Populations.Add(populationInfo.Name, populationInfo);
		}
	}

	public List<GameObjectBlueprint> GetBlueprintsWithTag(string tag)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
		foreach (KeyValuePair<string, GameObjectBlueprint> blueprint in Blueprints)
		{
			if (blueprint.Value.HasTag(tag) && !blueprint.Value.HasTag("BaseObject"))
			{
				list.Add(blueprint.Value);
			}
		}
		return list;
	}

	public List<GameObjectBlueprint> GetBlueprintsWantingPreload()
	{
		List<string> list = new List<string>();
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(WantLoadBlueprintAttribute)))
		{
			list.Add(item.Name);
		}
		List<GameObjectBlueprint> list2 = new List<GameObjectBlueprint>();
		foreach (GameObjectBlueprint value in Blueprints.Values)
		{
			foreach (string item2 in list)
			{
				if (value.HasPart(item2))
				{
					list2.Add(value);
					break;
				}
			}
		}
		return list2;
	}

	public void LoadBlueprints()
	{
		MutationFactory.CheckInit();
		ObjectBlueprintLoader objectBlueprintLoader = new ObjectBlueprintLoader();
		objectBlueprintLoader.LoadAllBlueprints();
		foreach (ObjectBlueprintLoader.ObjectBlueprintXMLData item in objectBlueprintLoader.BakedBlueprints())
		{
			Blueprints[item.Name] = LoadBakedXML(item);
			BlueprintList.Add(Blueprints[item.Name]);
		}
		foreach (GameObjectBlueprint blueprint in BlueprintList)
		{
			GameObjectBlueprint shallowParent = blueprint.ShallowParent;
			if (shallowParent != null)
			{
				shallowParent.hasChildren = true;
			}
		}
	}

	public KeyValuePair<string, Dictionary<string, string>> ParseXTagNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node)
	{
		return new KeyValuePair<string, Dictionary<string, string>>(node.NodeName.Substring(4), new Dictionary<string, string>(node.Attributes));
	}

	public Statistic ParseStatNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node)
	{
		Statistic statistic = new Statistic();
		statistic.Name = node.Name;
		if (node.Attributes.ContainsKey("Min"))
		{
			statistic.Min = Convert.ToInt32(node.Attributes["Min"]);
		}
		if (node.Attributes.ContainsKey("Max"))
		{
			statistic.Max = Convert.ToInt32(node.Attributes["Max"]);
		}
		if (node.Attributes.ContainsKey("Value"))
		{
			statistic.BaseValue = Convert.ToInt32(node.Attributes["Value"]);
		}
		if (node.Attributes.ContainsKey("Boost"))
		{
			statistic.Boost = Convert.ToInt32(node.Attributes["Boost"]);
		}
		if (node.Attributes.ContainsKey("sValue"))
		{
			statistic.sValue = node.Attributes["sValue"];
		}
		return statistic;
	}

	public InventoryObject ParseInventoryObjectNode(ObjectBlueprintLoader.ObjectBlueprintXMLChildNode node)
	{
		string attribute = node.GetAttribute("Blueprint");
		string attribute2 = node.GetAttribute("Number");
		string attribute3 = node.GetAttribute("Chance");
		string attribute4 = node.GetAttribute("NoSell");
		string attribute5 = node.GetAttribute("NoEquip");
		string attribute6 = node.GetAttribute("NotReal");
		string attribute7 = node.GetAttribute("Full");
		string attribute8 = node.GetAttribute("CellChance");
		string attribute9 = node.GetAttribute("CellFullChance");
		string attribute10 = node.GetAttribute("CellType");
		string attribute11 = node.GetAttribute("StringProperties");
		string attribute12 = node.GetAttribute("IntProperties");
		bool boostModChance = false;
		string attribute13 = node.GetAttribute("BoostModChance");
		if (!string.IsNullOrEmpty(attribute13) && Convert.ToBoolean(attribute13))
		{
			boostModChance = true;
		}
		string number = "1";
		int chance = 100;
		if (!string.IsNullOrEmpty(attribute2))
		{
			number = attribute2;
		}
		if (!string.IsNullOrEmpty(attribute3))
		{
			chance = Convert.ToInt32(attribute3);
		}
		bool noSell = false;
		if (!string.IsNullOrEmpty(attribute4))
		{
			noSell = Convert.ToBoolean(attribute4);
		}
		bool noEquip = false;
		if (!string.IsNullOrEmpty(attribute5))
		{
			noEquip = Convert.ToBoolean(attribute5);
		}
		bool notReal = false;
		if (!string.IsNullOrEmpty(attribute6))
		{
			notReal = Convert.ToBoolean(attribute6);
		}
		bool full = false;
		if (!string.IsNullOrEmpty(attribute7))
		{
			full = Convert.ToBoolean(attribute7);
		}
		int? cellChance = null;
		if (!string.IsNullOrEmpty(attribute8))
		{
			cellChance = Convert.ToInt32(attribute8);
		}
		int? cellFullChance = null;
		if (!string.IsNullOrEmpty(attribute9))
		{
			cellFullChance = Convert.ToInt32(attribute9);
		}
		Dictionary<string, string> dictionary = null;
		if (!string.IsNullOrEmpty(attribute11))
		{
			string[] array = attribute11.Split(',');
			dictionary = new Dictionary<string, string>(array.Length);
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(':');
				dictionary.Add(array3[0], array3[1]);
			}
		}
		Dictionary<string, int> dictionary2 = null;
		if (!string.IsNullOrEmpty(attribute12))
		{
			string[] array4 = attribute12.Split(',');
			dictionary2 = new Dictionary<string, int>(array4.Length);
			string[] array2 = array4;
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array5 = array2[i].Split(':');
				dictionary2.Add(array5[0], Convert.ToInt32(array5[1]));
			}
		}
		return new InventoryObject(attribute, number, chance, noEquip, noSell, notReal, full, cellChance, cellFullChance, attribute10, dictionary, dictionary2)
		{
			BoostModChance = boostModChance
		};
	}

	public GameObjectBlueprint LoadBakedXML(ObjectBlueprintLoader.ObjectBlueprintXMLData node)
	{
		try
		{
			GameObjectBlueprint gameObjectBlueprint = new GameObjectBlueprint();
			gameObjectBlueprint.Name = node.Name;
			gameObjectBlueprint.Inherits = node.Inherits;
			Blueprints[node.Name] = gameObjectBlueprint;
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item in node.NamedNodes("part"))
			{
				GamePartBlueprint gamePartBlueprint = new GamePartBlueprint(item.Key);
				gamePartBlueprint.Name = item.Value.Name;
				gamePartBlueprint.Parameters = item.Value.Attributes;
				gameObjectBlueprint.UpdatePart(gamePartBlueprint.Name, gamePartBlueprint);
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item2 in node.NamedNodes("mutation"))
			{
				GamePartBlueprint gamePartBlueprint2 = new GamePartBlueprint(item2.Key);
				gamePartBlueprint2.Name = item2.Value.Name;
				gamePartBlueprint2.Parameters = item2.Value.Attributes;
				gameObjectBlueprint.Mutations[gamePartBlueprint2.Name] = gamePartBlueprint2;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item3 in node.NamedNodes("builder"))
			{
				GamePartBlueprint gamePartBlueprint3 = new GamePartBlueprint(item3.Key);
				gamePartBlueprint3.Name = item3.Value.Name;
				gamePartBlueprint3.Parameters = item3.Value.Attributes;
				gameObjectBlueprint.Builders[gamePartBlueprint3.Name] = gamePartBlueprint3;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item4 in node.NamedNodes("skill"))
			{
				GamePartBlueprint gamePartBlueprint4 = new GamePartBlueprint(item4.Key);
				gamePartBlueprint4.Name = item4.Value.Name;
				gamePartBlueprint4.Parameters = item4.Value.Attributes;
				gameObjectBlueprint.Skills[gamePartBlueprint4.Name] = gamePartBlueprint4;
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item5 in node.NamedNodes("stat"))
			{
				gameObjectBlueprint.UpdateStat(item5.Key, ParseStatNode(item5.Value));
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item6 in node.NamedNodes("property"))
			{
				string attribute = item6.Value.GetAttribute("Value");
				if (attribute == null || (!attribute.Contains("{{{remove}}}") && !attribute.Contains("*delete")))
				{
					gameObjectBlueprint.Props[item6.Key] = attribute;
				}
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item7 in node.NamedNodes("intproperty"))
			{
				string attribute2 = item7.Value.GetAttribute("Value");
				if (attribute2 == null || (!attribute2.Contains("{{{remove}}}") && !attribute2.Contains("*delete")))
				{
					gameObjectBlueprint.IntProps[item7.Key] = Convert.ToInt32(attribute2);
				}
			}
			foreach (KeyValuePair<string, ObjectBlueprintLoader.ObjectBlueprintXMLChildNode> item8 in node.NamedNodes("tag").Concat(node.NamedNodes("stag")))
			{
				string text = item8.Value.Name;
				if (!item8.Value.Attributes.TryGetValue("Value", out var value))
				{
					value = "";
				}
				if (item8.Value.NodeName == "stag")
				{
					text = "Semantic" + text;
					if (value == "")
					{
						value = "true";
					}
				}
				if (!value.Contains("{{{remove}}}") && !value.Contains("*delete"))
				{
					gameObjectBlueprint.Tags.Add(text, value);
				}
			}
			if (node.Children.ContainsKey("inventoryobject"))
			{
				gameObjectBlueprint.Inventory = new List<InventoryObject>(node.Children["inventoryobject"].Unnamed.Count);
				foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode item9 in node.UnnamedNodes("inventoryobject"))
				{
					gameObjectBlueprint.Inventory.Add(ParseInventoryObjectNode(item9));
				}
			}
			foreach (string key in node.Children.Keys)
			{
				if (!key.StartsWith("xtag"))
				{
					continue;
				}
				if (gameObjectBlueprint.xTags == null)
				{
					gameObjectBlueprint.xTags = new Dictionary<string, Dictionary<string, string>>();
				}
				foreach (ObjectBlueprintLoader.ObjectBlueprintXMLChildNode item10 in node.UnnamedNodes(key))
				{
					KeyValuePair<string, Dictionary<string, string>> keyValuePair = ParseXTagNode(item10);
					gameObjectBlueprint.xTags.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
			return gameObjectBlueprint;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error while loading baked blueprint " + node.Name, x);
		}
		return null;
	}

	public GameObject CreateObject(string ObjectBlueprint)
	{
		return CreateObject(ObjectBlueprint, 0, 0, null, null, null, null);
	}

	public GameObject CreateObject(string ObjectBlueprint, int BonusModChance = 0, int SetModNumber = 0, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null, string Context = null, List<GameObject> provideInventory = null)
	{
		try
		{
			if (ObjectBlueprint == null)
			{
				return null;
			}
			if (ObjectBlueprint.Contains("{mod:1}"))
			{
				SetModNumber = 1;
				ObjectBlueprint = ObjectBlueprint.Replace("{mod:1}", "");
			}
			ObjectsCreated++;
			GameObjectBlueprint blueprint = GetBlueprint(ObjectBlueprint);
			return CreateObject(blueprint, BonusModChance, SetModNumber, beforeObjectCreated, afterObjectCreated, Context, provideInventory);
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Failed creating:" + ObjectBlueprint, ex);
			GameObject gameObject = CreateObject("PhysicalObject");
			gameObject.pRender.DisplayName = "[invalid blueprint:" + ObjectBlueprint + "]";
			gameObject.GetPart<Description>().Short = "Failed building: " + ObjectBlueprint + "\n\n" + ex.ToString();
			return gameObject;
		}
	}

	public GameObject CreateObject(string ObjectBlueprint, Action<GameObject> beforeObjectCreated)
	{
		try
		{
			if (ObjectBlueprint == null)
			{
				return null;
			}
			int setModNumber = 0;
			if (ObjectBlueprint.Contains("{mod:1}"))
			{
				setModNumber = 1;
				ObjectBlueprint = ObjectBlueprint.Replace("{mod:1}", "");
			}
			ObjectsCreated++;
			GameObjectBlueprint blueprint = GetBlueprint(ObjectBlueprint);
			return CreateObject(blueprint, 0, setModNumber, beforeObjectCreated);
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Failed creating:" + ObjectBlueprint, ex);
			GameObject gameObject = CreateObject("PhysicalObject");
			gameObject.pRender.DisplayName = "[invalid blueprint:" + ObjectBlueprint + "]";
			gameObject.GetPart<Description>().Short = "Failed building: " + ObjectBlueprint + "\n\n" + ex.ToString();
			return gameObject;
		}
	}

	private void CallLoadBlueprint()
	{
		int num = 0;
		foreach (GameObjectBlueprint item in GetBlueprintsWantingPreload())
		{
			num++;
			if (num % 20 == 0)
			{
				Event.ResetPool();
			}
			try
			{
				GameObject gameObject = CreateObject(item, -100, -1, null, null, "Initialization");
				if (gameObject == null)
				{
					Debug.LogError("CreateObject returned null for " + item.Name);
					continue;
				}
				gameObject.LoadBlueprint();
				Factory.Pool(gameObject);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception on object: " + item.Name + " " + ex.ToString());
			}
		}
	}

	private static void ProcessAsInventory(GameObject obj, InventoryObject inv, Inventory targetInventory)
	{
		if (inv != null)
		{
			if (inv.NoEquip)
			{
				obj.Property.Add("NoEquip", "1");
			}
			if (inv.NoSell)
			{
				obj.Property.Add("WontSell", "1");
			}
			if (inv.NotReal)
			{
				obj.pPhysics.IsReal = false;
			}
			if (inv.Full)
			{
				LiquidVolume liquidVolume = obj.LiquidVolume;
				if (liquidVolume != null && liquidVolume.MaxVolume > 0 && liquidVolume.ComponentLiquids.Count > 0)
				{
					liquidVolume.Volume = liquidVolume.MaxVolume;
					liquidVolume.FlushWeightCaches();
				}
			}
		}
		if (targetInventory != null)
		{
			eCommandTakeObject.SetParameter("Object", obj);
			targetInventory.FireEvent(eCommandTakeObject);
		}
	}

	public static void ProcessSpecification(string Blueprint, Action<GameObject> afterObjectCreated = null, InventoryObject inv = null, int Count = 1, int iBonusModChance = 0, string Context = null, Action<GameObject> beforeObjectCreated = null, GameObject owningObject = null, Inventory targetInventory = null, List<GameObject> provideInventory = null)
	{
		try
		{
			if (targetInventory == null && owningObject != null)
			{
				targetInventory = owningObject.Inventory;
			}
			if (Blueprint.Length > 0 && Blueprint.StartsWith("$CALLBLUEPRINTMETHOD:", StringComparison.CurrentCultureIgnoreCase))
			{
				ProcessSpecification(PopulationManager.resolveCallBlueprintSlug(Blueprint), afterObjectCreated, inv, Count, iBonusModChance, Context, beforeObjectCreated, owningObject, targetInventory, provideInventory);
				return;
			}
			if (Blueprint.Length > 0 && Blueprint.StartsWith("$CALLOBJECTMETHOD:", StringComparison.CurrentCultureIgnoreCase))
			{
				for (int i = 0; i < Count; i++)
				{
					foreach (GameObject item in PopulationManager.resolveCallObjectSlug(Blueprint))
					{
						afterObjectCreated?.Invoke(item);
						ProcessAsInventory(item, inv, targetInventory);
					}
				}
				return;
			}
			if (Blueprint.Length > 0 && Blueprint[0] == '#')
			{
				List<string> list = new List<string>(Blueprint.Substring(1).Split(','));
				for (int j = 0; j < Count; j++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					string randomElement = list.GetRandomElement();
					list.Remove(randomElement);
					ProcessSpecification(randomElement, afterObjectCreated, inv, 1, iBonusModChance, Context, beforeObjectCreated, owningObject, targetInventory, provideInventory);
				}
				return;
			}
			if (Blueprint.Length > 0 && Blueprint[0] == '*')
			{
				string tableName = Blueprint.Substring(1);
				if (provideInventory != null)
				{
					for (int k = 0; k < Count; k++)
					{
						InventoryObject inv2 = inv;
						int count = provideInventory.Count;
						GameObject obj = EncounterFactory.Factory.RollOneFromTable(tableName, iBonusModChance, 0, Context, provideInventory);
						if (provideInventory.Count < count)
						{
							inv2 = null;
						}
						afterObjectCreated?.Invoke(obj);
						ProcessAsInventory(obj, inv2, targetInventory);
					}
				}
				else
				{
					for (int l = 0; l < Count; l++)
					{
						GameObject obj2 = EncounterFactory.Factory.RollOneFromTable(tableName, iBonusModChance, 0, Context);
						afterObjectCreated?.Invoke(obj2);
						ProcessAsInventory(obj2, inv, targetInventory);
					}
				}
				return;
			}
			if (Blueprint.Length > 0 && Blueprint[0] == '@')
			{
				string text = Blueprint.Substring(1);
				if (text.Contains("{zonetier}"))
				{
					text = text.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
				}
				populationContext.Clear();
				populationContext.Add("zonetier", ZoneManager.zoneGenerationContextTier.ToString());
				populationContext.Add("zonetier+1", (ZoneManager.zoneGenerationContextTier + 1).ToString());
				if (owningObject != null)
				{
					populationContext.Add("ownertier", owningObject.GetTier().ToString());
					populationContext.Add("ownertechtier", owningObject.GetTechTier().ToString());
				}
				for (int m = 0; m < Count; m++)
				{
					foreach (PopulationResult item2 in PopulationManager.Generate(text, populationContext))
					{
						for (int n = 0; n < item2.Number; n++)
						{
							InventoryObject inv3 = inv;
							GameObject gameObject = null;
							if (provideInventory != null)
							{
								foreach (GameObject item3 in provideInventory)
								{
									if (item3.Blueprint == item2.Blueprint)
									{
										gameObject = item3;
										break;
									}
								}
								if (gameObject != null)
								{
									inv3 = null;
									if (gameObject.Count == 1)
									{
										provideInventory.Remove(gameObject);
									}
									else
									{
										gameObject = gameObject.RemoveOne();
									}
								}
							}
							if (gameObject == null)
							{
								GameObjectFactory factory = Factory;
								string blueprint = item2.Blueprint;
								string context = Context;
								gameObject = factory.CreateObject(blueprint, iBonusModChance, 0, beforeObjectCreated, null, context);
							}
							afterObjectCreated?.Invoke(gameObject);
							ProcessAsInventory(gameObject, inv3, targetInventory);
						}
					}
				}
				return;
			}
			if (provideInventory != null)
			{
				for (int num = 0; num < Count; num++)
				{
					InventoryObject inv4 = inv;
					GameObject gameObject2 = null;
					foreach (GameObject item4 in provideInventory)
					{
						if (item4.Blueprint == Blueprint)
						{
							gameObject2 = item4;
							break;
						}
					}
					if (gameObject2 != null)
					{
						inv4 = null;
						if (gameObject2.Count == 1)
						{
							provideInventory.Remove(gameObject2);
						}
						else
						{
							gameObject2 = gameObject2.RemoveOne();
						}
					}
					if (gameObject2 == null)
					{
						GameObjectFactory factory2 = Factory;
						string context = Context;
						gameObject2 = factory2.CreateObject(Blueprint, iBonusModChance, 0, beforeObjectCreated, null, context);
						if (num == 0 && Count > 1 && gameObject2.CanGenerateStacked() && gameObject2.GetPart("Stacker") is Stacker stacker && stacker.StackCount == 1)
						{
							stacker.StackCount = Count;
							num = Count;
						}
					}
					afterObjectCreated?.Invoke(gameObject2);
					ProcessAsInventory(gameObject2, inv4, targetInventory);
				}
				return;
			}
			for (int num2 = 0; num2 < Count; num2++)
			{
				GameObjectFactory factory3 = Factory;
				string context = Context;
				GameObject gameObject3 = factory3.CreateObject(Blueprint, iBonusModChance, 0, beforeObjectCreated, null, context);
				if (num2 == 0 && Count > 1 && gameObject3.CanGenerateStacked() && gameObject3.GetPart("Stacker") is Stacker stacker2 && stacker2.StackCount == 1)
				{
					stacker2.StackCount = Count;
					num2 = Count;
				}
				afterObjectCreated?.Invoke(gameObject3);
				ProcessAsInventory(gameObject3, inv, targetInventory);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GameObjectFactory::ProcessSpecification", x);
		}
	}

	public GameObject CreateObject(GameObjectBlueprint Blueprint, int BonusModChance = 0, int SetModNumber = 0, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null, string Context = null, List<GameObject> provideInventory = null)
	{
		if (Blueprint == null)
		{
			return null;
		}
		GameObject gameObject;
		if (gameObjectPool.Count > 0)
		{
			gameObject = gameObjectPool.Dequeue();
			if (gameObject == null)
			{
				XRLCore.LogError("Got null object from gameObjectPool");
				gameObject = new GameObject();
			}
		}
		else
		{
			gameObject = new GameObject();
		}
		if (Blueprint == null)
		{
			XRLCore.LogError("Null blueprint");
			return null;
		}
		gameObject.Blueprint = Blueprint.GetTag("CreateSubstituteBlueprint", Blueprint.Name);
		if (Context == "Initialization")
		{
			gameObject.id = "init:" + Blueprint.Name;
		}
		gameObject.Property = new Dictionary<string, string>(Blueprint.Props);
		gameObject.IntProperty = new Dictionary<string, int>(Blueprint.IntProps);
		for (GameObjectBlueprint gameObjectBlueprint = Blueprint; gameObjectBlueprint != null; gameObjectBlueprint = gameObjectBlueprint.ShallowParent)
		{
			if (gameObjectBlueprint.Stats == null)
			{
				XRLCore.LogError("Null stats on blueprint for " + Blueprint.Name);
			}
			else if (gameObject.Statistics == null)
			{
				XRLCore.LogError("Null statistics on new object for " + Blueprint.Name);
			}
			else
			{
				foreach (KeyValuePair<string, Statistic> stat in gameObjectBlueprint.Stats)
				{
					if (!gameObject.Statistics.ContainsKey(stat.Key))
					{
						gameObject.Statistics.Add(stat.Key, new Statistic(stat.Value));
						gameObject.Statistics[stat.Key].Owner = gameObject;
					}
				}
			}
		}
		gameObject.FinalizeStats();
		foreach (GamePartBlueprint value9 in Blueprint.allparts.Values)
		{
			if (value9.T == null)
			{
				XRLCore.LogError("Unknown part " + value9.Name + "!");
				return null;
			}
			IPart part = ((value9.T == typeof(XRL.World.Parts.Physics) && physicsPool != null && physicsPool.Count > 0) ? physicsPool.Dequeue() : ((!(value9.T == typeof(Render)) || renderPool == null || renderPool.Count <= 0) ? (Activator.CreateInstance(value9.T) as IPart) : renderPool.Dequeue()));
			part.ParentObject = gameObject;
			value9.InitializePartInstance(part);
			gameObject.AddPart(part, DoRegistration: true, Creation: true);
			if (value9.Parameters != null && value9.Parameters.TryGetValue("Builder", out var value))
			{
				(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders." + value)) as IPartBuilder).BuildPart(part, Context);
			}
			if (value9.finalizeBuild != null)
			{
				value9.finalizeBuild.Invoke(part, null);
			}
		}
		if (Blueprint.Mutations != null)
		{
			foreach (GamePartBlueprint value10 in Blueprint.Mutations.Values)
			{
				if (value10.Name == "MentalBlast")
				{
					value10.Name = "SunderMind";
				}
				string text = "XRL.World.Parts.Mutation." + value10.Name;
				Type type = ModManager.ResolveType(text);
				if (type == null)
				{
					MetricsManager.LogError("Unknown mutation " + text);
					return null;
				}
				if (!(Activator.CreateInstance(type) is BaseMutation baseMutation))
				{
					MetricsManager.LogError("Mutation " + text + " is not a BaseMutation");
					continue;
				}
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					if (value10.Parameters.TryGetValue(fieldInfo.Name, out var value2))
					{
						if (fieldInfo.FieldType == typeof(bool))
						{
							fieldInfo.SetValue(baseMutation, Convert.ToBoolean(value2));
						}
						else if (fieldInfo.FieldType == typeof(int))
						{
							fieldInfo.SetValue(baseMutation, Convert.ToInt32(value2));
						}
						else if (fieldInfo.FieldType == typeof(short))
						{
							fieldInfo.SetValue(baseMutation, Convert.ToInt16(value2));
						}
						else
						{
							fieldInfo.SetValue(baseMutation, value2);
						}
					}
				}
				PropertyInfo[] properties = type.GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (propertyInfo.Name != "Name" && value10.Parameters.TryGetValue(propertyInfo.Name, out var value3) && propertyInfo.CanWrite)
					{
						if (propertyInfo.PropertyType == typeof(bool))
						{
							propertyInfo.SetValue(baseMutation, Convert.ToBoolean(value3), null);
						}
						else if (propertyInfo.PropertyType == typeof(int))
						{
							propertyInfo.SetValue(baseMutation, Convert.ToInt32(value3), null);
						}
						else if (propertyInfo.PropertyType == typeof(short))
						{
							propertyInfo.SetValue(baseMutation, Convert.ToInt16(value3), null);
						}
						else if (propertyInfo.PropertyType == typeof(double))
						{
							propertyInfo.SetValue(baseMutation, Convert.ToDouble(value3), null);
						}
						else
						{
							propertyInfo.SetValue(baseMutation, value3, null);
						}
					}
				}
				if (value10.Parameters.TryGetValue("Builder", out var value4))
				{
					(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders." + value4)) as IPartBuilder).BuildPart(baseMutation, Context);
				}
				if (baseMutation.CapOverride == -1)
				{
					baseMutation.CapOverride = baseMutation.Level;
				}
				gameObject.RequirePart<Mutations>().AddMutation(baseMutation, baseMutation.Level);
			}
		}
		if (Blueprint.Tags != null)
		{
			if (Blueprint.Tags.TryGetValue("MutationPopulation", out var value5) && !value5.IsNullOrEmpty())
			{
				gameObject.MutateFromPopulationTable(value5, ZoneManager.zoneGenerationContextTier);
			}
			if (Blueprint.Tags.TryGetValue("Modded", out var value6))
			{
				if (!string.IsNullOrEmpty(value6))
				{
					int num = Convert.ToInt32(value6);
					if (SetModNumber < num)
					{
						SetModNumber = num;
					}
				}
				else if (SetModNumber < 1)
				{
					SetModNumber = 1;
				}
			}
		}
		if (Blueprint.Skills != null)
		{
			foreach (GamePartBlueprint value11 in Blueprint.Skills.Values)
			{
				string text2 = "XRL.World.Parts.Skill." + value11.Name;
				Type type2 = ModManager.ResolveType(text2);
				if (type2 == null)
				{
					MetricsManager.LogError("Unknown skill " + text2);
					return null;
				}
				if (!(Activator.CreateInstance(type2) is BaseSkill baseSkill))
				{
					MetricsManager.LogError("Skill " + text2 + " is not a BaseSkill");
					return null;
				}
				FieldInfo[] fields = type2.GetFields();
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if (value11.Parameters.TryGetValue(fieldInfo2.Name, out var value7))
					{
						if (fieldInfo2.FieldType == typeof(bool))
						{
							fieldInfo2.SetValue(baseSkill, Convert.ToBoolean(value7));
						}
						else if (fieldInfo2.FieldType == typeof(int))
						{
							fieldInfo2.SetValue(baseSkill, Convert.ToInt32(value7));
						}
						else if (fieldInfo2.FieldType == typeof(short))
						{
							fieldInfo2.SetValue(baseSkill, Convert.ToInt16(value7));
						}
						else
						{
							fieldInfo2.SetValue(baseSkill, value7);
						}
					}
				}
				if (value11.Parameters.TryGetValue("Builder", out var value8))
				{
					(Activator.CreateInstance(ModManager.ResolveType("XRL.World.PartBuilders." + value8)) as IPartBuilder).BuildPart(baseSkill, Context);
				}
				gameObject.RequirePart<XRL.World.Parts.Skills>().AddSkill(baseSkill);
			}
		}
		if (Blueprint.Inventory != null)
		{
			foreach (InventoryObject InventoryObject in Blueprint.Inventory)
			{
				if (!InventoryObject.Chance.in100())
				{
					continue;
				}
				int iBonusModChance = (InventoryObject.BoostModChance ? 30 : 0);
				Action<GameObject> beforeObjectCreated2 = null;
				if (InventoryObject.NeedsToPreconfigureObject())
				{
					beforeObjectCreated2 = delegate(GameObject GO)
					{
						InventoryObject.PreconfigureObject(GO);
					};
				}
				int count = InventoryObject.Number.RollCached();
				ProcessSpecification(InventoryObject.Blueprint, null, InventoryObject, count, iBonusModChance, Context, beforeObjectCreated2, gameObject, gameObject.Inventory, provideInventory);
			}
		}
		string tag = gameObject.GetTag("InventoryPopulationTable");
		if (tag != null)
		{
			gameObject.EquipFromPopulationTable(tag, ZoneManager.zoneGenerationContextTier, null, Context);
			gameObject.FireEvent("InventoryPopulated");
		}
		if (Blueprint.Builders != null)
		{
			foreach (GamePartBlueprint value12 in Blueprint.Builders.Values)
			{
				ApplyBuilder(gameObject, value12, Context);
			}
		}
		ModificationFactory.ApplyModifications(gameObject, Blueprint, BonusModChance, SetModNumber, Context);
		beforeObjectCreated?.Invoke(gameObject);
		GameObject ReplacementObject = null;
		BeforeObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		ObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		AfterObjectCreatedEvent.Process(gameObject, Context, ref ReplacementObject);
		if (ReplacementObject != null)
		{
			gameObject = ReplacementObject;
		}
		afterObjectCreated?.Invoke(gameObject);
		return gameObject;
	}

	public GameObject CreateSampleObject(GameObjectBlueprint Blueprint, Action<GameObject> beforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, beforeObjectCreated, null, "Sample");
	}

	public GameObject CreateSampleObject(string Blueprint, Action<GameObject> beforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, beforeObjectCreated, null, "Sample");
	}

	public GameObject CreateUnmodifiedObject(string Blueprint, string Context = null, Action<GameObject> beforeObjectCreated = null)
	{
		return CreateObject(Blueprint, -9999, 0, beforeObjectCreated, null, Context);
	}

	public static void ApplyBuilder(GameObject NewObject, string Builder)
	{
		ApplyBuilder(NewObject, new GamePartBlueprint(Builder));
	}

	public static void ApplyBuilder(GameObject NewObject, GamePartBlueprint Builder, string Context = null)
	{
		Type type = ModManager.ResolveType("XRL.World.Encounters.EncounterObjectBuilders." + Builder.Name);
		if (type == null)
		{
			Debug.LogError("Unknown builder " + Builder.Name);
			return;
		}
		object obj = Activator.CreateInstance(type);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			try
			{
				if (Builder.Parameters.TryGetValue(fieldInfo.Name, out var value))
				{
					if (fieldInfo.FieldType == typeof(bool))
					{
						fieldInfo.SetValue(obj, Convert.ToBoolean(value));
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						fieldInfo.SetValue(obj, Convert.ToInt32(value));
					}
					else if (fieldInfo.FieldType == typeof(short))
					{
						fieldInfo.SetValue(obj, Convert.ToInt16(value));
					}
					else if (fieldInfo.FieldType == typeof(double))
					{
						fieldInfo.SetValue(obj, Convert.ToDouble(value));
					}
					else if (fieldInfo.FieldType == typeof(float))
					{
						fieldInfo.SetValue(obj, Convert.ToSingle(value));
					}
					else
					{
						fieldInfo.SetValue(obj, value);
					}
				}
			}
			catch (Exception x)
			{
				string text = "";
				text = ((!Builder.Parameters.TryGetValue(fieldInfo.Name, out var value2)) ? ("Field write failed Blueprint=" + NewObject.Blueprint + " Name=" + fieldInfo.Name + " Field not found") : ("Field write failed Blueprint=" + NewObject.Blueprint + " Name=" + fieldInfo.Name + " Value=" + value2));
				MetricsManager.LogError(text, x);
			}
		}
		MethodInfo method = type.GetMethod("BuildObject");
		if (method != null)
		{
			method.Invoke(obj, new object[2] { NewObject, Context });
		}
	}

	public GameObjectBlueprint GetBlueprint(string Name)
	{
		if (Name == null)
		{
			MetricsManager.LogError("called with null Name");
			return null;
		}
		if (Blueprints.TryGetValue(Name, out var value))
		{
			return value;
		}
		MetricsManager.LogError("Unknown blueprint (tell support@freeholdentertainment.com the following text): " + Name);
		return null;
	}

	public GameObjectBlueprint GetBlueprintIfExists(string Name)
	{
		if (Name == null || !Blueprints.TryGetValue(Name, out var value))
		{
			return null;
		}
		return value;
	}

	public bool HasBlueprint(string Name)
	{
		return Blueprints.ContainsKey(Name);
	}

	public object CreateInstance(Type type)
	{
		if (type == typeof(Render) && renderPool.Count > 0)
		{
			return renderPool.Dequeue();
		}
		if (type == typeof(XRL.World.Parts.Physics) && physicsPool.Count > 0)
		{
			return physicsPool.Dequeue();
		}
		return Activator.CreateInstance(type);
	}

	/// <summary>
	///             Not intended for actual use by game objects, but makes finding blueprints via wish easier
	///             </summary>
	public GameObjectBlueprint GetBlueprintIgnoringCase(string name)
	{
		if (name == null)
		{
			return null;
		}
		foreach (string key in Blueprints.Keys)
		{
			if (string.Compare(key, name, ignoreCase: true) == 0)
			{
				return Blueprints[key];
			}
		}
		return null;
	}

	[WishCommand(null, null, Command = "bpxml")]
	public static void HandleBlueprintXML(string bpname)
	{
		GameObjectBlueprint blueprintIgnoringCase = Factory.GetBlueprintIgnoringCase(bpname);
		if (blueprintIgnoringCase == null)
		{
			Popup.Show("No blueprint named \"" + bpname + "\" found.");
		}
		else
		{
			Popup.Show(blueprintIgnoringCase.BlueprintXML().Replace("&", "&&").Replace("^", "^^"));
		}
	}
}

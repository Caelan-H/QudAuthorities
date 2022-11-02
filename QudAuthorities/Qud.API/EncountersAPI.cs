using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace Qud.API;

public static class EncountersAPI
{
	public const int ANIMATED_OBJECT_CHANCE_ONE_IN = 5000;

	public static GameObject GetACreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetACreatureBlueprint(filter));
	}

	public static GameObject GetASampleCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.createSample(GetACreatureBlueprint(filter));
	}

	public static GameObject GetACreatureFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		return GameObject.create(GetACreatureBlueprintFromFaction(faction, filter));
	}

	public static GameObject GetASampleCreatureFromFaction(string faction)
	{
		return GameObjectFactory.Factory.GetFactionMembers(faction).GetRandomElement()?.createSample();
	}

	public static GameObject GetAPlant()
	{
		return GameObject.create(GetAPlantBlueprint());
	}

	public static GameObject GetANonLegendaryCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetANonLegendaryCreatureBlueprint(filter));
	}

	public static GameObject GetANonLegendaryCreatureWithAnInventory(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetANonLegendaryCreatureWithAnInventoryBlueprint(filter));
	}

	public static GameObject GetALegendaryEligibleCreature(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetALegendaryEligibleCreatureBlueprint(filter));
	}

	public static GameObject GetALegendaryEligibleCreatureFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetALegendaryEligibleCreatureBlueprintFromFaction(faction, filter));
	}

	public static GameObject GetALegendaryEligibleCreatureWithAnInventory(Predicate<GameObjectBlueprint> filter = null)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		return GameObject.create(GetALegendaryEligibleCreatureWithAnInventoryBlueprint(filter));
	}

	public static GameObject GetCreatureAroundPlayerLevel()
	{
		if (XRLCore.Core.Game.Player.Body == null)
		{
			return GetCreatureAroundLevel(15);
		}
		int num = Stat.Random(-2, 2);
		return GetCreatureAroundLevel(Math.Max(XRLCore.Core.Game.Player.Body.Stat("Level") + num, 1));
	}

	public static GameObject GetNonLegendaryCreatureAroundPlayerLevel()
	{
		if (XRLCore.Core.Game.Player.Body == null)
		{
			return GetNonLegendaryCreatureAroundLevel(15);
		}
		int num = Stat.Random(-2, 2);
		return GetNonLegendaryCreatureAroundLevel(Math.Max(XRLCore.Core.Game.Player.Body.Stat("Level") + num, 1));
	}

	public static GameObject GetCreatureAroundLevel(int targetLevel)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		while (targetLevel > 0)
		{
			foreach (GameObjectBlueprint item in blueprintList)
			{
				if (!IsEligibleForDynamicEncounters(item) || !item.HasTag("Creature") || !item.HasStat("Level") || item.BaseStat("Level") != targetLevel)
				{
					continue;
				}
				if (item.HasTag("AggregateWith"))
				{
					string tag = item.GetTag("AggregateWith");
					if (list2.Contains(tag))
					{
						continue;
					}
					list2.Add(tag);
				}
				list.Add(item);
			}
			if (list.Count > 0)
			{
				return GameObject.create(list.GetRandomElement().Name);
			}
			targetLevel--;
		}
		return GameObject.create("Dog");
	}

	public static GameObject GetNonLegendaryCreatureAroundLevel(int targetLevel)
	{
		if (If.OneIn(5000))
		{
			return GetAnAnimatedObject();
		}
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		while (targetLevel > 0)
		{
			foreach (GameObjectBlueprint item in blueprintList)
			{
				if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item) || !item.HasTag("Creature") || !item.HasStat("Level") || item.BaseStat("Level") != targetLevel)
				{
					continue;
				}
				if (item.HasTag("AggregateWith"))
				{
					string tag = item.GetTag("AggregateWith");
					if (list2.Contains(tag))
					{
						continue;
					}
					list2.Add(tag);
				}
				list.Add(item);
			}
			if (list.Count > 0)
			{
				return GameObject.create(list.GetRandomElement().Name);
			}
			targetLevel--;
		}
		return GameObject.create("Dog");
	}

	public static GameObject GetAnObject(Predicate<GameObjectBlueprint> filter = null)
	{
		string anObjectBlueprint = GetAnObjectBlueprint(filter);
		if (anObjectBlueprint == null)
		{
			return null;
		}
		return GameObject.create(anObjectBlueprint);
	}

	public static GameObject GetAnObjectNoExclusions(Predicate<GameObjectBlueprint> filter = null)
	{
		string anObjectBlueprintNoExclusions = GetAnObjectBlueprintNoExclusions(filter);
		if (anObjectBlueprintNoExclusions == null)
		{
			return null;
		}
		return GameObject.create(anObjectBlueprintNoExclusions);
	}

	public static GameObject GetAnItem(Predicate<GameObjectBlueprint> filter = null)
	{
		string anItemBlueprint = GetAnItemBlueprint(filter);
		if (anItemBlueprint == null)
		{
			return null;
		}
		return GameObject.create(anItemBlueprint);
	}

	public static GameObject GetAnAnimatedObject()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !item.HasTag("Animatable"))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		GameObject gameObject = GameObject.create(list.GetRandomElement().Name);
		if (!gameObject.HasPart("Brain"))
		{
			AnimateObject.Animate(gameObject);
		}
		return gameObject;
	}

	public static GameObjectBlueprint GetACreatureBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !item.HasTag("Creature") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public static string GetACreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetACreatureBlueprintModel(filter)?.Name;
	}

	public static string GetACreatureBlueprintFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<string> list2 = new List<string>();
		List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(faction);
		factionMembers.ShuffleInPlace();
		foreach (GameObjectBlueprint item in factionMembers)
		{
			if (!IsEligibleForDynamicEncounters(item) || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetAPlantBlueprint()
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || (!item.HasTag("Plant") && !item.HasTag("PlantLike")))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetANonLegendaryCreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item))
			{
				continue;
			}
			if (item.HasTag("AggregateWith") && (filter == null || filter(item)))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetANonLegendaryCreatureWithAnInventoryBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item) || !item.HasPart("Inventory") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item) || !item.HasPart("Body") || !item.HasPart("Combat") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureBlueprintFromFaction(string faction, Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(faction);
		List<string> list2 = new List<string>();
		factionMembers.ShuffleInPlace();
		foreach (GameObjectBlueprint item in factionMembers)
		{
			if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item) || !item.HasPart("Body") || !item.HasPart("Combat") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetALegendaryEligibleCreatureWithAnInventoryBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !IsLegendaryEligible(item) || !item.HasPart("Body") || !item.HasPart("Combat") || !item.HasPart("Inventory") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static GameObjectBlueprint GetABlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!filter(item))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		return list.GetRandomElement();
	}

	public static string GetBlueprintWhere(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetABlueprintModel(filter)?.Name;
	}

	public static GameObjectBlueprint GetAnItemBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !item.HasTag("Item") || item.IsNatural() || item.GetPartParameter("Physics", "IsReal", "true").EqualsNoCase("false") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		return list.GetRandomElement();
	}

	public static string GetAnItemBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetAnItemBlueprintModel(filter)?.Name;
	}

	public static GameObjectBlueprint GetAnObjectBlueprintModel(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || item.IsNatural() || item.GetPartParameter("Physics", "IsReal", "true").EqualsNoCase("false") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		return list.GetRandomElement();
	}

	public static string GetAnObjectBlueprint(Predicate<GameObjectBlueprint> filter = null)
	{
		return GetAnObjectBlueprintModel(filter)?.Name;
	}

	public static string GetAnObjectBlueprintNoExclusions(Predicate<GameObjectBlueprint> filter = null)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (item.IntProps.ContainsKey("Natural") || item.GetPartParameter("Physics", "IsReal", "true").EqualsNoCase("false") || (filter != null && !filter(item)))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static string GetARandomDescendentOf(string Parent)
	{
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
		List<GameObjectBlueprint> blueprintList = GameObjectFactory.Factory.BlueprintList;
		List<string> list2 = new List<string>();
		blueprintList.ShuffleInPlace();
		foreach (GameObjectBlueprint item in blueprintList)
		{
			if (!IsEligibleForDynamicEncounters(item) || !item.DescendsFrom(Parent))
			{
				continue;
			}
			if (item.HasTag("AggregateWith"))
			{
				string tag = item.GetTag("AggregateWith");
				if (list2.Contains(tag))
				{
					continue;
				}
				list2.Add(tag);
			}
			list.Add(item);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement().Name;
	}

	public static bool IsEligibleForDynamicEncounters(GameObjectBlueprint B)
	{
		if (B.IsBaseBlueprint())
		{
			return false;
		}
		if (!B.HasPart("Render"))
		{
			return false;
		}
		return !B.HasTag("ExcludeFromDynamicEncounters");
	}

	public static bool IsLegendaryEligible(GameObjectBlueprint B)
	{
		if (!B.HasTag("Creature"))
		{
			return false;
		}
		if (B.HasPart("GivesRep"))
		{
			return false;
		}
		if (B.HasPart("Uplift"))
		{
			return false;
		}
		if (B.Name.Contains("Hero"))
		{
			return false;
		}
		return true;
	}
}

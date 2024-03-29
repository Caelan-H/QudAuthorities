using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class VillageBase : ZoneBuilderSandbox
{
	public const int CHANCE_FOR_NONDROMAD_MERCHANT = 20;

	public const int CHANCE_FOR_NONHUMAN_TINKER = 50;

	public const int CHANCE_FOR_NONHUMAN_APOTHECARY = 50;

	public const int CHANCE_FOR_STATIC_PER_VILLAGE_OBJECT_TO_VARY = 20;

	public const int CHANCE_FOR_STATIC_PER_BUILDING_OBJECT_TO_VARY = 20;

	public const int MERCHANT_CHANCE = 30;

	public const int TINKER_CHANCE = 25;

	public const int APOTHECARY_CHANCE = 25;

	public const int SHARED_MUTATION_CHANCE = 80;

	public const int CHANCE_FOR_SIGNATURE_LIQUID_IN_VESSEL = 80;

	public const int CHANCE_ABANDONED_VILLAGE_REPOPULATED = 70;

	public const int CHANCE_REGIONAL_PLANT = 98;

	public const int CHANCE_ANY_PLANT = 2;

	public bool isVillageZero;

	public int villageTier;

	public int villageTechTier = 1;

	public string villageTheme = "*Default";

	public HistoricEntity villageEntity;

	public HistoricEntitySnapshot villageSnapshot;

	public string region;

	public string locationName;

	public string regionName;

	public string stairs = "";

	public string monstertable = "";

	public string villageFaction = "";

	public string villagerBaseFaction = "Joppa";

	public const int APPEND_PROVERB_CHANCE = 40;

	public PopulationLayout townSquareLayout;

	public InfluenceMapRegion townSquare;

	public Rect2D townSquareRect;

	public string villageDoorStyle;

	public int townSquareDoorStyle;

	public long lastmem;

	public string signatureItemBlueprint;

	public GameObject signatureItemExample;

	public string signatureHistoricObjectType;

	public GameObject signatureHistoricObjectInstance;

	public string storyType;

	public GameObject villageMonumentPrototype;

	public GameObject villageLightSourcePrototype;

	public GameObject villageBookPrototype;

	public string villageName;

	protected CookingRecipe signatureDish;

	protected string signatureSkill;

	public string signatureLiquid;

	protected List<PopulationLayout> buildings = new List<PopulationLayout>();

	protected List<Location2D> burrowedDoors = new List<Location2D>();

	protected List<GameObject> requiredPlacementObjects = new List<GameObject>();

	protected List<GameObject> originalWalls = new List<GameObject>();

	protected List<GameObject> originalCreatures = new List<GameObject>();

	protected List<GameObject> originalPlants = new List<GameObject>();

	protected List<GameObject> originalItems = new List<GameObject>();

	protected List<GameObject> originalLiquids = new List<GameObject>();

	protected List<GameObject> originalFurniture = new List<GameObject>();

	protected List<Location2D> buildingPaths = new List<Location2D>();

	public GameObject villageDoorPrototype;

	public GameObject villageCanvasPrototype;

	public GameObject villageWallPrototype;

	public void generateVillageTheme()
	{
		villageTheme = RollOneFrom("Villages_VillageTheme");
	}

	public static GameObject getBaseVillager(string villagerBaseFaction, string region, string outlierChance = "10", string FactionVSRegionChance = "50", bool NoRep = false)
	{
		string dynTableName = "DynamicObjectsTable:" + region + "_Creatures";
		Func<GameObject> func = FuzzyFunctions.DoThisButRarelyDoThat(delegate
		{
			IEnumerable<GameObjectBlueprint> enumerable2 = from m in GameObjectFactory.Factory.GetFactionMembers(villagerBaseFaction)
				where (m.HasPart("Body") || m.HasTagOrProperty("BodySubstitute")) && (m.HasPart("Combat") || m.HasTagOrProperty("BodySubstitute")) && m.HasTag(dynTableName) && !m.HasTag("Merchant") && !m.HasTag("ExcludeFromVillagePopulations") && (!NoRep || !m.HasPart("GivesRep"))
				select m;
			return (enumerable2.Count() > 0) ? enumerable2.GetRandomElement().createOne() : null;
		}, FuzzyFunctions.DoThisButRarelyDoThat(delegate
		{
			IEnumerable<GameObjectBlueprint> enumerable = from m in GameObjectFactory.Factory.GetFactionMembers(villagerBaseFaction)
				where (m.HasPart("Body") || m.HasTagOrProperty("BodySubstitute")) && (m.HasPart("Combat") || m.HasPart("BodySubstitute")) && !m.HasTag("Merchant") && !m.HasTag("ExcludeFromVillagePopulations") && (!NoRep || !m.HasPart("GivesRep"))
				select m;
			return (enumerable.Count() > 0) ? enumerable.GetRandomElement().createOne() : null;
		}, () => EncountersAPI.GetACreature((GameObjectBlueprint ob) => ob.HasTag(dynTableName) && (ob.HasPart("Body") || ob.HasTagOrProperty("BodySubstitute")) && (ob.HasPart("Combat") || ob.HasPart("BodySubstitute")) && !ob.HasTag("Merchant") && !ob.HasTag("ExcludeFromVillagePopulations") && (!NoRep || !ob.HasPart("GivesRep"))), FactionVSRegionChance), outlierChance);
		try
		{
			return func();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("getBaseVillager", x);
			return null;
		}
	}

	public GameObject getBaseVillager(bool NoRep = false)
	{
		GameObject gameObject;
		try
		{
			gameObject = ((!(villageSnapshot.GetProperty("villageGatherReason") == "commune")) ? getBaseVillager(villagerBaseFaction, region) : getBaseVillager(villagerBaseFaction, region, "70", "70"));
			if (gameObject == null)
			{
				gameObject = getARegionalCreature();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Failed to generate villager.", x);
			gameObject = EncountersAPI.GetACreature((GameObjectBlueprint ob) => ob.HasPart("Combat") && ob.HasPart("Body") && (!NoRep || !ob.HasPart("GivesRep")));
		}
		if (signatureSkill != null)
		{
			gameObject.SetStringProperty("WaterRitual_Skill", signatureSkill);
		}
		if (signatureItemBlueprint != null && signatureItemExample.IsTakeable())
		{
			gameObject.SetStringProperty("SignatureItemBlueprint", signatureItemBlueprint);
			gameObject.TakeObject(signatureItemBlueprint, Stat.Random(1, 3), Silent: true, 0);
		}
		gameObject.SetIntProperty("Villager", 1);
		return gameObject;
	}

	public void setVillagerProperties(GameObject obj)
	{
		try
		{
			obj.pBrain.Hostile = false;
			obj.pBrain.Calm = true;
			obj.pBrain.Factions = "";
			obj.pBrain.FactionMembership.Clear();
			obj.pBrain.FactionMembership.Add(villageFaction, 100);
			obj.RemovePart("AIPilgrim");
			obj.RemovePart("ConversationScript");
			obj.SetIntProperty("ParticipantVillager", 1);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Failed to set villager properties.", x);
		}
	}

	public void AddVillagerConversation(GameObject obj, string message, string response = "Live and drink.", string Q1 = null, string A1 = null, bool TradeNote = false, bool AppendConversation = false, bool ClearLost = true)
	{
		string property = villageSnapshot.GetProperty("signatureTemperament", null);
		string append = (If.d100(40) ? ("\n\n" + villageSnapshot.GetProperty("proverb")) : null);
		if (Q1 != null && A1 != null)
		{
			if (AppendConversation)
			{
				ConversationsAPI.appendSimpleConversationToObject(obj, message, response, Q1, A1, property, null, append, TradeNote, ClearLost);
			}
			else
			{
				ConversationsAPI.addSimpleConversationToObject(obj, message, response, Q1, A1, property, null, append, TradeNote, ClearLost);
			}
		}
		else if (AppendConversation)
		{
			ConversationsAPI.appendSimpleConversationToObject(obj, message, response, property, null, append, TradeNote, ClearLost);
		}
		else
		{
			ConversationsAPI.addSimpleConversationToObject(obj, message, response, property, null, append, TradeNote, ClearLost);
		}
	}

	public void setVillageDomesticatedProperties(GameObject obj)
	{
		try
		{
			if (obj.pBrain != null)
			{
				obj.pBrain.Hostile = false;
				obj.pBrain.Calm = true;
				obj.pBrain.Factions = "";
				obj.pBrain.FactionMembership.Clear();
				obj.pBrain.FactionMembership.Add(villageFaction, 100);
			}
			obj.RemovePart("AIPilgrim");
			obj.RemovePart("ConversationScript");
			obj.SetIntProperty("VillageDomesticated", 1);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Failed to set village domesticated properties.", x);
		}
	}

	public string RollOneFrom(string PoptablePrefix)
	{
		return PopulationManager.RollOneFrom(ResolvePopulationTableName(PoptablePrefix)).Blueprint;
	}

	public GameObject generateVillager(bool bUnique = false)
	{
		GameObject gameObject = getBaseVillager();
		gameObject.pRender.ColorString = "&y";
		setVillagerProperties(gameObject);
		if (bUnique)
		{
			gameObject = HeroMaker.MakeHero(gameObject, null, "SpecialVillagerHeroTemplate_Villager");
			gameObject.SetIntProperty("NamedVillager", 1);
			gameObject.RequirePart<Interesting>();
		}
		gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		AddVillagerConversation(gameObject, gameObject.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."));
		preprocessVillager(gameObject);
		return gameObject;
	}

	public void preprocessVillager(GameObject obj, bool foreign = false)
	{
		obj.RemovePart("Lovely");
		obj.RemovePart("SecretObject");
		obj.RemovePart("ConvertSpawner");
		if (obj.HasPart("AIShopper"))
		{
			if (obj.pBrain != null)
			{
				obj.pBrain.Wanders = true;
				obj.pBrain.WandersRandomly = true;
			}
			obj.RemovePart("AIShopper");
		}
		if (obj.HasPart("AIPilgrim"))
		{
			if (obj.pBrain != null)
			{
				obj.pBrain.Wanders = true;
				obj.pBrain.WandersRandomly = true;
			}
			obj.RemovePart("AIPilgrim");
		}
		if (foreign)
		{
			return;
		}
		if (villageSnapshot.listProperties.ContainsKey("sharedMutations"))
		{
			foreach (string item in villageSnapshot.GetList("sharedMutations"))
			{
				if (If.Chance(80) && obj.HasPart("Mutations"))
				{
					Mutations mutations = obj.GetPart("Mutations") as Mutations;
					if (!mutations.HasMutation(item))
					{
						mutations.AddMutation(item, "1d4".RollCached());
					}
				}
			}
		}
		if (villageSnapshot.listProperties.ContainsKey("sharedDiseases"))
		{
			foreach (string item2 in villageSnapshot.GetList("sharedDiseases"))
			{
				if (!If.Chance(80))
				{
					continue;
				}
				if (item2 == "Glotrot")
				{
					obj.ApplyEffect(new Glotrot());
					if (obj.GetEffect("Glotrot") is Glotrot glotrot)
					{
						glotrot.Stage = Stat.Random(0, 3);
					}
				}
				if (item2 == "Ironshank")
				{
					obj.ApplyEffect(new Ironshank());
					if (obj.GetEffect("Ironshank") is Ironshank ironshank)
					{
						ironshank.SetPenalty(Stat.Random(1, 75));
					}
				}
			}
		}
		if (!villageSnapshot.listProperties.ContainsKey("sharedTransformations"))
		{
			return;
		}
		foreach (string item3 in villageSnapshot.GetList("sharedTransformations"))
		{
			if (If.Chance(80) && item3 == "Mechanical")
			{
				Roboticized.Roboticize(obj);
			}
		}
	}

	public int clamp(int n, int low, int high)
	{
		if (n < low)
		{
			return low;
		}
		if (n > high)
		{
			return high;
		}
		return n;
	}

	public void partition(Rect2D rect, ref int nSegments, List<ISultanDungeonSegment> segments)
	{
		if (nSegments <= 0)
		{
			segments.Add(new SultanRectDungeonSegment(rect));
		}
		else if (Stat.Random(0, 1) == 0)
		{
			int num = rect.y2 - rect.y1;
			int num2 = (rect.y2 - rect.y1) / 2;
			int num3 = (rect.y2 - rect.y1) / 8;
			int num4 = clamp((int)Stat.GaussianRandom(0f, num3) + num2, rect.y1 + 4, rect.y2 - 4);
			if (num <= 8)
			{
				segments.Add(new SultanRectDungeonSegment(rect));
				return;
			}
			nSegments--;
			Rect2D rect2 = new Rect2D(rect.x1, rect.y1, rect.x2, num4);
			partition(rect2, ref nSegments, segments);
			nSegments--;
			Rect2D rect3 = new Rect2D(rect.x1, num4 + 1, rect.x2, rect.y2);
			partition(rect3, ref nSegments, segments);
		}
		else
		{
			int num5 = rect.x2 - rect.x1;
			int num6 = (rect.x2 - rect.x1) / 2;
			int num7 = (rect.x2 - rect.x1) / 8;
			int num8 = clamp((int)Stat.GaussianRandom(0f, num7) + num6, rect.x1 + 4, rect.x2 - 4);
			if (num5 <= 16)
			{
				segments.Add(new SultanRectDungeonSegment(rect));
				return;
			}
			nSegments--;
			Rect2D rect4 = new Rect2D(rect.x1, rect.y1, num8, rect.y2);
			partition(rect4, ref nSegments, segments);
			nSegments--;
			Rect2D rect5 = new Rect2D(num8 + 1, rect.y1, rect.x2, rect.y2);
			partition(rect5, ref nSegments, segments);
		}
	}

	public string ResolveOneObjectFromArgs(List<string> arg, string def = null, int tier = 1)
	{
		if (arg == null || arg.Count == 0)
		{
			return def;
		}
		string text = arg[Stat.Random(0, arg.Count - 1)];
		if (text.Length == 0)
		{
			return def;
		}
		if (text[0] == '$')
		{
			return PopulationManager.RollOneFrom(text.Substring(1), new Dictionary<string, string> { 
			{
				"zonetier",
				tier.ToString()
			} }, "SmallBoulder").Blueprint;
		}
		return text;
	}

	public List<PopulationResult> ResolveOnePopulationFromArgs(List<string> arg, string def = null, int Tier = 1)
	{
		if (arg == null || arg.Count == 0)
		{
			return PopulationManager.Generate(def);
		}
		return PopulationManager.Generate(arg.GetRandomElement(), "zonetier", Tier.ToString());
	}

	public string ResolveOneTableFromArgs(List<string> arg, string def = null)
	{
		if (arg == null || arg.Count == 0)
		{
			return def;
		}
		return arg.GetRandomElement();
	}

	public void LogMem(string step)
	{
	}

	public bool PointInSegments(List<ISultanDungeonSegment> segments, int x, int y)
	{
		for (int i = 0; i < segments.Count; i++)
		{
			if (segments[i].contains(Location2D.get(x, y)))
			{
				return true;
			}
		}
		return false;
	}

	public void FabricateIslandPond(PopulationLayout layout, string liquidBlueprint)
	{
		List<Location2D> list = layout.region.reducyBy(1);
		list.RemoveAll(onEdge);
		foreach (Location2D item in list)
		{
			zone.GetCell(item).ClearObjectsWithTag("Wall");
			zone.GetCell(item).AddObject(liquidBlueprint);
		}
		foreach (Location2D item2 in layout.region.reducyBy(4))
		{
			zone.GetCell(item2).Clear();
		}
	}

	public void FabricateAerie(PopulationLayout layout)
	{
		zone.CacheZoneConnection("U", layout.position, "aerie");
		zone.GetCell(layout.position.FromDirection("NW")).AddObject(getAVillageWall());
		zone.GetCell(layout.position.FromDirection("NE")).AddObject(getAVillageWall());
		zone.GetCell(layout.position.FromDirection("SW")).AddObject(getAVillageWall());
		zone.GetCell(layout.position.FromDirection("SE")).AddObject(getAVillageWall());
		zone.GetCell(layout.position).AddObject("StairsUp").SetIntProperty("IdleStairs", 1);
	}

	public bool onEdge(Location2D location)
	{
		if (location.x <= 1 || location.x >= 79)
		{
			return true;
		}
		if (location.y <= 1 || location.y >= 24)
		{
			return true;
		}
		return false;
	}

	public void FabricateBurrow(PopulationLayout layout)
	{
		zone.CacheZoneConnection("D", layout.position, "burrow");
		List<Location2D> border = layout.region.getBorder(2);
		border.RemoveAll(onEdge);
		foreach (Location2D item in border)
		{
			zone.GetCell(item).AddObject("Mudroot");
		}
		zone.GetCell(layout.position).AddObject("StairsDown").SetIntProperty("IdleStairs", 1);
	}

	public void FabricatePond(PopulationLayout layout, string liquidBlueprint)
	{
		List<Location2D> list = layout.region.reducyBy(1);
		list.RemoveAll(onEdge);
		foreach (Location2D item in list)
		{
			zone.GetCell(item).ClearObjectsWithTag("Wall");
			zone.GetCell(item).AddObject(liquidBlueprint);
		}
	}

	public void FabricateWalledPond(PopulationLayout layout, string liquidBlueprint)
	{
		List<Location2D> border = layout.region.getBorder(2);
		border.RemoveAll(onEdge);
		List<Location2D> list = layout.region.reducyBy(3);
		list.RemoveAll(onEdge);
		foreach (Location2D item in border)
		{
			zone.GetCell(item).AddObject("Mudroot");
		}
		foreach (Location2D item2 in list)
		{
			zone.GetCell(item2).ClearObjectsWithTag("Wall");
			zone.GetCell(item2).AddObject(liquidBlueprint);
		}
	}

	public void FabricateWalledIslandPond(PopulationLayout layout, string liquidBlueprint)
	{
		layout.region.getBorder(2).RemoveAll(onEdge);
		List<Location2D> list = layout.region.reducyBy(3);
		list.RemoveAll(onEdge);
		foreach (Location2D item in list)
		{
			zone.GetCell(item).AddObject("Mudroot");
		}
		foreach (Location2D item2 in list)
		{
			zone.GetCell(item2).ClearObjectsWithTag("Wall");
			zone.GetCell(item2).AddObject(liquidBlueprint);
		}
		foreach (Location2D item3 in layout.region.reducyBy(5))
		{
			zone.GetCell(item3).Clear();
		}
	}

	public Rect2D BackFromEdge(Rect2D Rect)
	{
		if (Rect.x1 == 0)
		{
			Rect = new Rect2D(1, Rect.y1, Rect.x2, Rect.y2);
		}
		if (Rect.x2 == zone.Width - 1)
		{
			Rect = new Rect2D(Rect.x1, Rect.y1, zone.Width - 2, Rect.y2);
		}
		if (Rect.y1 == 0)
		{
			Rect = new Rect2D(Rect.x1, 1, Rect.x2, Rect.y2);
		}
		if (Rect.y2 == zone.Height - 1)
		{
			Rect = new Rect2D(Rect.x1, Rect.y1, Rect.x2, zone.Height - 2);
		}
		return Rect;
	}

	public void FabricateHistoricHall(InfluenceMapRegion region)
	{
		townSquareRect = BackFromEdge(GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft));
		townSquareDoorStyle = ((Stat.Random(0, 1) == 0) ? Stat.Random(1, 3) : Stat.Random(1, 7));
		int num = (townSquareRect.x1 + townSquareRect.x2) / 2;
		bool flag = (townSquareRect.x2 - townSquareRect.x1 + 1) % 2 == 1;
		int num2 = (townSquareRect.y1 + townSquareRect.y2) / 2;
		bool flag2 = (townSquareRect.y2 - townSquareRect.y1 + 1) % 2 == 1;
		int num3 = townSquareRect.x1;
		while (num3 <= townSquareRect.x2)
		{
			Cell cell = zone.GetCell(num3, townSquareRect.y1);
			Cell cell2 = zone.GetCell(num3, townSquareRect.y2);
			cell.Clear();
			cell2.Clear();
			int num4;
			if (num3 != num)
			{
				if (!flag)
				{
					num4 = ((num3 == num + 1) ? 1 : 0);
					if (num4 != 0)
					{
						goto IL_0138;
					}
				}
				else
				{
					num4 = 0;
				}
				goto IL_0216;
			}
			num4 = 1;
			goto IL_0138;
			IL_022e:
			if (num4 != 0 && (townSquareDoorStyle == 1 || townSquareDoorStyle == 2 || townSquareDoorStyle == 5))
			{
				cell2.AddObject(flag ? getAVillageDoor() : ((num3 == num) ? GameObject.create("Double " + villageDoorStyle + " W") : GameObject.create("Double " + villageDoorStyle + " E"))).SetIntProperty("ReplaceWithWall", 1);
				if (flag || num3 == num)
				{
					tryPlace(num3 - 1, townSquareRect.y2 + 1, getAVillageLightSource);
				}
				if (flag || num3 == num + 1)
				{
					tryPlace(num3 + 1, townSquareRect.y2 + 1, getAVillageLightSource);
				}
			}
			else
			{
				cell2.AddObject(getAVillageWall());
			}
			num3++;
			continue;
			IL_0138:
			if (townSquareDoorStyle != 1 && townSquareDoorStyle != 2 && townSquareDoorStyle != 4)
			{
				goto IL_0216;
			}
			cell.AddObject(flag ? getAVillageDoor() : ((num3 == num) ? GameObject.create("Double " + villageDoorStyle + " W") : GameObject.create("Double " + villageDoorStyle + " E"))).SetIntProperty("ReplaceWithWall", 1);
			if (flag || num3 == num)
			{
				tryPlace(num3 - 1, townSquareRect.y1 - 1, getAVillageLightSource);
			}
			if (flag || num3 == num + 1)
			{
				tryPlace(num3 + 1, townSquareRect.y1 - 1, getAVillageLightSource);
			}
			goto IL_022e;
			IL_0216:
			cell.AddObject(getAVillageWall());
			goto IL_022e;
		}
		int num5 = townSquareRect.y1 + 1;
		while (num5 <= townSquareRect.y2 - 1)
		{
			Cell cell3 = zone.GetCell(townSquareRect.x1, num5);
			Cell cell4 = zone.GetCell(townSquareRect.x2, num5);
			cell3.Clear();
			cell4.Clear();
			int num6;
			if (num5 != num2)
			{
				if (!flag2)
				{
					num6 = ((num5 == num2 + 1) ? 1 : 0);
					if (num6 != 0)
					{
						goto IL_03bc;
					}
				}
				else
				{
					num6 = 0;
				}
				goto IL_049a;
			}
			num6 = 1;
			goto IL_03bc;
			IL_04b2:
			if (num6 != 0 && (townSquareDoorStyle == 1 || townSquareDoorStyle == 3 || townSquareDoorStyle == 6))
			{
				cell4.AddObject(flag2 ? getAVillageDoor() : ((num5 == num2) ? GameObject.create("Double " + villageDoorStyle + " N") : GameObject.create("Double " + villageDoorStyle + " S"))).SetIntProperty("ReplaceWithWall", 1);
				if (flag2 || num5 == num2)
				{
					tryPlace(townSquareRect.x2 + 1, num5 - 1, getAVillageLightSource);
				}
				if (flag2 || num5 == num2 + 1)
				{
					tryPlace(townSquareRect.x2 + 1, num5 + 1, getAVillageLightSource);
				}
			}
			else
			{
				cell4.AddObject(getAVillageWall());
			}
			num5++;
			continue;
			IL_03bc:
			if (townSquareDoorStyle != 1 && townSquareDoorStyle != 3 && townSquareDoorStyle != 7)
			{
				goto IL_049a;
			}
			cell3.AddObject(flag2 ? getAVillageDoor() : ((num5 == num2) ? GameObject.create("Double " + villageDoorStyle + " N") : GameObject.create("Double " + villageDoorStyle + " S"))).SetIntProperty("ReplaceWithWall", 1);
			if (flag2 || num5 == num2)
			{
				tryPlace(townSquareRect.x1 - 1, num5 - 1, getAVillageLightSource);
			}
			if (flag2 || num5 == num2 + 1)
			{
				tryPlace(townSquareRect.x1 - 1, num5 + 1, getAVillageLightSource);
			}
			goto IL_04b2;
			IL_049a:
			cell3.AddObject(getAVillageWall());
			goto IL_04b2;
		}
		Rect2D rect2D = townSquareRect.ReduceBy(2, 2);
		if (rect2D.Area > 4)
		{
			tryPlace(rect2D.x1, rect2D.y1, getAVillageLightSource);
			tryPlace(rect2D.x2, rect2D.y2, getAVillageLightSource);
			tryPlace(rect2D.x1, rect2D.y2, getAVillageLightSource);
			tryPlace(rect2D.x2, rect2D.y1, getAVillageLightSource);
		}
	}

	public void FabricateLibrary(InfluenceMapRegion region)
	{
		townSquareRect = BackFromEdge(GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft));
		townSquareDoorStyle = Stat.Random(1, 4);
		int num = (townSquareRect.x1 + townSquareRect.x2) / 2;
		bool flag = (townSquareRect.x2 - townSquareRect.x1 + 1) % 2 == 1;
		int num2 = (townSquareRect.y1 + townSquareRect.y2) / 2;
		bool flag2 = (townSquareRect.y2 - townSquareRect.y1 + 1) % 2 == 1;
		int num3 = townSquareRect.x1;
		while (num3 <= townSquareRect.x2)
		{
			Cell cell = zone.GetCell(num3, townSquareRect.y1);
			Cell cell2 = zone.GetCell(num3, townSquareRect.y2);
			cell.Clear();
			cell2.Clear();
			int num4;
			if (num3 != num)
			{
				if (!flag)
				{
					num4 = ((num3 == num + 1) ? 1 : 0);
					if (num4 != 0)
					{
						goto IL_0126;
					}
				}
				else
				{
					num4 = 0;
				}
				goto IL_023a;
			}
			num4 = 1;
			goto IL_0126;
			IL_0252:
			if (num4 != 0 && townSquareDoorStyle == 2)
			{
				cell2.AddObject(flag ? getAVillageDoor() : ((num3 == num) ? GameObject.create("Double " + villageDoorStyle + " W") : GameObject.create("Double " + villageDoorStyle + " E"))).SetIntProperty("ReplaceWithWall", 1);
				if (flag || num3 == num)
				{
					tryPlace(num3 - 1, townSquareRect.y2 + 1, getAVillageLightSource);
					tryPlace(num3 - 1, townSquareRect.y2 - 1, getAVillageLightSource);
				}
				if (flag || num3 == num + 1)
				{
					tryPlace(num3 + 1, townSquareRect.y2 + 1, getAVillageLightSource);
					tryPlace(num3 + 1, townSquareRect.y2 - 1, getAVillageLightSource);
				}
			}
			else
			{
				cell2.AddObject(getAVillageWall());
			}
			num3++;
			continue;
			IL_0126:
			if (townSquareDoorStyle != 1)
			{
				goto IL_023a;
			}
			cell.AddObject(flag ? getAVillageDoor() : ((num3 == num) ? GameObject.create("Double " + villageDoorStyle + " W") : GameObject.create("Double " + villageDoorStyle + " E"))).SetIntProperty("ReplaceWithWall", 1);
			if (flag || num3 == num)
			{
				tryPlace(num3 - 1, townSquareRect.y1 - 1, getAVillageLightSource);
				tryPlace(num3 - 1, townSquareRect.y1 + 1, getAVillageLightSource);
			}
			if (flag || num3 == num + 1)
			{
				tryPlace(num3 + 1, townSquareRect.y1 - 1, getAVillageLightSource);
				tryPlace(num3 + 1, townSquareRect.y1 + 1, getAVillageLightSource);
			}
			goto IL_0252;
			IL_023a:
			cell.AddObject(getAVillageWall());
			goto IL_0252;
		}
		int num5 = townSquareRect.y1 + 1;
		while (num5 <= townSquareRect.y2 - 1)
		{
			Cell cell3 = zone.GetCell(townSquareRect.x1, num5);
			Cell cell4 = zone.GetCell(townSquareRect.x2, num5);
			cell3.Clear();
			cell4.Clear();
			int num6;
			if (num5 != num2)
			{
				if (!flag2)
				{
					num6 = ((num5 == num2 + 1) ? 1 : 0);
					if (num6 != 0)
					{
						goto IL_0416;
					}
				}
				else
				{
					num6 = 0;
				}
				goto IL_052a;
			}
			num6 = 1;
			goto IL_0416;
			IL_0542:
			if (num6 != 0 && townSquareDoorStyle == 3)
			{
				cell4.AddObject(flag2 ? getAVillageDoor() : ((num5 == num2) ? GameObject.create("Double " + villageDoorStyle + " N") : GameObject.create("Double " + villageDoorStyle + " S"))).SetIntProperty("ReplaceWithWall", 1);
				if (flag2 || num5 == num2)
				{
					tryPlace(townSquareRect.x2 + 1, num5 - 1, getAVillageLightSource);
					tryPlace(townSquareRect.x2 - 1, num5 - 1, getAVillageLightSource);
				}
				if (flag2 || num5 == num2 + 1)
				{
					tryPlace(townSquareRect.x2 + 1, num5 + 1, getAVillageLightSource);
					tryPlace(townSquareRect.x2 - 1, num5 + 1, getAVillageLightSource);
				}
			}
			else
			{
				cell4.AddObject(getAVillageWall());
			}
			num5++;
			continue;
			IL_0416:
			if (townSquareDoorStyle != 4)
			{
				goto IL_052a;
			}
			cell3.AddObject(flag2 ? getAVillageDoor() : ((num5 == num2) ? GameObject.create("Double " + villageDoorStyle + " N") : GameObject.create("Double " + villageDoorStyle + " S"))).SetIntProperty("ReplaceWithWall", 1);
			if (flag2 || num5 == num2)
			{
				tryPlace(townSquareRect.x1 - 1, num5 - 1, getAVillageLightSource);
				tryPlace(townSquareRect.x1 + 1, num5 - 1, getAVillageLightSource);
			}
			if (flag2 || num5 == num2 + 1)
			{
				tryPlace(townSquareRect.x1 - 1, num5 + 1, getAVillageLightSource);
				tryPlace(townSquareRect.x1 + 1, num5 + 1, getAVillageLightSource);
			}
			goto IL_0542;
			IL_052a:
			cell3.AddObject(getAVillageWall());
			goto IL_0542;
		}
	}

	public void FabricateGraveyard(InfluenceMapRegion region)
	{
		townSquareRect = BackFromEdge(GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft));
		string text = null;
		if (Stat.Random(1, 100) <= 75)
		{
			string wall = "?";
			if (region.Center.x <= 40)
			{
				wall = "E";
			}
			if (region.Center.x >= 40)
			{
				wall = "W";
			}
			if (townSquareRect.y2 > 16)
			{
				wall = "N";
			}
			if (townSquareRect.y1 < 8)
			{
				wall = "S";
			}
			if (townSquareRect.y1 <= 0)
			{
				wall = "S";
			}
			if (townSquareRect.y2 >= 24)
			{
				wall = "N";
			}
			if (townSquareRect.y1 <= 0 && townSquareRect.y2 >= 24)
			{
				if (region.Center.x <= 40)
				{
					wall = "E";
				}
				if (region.Center.x >= 40)
				{
					wall = "W";
				}
			}
			Point2D randomDoorCell = townSquareRect.GetRandomDoorCell(wall);
			ZoneBuilderSandbox.PlaceObjectOnRect(zone, "BrinestalkFence", townSquareRect);
			GetCell(zone, randomDoorCell)?.Clear();
			GetCell(zone, randomDoorCell)?.AddObject("Brinestalk Gate");
			text = townSquareRect.GetCellSide(randomDoorCell);
		}
		if (Stat.Random(1, 100) <= 75)
		{
			if (text == null)
			{
				text = Directions.GetRandomCardinalDirection();
			}
			Rect2D r = townSquareRect.ReduceBy(0, 0);
			int num = 0;
			if (text == "N")
			{
				num = ((Stat.Random(0, 1) == 0) ? 2 : 3);
			}
			if (text == "S")
			{
				num = ((Stat.Random(0, 1) != 0) ? 1 : 0);
			}
			if (text == "E")
			{
				num = ((Stat.Random(0, 1) != 0) ? 3 : 0);
			}
			if (text == "W")
			{
				num = ((Stat.Random(0, 1) == 0) ? 1 : 2);
			}
			if (num == 0 || num == 1)
			{
				r.y2 = r.y1 + 3;
			}
			else
			{
				r.y1 = r.y2 - 3;
			}
			if (num == 0 || num == 3)
			{
				r.x2 = r.x1 + 3;
			}
			else
			{
				r.x1 = r.x2 - 3;
			}
			ClearRect(zone, r);
			ZoneBuilderSandbox.PlaceObjectOnRect(zone, getAVillageWall(), r);
			Point2D randomDoorCell2 = r.GetRandomDoorCell(text, 1);
			burrowedDoors.Add(Location2D.get(randomDoorCell2.x, randomDoorCell2.y));
			zone.GetCell(randomDoorCell2)?.Clear();
			zone.GetCell(randomDoorCell2)?.AddObject(getAVillageDoor());
		}
		int num2 = Stat.Random(3, 10);
		for (int i = 0; i < num2; i++)
		{
			GameObject @object = GameObject.create("Tombstone", 0, 0, null, delegate(GameObject o)
			{
				if (o.GetPart("Tombstone") is Tombstone tombstone)
				{
					tombstone.name = NameMaker.MakeName(getBaseVillager());
				}
			});
			ZoneBuilderSandbox.PlaceObjectInRect(zone, townSquareRect.ReduceBy(1, 1), @object);
		}
	}

	public void FabricateHut(PopulationLayout layout, bool isRound)
	{
		Location2D position = layout.position;
		Zone zone = layout.zone;
		int num = Math.Max(6, Stat.Random(layout.innerRect.Width - 2, layout.innerRect.Width));
		int num2 = Math.Max(6, Stat.Random(layout.innerRect.Height - 2, layout.innerRect.Height));
		int num3 = position.x - num / 2;
		int num4 = position.x + num / 2;
		int num5 = position.y - num2 / 2;
		int num6 = position.y + num2 / 2;
		_ = 79;
		int num7 = 0;
		int num8 = 24;
		int num9 = 0;
		if (num8 > num5)
		{
			num8 = num5;
		}
		if (num7 < num4)
		{
			num7 = num4;
		}
		if (num9 < num6)
		{
			num9 = num6;
		}
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num5; j <= num6; j++)
			{
				zone.GetCell(i, j).Clear();
				zone.GetCell(i, j).AddObject("DirtPath");
			}
		}
		if (isRound)
		{
			for (int k = num3 + 1; k <= num4 - 1; k++)
			{
				zone.GetCell(k, num5).Clear();
				zone.GetCell(k, num5).AddObject(getAVillageWall());
				zone.GetCell(k, num6).Clear();
				zone.GetCell(k, num6).AddObject(getAVillageWall());
			}
			for (int l = num5 + 1; l <= num6 - 1; l++)
			{
				zone.GetCell(num3, l).Clear();
				zone.GetCell(num3, l).AddObject(getAVillageWall());
				zone.GetCell(num4, l).Clear();
				zone.GetCell(num4, l).AddObject(getAVillageWall());
			}
			zone.GetCell(num3 + 1, num5 + 1).Clear();
			zone.GetCell(num3 + 1, num5 + 1).AddObject(getAVillageWall());
			zone.GetCell(num4 - 1, num5 + 1).Clear();
			zone.GetCell(num4 - 1, num5 + 1).AddObject(getAVillageWall());
			zone.GetCell(num3 + 1, num6 - 1).Clear();
			zone.GetCell(num3 + 1, num6 - 1).AddObject(getAVillageWall());
			zone.GetCell(num4 - 1, num6 - 1).Clear();
			zone.GetCell(num4 - 1, num6 - 1).AddObject(getAVillageWall());
		}
		else
		{
			for (int m = num3; m <= num4; m++)
			{
				zone.GetCell(m, num5).Clear();
				zone.GetCell(m, num5).AddObject(getAVillageWall());
				zone.GetCell(m, num6).Clear();
				zone.GetCell(m, num6).AddObject(getAVillageWall());
			}
			for (int n = num5; n <= num6; n++)
			{
				zone.GetCell(num3, n).Clear();
				zone.GetCell(num3, n).AddObject(getAVillageWall());
				zone.GetCell(num4, n).Clear();
				zone.GetCell(num4, n).AddObject(getAVillageWall());
			}
		}
	}

	public void FabricateTent(PopulationLayout layout)
	{
		Zone z = layout.zone;
		Rect2D innerRect = layout.innerRect;
		Location2D location = innerRect.Center.location;
		Location2D location2 = innerRect.GetRandomDoorCell(Calc.GetOppositeDirection(location.RegionDirection(80, 25)[0])).location;
		if (innerRect.DoorDirection == "N" || innerRect.DoorDirection == "S")
		{
			innerRect.Pinch = innerRect.Height / 2;
		}
		if (innerRect.DoorDirection == "E" || innerRect.DoorDirection == "W")
		{
			innerRect.Pinch = innerRect.Width / 2;
		}
		ZoneBuilderSandbox.PlaceObjectOnRect(z, getAVillageCanvas(), innerRect, bClear: true);
		GetCell(z, location2).Clear();
		burrowedDoors.Add(GetCell(z, location2).location);
		layout.position = innerRect.Center.location;
	}

	public static void MakeCaveBuilding(Zone Z, InfluenceMap IF, InfluenceMapRegion R, string WallObject = "Shale")
	{
		int num = 4;
		foreach (Location2D cell in R.Cells)
		{
			if (IF.CostMap[cell.x, cell.y] == num)
			{
				Z.GetCell(cell.x, cell.y).Clear();
				Z.GetCell(cell.x, cell.y).AddObject(WallObject);
			}
		}
	}

	public void SnakeToConnections(Location2D townCenter)
	{
		foreach (CachedZoneConnection item in zone.ZoneConnectionCache)
		{
			if (!(item.TargetDirection == "-") || zone.IsReachable(item.X, item.Y))
			{
				continue;
			}
			Pathfinder pathfinder = new Pathfinder(zone.Width, zone.Height);
			if (!pathfinder.FindPath(Location2D.get(item.X, item.Y), townCenter, bDisplay: false, bOrdinalDirectionsOnly: true))
			{
				continue;
			}
			foreach (PathfinderNode step in pathfinder.Steps)
			{
				if (zone.GetCell(step.X, step.Y).HasObjectWithTag("Wall"))
				{
					burrowedDoors.Add(Location2D.get(step.X, step.Y));
					zone.GetCell(step.X, step.Y).ClearObjectsWithTag("Wall");
					pathfinder.CurrentNavigationMap[step.X, step.Y] = 0;
					step.weight = 0;
				}
				zone.SetReachable(step.X, step.Y);
			}
		}
	}

	public void CarvePathwaysFromLocations(Zone Z, bool bCarveDoors, InfluenceMap map, Location2D townCenter)
	{
		List<Location2D> list = new List<Location2D>();
		if (map != null)
		{
			foreach (Location2D seed in map.Seeds)
			{
				list.Add(seed);
			}
		}
		CarvePathwaysFromLocations(Z, bCarveDoors, list, townCenter);
	}

	public void CarvePathwaysFromLocations(Zone Z, bool bCarveDoors, List<PopulationLayout> buildings, Location2D townCenter)
	{
		List<Location2D> list = new List<Location2D>();
		if (buildings != null)
		{
			foreach (PopulationLayout building in buildings)
			{
				list.Add(building.position);
			}
		}
		CarvePathwaysFromLocations(Z, bCarveDoors, list, townCenter);
	}

	public void CarvePathwaysFromLocations(Zone Z, bool bCarveDoors, List<Location2D> locations, Location2D townCenter)
	{
		int num = 80;
		int num2 = 25;
		InfluenceMap influenceMap = new InfluenceMap(num, num2);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				influenceMap.Walls[i, j] = (Z.GetCell(i, j).HasObjectWithTag("Wall") ? 1 : 0);
			}
		}
		if (locations != null)
		{
			foreach (Location2D location in locations)
			{
				influenceMap.AddSeed(location);
			}
		}
		influenceMap.SeedAllUnseeded();
		if (influenceMap.Seeds.Count == 0)
		{
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.SeedAllUnseeded();
		}
		int num3 = 0;
		Pathfinder pathfinder = new Pathfinder(num, num2);
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				if (influenceMap.Walls[k, l] > 0)
				{
					pathfinder.CurrentNavigationMap[k, l] = 999;
				}
				else
				{
					pathfinder.CurrentNavigationMap[k, l] = noiseMap.Noise[k, l];
				}
			}
		}
		LogMem("step5");
		List<int> list = new List<int>();
		list.Add(num3);
		for (int m = 0; m < influenceMap.Seeds.Count; m++)
		{
			if (list.Contains(m))
			{
				continue;
			}
			int num4 = influenceMap.FindClosestSeedToInList(m, list);
			if (num4 == -1)
			{
				num4 = num3;
			}
			_ = influenceMap.Seeds[num4];
			if (pathfinder.FindPath(influenceMap.Seeds[num4], influenceMap.Seeds[m], bDisplay: false, bOrdinalDirectionsOnly: true))
			{
				int num5 = 0;
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					if (Z.GetCell(step.X, step.Y).HasObjectWithTag("Wall"))
					{
						burrowedDoors.Add(Location2D.get(step.X, step.Y));
						Z.GetCell(step.X, step.Y).ClearObjectsWithTag("Wall");
						if (num5 == 0 && bCarveDoors)
						{
							burrowedDoors.Add(Location2D.get(step.X, step.Y));
							Z.GetCell(step.X, step.Y).AddObject(getAVillageDoor());
							num5++;
						}
						pathfinder.CurrentNavigationMap[step.X, step.Y] = 0;
						step.weight = 0;
					}
				}
			}
			list.Add(m);
		}
		for (int n = 0; n < list.Count; n++)
		{
			if (!Z.IsReachable(influenceMap.Seeds[n].x, influenceMap.Seeds[n].y))
			{
				Z.BuildReachableMap(influenceMap.Seeds[n].x, influenceMap.Seeds[n].y);
			}
		}
	}

	public void Clean(Zone Z, int minsize = 9)
	{
		int num = 80;
		int num2 = 25;
		InfluenceMap influenceMap = new InfluenceMap(num, num2);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				influenceMap.Walls[i, j] = ((!Z.GetCell(i, j).HasObjectWithTag("Wall")) ? 1 : 0);
			}
		}
		influenceMap.SeedAllUnseeded();
		if (influenceMap.Seeds.Count == 0)
		{
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.SeedAllUnseeded();
		}
		foreach (InfluenceMapRegion region in influenceMap.Regions)
		{
			if (region.Size >= minsize)
			{
				continue;
			}
			foreach (Location2D cell in region.Cells)
			{
				Z.GetCell(cell).ClearObjectsWithTag("Wall");
			}
		}
	}

	public void cleanup()
	{
		buildings = null;
		villageSnapshot = null;
		villageEntity = null;
		templates = null;
		MemoryHelper.GCCollect();
	}

	public string ResolvePopulationTableName(string tablePrefix)
	{
		if (PopulationManager.HasTable(tablePrefix + "_Faction_" + villagerBaseFaction))
		{
			return tablePrefix + "_Faction_" + villagerBaseFaction;
		}
		if (PopulationManager.HasTable(tablePrefix + "_" + region))
		{
			return tablePrefix + "_" + region;
		}
		return tablePrefix + "_*Default";
	}

	public GameObject generateVillageOven()
	{
		GameObject gameObject = GameObjectFactory.create("Oven");
		gameObject.SetIntProperty("QuestItem", 1);
		Campfire part = gameObject.GetPart<Campfire>();
		part.specificProcgenMeals = new List<CookingRecipe>();
		part.specificProcgenMeals.Add(signatureDish);
		return gameObject;
	}

	public void generateSignatureItems()
	{
		if (villageSnapshot.hasProperty("signatureItem"))
		{
			signatureItemBlueprint = villageSnapshot.GetProperty("signatureItem");
			signatureItemExample = GameObject.create(signatureItemBlueprint);
		}
		if (villageSnapshot.hasProperty("signatureHistoricObjectType"))
		{
			signatureHistoricObjectType = villageSnapshot.GetProperty("signatureHistoricObjectType");
			signatureHistoricObjectInstance = GameObject.create(signatureHistoricObjectType);
			signatureHistoricObjectInstance.DisplayName = villageSnapshot.GetProperty("signatureHistoricObjectName");
		}
	}

	public void fenceOff(GameObject go)
	{
		if (go.GetCurrentCell() == null)
		{
			return;
		}
		foreach (Cell localAdjacentCell in go.GetCurrentCell().GetLocalAdjacentCells())
		{
			if (!localAdjacentCell.HasObjectWithTag("Wall"))
			{
				localAdjacentCell.AddObject("IronFence");
			}
		}
	}

	public string getZoneDefaultLiquid(Zone zone)
	{
		return zone?.GetTerrainObject()?.GetTag("VillageDefaultLiquid", "SaltyWaterPuddle") ?? "SaltyWaterPuddle";
	}

	public void placeNonTakeableSignatureItems()
	{
		if (signatureItemBlueprint != null && !signatureItemExample.IsTakeable())
		{
			string hint = null;
			if (signatureItemExample.HasTag("Furniture"))
			{
				hint = "Inside";
			}
			int num = signatureItemExample.GetTag("VillageSignatureItemNumber", "2-6").RollCached();
			for (int i = 0; i < num; i++)
			{
				PlaceObjectInBuilding(GameObject.create(signatureItemBlueprint), buildings.GetRandomElement(), hint);
			}
		}
	}

	public GameObject getAGenericFarmPlantNearTier()
	{
		int num = 0;
		GameObject gameObject;
		do
		{
			gameObject = GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:FarmablePlants").Blueprint);
			if (num > 10)
			{
				gameObject = GameObject.create("Watervine");
				break;
			}
			num++;
		}
		while (gameObject.HasTag("NoVillageFarming") || gameObject.GetTier() > villageTier + 1);
		return gameObject;
	}

	public GameObject getARegionalPlant()
	{
		GameObject gameObject;
		if (isVillageZero)
		{
			gameObject = GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:" + region + "_FarmablePlants").Blueprint);
		}
		else
		{
			int num = 0;
			do
			{
				if (num > 10)
				{
					gameObject = GameObject.create("Watervine");
					break;
				}
				if (originalPlants.Count > num && num < 3)
				{
					gameObject = originalPlants.GetRandomElement();
				}
				else
				{
					string blueprint = PopulationManager.RollOneFrom("DynamicObjectsTable:" + region + "_Plants").Blueprint;
					gameObject = ((blueprint == null) ? getAGenericFarmPlantNearTier() : GameObject.create(blueprint));
				}
				num++;
			}
			while (gameObject.HasTag("NoVillageFarming"));
		}
		return gameObject;
	}

	public GameObject getARandomPlantNearTier()
	{
		return EncountersAPI.GetAnObject((GameObjectBlueprint o) => (o.HasTagOrProperty("Plant") || o.HasTagOrProperty("PlantLike")) && (o.Tier == villageTier || o.Tier == villageTier - 1 || o.Tier == villageTier - 2) && !o.HasTagOrProperty("NoVillageFarming"));
	}

	public GameObject getAFarmablePlant()
	{
		GameObject gameObject;
		if (isVillageZero)
		{
			gameObject = getARegionalPlant();
		}
		else
		{
			int num = Stat.Roll(1, 100);
			string tagOrStringProperty = GameObject.create("Terrain" + region).GetTagOrStringProperty("VillageRegionalFarmPlantChance");
			int num2 = ((!string.IsNullOrEmpty(tagOrStringProperty)) ? int.Parse(tagOrStringProperty) : 98);
			gameObject = ((num <= num2) ? getARegionalPlant() : ((num > num2 + 2) ? getAGenericFarmPlantNearTier() : getARandomPlantNearTier()));
		}
		setVillageDomesticatedProperties(gameObject);
		return gameObject;
	}

	public GameObject getARegionalCreature()
	{
		if (originalCreatures.Count > 0)
		{
			return originalCreatures.GetRandomElement();
		}
		return EncountersAPI.GetALegendaryEligibleCreature();
	}

	public GameObject getAVillageMonument()
	{
		if (villageMonumentPrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_MonumentType")).Blueprint;
			villageMonumentPrototype = GameObject.create(blueprint);
			LiquidVolume liquidVolume = villageMonumentPrototype.LiquidVolume;
			if (liquidVolume != null)
			{
				liquidVolume.InitialLiquid = signatureLiquid + "-1000";
			}
			villageMonumentPrototype.RequirePart<Interesting>();
		}
		return villageMonumentPrototype.DeepCopy();
	}

	public GameObject getAVillageLightSource()
	{
		if (villageLightSourcePrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_LightSourceType")).Blueprint;
			villageLightSourcePrototype = GameObject.create(blueprint);
		}
		return villageLightSourcePrototype.DeepCopy();
	}

	public GameObject getAVillageBook()
	{
		if (villageBookPrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BookType")).Blueprint;
			villageBookPrototype = GameObject.create(blueprint);
		}
		return villageBookPrototype.DeepCopy();
	}

	public bool fabricateStoryBuilding()
	{
		if (JournalAPI.GetNotesForVillage(villageEntity.id).Count > 0)
		{
			if (storyType == "Historic Hall" || storyType == "Holograms")
			{
				FabricateHistoricHall(townSquare);
				return true;
			}
			if (storyType == "Library")
			{
				FabricateLibrary(townSquare);
				return true;
			}
			if (storyType == "Graveyard")
			{
				FabricateGraveyard(townSquare);
				return true;
			}
		}
		return true;
	}

	protected HistoricPerspective storyPerspective(JournalVillageNote story)
	{
		return villageSnapshot.requirePerspective(villageEntity.history.GetEvent(story.getEventId()), Stat.Random(300, 1000));
	}

	public void clearDegenerateDoors()
	{
		bool flag;
		do
		{
			flag = false;
			for (int i = 1; i < zone.Width - 1; i++)
			{
				for (int j = 1; j < zone.Height - 1; j++)
				{
					Cell cell = zone.GetCell(i, j);
					GameObject firstObjectWithTag = cell.GetFirstObjectWithTag("Door");
					if (firstObjectWithTag == null)
					{
						continue;
					}
					bool sync = false;
					bool foundSync = false;
					if (firstObjectWithTag.HasPart("Door"))
					{
						sync = firstObjectWithTag.GetPart<Door>().SyncAdjacent;
					}
					Predicate<Cell> obj2 = delegate(Cell AC)
					{
						if (AC.HasObjectWithIntProperty("Wall"))
						{
							return true;
						}
						bool result = false;
						if (sync)
						{
							AC.ForeachObjectWithTag("Door", delegate(GameObject obj)
							{
								if (obj.GetPart<Door>().SyncAdjacent)
								{
									result = true;
									foundSync = true;
									return false;
								}
								return true;
							});
						}
						return result;
					};
					int num = 0;
					int num2 = 0;
					if (obj2(zone.GetCell(i - 1, j)))
					{
						num2++;
					}
					if (obj2(zone.GetCell(i + 1, j)))
					{
						num2++;
					}
					if (obj2(zone.GetCell(i, j - 1)))
					{
						num++;
					}
					if (obj2(zone.GetCell(i, j + 1)))
					{
						num++;
					}
					if (num + num2 != 2 || num == 1 || num2 == 1 || (sync && !foundSync))
					{
						bool flag2 = cell.HasObjectWithIntProperty("ReplaceWithWall");
						cell.Clear();
						flag = true;
						if (flag2)
						{
							cell.AddObject(getAVillageWall());
						}
					}
				}
			}
		}
		while (flag);
	}

	public Cell checkForOpenWall(GameObject go, string deltaUp, string deltaLeft, string deltaRight)
	{
		Cell cellFromDirection = go.GetCurrentCell().GetCellFromDirection(deltaUp);
		if (cellFromDirection == null)
		{
			return null;
		}
		if (!cellFromDirection.HasObjectWithTag("Wall"))
		{
			return null;
		}
		Cell cellFromDirection2 = cellFromDirection.GetCellFromDirection(deltaLeft);
		if (cellFromDirection2 == null)
		{
			return null;
		}
		if (cellFromDirection2.HasObjectWithTag("Wall"))
		{
			return null;
		}
		Cell cellFromDirection3 = cellFromDirection.GetCellFromDirection(deltaRight);
		if (cellFromDirection3 == null)
		{
			return null;
		}
		if (cellFromDirection3.HasObjectWithTag("Wall"))
		{
			return null;
		}
		return cellFromDirection;
	}

	public void applyDoorFilters()
	{
		string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_DoorFilter")).Blueprint;
		string[] array = blueprint.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			if (blueprint == "Pair")
			{
				List<Cell> list = new List<Cell>();
				foreach (GameObject item in zone.GetObjectsWithTag("Door"))
				{
					if (item.HasPart("Door") && item.GetPart<Door>().SyncAdjacent)
					{
						continue;
					}
					string deltaUp = "N";
					string deltaUp2 = "S";
					string deltaLeft = "W";
					string deltaRight = "E";
					for (int j = 0; j <= 1; j++)
					{
						list.Clear();
						if (j == 1)
						{
							deltaUp = "E";
							deltaUp2 = "W";
							deltaLeft = "N";
							deltaRight = "S";
						}
						list.AddIfNotNull(checkForOpenWall(item, deltaUp, deltaLeft, deltaRight));
						list.AddIfNotNull(checkForOpenWall(item, deltaUp2, deltaLeft, deltaRight));
						if (list.Count > 0)
						{
							list.GetRandomElement().Clear().AddObject(item.DeepCopy());
						}
					}
				}
			}
			if (!(blueprint == "Divided"))
			{
				continue;
			}
			List<Cell> list2 = new List<Cell>();
			foreach (GameObject item2 in zone.GetObjectsWithTag("Door"))
			{
				if (item2.HasPart("Door") && item2.GetPart<Door>().SyncAdjacent)
				{
					continue;
				}
				string text = "N";
				string text2 = "S";
				string deltaLeft2 = "W";
				string deltaRight2 = "E";
				for (int k = 0; k <= 1; k++)
				{
					list2.Clear();
					if (k == 1)
					{
						text = "E";
						text2 = "W";
						deltaLeft2 = "N";
						deltaRight2 = "S";
					}
					if (checkForOpenWall(item2, text, deltaLeft2, deltaRight2) != null && checkForOpenWall(item2.GetCurrentCell().GetCellFromDirection(text).Objects[0], text, deltaLeft2, deltaRight2) != null && checkForOpenWall(item2, text2, deltaLeft2, deltaRight2) != null && checkForOpenWall(item2.GetCurrentCell().GetCellFromDirection(text2).Objects[0], text, deltaLeft2, deltaRight2) != null)
					{
						item2.GetCurrentCell().GetCellFromDirection(text).Clear()
							.AddObject(item2.DeepCopy());
						item2.GetCurrentCell().GetCellFromDirection(text2).Clear()
							.AddObject(item2.DeepCopy());
						item2.GetCurrentCell().Clear().AddObject(getAVillageWall());
					}
				}
			}
		}
	}

	protected bool tryPlace(int x, int y, List<GameObject> objs)
	{
		if (objs.Count == 0)
		{
			return false;
		}
		Cell cell = zone.GetCell(x, y);
		if (cell != null && cell.IsEmptyForPopulation())
		{
			cell.AddObject(objs[0]);
			objs.RemoveAt(0);
			return true;
		}
		return false;
	}

	protected bool tryPlace(int x, int y, string Blueprint)
	{
		Cell cell = zone.GetCell(x, y);
		if (cell != null && cell.IsEmptyForPopulation())
		{
			cell.AddObject(Blueprint);
			return true;
		}
		return false;
	}

	protected bool tryPlace(int x, int y, Producer<GameObject> generator)
	{
		Cell cell = zone.GetCell(x, y);
		if (cell != null && cell.IsEmptyForPopulation())
		{
			GameObject gameObject = generator();
			if (gameObject != null)
			{
				cell.AddObject(gameObject);
				return true;
			}
		}
		return false;
	}

	protected int placeInRectSymmetricallyIfPossible(Rect2D rect, List<GameObject> objs, bool Walled, PopulationLayout backupLayout = null, string backupLayoutHint = null)
	{
		int num = objs.Count;
		if (objs.Count > 0)
		{
			int num2 = (rect.x1 + rect.x2) / 2;
			bool flag = (rect.x2 - rect.x1 + 1) % 2 == 1;
			int num3 = (rect.y1 + rect.y2) / 2;
			bool flag2 = (rect.y2 - rect.y1 + 1) % 2 == 1;
			if (objs.Count == 1)
			{
				tryPlace(num2, num3, objs);
				if (objs.Count > 0)
				{
					tryPlace(num2 + 1, num3, objs);
				}
				if (objs.Count > 0)
				{
					tryPlace(num2, num3 + 1, objs);
				}
				if (objs.Count > 0)
				{
					tryPlace(num2 + 1, num3 + 1, objs);
				}
			}
			else if (objs.Count == 2)
			{
				switch (Stat.Random(0, 3))
				{
				case 0:
					tryPlace((flag || !Walled) ? (num2 - 1) : num2, num3, objs);
					tryPlace(num2 + 1, num3, objs);
					break;
				case 1:
					tryPlace(num2, (flag2 || !Walled) ? (num3 - 1) : num3, objs);
					tryPlace(num2, num3 + 1, objs);
					break;
				case 2:
					tryPlace((flag || !Walled) ? (num2 - 1) : num2, (flag2 || !Walled) ? (num3 - 1) : num3, objs);
					tryPlace(num2 + 1, num3 + 1, objs);
					break;
				case 3:
					tryPlace((flag || !Walled) ? (num2 - 1) : num2, num3 + 1, objs);
					tryPlace(num2 + 1, (flag2 || !Walled) ? (num3 - 1) : num3, objs);
					break;
				}
				tryPlace(num2, num3, objs);
			}
			else if (objs.Count == 3)
			{
				if (Stat.Random(0, 1) == 0)
				{
					tryPlace(num2 - 1, num3 - 1, objs);
					tryPlace(num2, num3, objs);
					tryPlace(num2 + 1, num3 + 1, objs);
				}
				else
				{
					tryPlace(num2 + 1, num3 - 1, objs);
					tryPlace(num2, num3, objs);
					tryPlace(num2 - 1, num3 + 1, objs);
				}
			}
			else if (objs.Count == 5)
			{
				if (Stat.Random(0, 1) == 0)
				{
					tryPlace(num2, num3, objs);
					tryPlace(num2 + 1, num3 + 1, objs);
					tryPlace(num2 - 1, num3 - 1, objs);
					tryPlace(num2 - 1, num3 + 1, objs);
					tryPlace(num2 + 1, num3 - 1, objs);
				}
				else if (Stat.Random(0, 1) == 0)
				{
					tryPlace(num2 - 2, num3 - 2, objs);
					tryPlace(num2 - 1, num3 - 1, objs);
					tryPlace(num2, num3, objs);
					tryPlace(num2 + 1, num3 + 1, objs);
					tryPlace(num2 + 2, num3 + 2, objs);
				}
				else
				{
					tryPlace(num2 + 2, num3 - 2, objs);
					tryPlace(num2 + 1, num3 - 1, objs);
					tryPlace(num2, num3, objs);
					tryPlace(num2 - 1, num3 + 1, objs);
					tryPlace(num2 - 2, num3 + 2, objs);
				}
			}
			else if (Stat.Random(0, 1) == 0)
			{
				tryPlace(num2, num3, objs);
				if ((!flag && !flag2) || !Walled)
				{
					tryPlace(num2 + 1, num3 + 1, objs);
				}
				if (!flag || !Walled)
				{
					tryPlace(num2 + 1, num3, objs);
				}
				if (!flag2 || !Walled)
				{
					tryPlace(num2, num3 + 1, objs);
				}
				if (objs.Count > 0)
				{
					Rect2D rect2D = rect.ReduceBy(1, 1);
					if (rect2D.Area > 4)
					{
						tryPlace(rect2D.x1, rect2D.y1, objs);
						tryPlace(rect2D.x2, rect2D.y2, objs);
						tryPlace(rect2D.x1, rect2D.y2, objs);
						tryPlace(rect2D.x2, rect2D.y1, objs);
					}
				}
			}
			else
			{
				Rect2D rect2D2 = rect.ReduceBy(1, 1);
				if (rect2D2.Area > 4)
				{
					tryPlace(rect2D2.x1, rect2D2.y1, objs);
					tryPlace(rect2D2.x2, rect2D2.y2, objs);
					tryPlace(rect2D2.x1, rect2D2.y2, objs);
					tryPlace(rect2D2.x2, rect2D2.y1, objs);
				}
				tryPlace(num2, num3, objs);
				if ((!flag && !flag2) || !Walled)
				{
					tryPlace(num2 + 1, num3 + 1, objs);
				}
				if (!flag || !Walled)
				{
					tryPlace(num2 + 1, num3, objs);
				}
				if (!flag2 || !Walled)
				{
					tryPlace(num2, num3 + 1, objs);
				}
			}
			{
				foreach (GameObject obj in objs)
				{
					if (!ZoneBuilderSandbox.PlaceObjectInRect(zone, rect, obj) && (backupLayout == null || !PlaceObjectInBuilding(obj, backupLayout, backupLayoutHint)))
					{
						Debug.LogWarning("failed to place " + obj.DisplayName);
						num--;
					}
				}
				return num;
			}
		}
		return num;
	}

	public void placeStatues()
	{
		List<GameObject> list = new List<GameObject>();
		if (villageSnapshot.hasProperty("worships_creature"))
		{
			GameObject cachedObjects = The.ZoneManager.GetCachedObjects(villageSnapshot.GetProperty("worships_creature_id"));
			int num = Stat.Random(1, 3);
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = GameObject.create(RollOneFrom("Village_RandomBaseStatue"));
				gameObject.GetPart<RandomStatue>().SetCreature(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
				list.Add(gameObject);
			}
		}
		if (villageSnapshot.hasProperty("despises_creature"))
		{
			GameObject cachedObjects2 = The.ZoneManager.GetCachedObjects(villageSnapshot.GetProperty("despises_creature_id"));
			int num2 = Stat.Random(1, 3);
			for (int j = 0; j < num2; j++)
			{
				GameObject gameObject2 = GameObject.create(RollOneFrom("Village_RandomBaseStatue"));
				gameObject2.GetPart<RandomStatue>().SetCreature(cachedObjects2.DeepCopy(CopyEffects: false, CopyID: true));
				gameObject2.AddPart(new ModDesecrated());
				list.Add(gameObject2);
			}
		}
		if (villageSnapshot.hasProperty("worships_faction"))
		{
			int num3 = Stat.Random(1, 3);
			for (int k = 0; k < num3; k++)
			{
				GameObject gameObject3 = EncountersAPI.GetALegendaryEligibleCreatureFromFaction(villageSnapshot.GetProperty("worships_faction")) ?? EncountersAPI.GetACreatureFromFaction(villageSnapshot.GetProperty("worships_faction"));
				if (gameObject3 != null)
				{
					GameObject gameObject4 = GameObject.create(RollOneFrom("Village_RandomBaseStatue"));
					gameObject4.GetPart<RandomStatue>().SetCreature(gameObject3.DeepCopy(CopyEffects: false, CopyID: true));
					list.Add(gameObject4);
				}
			}
		}
		if (villageSnapshot.hasProperty("despises_faction"))
		{
			int num4 = Stat.Random(1, 3);
			for (int l = 0; l < num4; l++)
			{
				GameObject gameObject5 = EncountersAPI.GetALegendaryEligibleCreatureFromFaction(villageSnapshot.GetProperty("despises_faction")) ?? EncountersAPI.GetACreatureFromFaction(villageSnapshot.GetProperty("despises_faction"));
				if (gameObject5 != null)
				{
					GameObject gameObject6 = GameObject.create(RollOneFrom("Village_RandomBaseStatue"));
					gameObject6.GetPart<RandomStatue>().SetCreature(gameObject5.DeepCopy(CopyEffects: false, CopyID: true));
					gameObject6.AddPart(new ModDesecrated());
					list.Add(gameObject6);
				}
			}
		}
		if (villageSnapshot.hasProperty("worships_sultan"))
		{
			int num5 = Stat.Random(1, 3);
			for (int m = 0; m < num5; m++)
			{
				GameObject gameObject7 = GameObject.create("SultanShrine");
				gameObject7.SetStringProperty("ForceSultan", villageSnapshot.GetProperty("worships_sultan_id"));
				list.Add(gameObject7);
			}
		}
		if (villageSnapshot.hasProperty("despises_sultan"))
		{
			int num6 = Stat.Random(1, 3);
			for (int n = 0; n < num6; n++)
			{
				GameObject gameObject8 = GameObject.create("SultanShrine");
				gameObject8.SetStringProperty("ForceSultan", villageSnapshot.GetProperty("despises_sultan_id"));
				gameObject8.AddPart(new ModDesecrated());
				list.Add(gameObject8);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			switch (Stat.Random(0, 1))
			{
			case 0:
				PlaceObjectInBuilding(item, buildings.GetRandomElement());
				break;
			case 1:
				ZoneBuilderSandbox.PlaceObject(item, zone);
				break;
			}
		}
	}

	protected void placeExtraBooks(List<GameObject> bookshelves)
	{
		string text = ResolvePopulationTableName("Villages_ExtraBooks");
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		foreach (GameObject bookshelf in bookshelves)
		{
			foreach (PopulationResult item in PopulationManager.Generate(text))
			{
				if (string.IsNullOrEmpty(item.Blueprint))
				{
					continue;
				}
				for (int i = 0; i < item.Number; i++)
				{
					GameObject gameObject;
					if (item.Blueprint.EndsWith(".json"))
					{
						gameObject = GameObject.create("MarkovBook");
						gameObject.GetPart<MarkovBook>().SetContents(Stat.Random(0, 2147483646), item.Blueprint);
					}
					else
					{
						gameObject = GameObject.create(item.Blueprint);
					}
					bookshelf.Inventory.AddObject(gameObject);
				}
			}
		}
	}

	public void placeStories()
	{
		try
		{
			List<JournalVillageNote> notesForVillage = JournalAPI.GetNotesForVillage(villageEntity.id);
			int num = 75;
			if (notesForVillage.Count <= 0)
			{
				return;
			}
			string objectMarkingStyle = null;
			HistoricPerspective perspective = null;
			JournalVillageNote story = null;
			int storyPlacements = 0;
			Action<GameObject> action = delegate(GameObject obj)
			{
				if (obj != null && !obj.HasPart("VillageStoryReveal"))
				{
					string text;
					string text2;
					if (objectMarkingStyle == "engraving")
					{
						text = "Y";
						text2 = "C";
					}
					else
					{
						text = perspective.mainColor;
						text2 = perspective.supportColor;
					}
					if (objectMarkingStyle == "tattoo")
					{
						string desc = "a scene from the history of the village {{M|" + villageName + "}}{{C|: " + story.text.Split('|')[0] + "}}";
						if (Tattoos.ApplyTattoo(obj, desc, "&" + text, text2) == Tattoos.ApplyResult.Success)
						{
							obj.AddPart(new VillageStoryReveal(story, objectMarkingStyle));
						}
					}
					else
					{
						obj.AddPart(new VillageStoryReveal(story, objectMarkingStyle));
						obj.pRender.ColorString = "&" + text;
						obj.pRender.DetailColor = text2;
						if (!string.IsNullOrEmpty(obj.pRender.TileColor))
						{
							obj.pRender.TileColor = obj.pRender.ColorString;
						}
					}
					storyPlacements++;
				}
			};
			int num2 = 0;
			bool flag = false;
			while (true)
			{
				if (storyType == "Monuments")
				{
					objectMarkingStyle = "monument";
					List<GameObject> list = new List<GameObject>(notesForVillage.Count);
					foreach (JournalVillageNote item in notesForVillage)
					{
						story = item;
						perspective = storyPerspective(story);
						GameObject aVillageMonument = getAVillageMonument();
						action(aVillageMonument);
						list.Add(aVillageMonument);
					}
					Rect2D rect = BackFromEdge(GridTools.MaxRectByArea(townSquare.GetGrid()).Translate(townSquare.BoundingBox.UpperLeft));
					while (rect.Area > 36)
					{
						rect = rect.ReduceBy((rect.Width >= rect.Height) ? 1 : 0, (rect.Height >= rect.Width) ? 1 : 0);
					}
					storyPlacements -= notesForVillage.Count;
					storyPlacements += placeInRectSymmetricallyIfPossible(rect, list, Walled: false, townSquareLayout);
				}
				else if (storyType == "Historic Hall")
				{
					objectMarkingStyle = "monument";
					List<GameObject> list2 = new List<GameObject>(notesForVillage.Count);
					foreach (JournalVillageNote item2 in notesForVillage)
					{
						story = item2;
						perspective = storyPerspective(story);
						GameObject aVillageMonument2 = getAVillageMonument();
						action(aVillageMonument2);
						list2.Add(aVillageMonument2);
					}
					storyPlacements -= notesForVillage.Count;
					storyPlacements += placeInRectSymmetricallyIfPossible(townSquareRect, list2, Walled: true, townSquareLayout, "Inside");
					foreach (PopulationResult item3 in PopulationManager.Generate(ResolvePopulationTableName("Village_HistoricHallContents")))
					{
						for (int i = 0; i < item3.Number; i++)
						{
							GameObject gameObject = GameObject.create(item3.Blueprint);
							if (!ZoneBuilderSandbox.PlaceObjectInRect(zone, townSquareRect, gameObject) && !PlaceObjectInBuilding(gameObject, townSquareLayout, "Inside"))
							{
								Debug.LogWarning("failed to place " + gameObject.DisplayName);
							}
						}
					}
				}
				else if (storyType == "Town Crier")
				{
					GameObject gameObject2 = GameObject.create("Village Crier");
					setVillagerProperties(gameObject2);
					AddVillagerConversation(gameObject2, gameObject2.GetTag("SimpleConversation", "I know a lot about the town!"));
					for (int j = 0; j < notesForVillage.Count; j++)
					{
						story = notesForVillage[j];
						perspective = storyPerspective(story);
						ConversationsAPI.addSimpleRootInformationOption(gameObject2, "Tell me about {{" + perspective.mainColor + "|" + story.text.Snippet() + "}}...", story.text.Split('|')[0]);
						storyPlacements++;
					}
					gameObject2.Inventory.AddObject(generatePreacherBook());
					if (!ZoneBuilderSandbox.PlaceObject(zone, townSquare, gameObject2))
					{
						Debug.LogWarning("failed to place " + gameObject2.DisplayName);
						storyPlacements -= notesForVillage.Count;
					}
				}
				else if (storyType == "Books")
				{
					List<GameObject> list3 = generateHistoryBooks();
					List<GameObject> list4 = new List<GameObject>(list3.Count);
					foreach (GameObject item4 in list3)
					{
						GameObject randomElement;
						if (Stat.Random(0, 9) < list4.Count)
						{
							randomElement = list4.GetRandomElement();
							randomElement.Inventory.AddObject(item4);
							continue;
						}
						randomElement = GameObject.create("Bookshelf");
						if (PlaceObjectInBuilding(randomElement, buildings.GetRandomElement(), "AlongInsideWall"))
						{
							randomElement.Inventory.AddObject(item4);
							list4.Add(randomElement);
							storyPlacements++;
						}
					}
					placeExtraBooks(list4);
				}
				else if (storyType == "Library")
				{
					List<GameObject> list5 = generateHistoryBooks();
					int num3 = (townSquareRect.x1 + townSquareRect.x2) / 2;
					int num4 = (townSquareRect.y1 + townSquareRect.y2) / 2;
					int num5;
					int num6;
					bool flag2;
					if (townSquareDoorStyle == 1)
					{
						num5 = num3;
						num6 = townSquareRect.y2 - 1;
						flag2 = true;
					}
					else if (townSquareDoorStyle == 2)
					{
						num5 = num3;
						num6 = townSquareRect.y1 + 1;
						flag2 = true;
					}
					else if (townSquareDoorStyle == 3)
					{
						num5 = townSquareRect.x1 + 1;
						num6 = num4;
						flag2 = false;
					}
					else if (townSquareDoorStyle == 4)
					{
						num5 = townSquareRect.x2 - 1;
						num6 = num4;
						flag2 = false;
					}
					else
					{
						num5 = num3;
						num6 = num4;
						flag2 = Stat.Random(0, 1) == 0;
					}
					List<GameObject> list6 = new List<GameObject>(list5.Count);
					int num7 = 0;
					foreach (GameObject item5 in list5)
					{
						GameObject randomElement2;
						if (Stat.Random(0, 9) < list6.Count)
						{
							randomElement2 = list6.GetRandomElement();
							randomElement2.Inventory.AddObject(item5);
							continue;
						}
						randomElement2 = GameObject.create("Bookshelf");
						if (flag2)
						{
							num5 += num7;
						}
						else
						{
							num6 += num7;
						}
						if (num7 > 0)
						{
							num7++;
							num7 = -num7;
						}
						else
						{
							num7 = -num7;
							num7++;
						}
						Cell cell = zone.GetCell(num5, num6);
						if (cell.IsEmptyForPopulation() && townSquareRect.PointIn(Location2D.get(num5, num6)))
						{
							cell.AddObject(randomElement2);
							randomElement2.Inventory.AddObject(item5);
							list6.Add(randomElement2);
							storyPlacements++;
						}
						else if (ZoneBuilderSandbox.PlaceObjectInRect(zone, townSquareRect, randomElement2) || PlaceObjectInBuilding(randomElement2, townSquareLayout))
						{
							randomElement2.Inventory.AddObject(item5);
							list6.Add(randomElement2);
							storyPlacements++;
						}
					}
					placeExtraBooks(list6);
				}
				else if (storyType == "Graveyard")
				{
					objectMarkingStyle = "monument";
					List<GameObject> list7 = new List<GameObject>(notesForVillage.Count);
					foreach (JournalVillageNote item6 in notesForVillage)
					{
						story = item6;
						perspective = storyPerspective(story);
						GameObject aVillageMonument3 = getAVillageMonument();
						action(aVillageMonument3);
						list7.Add(aVillageMonument3);
					}
					storyPlacements -= notesForVillage.Count;
					storyPlacements += placeInRectSymmetricallyIfPossible(townSquareRect, list7, Walled: false, townSquareLayout, "Inside");
				}
				else if (storyType == "Tattoos" || storyType == "Tattoo")
				{
					objectMarkingStyle = "tattoo";
					List<GameObject> list8 = zone.GetObjectsWithProperty("ParticipantVillager").ShuffleInPlace();
					int chance = 50;
					foreach (GameObject item7 in list8)
					{
						if (chance.in100() && item7.HasProperty("ParticipantVillager"))
						{
							story = notesForVillage.GetRandomElement();
							perspective = storyPerspective(story);
							action(item7);
						}
					}
				}
				else if (storyType == "Animatronic")
				{
					GameObject gameObject3 = GameObject.create("Village Robot");
					setVillagerProperties(gameObject3);
					AddVillagerConversation(gameObject3, gameObject3.GetTag("SimpleConversation", "I know a lot about the town!"));
					for (int k = 0; k < notesForVillage.Count; k++)
					{
						story = notesForVillage[k];
						perspective = storyPerspective(story);
						ConversationsAPI.addSimpleRootInformationOption(gameObject3, "Tell me about {{" + perspective.mainColor + "|" + story.text.Snippet() + "}}...", story.text.Split('|')[0]);
						storyPlacements++;
					}
					if (!ZoneBuilderSandbox.PlaceObject(zone, townSquare, gameObject3))
					{
						storyPlacements -= notesForVillage.Count;
						Debug.LogWarning("failed to place " + gameObject3.DisplayName);
					}
				}
				else if (storyType == "Holograms")
				{
					objectMarkingStyle = "light-pattern";
					List<GameObject> list9 = new List<GameObject>(notesForVillage.Count);
					foreach (JournalVillageNote item8 in notesForVillage)
					{
						story = item8;
						perspective = storyPerspective(story);
						GameObject gameObject4 = (If.CoinFlip() ? GameObject.create("Village Hologram 1") : GameObject.create("Village Hologram 2"));
						action(gameObject4);
						list9.Add(gameObject4);
					}
					storyPlacements -= notesForVillage.Count;
					storyPlacements += placeInRectSymmetricallyIfPossible(townSquareRect, list9, Walled: true, townSquareLayout, "Inside");
				}
				else if (storyType == "Vessels" || storyType == "Furniture")
				{
					if (objectMarkingStyle == null)
					{
						objectMarkingStyle = ((Stat.Random(0, 2) == 0) ? "engraving" : "painting");
					}
					Dictionary<int, bool> dictionary = new Dictionary<int, bool>(notesForVillage.Count);
					for (int l = 0; l < notesForVillage.Count; l++)
					{
						dictionary.Add(l, value: false);
					}
					int num8 = 0;
					foreach (PopulationLayout building in buildings)
					{
						if (Stat.Random(1, 100) > num)
						{
							continue;
						}
						if (storyType == "Vessels")
						{
							foreach (Location2D cell2 in building.region.Cells)
							{
								int num9 = storyPlacements;
								story = notesForVillage.GetRandomElement();
								perspective = storyPerspective(story);
								action(zone.GetCell(cell2).GetFirstObjectWithPropertyOrTag("Vessel"));
								if (num9 > storyPlacements)
								{
									int key = notesForVillage.IndexOf(story);
									if (!dictionary[key])
									{
										dictionary[key] = true;
										num8++;
									}
								}
							}
							continue;
						}
						if (storyType == "Furniture")
						{
							foreach (Location2D cell3 in building.region.Cells)
							{
								int num10 = storyPlacements;
								story = notesForVillage.GetRandomElement();
								perspective = storyPerspective(story);
								action(zone.GetCell(cell3).GetFirstObjectWithPropertyOrTagAndNotPropertyOrTag("Furniture", "Door"));
								if (num10 > storyPlacements)
								{
									int key2 = notesForVillage.IndexOf(story);
									if (!dictionary[key2])
									{
										dictionary[key2] = true;
										num8++;
									}
								}
							}
							continue;
						}
						throw new Exception("internally inconsistent story type " + storyType);
					}
					if (num8 < notesForVillage.Count)
					{
						flag = true;
					}
				}
				else
				{
					if (!(storyType == "Doors") && !(storyType == "Walls"))
					{
						throw new Exception("unsupported story type " + storyType);
					}
					if (objectMarkingStyle == null)
					{
						objectMarkingStyle = ((Stat.Random(0, 2) == 0) ? "engraving" : "painting");
					}
					int num11 = Stat.Random(0, notesForVillage.Count - 1);
					int num12 = 0;
					foreach (PopulationLayout building2 in buildings)
					{
						if (Stat.Random(1, 100) > num)
						{
							continue;
						}
						int num13 = storyPlacements;
						story = notesForVillage[num11];
						perspective = storyPerspective(story);
						if (storyType == "Doors")
						{
							foreach (Location2D cell4 in building2.originalRegion.Cells)
							{
								action(zone.GetCell(cell4).GetFirstObjectWithTag("Door"));
							}
						}
						else
						{
							if (!(storyType == "Walls"))
							{
								throw new Exception("internally inconsistent story type " + storyType);
							}
							foreach (Location2D cell5 in building2.originalRegion.Cells)
							{
								action(zone.GetCell(cell5).GetObjectWithTagOrProperty("Wall"));
							}
						}
						if (num13 > storyPlacements)
						{
							num11++;
							num12++;
							if (num11 >= notesForVillage.Count)
							{
								num11 = 0;
							}
						}
					}
					if (num12 < notesForVillage.Count)
					{
						flag = true;
					}
				}
				if ((storyPlacements == 0 || flag) && ++num2 < 5)
				{
					num += 5;
					continue;
				}
				break;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("placeStories", x);
		}
	}

	public List<GameObject> generateHistoryBooks()
	{
		List<JournalVillageNote> notesForVillage = JournalAPI.GetNotesForVillage(villageEntity.id);
		List<GameObject> list = new List<GameObject>(notesForVillage.Count);
		int num = 0;
		foreach (JournalVillageNote item in notesForVillage)
		{
			num++;
			GameObject aVillageBook = getAVillageBook();
			string text = "History of " + villageName + " Vol. " + Grammar.GetRomanNumeral(num);
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			HistoricPerspective historicPerspective = villageSnapshot.requirePerspective(villageEntity.history.GetEvent(item.getEventId()));
			stringBuilder.Append("{{w|This book recounts a tale from the history of the village " + villageName + ".}}\n\n");
			stringBuilder.Append(item.text.Split('|')[0]);
			stringBuilder2.Append(item.secretid);
			aVillageBook.AddPart(new VillageHistoryBook(text, stringBuilder.ToString(), stringBuilder2.ToString()));
			aVillageBook.pRender.DisplayName = text.ToString();
			aVillageBook.pRender.ColorString = "&" + historicPerspective.mainColor;
			aVillageBook.pRender.DetailColor = historicPerspective.supportColor;
			list.Add(aVillageBook);
		}
		return list;
	}

	public GameObject generatePreacherBook()
	{
		List<JournalVillageNote> notesForVillage = JournalAPI.GetNotesForVillage(villageEntity.id);
		GameObject gameObject = GameObject.create("Book");
		string text = "History of " + villageName;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.Append("{{w|This book recounts a tale from the history of the village " + villageName + ".}}\n\n");
		foreach (JournalVillageNote item in notesForVillage)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(item.text);
			if (stringBuilder2.Length > 0)
			{
				stringBuilder2.Append(",");
			}
			stringBuilder2.Append(item.secretid);
		}
		gameObject.AddPart(new VillageHistoryBook(text, stringBuilder.ToString(), stringBuilder2.ToString()));
		gameObject.pRender.DisplayName = text.ToString();
		return gameObject;
	}

	public void generateSignatureDish(string Style = "Focal")
	{
		_ = isVillageZero;
		int num = 0;
		int num2 = 25;
		int num3 = 50;
		if (villageTier >= 0 && villageTier <= 1)
		{
			num2 = 25;
			num3 = 85;
		}
		if (villageTier >= 2 && villageTier <= 3)
		{
			num2 = 15;
			num3 = 65;
		}
		if (villageTier >= 4 && villageTier <= 5)
		{
			num2 = 5;
			num3 = 52;
		}
		if (villageTier >= 6 && villageTier <= 7)
		{
			num2 = 0;
			num3 = 35;
		}
		if (villageTier >= 8)
		{
			num2 = 0;
			num3 = 25;
		}
		int num4 = Stat.Random(1, 100);
		num = ((num4 <= num2) ? 1 : ((num4 > num3) ? 3 : 2));
		List<GameObject> list = new List<GameObject>();
		int num5 = 0;
		if (villageSnapshot.listProperties.ContainsKey("signatureDishIngredients"))
		{
			List<string> list2 = villageSnapshot.GetList("signatureDishIngredients");
			num = Math.Max(num, list2.Count);
			foreach (string item in list2)
			{
				list.Add(GameObject.create(item));
			}
		}
		for (int i = list.Count; i < num; i++)
		{
			string ingredientBlueprint = null;
			if (Stat.Random(0, 100) <= 80)
			{
				do
				{
					ingredientBlueprint = PopulationManager.RollOneFrom("DynamicObjectsTable:" + region + "_Ingredients").Blueprint;
					num5++;
				}
				while (list.Any((GameObject o) => o.Blueprint == ingredientBlueprint) && num5 < 25);
				num5 = 0;
				if (ingredientBlueprint != null)
				{
				}
			}
			else
			{
				ingredientBlueprint = CookingRecipe.RollOvenSafeIngredient("Ingredients" + villageTier);
			}
			if (ingredientBlueprint == null)
			{
				ingredientBlueprint = CookingRecipe.RollOvenSafeIngredient("Ingredients" + villageTier);
			}
			if (ingredientBlueprint != null)
			{
				list.Add(GameObjectFactory.Factory.CreateObject(ingredientBlueprint));
			}
		}
		string dishNames = ((!GameObjectFactory.Factory.Blueprints.ContainsKey(villagerBaseFaction + "_Data")) ? "generic" : GameObjectFactory.Factory.Blueprints[villagerBaseFaction + "_Data"].GetTag("DishNames", "generic"));
		signatureDish = CookingRecipe.FromIngredients(list, null, null, dishNames);
		if (villageSnapshot.GetProperty("signatureDishName") != "unknown")
		{
			signatureDish.DisplayName = villageSnapshot.GetProperty("signatureDishName");
		}
	}

	public void generateSignatureLiquid()
	{
		if (villageSnapshot.hasProperty("signatureLiquid"))
		{
			signatureLiquid = villageSnapshot.GetProperty("signatureLiquid");
		}
		else if (villageSnapshot.listProperties.ContainsKey("signatureLiquids"))
		{
			signatureLiquid = villageSnapshot.GetList("signatureLiquids").GetRandomElement();
		}
		else
		{
			signatureLiquid = PopulationManager.RollOneFrom("Villages_LiquidType_" + ImportedFoodorDrink.getLiquidTierGroup(villageSnapshot.Tier) + "_*Default").Blueprint + "-1000";
		}
	}

	public void generateSignatureSkill()
	{
		if (villageSnapshot.hasProperty("signatureSkill"))
		{
			signatureSkill = villageSnapshot.GetProperty("signatureSkill");
		}
		if (isVillageZero)
		{
			if (region == "Saltmarsh")
			{
				signatureSkill = "CookingAndGathering_Harvestry";
			}
			if (region == "DesertCanyon")
			{
				signatureSkill = "Survival";
			}
			if (region == "Saltdunes")
			{
				signatureSkill = "Discipline_FastingWay";
			}
			if (region == "Hills")
			{
				signatureSkill = "CookingAndGathering_Butchery";
			}
		}
	}

	public void makeSureThereIsEnoughSpace()
	{
		int num = 3;
		while (num > 0 && zone.GetCells().Count((Cell c) => c.HasWall()) > 1000)
		{
			NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					if (noiseMap.Noise[i, j] > 1)
					{
						zone.GetCell(i, j).ClearObjectsWithTag("Wall");
					}
				}
			}
			num--;
		}
		if (num < 3)
		{
			Clean(zone);
			CarvePathwaysFromLocations(zone, bCarveDoors: false, new List<Location2D>(), Location2D.get(40, 14));
		}
	}

	public void generateStoryType()
	{
		storyType = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StoryEmbodiment")).Blueprint;
		villageEntity.SetPropertyAtCurrentYear("storyType", storyType);
	}

	public void addInitialStructures()
	{
		List<ISultanDungeonSegment> list = new List<ISultanDungeonSegment>();
		int num = 7;
		int num2 = 72;
		int num3 = 7;
		int num4 = 17;
		string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_InitialStructureSegmentation"), null, "Full").Blueprint;
		if (blueprint == "None")
		{
			return;
		}
		string[] array = blueprint.Split(';');
		foreach (string text in array)
		{
			switch (text)
			{
			case "FullHMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment3 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment3.mutator = "HMirror";
				list.Add(sultanRectDungeonSegment3);
				continue;
			}
			case "FullVMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment2 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment2.mutator = "VMirror";
				list.Add(sultanRectDungeonSegment2);
				continue;
			}
			case "FullHVMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment.mutator = "HVMirror";
				list.Add(sultanRectDungeonSegment);
				continue;
			}
			case "Full":
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
				continue;
			}
			if (text.StartsWith("BSP:"))
			{
				int nSegments = Convert.ToInt32(text.Split(':')[1]);
				partition(new Rect2D(2, 2, 78, 24), ref nSegments, list);
			}
			else if (text.StartsWith("Ring:"))
			{
				int num5 = Convert.ToInt32(text.Split(':')[1]);
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
				if (num5 == 2)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(20, 8, 60, 16)));
				}
				if (num5 == 3)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(15, 8, 65, 16)));
					list.Add(new SultanRectDungeonSegment(new Rect2D(25, 10, 55, 14)));
				}
			}
			else if (text.StartsWith("Blocks"))
			{
				string[] array2 = text.Split(':')[1].Split(',');
				int num6 = array2[0].RollCached();
				for (int j = 0; j < num6; j++)
				{
					int num7 = array2[1].RollCached();
					int num8 = array2[2].RollCached();
					int num9 = Stat.Random(2, 78 - num7);
					int num10 = Stat.Random(2, 23 - num8);
					int num11 = num9 + num7;
					int num12 = num10 + num8;
					if (num < num9)
					{
						num = num9;
					}
					if (num2 > num11)
					{
						num2 = num11;
					}
					if (num3 < num10)
					{
						num3 = num10;
					}
					if (num4 > num12)
					{
						num4 = num12;
					}
					SultanRectDungeonSegment sultanRectDungeonSegment4 = new SultanRectDungeonSegment(new Rect2D(num9, num10, num9 + num7, num10 + num8));
					if (text.Contains("[HMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "HMirror";
					}
					if (text.Contains("[VMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "VMirror";
					}
					if (text.Contains("[HVMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "HVMirror";
					}
					list.Add(sultanRectDungeonSegment4);
				}
			}
			else if (text.StartsWith("Circle"))
			{
				string[] array3 = text.Split(':')[1].Split(',');
				list.Add(new SultanCircleDungeonSegment(Location2D.get(array3[0].RollCached(), array3[1].RollCached()), array3[2].RollCached()));
			}
			else if (text.StartsWith("Tower"))
			{
				string[] array4 = text.Split(':')[1].Split(',');
				list.Add(new SultanTowerDungeonSegment(Location2D.get(array4[0].RollCached(), array4[1].RollCached()), array4[2].RollCached(), array4[3].RollCached()));
			}
		}
		ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
		for (int k = 0; k < list.Count; k++)
		{
			string text2 = "";
			text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
			int n = 3;
			string text3 = "";
			string text4 = "";
			text4 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
			int n2 = 3;
			if (text2.Contains(","))
			{
				string[] array5 = text2.Split(',');
				text2 = array5[0];
				text3 = array5[1];
			}
			WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(text2, n, list[k].width(), list[k].height(), periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			if (!string.IsNullOrEmpty(text3))
			{
				waveCollapseFastModel.ClearColors(text3);
			}
			waveCollapseFastModel.UpdateSample(text4.Split(',')[0], n2, periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			ColorOutputMap colorOutputMap2 = new ColorOutputMap(waveCollapseFastModel);
			colorOutputMap2.ReplaceBorders(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), new Color32(0, 0, 0, byte.MaxValue));
			if (list[k].mutator == "HMirror")
			{
				colorOutputMap2.HMirror();
			}
			if (list[k].mutator == "VMirror")
			{
				colorOutputMap2.VMirror();
			}
			if (list[k].mutator == "HVMirror")
			{
				colorOutputMap2.HMirror();
				colorOutputMap2.VMirror();
			}
			colorOutputMap.Paste(colorOutputMap2, list[k].x1, list[k].y1);
			waveCollapseFastModel = null;
			MemoryHelper.GCCollect();
		}
		string text5 = RollOneFrom("Village_InitialStructureSegmentationFullscreenMutation");
		int num13 = 0;
		int num14 = 0;
		for (int l = 0; l < list.Count; l++)
		{
			string text6 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureWall")).Blueprint;
			if (text6 == "*auto")
			{
				text6 = GetDefaultWall(zone);
			}
			for (int m = list[l].x1; m < list[l].x2; m++)
			{
				for (int num15 = list[l].y1; num15 < list[l].y2; num15++)
				{
					if (!list[l].contains(m, num15))
					{
						continue;
					}
					int num16 = l + 1;
					while (true)
					{
						if (num16 < list.Count)
						{
							if (list[num16].contains(m, num15))
							{
								break;
							}
							num16++;
							continue;
						}
						Color32 a = colorOutputMap.getPixel(m, num15);
						if (list[l].HasCustomColor(m, num15))
						{
							a = list[l].GetCustomColor(m, num15);
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.BLACK))
						{
							zone.GetCell(m + num13, num15 + num14).ClearObjectsWithTag("Wall");
							zone.GetCell(m + num13, num15 + num14).AddObject(text6);
							if (text5 == "VMirror" || text5 == "HVMirror")
							{
								zone.GetCell(m + num13, zone.Height - (num15 + num14) - 1).ClearObjectsWithTag("Wall");
								zone.GetCell(m + num13, zone.Height - (num15 + num14) - 1).AddObject(text6);
							}
							if (text5 == "HMirror" || text5 == "HVMirror")
							{
								zone.GetCell(zone.Width - (m + num13) - 1, num15 + num14).ClearObjectsWithTag("Wall");
								zone.GetCell(zone.Width - (m + num13) - 1, num15 + num14).AddObject(text6);
							}
							if (text5 == "HVMirror")
							{
								zone.GetCell(zone.Width - (m + num13) - 1, zone.Height - (num15 + num14) - 1).ClearObjectsWithTag("Wall");
								zone.GetCell(zone.Width - (m + num13) - 1, zone.Height - (num15 + num14) - 1).AddObject(text6);
							}
						}
						break;
					}
				}
			}
		}
	}

	public GameObject getAVillageDoor()
	{
		if (villageDoorPrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_DoorType")).Blueprint;
			if (blueprint == "Door")
			{
				villageDoorPrototype = GameObjectFactory.Factory.CreateObject(villageDoorStyle);
			}
			if (blueprint == "Archway")
			{
				villageDoorPrototype = GameObjectFactory.Factory.CreateObject("Archway");
			}
		}
		return villageDoorPrototype.DeepCopy();
	}

	public void getVillageDoorStyle()
	{
		if (villageTechTier < 3)
		{
			villageDoorStyle = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_DoorStyle_Lowtier")).Blueprint;
		}
		else if (villageTechTier < 6)
		{
			villageDoorStyle = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_DoorStyle_Midtier")).Blueprint;
		}
		else
		{
			villageDoorStyle = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_DoorStyle_Hightier")).Blueprint;
		}
	}

	public GameObject getAVillageCanvas()
	{
		if (villageCanvasPrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_ConstructionAdjectives")).Blueprint;
			GameObject aRegionalCreature = getARegionalCreature();
			GameObject aRegionalCreature2 = getARegionalCreature();
			villageCanvasPrototype = GameObject.create("CanvasWall");
			villageCanvasPrototype.DisplayName = aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getSingularSemantic("Canvas", "canvas");
			if (aRegionalCreature.Blueprint == aRegionalCreature2.Blueprint)
			{
				villageCanvasPrototype.pRender.ColorString = aRegionalCreature.pRender.ColorString;
				villageCanvasPrototype.pRender.TileColor = "&" + aRegionalCreature.pRender.GetForegroundColor();
				villageCanvasPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getPluralSemantic("Canvas", "canvas") + " bound with " + blueprint + " rope.";
			}
			else
			{
				villageCanvasPrototype.pRender.ColorString = "&" + aRegionalCreature.pRender.GetForegroundColor() + "^" + aRegionalCreature2.pRender.GetForegroundColor();
				villageCanvasPrototype.pRender.TileColor = "&" + aRegionalCreature.pRender.GetForegroundColor();
				villageCanvasPrototype.pRender.DetailColor = aRegionalCreature2.pRender.GetForegroundColor();
				villageCanvasPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getPluralSemantic("Canvas", "canvas") + " bound with " + blueprint + " " + aRegionalCreature2.ShortDisplayName + " " + aRegionalCreature2.getSingularSemantic("Fiber", "sinew") + ".";
			}
			villageCanvasPrototype.GetPart<Description>().Short = Grammar.InitCapWithFormatting(ConsoleLib.Console.ColorUtility.StripFormatting(HistoricStringExpander.ExpandString(villageCanvasPrototype.GetPart<Description>().Short)));
		}
		return villageCanvasPrototype.DeepCopy();
	}

	public GameObject getAVillageWall()
	{
		if (villageWallPrototype == null)
		{
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_ConstructionAdjectives")).Blueprint;
			switch (PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_VillageWallStyle")).Blueprint)
			{
			case "canvas":
				villageWallPrototype = getAVillageCanvas();
				break;
			case "planks":
				if (Stat.Random(1, 2) == 1)
				{
					GameObject aRegionalPlant2 = getARegionalPlant();
					GameObject aRegionalPlant3 = getARegionalPlant();
					villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallWood");
					villageWallPrototype.DisplayName = aRegionalPlant2.ShortDisplayName + " " + aRegionalPlant2.getSingularSemantic("Plank", "plank") + " wall";
					if (aRegionalPlant2.Blueprint == aRegionalPlant3.Blueprint)
					{
						villageWallPrototype.pRender.ColorString = aRegionalPlant2.pRender.ColorString;
						villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalPlant2.getPluralSemantic("Plank", "plank") + " of " + aRegionalPlant2.ShortDisplayName + " bound with " + blueprint + " rope.";
					}
					else
					{
						villageWallPrototype.pRender.ColorString = "&" + aRegionalPlant2.pRender.GetForegroundColor() + "^" + aRegionalPlant3.pRender.GetForegroundColor();
						villageWallPrototype.pRender.TileColor = "&" + aRegionalPlant2.pRender.GetForegroundColor();
						villageWallPrototype.pRender.DetailColor = aRegionalPlant3.pRender.GetForegroundColor();
						villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalPlant2.getPluralSemantic("Plank", "plank") + " of " + aRegionalPlant2.ShortDisplayName + " bound with " + blueprint + " " + aRegionalPlant3.getPluralSemantic("Fiber", "strip") + " of " + aRegionalPlant3.ShortDisplayName + " " + aRegionalPlant3.getSingularSemantic("FiberMaterial", "bark") + ".";
					}
				}
				else
				{
					GameObject aRegionalPlant4 = getARegionalPlant();
					GameObject aRegionalCreature4 = getARegionalCreature();
					villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallWood");
					villageWallPrototype.DisplayName = aRegionalPlant4.ShortDisplayName + " " + aRegionalPlant4.getSingularSemantic("Plank", "plank") + " wall";
					villageWallPrototype.pRender.ColorString = "&" + aRegionalPlant4.pRender.GetForegroundColor() + "^" + aRegionalCreature4.pRender.GetForegroundColor();
					villageWallPrototype.pRender.TileColor = "&" + aRegionalPlant4.pRender.GetForegroundColor();
					villageWallPrototype.pRender.DetailColor = aRegionalCreature4.pRender.GetForegroundColor();
					villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalPlant4.getPluralSemantic("Plank", "plank") + " of " + aRegionalPlant4.ShortDisplayName + " bound with " + blueprint + " " + aRegionalCreature4.ShortDisplayName + " " + aRegionalCreature4.getSingularSemantic("Fiber", "sinew") + ".";
				}
				break;
			case "bone":
				if (Stat.Random(1, 2) == 1)
				{
					GameObject aRegionalCreature = getARegionalCreature();
					GameObject aRegionalPlant = getARegionalPlant();
					villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallWood");
					villageWallPrototype.DisplayName = aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getSingularSemantic("HardMaterial", "plank") + " wall";
					if (aRegionalCreature.Blueprint == aRegionalPlant.Blueprint)
					{
						villageWallPrototype.pRender.ColorString = aRegionalCreature.pRender.ColorString;
						villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getPluralSemantic("HardMaterial", "plank") + " bound with " + blueprint + " rope.";
					}
					else
					{
						villageWallPrototype.pRender.ColorString = "&" + aRegionalCreature.pRender.GetForegroundColor() + "^" + aRegionalPlant.pRender.GetForegroundColor();
						villageWallPrototype.pRender.TileColor = "&" + aRegionalCreature.pRender.GetForegroundColor();
						villageWallPrototype.pRender.DetailColor = aRegionalPlant.pRender.GetForegroundColor();
						villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalCreature.ShortDisplayName + " " + aRegionalCreature.getPluralSemantic("HardMaterial", "plank") + " bound with " + blueprint + " " + aRegionalPlant.getPluralSemantic("Fiber", "strip") + " of " + aRegionalPlant.ShortDisplayName + " " + aRegionalPlant.getSingularSemantic("FiberMaterial", "bark") + ".";
					}
				}
				else
				{
					GameObject aRegionalCreature2 = getARegionalCreature();
					GameObject aRegionalCreature3 = getARegionalCreature();
					villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallFluted");
					villageWallPrototype.DisplayName = aRegionalCreature2.ShortDisplayName + " " + aRegionalCreature2.getSingularSemantic("HardMaterial", "plank") + " wall";
					villageWallPrototype.pRender.ColorString = "&" + aRegionalCreature2.pRender.GetForegroundColor() + "^" + aRegionalCreature3.pRender.GetForegroundColor();
					villageWallPrototype.pRender.TileColor = "&" + aRegionalCreature2.pRender.GetForegroundColor();
					villageWallPrototype.pRender.DetailColor = aRegionalCreature3.pRender.GetForegroundColor();
					villageWallPrototype.GetPart<Description>().Short = blueprint + " " + aRegionalCreature2.ShortDisplayName + " " + aRegionalCreature2.getPluralSemantic("HardMaterial", "plank") + " bound with " + blueprint + " " + aRegionalCreature3.ShortDisplayName + " " + aRegionalCreature3.getSingularSemantic("Fiber", "sinew") + ".";
				}
				break;
			case "stone":
				villageWallPrototype = GameObject.create("BaseWallStone");
				villageWallPrototype.pRender.DisplayName = "stone wall";
				villageWallPrototype.pRender.ColorString = "&y^w";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "w";
				villageWallPrototype.GetPart<Description>().Short = "A wall of solid stone.";
				break;
			case "brick":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallBrick");
				villageWallPrototype.pRender.DisplayName = "brick wall";
				villageWallPrototype.pRender.ColorString = "&r^w";
				villageWallPrototype.pRender.TileColor = "&r";
				villageWallPrototype.pRender.DetailColor = "w";
				villageWallPrototype.GetPart<Description>().Short = "A wall of fired brick.";
				break;
			case "cyber":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallCyber");
				villageWallPrototype.pRender.DisplayName = "etched metal wall";
				villageWallPrototype.pRender.ColorString = "&B^y";
				villageWallPrototype.pRender.TileColor = "&B";
				villageWallPrototype.pRender.DetailColor = "y";
				villageWallPrototype.GetPart<Description>().Short = "A wall of etched metal.";
				break;
			case "fluted":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallFluted");
				villageWallPrototype.pRender.DisplayName = "fluted stone wall";
				villageWallPrototype.pRender.ColorString = "&y^K";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "K";
				villageWallPrototype.GetPart<Description>().Short = "A wall of fluted stone.";
				break;
			case "granite":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallGranite");
				villageWallPrototype.pRender.DisplayName = "granite wall";
				villageWallPrototype.pRender.ColorString = "&y^r";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "r";
				villageWallPrototype.GetPart<Description>().Short = "A wall of solid granite.";
				break;
			case "marble":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallMarble");
				villageWallPrototype.pRender.DisplayName = "marble wall";
				villageWallPrototype.pRender.ColorString = "&y^b";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "b";
				villageWallPrototype.GetPart<Description>().Short = "A wall of rough marble.";
				break;
			case "metal":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallMetal");
				villageWallPrototype.pRender.DisplayName = "metal wall";
				villageWallPrototype.pRender.ColorString = "&c";
				villageWallPrototype.GetPart<Description>().Short = "A wall of sheet metal.";
				break;
			case "dirt":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallMud");
				villageWallPrototype.pRender.DisplayName = "dirt wall";
				villageWallPrototype.pRender.ColorString = "&w";
				villageWallPrototype.GetPart<Description>().Short = "A wall of packed dirt.";
				break;
			case "rock":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallRock");
				villageWallPrototype.pRender.DisplayName = "rock wall";
				villageWallPrototype.pRender.ColorString = "&y^K";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "K";
				villageWallPrototype.GetPart<Description>().Short = "A wall of rock.";
				break;
			case "secure":
				villageWallPrototype = GameObjectFactory.Factory.CreateObject("BaseWallSecure");
				villageWallPrototype.pRender.DisplayName = "stone wall";
				villageWallPrototype.pRender.ColorString = "&y^W";
				villageWallPrototype.pRender.TileColor = "&y";
				villageWallPrototype.pRender.DetailColor = "W";
				villageWallPrototype.GetPart<Description>().Short = "A wall of polished stone.";
				break;
			}
			villageWallPrototype.GetPart<Description>().Short = Grammar.InitCapWithFormatting(ConsoleLib.Console.ColorUtility.StripFormatting(HistoricStringExpander.ExpandString(villageWallPrototype.GetPart<Description>().Short)));
		}
		return villageWallPrototype.DeepCopy();
	}
}

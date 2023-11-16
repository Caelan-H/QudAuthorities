using System;
using HistoryKit;
using XRL.Names;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Annals;

[Serializable]
public class InitializeVillage : HistoricEvent
{
	public string region;

	public bool bVillageZero;

	public InitializeVillage(string _region, bool _bVillageZero = false)
	{
		region = _region;
		bVillageZero = _bVillageZero;
	}

	public override void Generate()
	{
		setEntityProperty("type", "village");
		string value;
		do
		{
			value = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site");
		}
		while (history.GetEntitiesWherePropertyEquals("name", value).Count > 0);
		setEntityProperty("name", value);
		setEntityProperty("region", region);
		addListProperty("palette", Crayons.GetRandomDistinctColorsAll(3));
		string primaryFaction;
		if (bVillageZero)
		{
			setEntityProperty("tier", "0");
			setEntityProperty("techTier", "0");
			primaryFaction = GameObject.create(PopulationManager.RollOneFrom("VillageOneBaseFaction_" + region).Blueprint).GetPrimaryFaction();
			setEntityProperty("villageZero", "true");
		}
		else
		{
			GameObjectBlueprint gameObjectBlueprint = (GameObjectFactory.Factory.Blueprints.ContainsKey("Terrain" + region) ? GameObjectFactory.Factory.Blueprints["Terrain" + region] : null);
			if (gameObjectBlueprint != null)
			{
				setEntityProperty("tier", gameObjectBlueprint.GetTag("RegionTier", "1"));
			}
			else
			{
				setEntityProperty("tier", "1");
			}
			gameObjectBlueprint = (GameObjectFactory.Factory.Blueprints.ContainsKey("Terrain" + region) ? GameObjectFactory.Factory.Blueprints["Terrain" + region] : null);
			if (gameObjectBlueprint != null)
			{
				setEntityProperty("techTier", Random(1, int.Parse(gameObjectBlueprint.GetTag("RegionTier", "1")) + 1).ToString());
			}
			else
			{
				setEntityProperty("techTier", "1");
			}
			primaryFaction = GameObject.create(PopulationManager.RollOneFrom("LairOwners_" + region).Blueprint).pBrain.GetPrimaryFaction();
		}
		if (GameObjectFactory.Factory.GetFactionMembers(primaryFaction).GetRandomElement().HasTag("Humanoid"))
		{
			setEntityProperty("villagerPopulation", "humanoid");
		}
		else
		{
			setEntityProperty("villagerPopulation", "nonHumanoid");
		}
		setEntityProperty("baseFaction", primaryFaction);
		string text = ExpandString("<spice.villages.reasonForFounding.!random>");
		setEntityProperty("reasonForFounding", text);
		setEntityProperty("defaultSacredThing", ExpandString("<spice.villages.reasonForFounding." + text + ".sacredThing.!random>", QudHistoryHelpers.BuildContextFromObjectTextFragments(GameObjectFactory.Factory.GetFactionMembers(primaryFaction).GetRandomElement().Name)));
		setEntityProperty("defaultProfaneThing", ExpandString("<spice.villages.reasonForFounding." + text + ".profaneThing.!random>", QudHistoryHelpers.BuildContextFromObjectTextFragments(GameObjectFactory.Factory.GetFactionMembers(primaryFaction).GetRandomElement().Name)));
		setEntityProperty("governor", "the mayor");
		duration = 0L;
	}
}

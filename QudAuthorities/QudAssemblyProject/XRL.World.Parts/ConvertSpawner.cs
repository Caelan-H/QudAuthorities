using System;
using Qud.API;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ConvertSpawner : IPart
{
	public string Faction = "Mechanimists";

	public bool DoesWander = true;

	public bool IsPilgrim;

	public const int CHANCE_FOR_NONGOATFOLK_JUNGLE_CONVERT = 40;

	public const int CHANCE_FOR_ALL_JUNGLE_CONVERT = 10;

	public const int CHANCE_FOR_HUMANOID_CONVERT = 15;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		GameObject gameObject = ((Faction == "Mechanimists") ? CreateMechanimistConvert(ParentObject, DoesWander, IsPilgrim) : ((Faction == "Kyakukya") ? CreateKyakukyanConvert(ParentObject, DoesWander, IsPilgrim) : ((!(Faction == "YdFreehold")) ? CreateMechanimistConvert(ParentObject, DoesWander, IsPilgrim) : CreateYdFreeholdConvert(ParentObject, DoesWander, IsPilgrim))));
		gameObject.FireEvent("VillageInit");
		gameObject.SetIntProperty("Social", 1);
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}

	public static GameObject CreateMechanimistConvert(GameObject ParentObject, bool DoesWander, bool IsPilgrim)
	{
		_ = ParentObject.CurrentCell;
		GameObject baseObject;
		do
		{
			baseObject = GetBaseObject(ParentObject);
		}
		while (baseObject.pBrain == null || baseObject.pBrain.FactionMembership.ContainsKey("Mechanimists"));
		baseObject.pBrain.FactionMembership.Clear();
		baseObject.pBrain.FactionMembership.Add("Mechanimists", 100);
		baseObject.pBrain.Hostile = false;
		if (DoesWander)
		{
			baseObject.pBrain.Wanders = true;
			baseObject.pBrain.WandersRandomly = true;
			baseObject.AddPart(new AIShopper());
		}
		else
		{
			baseObject.pBrain.Wanders = false;
			baseObject.pBrain.WandersRandomly = false;
			baseObject.AddPart(new Sitting());
		}
		baseObject.TakeObject("Canticles3", Silent: false, 0);
		ConversationScript part = baseObject.GetPart<ConversationScript>();
		if (IsPilgrim)
		{
			baseObject.RequirePart<AIPilgrim>();
			if (part != null)
			{
				part.Append = "\n\nGlory to Shekhinah.~\n\nHumble before my Fathers, I walk.~\n\nShow mercy to a weary pilgrim.~\n\nPraise be upon Nisroch, who shelters us stiltseekers.";
			}
		}
		else
		{
			baseObject.RemovePart("AIPilgrim");
			if (part != null)
			{
				part.Append = "\n\nGlory to Shekhinah.~\n\nMay the ground shake but the Six Day Stilt never tumble!~\n\nPraise our argent Fathers! Wisest of all beings.";
			}
		}
		if (ParentObject.HasTag("IsLibrarian"))
		{
			baseObject.AddPart(new MechanimistLibrarian());
			baseObject.SetStringProperty("Mayor", "Mechanimists");
		}
		else
		{
			baseObject.RequirePart<SocialRoles>().RequireRole("Mechanimist convert");
		}
		return baseObject;
	}

	public static GameObject CreateKyakukyanConvert(GameObject ParentObject, bool DoesWander, bool IsPilgrim)
	{
		_ = ParentObject.CurrentCell;
		GameObject baseObject_Kyakukya;
		do
		{
			baseObject_Kyakukya = GetBaseObject_Kyakukya();
		}
		while (baseObject_Kyakukya.pBrain == null || baseObject_Kyakukya.pBrain.FactionMembership.ContainsKey("Kyakukya"));
		baseObject_Kyakukya.pBrain.FactionMembership.Clear();
		baseObject_Kyakukya.pBrain.FactionMembership.Add("Kyakukya", 100);
		baseObject_Kyakukya.pBrain.Hostile = false;
		if (DoesWander)
		{
			baseObject_Kyakukya.pBrain.Wanders = true;
			baseObject_Kyakukya.pBrain.WandersRandomly = true;
		}
		else
		{
			baseObject_Kyakukya.pBrain.Wanders = false;
			baseObject_Kyakukya.pBrain.WandersRandomly = false;
			baseObject_Kyakukya.AddPart(new Sitting());
		}
		baseObject_Kyakukya.RemovePart("AIPilgrim");
		baseObject_Kyakukya.TakeObject("Grave Goods", Silent: false, 0);
		baseObject_Kyakukya.TakeObject("Plump Mushroom", Stat.Random(2, 5), Silent: false, 0);
		ConversationScript part = baseObject_Kyakukya.GetPart<ConversationScript>();
		if (part != null)
		{
			part.Append = "\n\nSix fingers to the earthen lips.~\n\nPlease you to seek him.~\n\nCome gather! -and weave for the passing.~\n\nOur roots loosen jewels from the soil.~\n\nBe ape-still and muse.~\n\nWhat will you strum and dust for Saad?";
		}
		baseObject_Kyakukya.RequirePart<SocialRoles>().RequireRole("worshipper of Oboroqoru");
		return baseObject_Kyakukya;
	}

	public static GameObject CreateYdFreeholdConvert(GameObject ParentObject, bool DoesWander, bool IsPilgrim)
	{
		_ = ParentObject.CurrentCell;
		GameObject baseObject;
		do
		{
			baseObject = GetBaseObject(ParentObject);
		}
		while (baseObject.pBrain == null || baseObject.pBrain.FactionMembership.ContainsKey("YdFreehold"));
		baseObject.pBrain.FactionMembership.Clear();
		baseObject.pBrain.FactionMembership.Add("YdFreehold", 100);
		baseObject.pBrain.Hostile = false;
		baseObject.Reefer = true;
		if (DoesWander)
		{
			baseObject.pBrain.Wanders = true;
			baseObject.pBrain.WandersRandomly = true;
			baseObject.SetIntProperty("WanderUpStairs", 1);
			baseObject.SetIntProperty("WanderDownStairs", 1);
		}
		else
		{
			baseObject.pBrain.Wanders = false;
			baseObject.pBrain.WandersRandomly = false;
			baseObject.AddPart(new Sitting());
		}
		baseObject.RemovePart("AIPilgrim");
		baseObject.RequirePart<SocialRoles>().RequireRole("denizen of the Yd Freehold");
		return baseObject;
	}

	private static GameObject GetBaseObject(GameObject ParentObject)
	{
		if (!ParentObject.HasTag("IsLibrarian"))
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations"));
		}
		return EncountersAPI.GetALegendaryEligibleCreatureWithAnInventory((GameObjectBlueprint o) => !o.HasTag("NoLibrarian") && !o.HasTag("ExcludeFromVillagePopulations"));
	}

	private static GameObject GetBaseObject_Kyakukya()
	{
		int num = Stat.Roll(1, 100);
		if (num <= 40)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("DynamicObjectsTable:Jungle_Creatures") && !o.InheritsFrom("Goatfolk"));
		}
		if (num <= 50)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("DynamicObjectsTable:Jungle_Creatures"));
		}
		if (num <= 65)
		{
			return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations") && o.HasTag("Humanoid") && !o.InheritsFrom("Goatfolk"));
		}
		return EncountersAPI.GetALegendaryEligibleCreature((GameObjectBlueprint o) => !o.HasTag("ExcludeFromVillagePopulations"));
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

[Serializable]
public class HindrenQuestOutcome : IPart
{
	public long startTick;

	public long ticksToApply = 3600L;

	public string thief;

	public string circumstance;

	public string motive;

	public string climate;

	public string loveState;

	public bool outcomeApplied;

	public string outcome => $"outcome-{thief}-{climate}-{loveState}";

	private Zone zone => HindrenMysteryGamestate.instance.getBeyLahZone();

	public List<GameObject> allVillagers => zone.GetObjects((GameObject o) => o.GetBlueprint().InheritsFrom("BaseHindren"));

	public string DetermineClimate()
	{
		List<string> list = new List<string> { "trade", "craft" };
		List<string> list2 = new List<string> { "illness", "violence" };
		if (list.Contains(circumstance) && list.Contains(motive))
		{
			return climate = "prosperous";
		}
		if (list2.Contains(circumstance) && list2.Contains(motive))
		{
			return climate = "tumultuous";
		}
		return climate = "mixed";
	}

	public HindrenQuestOutcome()
	{
		if (The.Game != null)
		{
			startTick = The.Game.TimeTicks;
		}
	}

	[Obsolete("save compat")]
	public override void Attach()
	{
		Dictionary<string, string> stringGameState = The.Game.StringGameState;
		if (stringGameState.TryGetValue("HindrenMysteryOutcomeClimate", out var value))
		{
			stringGameState["HindrenMysteryOutcomeClimate"] = IKithAndKinPart.KeyOf(value);
		}
		if (stringGameState.TryGetValue("HindrenMysteryOutcomeCircumstance", out value))
		{
			stringGameState["HindrenMysteryOutcomeCircumstance"] = IKithAndKinPart.KeyOf(value);
		}
		if (stringGameState.TryGetValue("HindrenMysteryOutcomeThief", out value))
		{
			stringGameState["HindrenMysteryOutcomeThief"] = IKithAndKinPart.KeyOf(value);
		}
		if (stringGameState.TryGetValue("HindrenMysteryOutcomeHindriarch", out value))
		{
			stringGameState["HindrenMysteryOutcomeHindriarch"] = IKithAndKinPart.KeyOf(value);
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (The.Game != null)
		{
			startTick = The.Game.TimeTicks;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!outcomeApplied && XRLCore.Core.Game.TimeTicks - startTick >= ticksToApply)
		{
			outcomeApplied = true;
			ApplyOutcome(climate, thief, circumstance, motive, loveState);
		}
		return base.HandleEvent(E);
	}

	public string CurrentHindriarch()
	{
		if (thief == "keh")
		{
			return "esk";
		}
		if (thief == "kese" && (!(loveState == "love") || !(climate == "tumultuous")))
		{
			return "esk";
		}
		return "keh";
	}

	public void ApplyPrologue()
	{
		string text = CurrentHindriarch();
		if (text == "esk")
		{
			EskhindReturns();
			KehIsReplacedByEskhind();
		}
		if (text == "keh")
		{
			KehRemainsInPower();
		}
		zone.FindObject("Neelahind").RequirePart<GivesRep>();
		if (climate == "prosperous")
		{
			XRLCore.Core.Game.PlayerReputation.modify("Hindren", 200);
		}
		else if (climate == "tumultuous")
		{
			XRLCore.Core.Game.PlayerReputation.modify("Hindren", 50);
			if (!(loveState == "love") || !(thief == "keh"))
			{
				VillageIsDoomed();
			}
		}
		else if (climate == "mixed")
		{
			XRLCore.Core.Game.PlayerReputation.modify("Hindren", 100);
		}
		if (thief == "esk" || thief == "kendren")
		{
			EskhindRemainsInExile();
			if (loveState == "love")
			{
				NeelahindJoinsEskhindInExile();
			}
		}
		else if (thief == "keh")
		{
			KehIsExiled();
		}
		else if (thief == "kese")
		{
			KesehindIsExiled();
			if (loveState == "love" && climate == "tumultuous")
			{
				EskhindRemainsInExile();
				NeelahindJoinsEskhindInExile();
			}
		}
		foreach (GameObject @object in zone.GetObjects((GameObject o) => o.HasPart("HindrenClueItem") && !o.HasPart("HindrenMysteryCriticalNPC")))
		{
			@object.Destroy();
		}
		foreach (GameObject object2 in zone.GetObjects((GameObject o) => o.HasPart("HindrenClueRumorHaver")))
		{
			object2.RemovePart("HindrenClueRumorHaver");
		}
	}

	public void ApplyOutcome(string climate, string thief, string circumstance, string motive, string loveState)
	{
		The.Game.SetBooleanGameState("HindrenQuestFullyResolved", Value: true);
		switch (climate)
		{
		case "prosperous":
			if (thief == "esk" && loveState == "love")
			{
				VillageSurvives();
			}
			else if (thief == "kese" && loveState == "nolove")
			{
				VillageSurvives();
			}
			else
			{
				VillageProspers();
			}
			break;
		case "mixed":
			if (thief == "keh" && loveState == "love")
			{
				VillageProspers();
			}
			else
			{
				VillageSurvives();
			}
			break;
		case "tumultuous":
			if (thief == "keh" && loveState == "love")
			{
				VillageSurvives();
			}
			else
			{
				VillageRavaged();
			}
			break;
		}
	}

	public void VillageProspers()
	{
		The.Game.SetBooleanGameState("HindrenVillageProspers", Value: true);
		GameObject gameObject = zone.FindObject("Isahind");
		if (gameObject != null)
		{
			GameObjectFactory.ApplyBuilder(gameObject, "Tier2Wares");
		}
	}

	public static void Exile(GameObject Object, GlobalLocation Destination = null)
	{
		Object.AddPart(new HindrenMysteryExile
		{
			Destination = Destination
		});
		Object.RequirePart<SocialRoles>().RequireRole("Hindren Pariah");
		Object.RemovePart("GivesRep");
	}

	public void Exile(string blueprint, GlobalLocation destination = null)
	{
		zone.FindObjects(blueprint).ForEach(delegate(GameObject o)
		{
			Exile(o, destination);
		});
	}

	public void VillageSurvives()
	{
		The.Game.SetBooleanGameState("HindrenVillageSurvives", Value: true);
	}

	public void VillageRavaged()
	{
		allVillagers.ForEach(delegate(GameObject o)
		{
			o.Destroy();
		});
		for (int i = 0; i < Stat.Random(6, 8); i++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("Hindren Corpse");
		}
		for (int j = 0; j < Stat.Random(6, 8); j++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("Bloodsplatter");
		}
		for (int k = 0; k < Stat.Random(6, 8); k++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("Firestarter");
		}
		The.Game.SetBooleanGameState("HindrenVillageRavaged", Value: true);
	}

	public void VillageIsDoomed()
	{
		The.Game.SetBooleanGameState("HindrenVillageDoomed", Value: true);
	}

	public void VillageCollapses()
	{
		allVillagers.ForEach(delegate(GameObject o)
		{
			o.Destroy();
		});
		The.Game.SetBooleanGameState("HindrenVillageCollapses", Value: true);
		for (int i = 0; i < Stat.Random(6, 8); i++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("HindrenAfflicted");
		}
		for (int j = 0; j < Stat.Random(6, 8); j++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("Hindren Corpse");
		}
		for (int k = 0; k < Stat.Random(30, 40); k++)
		{
			zone.GetEmptyCellsShuffled().First().AddObject("PutrescencePool");
		}
	}

	public void KehIsReplacedByEskhind()
	{
		GameObject gameObject = zone.FindObject("Keh");
		if (gameObject != null)
		{
			GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint("PariahKeh");
			gameObject.DisplayName = blueprint.ResolvePartParameter("Render", "DisplayName");
			gameObject.RequirePart<Description>().Short = blueprint.ResolvePartParameter("Description", "Short");
			gameObject.pBrain.PushGoal(new MoveTo(zone.GetCell(30, 20)));
			gameObject.pBrain.StartingCell = zone.GetCell(30, 20).GetGlobalLocation();
			gameObject.RemoveStringProperty("Mayor");
		}
		GameObject gameObject2 = zone.FindObject("Eskhind");
		if (gameObject2 != null)
		{
			gameObject2.DisplayName = "Hindriarch Esk";
			gameObject2.SetStringProperty("WaterRitual_Skill", "Dual_Wield");
			gameObject2.pBrain.FactionMembership.Clear();
			gameObject2.pBrain.FactionMembership.Add("Hindren", 100);
			gameObject2.RemovePart("GivesRep");
			gameObject2.AddPart(new GivesRep());
			gameObject2.SetStringProperty("Mayor", "Hindren");
		}
		GameObject gameObject3 = zone.FindObject("Meyehind");
		if (gameObject3 != null)
		{
			gameObject3.pBrain.FactionMembership.Clear();
			gameObject3.pBrain.FactionMembership.Add("Hindren", 100);
		}
		GameObject gameObject4 = zone.FindObject("Liihart");
		if (gameObject4 != null)
		{
			gameObject4.pBrain.FactionMembership.Clear();
			gameObject4.pBrain.FactionMembership.Add("Hindren", 100);
		}
		EskhindIsAppointedAdvisor();
	}

	public void KehRemainsInPower()
	{
		GameObject gameObject = zone.FindObject("Keh");
		if (gameObject != null)
		{
			gameObject.SetStringProperty("WaterRitual_Skill", "Persuasion_Berate");
			gameObject.pBrain.FactionMembership.Clear();
			gameObject.pBrain.FactionMembership.Add("Hindren", 100);
			gameObject.RemovePart("GivesRep");
			gameObject.AddPart(new GivesRep());
		}
	}

	public void KehIsExiled()
	{
		Exile("Keh");
	}

	public void EskhindIsAppointedAdvisor()
	{
		GameObject gameObject = zone.FindObject("Eskhind");
		if (gameObject != null)
		{
			Cell cell = zone.GetCell(73, 7);
			gameObject.pBrain.PushGoal(new MoveTo(cell, careful: false, overridesCombat: true));
			gameObject.pBrain.StartingCell = cell.GetGlobalLocation();
		}
		GameObject gameObject2 = zone.FindObject("Meyehind");
		if (gameObject2 != null)
		{
			Cell cell2 = zone.GetCell(73, 9);
			gameObject2.pBrain.PushGoal(new MoveTo(cell2));
			gameObject2.pBrain.StartingCell = cell2.GetGlobalLocation();
		}
		GameObject gameObject3 = zone.FindObject("Liihart");
		if (gameObject3 != null)
		{
			Cell cell3 = zone.GetCell(72, 10);
			gameObject3.pBrain.PushGoal(new MoveTo(cell3));
			gameObject3.pBrain.StartingCell = cell3.GetGlobalLocation();
		}
	}

	public void EskhindRemainsInExile()
	{
		Exile("Eskhind", GlobalLocation.FromZoneId(The.Game.GetStringGameState("HollowTreeZoneId"), 35, 9));
		Exile("Meyehind", GlobalLocation.FromZoneId(The.Game.GetStringGameState("HollowTreeZoneId"), 35, 10));
		Exile("Liihart", GlobalLocation.FromZoneId(The.Game.GetStringGameState("HollowTreeZoneId"), 34, 9));
	}

	public void EskhindReturns()
	{
		The.Game.SetBooleanGameState("EskhindReturned", Value: true);
		zone.FindObject("Eshkind")?.SetIntProperty("ReturnedEskhind", 1);
	}

	public void NeelahindJoinsEskhindInExile()
	{
		The.Game.SetBooleanGameState("HindrenMysteryLovebirdsEloped", Value: true);
		Exile("Neelahind", GlobalLocation.FromZoneId(The.Game.GetStringGameState("HollowTreeZoneId"), 35, 10));
	}

	public void KesehindIsExiled()
	{
		Exile("Kesehind");
	}
}

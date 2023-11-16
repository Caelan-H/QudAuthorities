using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PetEitherOr : IPart
{
	public bool triggerChirality;

	public bool Either;

	public int Cooldown;

	private int type;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Cooldown > 0)
		{
			Cooldown--;
			if (Cooldown <= 0)
			{
				int num = 0;
				while (ParentObject.HasPart("AnimatedMaterialGeneric") && ++num <= 100)
				{
					ParentObject.RemovePart("AnimatedMaterialGeneric");
				}
				explode();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (IComponent<GameObject>.ThePlayer != null)
		{
			if (!IComponent<GameObject>.ThePlayer.HasPart("PetEitherOrRespawner"))
			{
				IComponent<GameObject>.ThePlayer.AddPart(new PetEitherOrRespawner());
			}
			PetEitherOrRespawner part = IComponent<GameObject>.ThePlayer.GetPart<PetEitherOrRespawner>();
			if (Either)
			{
				part.respawnEither = true;
			}
			if (!Either)
			{
				part.respawnOr = true;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "PlayerGainedLevel");
		Object.RegisterPartEvent(this, "AccomplishmentAdded");
		base.Register(Object);
	}

	public bool testChirality()
	{
		return triggerChirality == Either;
	}

	public void trigger()
	{
		IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " starts to flicker.");
		if (!ParentObject.HasPart("AnimatedMaterialGeneric"))
		{
			ParentObject.AddPart(new AnimatedMaterialGeneric());
			AnimatedMaterialGeneric obj = ParentObject.GetPart("AnimatedMaterialGeneric") as AnimatedMaterialGeneric;
			obj.AnimationLength = 20;
			obj.ColorStringAnimationFrames = "0=" + ParentObject.pRender.ColorString + ",10=&" + ParentObject.pRender.DetailColor;
			obj.DetailColorAnimationFrames = "0=" + ParentObject.pRender.DetailColor + ",10=" + ParentObject.pRender.ColorString.Substring(1);
		}
		Cooldown = Stat.SeededRandom(XRLCore.Core.Game.Turns.ToString(), 8, 20);
		type = Stat.SeededRandom(XRLCore.Core.Game.Turns.ToString(), 1, 11);
		ParentObject.pBrain.PushGoal(new WanderDuration(Cooldown));
		triggerChirality = XRLCore.Core.Game.Turns % 2 == 0;
		if (ParentObject.pPhysics.CurrentCell.ParentZone.CountObjects((GameObject o) => o.HasTag("EitherOrPet")) <= 1)
		{
			triggerChirality = Either;
		}
	}

	public void boom()
	{
		ParentObject.DustPuff();
		ParentObject.PsychicPulse();
		ParentObject.Destroy();
	}

	public void wall(string blueprint)
	{
		List<GameObject> objectsWithTag = ParentObject.pPhysics.CurrentCell.ParentZone.GetObjectsWithTag("EitherOrPet");
		if (objectsWithTag.Count < 2)
		{
			return;
		}
		FindPath findPath = new FindPath(objectsWithTag[0].pPhysics.CurrentCell.ParentZone, objectsWithTag[0].pPhysics.CurrentCell.X, objectsWithTag[0].pPhysics.CurrentCell.Y, objectsWithTag[1].pPhysics.CurrentCell.ParentZone, objectsWithTag[1].pPhysics.CurrentCell.X, objectsWithTag[1].pPhysics.CurrentCell.Y, PathGlobal: true, PathUnlimited: true, GameObject.create("Drillbot"), AddNoise: false, CardinalOnly: true);
		if (!findPath.bFound)
		{
			return;
		}
		for (int i = 0; i < findPath.Steps.Count; i++)
		{
			GameObject gameObject = ((blueprint[0] != '*') ? findPath.Steps[i].AddObject(blueprint) : findPath.Steps[i].AddObject(PopulationManager.RollOneFrom(blueprint.Substring(1)).Blueprint));
			if (gameObject != null && gameObject.HasPart("Combat"))
			{
				gameObject.SetActive();
			}
		}
	}

	public void explode()
	{
		ParentObject.pBrain.Goals.Clear();
		if (ParentObject.pPhysics.CurrentCell == null)
		{
			return;
		}
		if (type == 1 && testChirality())
		{
			ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocation().AddObject("Space-Time Vortex");
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " implodes.");
			boom();
		}
		if (type == 2 && testChirality())
		{
			ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocation().AddObject(PopulationManager.RollOneFrom("Village_RandomBaseStatue_*Default").Blueprint);
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is smeared into stone by the rasp of time.");
			boom();
		}
		if (type == 3 && testChirality())
		{
			for (int i = 0; i < Stat.Random(4, 6); i++)
			{
				GameObject gameObject = ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocation().AddObject("ClockworkBeetle");
				gameObject.pBrain.SetFeeling(IComponent<GameObject>.ThePlayer, 0);
				gameObject.SetActive();
			}
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " crumbles into beetles.");
			boom();
		}
		if (type == 4 && testChirality())
		{
			int num = 0;
			foreach (Cell connectedSpawnLocation2 in ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocations(3))
			{
				GameObject anObject = EncountersAPI.GetAnObject();
				connectedSpawnLocation2.AddObject(anObject);
				if (anObject.HasPart("Combat"))
				{
					anObject.SetActive();
				}
				num++;
				if (num >= 3)
				{
					break;
				}
			}
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is vacuumed to another place and time. The void that remains is filled with three important objects from one of your side lives.");
			boom();
		}
		if (type == 5 && testChirality())
		{
			Cell connectedSpawnLocation = ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocation();
			if (connectedSpawnLocation != null)
			{
				int zoneTier = ParentObject.pPhysics.CurrentCell.ParentZone.NewTier;
				GameObject aCreature = EncountersAPI.GetACreature((GameObjectBlueprint o) => o.Tier == zoneTier || o.Tier == zoneTier + 1);
				connectedSpawnLocation.AddObject(aCreature);
				aCreature.SetActive();
				IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + ParentObject.GetVerb("atomize") + " and" + ParentObject.GetVerb("recombine") + " into " + aCreature.a + aCreature.DisplayNameOnly + ".");
			}
			boom();
		}
		if (type == 6 && testChirality())
		{
			List<string> list = new List<string>();
			foreach (Cell adjacentCell in ParentObject.pPhysics.CurrentCell.GetAdjacentCells(3))
			{
				foreach (GameObject @object in adjacentCell.Objects)
				{
					if (AnimateObject.CanAnimate(@object) && Stat.Random(1, 100) <= 50)
					{
						AnimateObject.Animate(@object);
						@object.SetActive();
						list.Add(@object.DisplayNameOnly);
					}
				}
			}
			if (list.Count == 0)
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.DisplayNameOnly) + " consciousness dissipates.");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.DisplayNameOnly) + " consciousness dissipates into " + Grammar.MakeAndList(list) + ".");
			}
			boom();
		}
		if (type == 7 && testChirality())
		{
			List<string> list2 = new List<string>();
			for (int j = 0; j < 3; j++)
			{
				list2.Add(PopulationManager.RollOneFrom("RandomLiquid").Blueprint);
			}
			foreach (Cell adjacentCell2 in ParentObject.pPhysics.CurrentCell.GetAdjacentCells(2))
			{
				adjacentCell2.AddObject(LiquidVolume.create(list2));
			}
			GameObject gameObject2 = LiquidVolume.create(list2);
			string text = (gameObject2.DisplayName.Contains("pool of ") ? gameObject2.DisplayName.Substring(gameObject2.DisplayName.IndexOf("of", gameObject2.DisplayName.IndexOf("of") + 1) + 2) : "liquid");
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " liquifies into several pools of" + text + "&y.");
			boom();
		}
		if (type == 8)
		{
			if (testChirality())
			{
				foreach (GameObject item in ParentObject.pPhysics.CurrentCell.ParentZone.GetObjectsWithPart("Combat"))
				{
					item.ApplyEffect(new BlinkingTicSickness(1200));
				}
			}
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is folded a trillion times by the pressure of the nether, causing the local region of spacetime to lose contiguity.");
			boom();
		}
		if (type == 9)
		{
			wall("Forcefield");
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is vectorized into a line of force.");
			boom();
		}
		if (type == 10)
		{
			wall("RealityStabilizationField");
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is vectorized into a line of normality.");
			boom();
		}
		if (type == 11)
		{
			wall("*PlantSummoning" + ParentObject.pPhysics.CurrentCell.ParentZone.NewTier);
			IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayNameOnly + " is vectorized into a line of plants.");
			boom();
		}
	}

	public static bool ContainsAccomplishmentExclusion(string accomplishment)
	{
		if (!accomplishment.StartsWith("You read ") && !accomplishment.Contains("arrived at Joppa") && !accomplishment.Contains(", you arrived at "))
		{
			return accomplishment.StartsWith("Sheba Hagadias");
		}
		return true;
	}

	public static string GetEitherOrAccomplishment()
	{
		JournalAccomplishment randomElement = (from c in JournalAPI.Accomplishments
			select (c) into c
			where !ContainsAccomplishmentExclusion(c.text)
			select c).GetRandomElement();
		if (randomElement == null)
		{
			return Grammar.Weirdify("You've done nothing.");
		}
		return ColorUtility.StripFormatting(Grammar.Weirdify(randomElement.text, 75));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AccomplishmentAdded")
		{
			string stringParameter = E.GetStringParameter("Text");
			if (!string.IsNullOrEmpty(stringParameter) && !ContainsAccomplishmentExclusion(stringParameter))
			{
				trigger();
			}
		}
		else if (E.ID == "PlayerGainedLevel")
		{
			ParentObject.GetPart<Leveler>()?.LevelUp();
			trigger();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.pRender != null)
		{
			ParentObject.pRender.Tile = IComponent<GameObject>.ThePlayer.pRender.Tile;
			E.RenderString = IComponent<GameObject>.ThePlayer.pRender.RenderString;
			E.Tile = IComponent<GameObject>.ThePlayer.pRender.Tile;
		}
		if (Cooldown <= 0)
		{
			if (ParentObject.pPhysics != null && ParentObject.pPhysics.CurrentCell != null && ParentObject.pPhysics.CurrentCell.CountObjectsWithTag("EitherOrPet") > 1)
			{
				E.ColorString = "&Y";
				E.DetailColor = "y";
			}
			else if (Either)
			{
				E.ColorString = "&c";
				E.DetailColor = "C";
			}
			else
			{
				E.ColorString = "&r";
				E.DetailColor = "R";
			}
		}
		return true;
	}
}

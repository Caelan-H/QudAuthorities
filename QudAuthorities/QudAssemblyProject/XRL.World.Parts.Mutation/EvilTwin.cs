using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class EvilTwin : BaseMutation
{
	public const int CHANCE_PER_ZONE_NON_LEVEL_BASED = 5;

	public const int CHANCE_PER_ZONE_LEVEL_BASED = 25;

	public const int BASE_CHANCE_PER_LEVEL_LEVEL_BASED = 20;

	public const int CHANCE_PER_LEVEL_LEVEL_BASED = 2;

	public int Due;

	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public EvilTwin()
	{
		DisplayName = "Evil Twin ({{r|D}})";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Acting on some inscrutable impulse, a parallel version of yourself travels through space and time to destroy you.\n\nEach time you embark on a new location, there's a small chance your evil twin has tracked you there and attempts to kill you.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	private static void ApplyHostility(GameObject who, Brain pBrain, int Depth)
	{
		if (who != null && Depth < 100)
		{
			pBrain.SetFeeling(who, -100);
			ApplyHostility(who.PartyLeader, pBrain, Depth + 1);
			if (who.GetEffect("Dominated") is Dominated dominated)
			{
				ApplyHostility(dominated.Dominator, pBrain, Depth + 1);
			}
		}
	}

	public static bool CreateEvilTwin(GameObject Original, string Prefix, Cell targetCell = null, string message = "{{c|You sense a sinister presence nearby.}}", string colorString = "&K", GameObject Actor = null, string messageForActor = null)
	{
		if (Actor == null)
		{
			Actor = Original;
		}
		string text = "Twin";
		if (!string.IsNullOrEmpty(Prefix))
		{
			text = Prefix + (Prefix.EndsWith("-") ? "" : " ") + text;
		}
		if (!CanBeReplicatedEvent.Check(Original, Actor, text))
		{
			return false;
		}
		GameObject gameObject = Original.DeepCopy();
		if (gameObject.HasPart("EvilTwin"))
		{
			Mutations part = gameObject.GetPart<Mutations>();
			part.RemoveMutation(part.GetMutation("EvilTwin"));
			gameObject.RemovePart("EvilTwin");
		}
		if (Original.IsPlayer())
		{
			gameObject.SetStringProperty("PlayerCopy", "true");
		}
		gameObject.SetIntProperty("CopyAllowFeelingAdjust", 1);
		gameObject.SetStringProperty("EvilTwin", "true");
		gameObject.SetIntProperty("Entropic", 1);
		Brain pBrain = gameObject.pBrain;
		pBrain.PartyLeader = null;
		pBrain.Hostile = true;
		pBrain.Calm = false;
		pBrain.Hibernating = false;
		pBrain.Staying = false;
		pBrain.Passive = false;
		pBrain.Factions = (Original.IsPlayer() ? "highly entropic beings-100,Mean-100,Playerhater-99" : "highly entropic beings-100,Mean-100");
		pBrain.InitFromFactions();
		if (Actor != null)
		{
			pBrain.SetFeeling(Actor, 0);
			Actor.SetFeeling(gameObject, 0);
		}
		if (!string.IsNullOrEmpty(Prefix))
		{
			gameObject.DisplayName = Prefix + (Prefix.EndsWith("-") ? "" : " ") + gameObject.DisplayNameOnlyDirect;
		}
		if (gameObject.GetPart("Description") is Description description && description._Short == "It's you.")
		{
			description.Short = "It's evil you.";
		}
		Event @event = Event.New("EvilTwinAttitudeSetup");
		@event.SetParameter("Original", Original);
		@event.SetParameter("Twin", gameObject);
		@event.SetParameter("Actor", Actor);
		if (Original.FireEvent(@event))
		{
			pBrain.PushGoal(new Kill(Original));
			ApplyHostility(Original, pBrain, 0);
		}
		Zone currentZone = Original.CurrentZone;
		gameObject.pRender.ColorString = colorString;
		Temporary.AddHierarchically(gameObject);
		gameObject.pBrain?.PerformEquip();
		if (targetCell == null)
		{
			int num = 1000;
			while (num > 0)
			{
				num--;
				int x = Stat.Random(0, currentZone.Width - 1);
				int y = Stat.Random(0, currentZone.Height - 1);
				Cell cell = currentZone.GetCell(x, y);
				if (cell.IsEmpty())
				{
					targetCell = cell;
					break;
				}
			}
		}
		if (targetCell == null)
		{
			gameObject.Obliterate();
			return false;
		}
		targetCell.AddObject(gameObject);
		gameObject.MakeActive();
		WasReplicatedEvent.Send(Original, Actor, gameObject, text);
		ReplicaCreatedEvent.Send(gameObject, Actor, Original, text);
		if (!string.IsNullOrEmpty(message) && Original.IsPlayer())
		{
			Popup.Show(message);
		}
		if (!string.IsNullOrEmpty(messageForActor) && Actor.IsPlayer())
		{
			Popup.Show(messageForActor);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!E.Object.OnWorldMap())
		{
			string text = E.Object.CurrentZone?.ZoneID;
			if (!string.IsNullOrEmpty(text) && !Visited.ContainsKey(text))
			{
				Visited[text] = true;
				if (!E.Object.DisplayName.Contains("[Creature]"))
				{
					if (GlobalConfig.GetBoolSetting("EvilTwinLevelBased"))
					{
						if (Due > 0 && 25.in100() && CreateEvilTwin(ParentObject, "Evil"))
						{
							Due--;
						}
					}
					else if (5.in100())
					{
						CreateEvilTwin(ParentObject, "Evil");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLevelGained");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLevelGained" && GlobalConfig.GetBoolSetting("EvilTwinLevelBased") && (20 + 2 * ParentObject.Stat("Level")).in100())
		{
			Due++;
		}
		return base.FireEvent(E);
	}
}

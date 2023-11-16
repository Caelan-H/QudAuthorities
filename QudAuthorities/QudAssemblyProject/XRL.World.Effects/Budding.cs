using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Budding : Effect
{
	public const string DEFAULT_REPLICATION_CONTEXT = "Budding";

	public int numClones = 1;

	public int baseDuration = 20;

	public string ActorID;

	public string ReplicationContext = "Budding";

	public Budding()
	{
		base.Duration = 20;
		base.DisplayName = "{{r|budding}}";
	}

	public Budding(GameObject Actor = null, int numClones = 1, string ReplicationContext = "Budding")
		: this()
	{
		ActorID = Actor?.id;
		this.numClones = numClones;
		this.ReplicationContext = ReplicationContext;
	}

	public Budding(string ActorID = null, int numClones = 1, string ReplicationContext = "Budding")
		: this()
	{
		this.ActorID = ActorID;
		this.numClones = numClones;
		this.ReplicationContext = ReplicationContext;
	}

	public override int GetEffectType()
	{
		return 67108880;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{r|budding}}";
	}

	public override string GetDetails()
	{
		return "Will spawn a clone soon.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Budding"))
		{
			return false;
		}
		if (!Cloning.CanBeCloned(Object, GameObject.findById(ActorID), ReplicationContext))
		{
			return false;
		}
		BodyPart bodyPart = Object.Body?.GetFirstPart("Back");
		if (Visible())
		{
			if (bodyPart != null)
			{
				IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + Object.poss(bodyPart.GetOrdinalName()) + " as " + Object.does("begin", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + Object.t() + " as " + Object.does("begin", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
			}
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (numClones > 0)
		{
			BodyPart bodyPart = Object.Body?.GetFirstPart("Back");
			if (Visible())
			{
				if (bodyPart != null)
				{
					IComponent<GameObject>.AddPlayerMessage("The grotesque protuberance on " + Object.poss(bodyPart.GetOrdinalName()) + " subsides.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Object.Poss("grotesque protuberance") + " subsides.");
				}
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != BeforeBeginTakeActionEvent.ID || base.Object?.pBrain == null))
		{
			if (ID == EndTurnEvent.ID)
			{
				return base.Object?.pBrain == null;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (base.Object?.pBrain != null)
		{
			ProcessTurn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (base.Object?.pBrain == null)
		{
			ProcessTurn();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			base.Object.RemoveEffect(this);
		}
		return base.FireEvent(E);
	}

	public void ProcessTurn()
	{
		Zone zone = base.Object?.CurrentZone;
		if (zone == null || !zone.IsActive() || zone.IsWorldMap() || base.Duration <= 0 || base.Duration == 9999)
		{
			return;
		}
		base.Duration--;
		if (base.Duration > 0 || !Cloning.CanBeCloned(base.Object, null, ReplicationContext))
		{
			return;
		}
		if (Cloning.GenerateBuddedClone(base.Object, GameObject.findById(ActorID), DuplicateGear: false, BecomesCompanion: true, 1, ReplicationContext) != null)
		{
			numClones--;
			if (numClones > 0)
			{
				base.Duration = baseDuration;
			}
		}
		else
		{
			base.Duration = 1;
		}
	}
}

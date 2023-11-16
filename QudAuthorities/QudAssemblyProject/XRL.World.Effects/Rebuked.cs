using System;
using HistoryKit;
using Qud.API;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Rebuked : Effect
{
	public GameObject Rebuker;

	public Rebuked()
	{
		base.DisplayName = "{{C|rebuked}}";
		base.Duration = 1;
	}

	public Rebuked(GameObject Rebuker)
		: this()
	{
		this.Rebuker = Rebuker;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Rebuked into obedience by another creature.";
	}

	public override string GetDescription()
	{
		return "{{C|rebuked}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor == Rebuker)
		{
			E.AddAction("Dismiss", "dismiss", "DismissServitor", null, 'd', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DismissServitor" && E.Actor == Rebuker && E.Item == base.Object && Rebuker.CheckCompanionDirection(base.Object))
		{
			IComponent<GameObject>.XDidYToZ(Rebuker, "dismiss", base.Object, "from " + Rebuker.its + " service");
			base.Object.RemoveEffect(this);
			E.Actor.CompanionDirectionEnergyCost(E.Item, 100, "Dismiss");
			E.Item.FireEvent(Event.New("DismissedFromService", "Object", E.Item, "Leader", E.Actor, "Effect", this));
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.validate(ref Rebuker))
		{
			return false;
		}
		if (Object.pBrain == null)
		{
			return false;
		}
		Object.RemoveEffect("Rebuked");
		IComponent<GameObject>.XDidYToZ(Rebuker, "rebuke", Object, "into submission", null, null, Rebuker);
		if (Rebuker.IsPlayer() && !Rebuker.HasEffect("Dominated"))
		{
			JournalAPI.AddAccomplishment("You rebuked " + Object.a + Object.ShortDisplayName + " into submission.", HistoricStringExpander.ExpandString("<spice.commonPhrases.onlooker.!random.capitalize>! <spice.commonPhrases.remember.!random.capitalize> the admonishment =name= gave " + Object.a + Object.ShortDisplayNameWithoutEpithetStripped + " when " + Object.it + " presumed to speak the sacred tongue!"), "general", JournalAccomplishment.MuralCategory.Trysts, JournalAccomplishment.MuralWeight.Low, null, -1L);
		}
		Object.Heartspray("&C", "&c", "&B");
		Object.pBrain.BecomeCompanionOf(Rebuker);
		ApplyRebuked(SyncTarget: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.pBrain != null && (!GameObject.validate(ref Rebuker) || Object.pBrain.PartyLeader == Rebuker))
		{
			Object.pBrain.PartyLeader = null;
			Object.pBrain.Goals.Clear();
		}
		UnapplyRebuked();
		Rebuker = null;
		Object.UpdateVisibleStatusColor();
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	private Persuasion_RebukeRobot GetSkill(bool SyncTarget = false)
	{
		if (!GameObject.validate(ref Rebuker))
		{
			return null;
		}
		if (!(Rebuker.GetPart("Persuasion_RebukeRobot") is Persuasion_RebukeRobot persuasion_RebukeRobot))
		{
			return null;
		}
		if (SyncTarget)
		{
			persuasion_RebukeRobot.SyncTarget(base.Object);
		}
		if (!base.Object.idmatch(persuasion_RebukeRobot.TargetID))
		{
			return null;
		}
		return persuasion_RebukeRobot;
	}

	public void ApplyRebuked(bool SyncTarget = false)
	{
		GetSkill(SyncTarget);
	}

	public void UnapplyRebuked()
	{
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && GetSkill() == null)
		{
			base.Duration = 0;
		}
		return base.FireEvent(E);
	}
}

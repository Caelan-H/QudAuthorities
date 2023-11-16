using System;
using Qud.API;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Proselytized : Effect
{
	public GameObject Proselytizer;

	public Proselytized()
	{
		base.DisplayName = "{{Y|proselytized}}";
		base.Duration = 1;
	}

	public Proselytized(GameObject Proselytizer)
		: this()
	{
		this.Proselytizer = Proselytizer;
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
		return "Recruited by another creature via proselytization.";
	}

	public override string GetDescription()
	{
		return "{{Y|proselytized}}";
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
		if (E.Actor == Proselytizer)
		{
			E.AddAction("Dismiss", "dismiss", "DismissProselyte", null, 'd', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DismissProselyte" && E.Actor == Proselytizer && E.Item == base.Object && Proselytizer.CheckCompanionDirection(base.Object))
		{
			IComponent<GameObject>.XDidYToZ(Proselytizer, "dismiss", base.Object, "from " + Proselytizer.its + " service");
			Proselytizer.pBrain?.PartyMembers?.Remove(base.Object.id);
			base.Object.RemoveEffect(this);
			E.Actor.CompanionDirectionEnergyCost(E.Item, 100, "Dismiss");
			E.Item.FireEvent(Event.New("DismissedFromService", "Object", E.Item, "Leader", E.Actor, "Effect", this));
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.validate(ref Proselytizer))
		{
			return false;
		}
		if (Object.pBrain == null)
		{
			return false;
		}
		if (!Object.FireEvent("ApplyProselytize"))
		{
			return false;
		}
		Object.RemoveEffect("Proselytized");
		IComponent<GameObject>.XDidYToZ(Proselytizer, "convince", Object, "to join " + Proselytizer.them, "!", null, Proselytizer);
		if (Proselytizer.IsPlayer() && !Proselytizer.HasEffect("Dominated"))
		{
			JournalAPI.AddAccomplishment("You convinced " + Object.a + Object.ShortDisplayName + " to join your cause.", "Few were possessed of such potent charm as =name=, who -- on the " + Calendar.getDay() + " of " + Calendar.getMonth() + " -- bent the will of " + Object.a + Object.ShortDisplayName + " with mere words.", "general", JournalAccomplishment.MuralCategory.Treats, JournalAccomplishment.MuralWeight.Low, null, -1L);
		}
		Object.Heartspray();
		Object.pBrain.AdjustFeeling(Proselytizer, 100);
		Object.pBrain.BecomeCompanionOf(Proselytizer);
		Persuasion_Proselytize.SyncTarget(Proselytizer, Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.validate(ref Proselytizer) && Object.PartyLeader == Proselytizer && !Proselytizer.SupportsFollower(Object))
		{
			Object.pBrain.PartyLeader = null;
			Object.pBrain.Goals.Clear();
		}
		Proselytizer = null;
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

	public bool IsSupported()
	{
		if (GameObject.validate(ref Proselytizer))
		{
			return Proselytizer.SupportsFollower(base.Object, 1);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && !IsSupported())
		{
			base.Duration = 0;
		}
		return base.FireEvent(E);
	}
}

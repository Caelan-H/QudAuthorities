using System;
using UnityEngine;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Dominated : Effect
{
	public GameObject Dominator;

	public bool RoboDom;

	public Guid AAID;

	public string DominatorDebugName;

	public bool FromOriginalPlayerBody;

	[NonSerialized]
	public bool BeingRemovedBySource;

	[NonSerialized]
	public bool Metempsychosis;

	public Dominated()
	{
		base.DisplayName = "dominated";
	}

	public Dominated(GameObject Dominator, int Duration)
		: this()
	{
		this.Dominator = Dominator;
		base.Duration = Duration;
		DominatorDebugName = Dominator?.DebugName;
		FromOriginalPlayerBody = Dominator.IsOriginalPlayerBody();
	}

	public Dominated(GameObject Dominator, bool RoboDom, int Duration)
		: this(Dominator, Duration)
	{
		this.RoboDom = RoboDom;
	}

	public override int GetEffectType()
	{
		return 32770;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Under someone else's control.";
	}

	public override string GetDescription()
	{
		return "dominated (" + base.Duration.Things("turn") + " remaining)";
	}

	public override bool Apply(GameObject Object)
	{
		if (RoboDom)
		{
			if (!Object.FireEvent("ApplyRoboDomination"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "RoboDomination", this))
			{
				return false;
			}
		}
		else
		{
			if (!Object.FireEvent("ApplyDomination"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "Domination", this))
			{
				return false;
			}
		}
		try
		{
			Object.FireEvent(Event.New("DominationStarted", "Subject", Object, "Dominator", Dominator, "Effect", this));
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in DominationStarted event: " + ex.ToString());
		}
		Object.UpdateVisibleStatusColor();
		AAID = Object.AddActivatedAbility("End Domination", "CommandEndDomination", "Mental Mutation");
		if (Dominator != null && Dominator.GetEffect("Lost") is Lost e)
		{
			Dominator.RemoveEffect(e, NeedStackCheck: false);
			Object.ApplyEffect(e);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Lost lost = null;
		try
		{
			lost = Object.GetEffect("Lost") as Lost;
			if (lost != null)
			{
				Object.RemoveEffect(lost, NeedStackCheck: false);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception removing Lost", x);
		}
		try
		{
			Event @event = Event.New("DominationEnded");
			@event.SetParameter("Subject", Object);
			@event.SetParameter("Dominator", Dominator);
			@event.SetParameter("Effect", this);
			if (Metempsychosis)
			{
				@event.SetFlag("Metempsychosis", State: true);
			}
			Object.FireEvent(@event);
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("exception from DominationEnded event", x2);
		}
		try
		{
			Object.RemoveActivatedAbility(ref AAID);
		}
		catch (Exception x3)
		{
			MetricsManager.LogError("exception removing End Domination ability", x3);
		}
		if (BeingRemovedBySource)
		{
			return;
		}
		try
		{
			if (Object.OnWorldMap())
			{
				Object.PullDown();
			}
			if (GameObject.validate(ref Dominator) && Dominator.HasHitpoints())
			{
				if (lost != null)
				{
					Dominator.ApplyEffect(lost);
				}
				Dominator.FireEvent("DominationBroken");
			}
			else if (Object.IsPlayer())
			{
				Domination.Metempsychosis(Object, FromOriginalPlayerBody);
			}
		}
		catch (Exception x4)
		{
			MetricsManager.LogError("exception in Domination cleanup", x4);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (!GameObject.validate(ref Dominator))
			{
				MetricsManager.LogError("ending domination because of loss of dominator, was " + (DominatorDebugName ?? "null"));
				base.Object.RemoveEffect(this);
			}
			else if (base.Duration != 9999)
			{
				base.Duration--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		base.Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeforeDie");
		Object.RegisterEffectEvent(this, "CommandEndDomination");
		Object.RegisterEffectEvent(this, "InterruptDomination");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeforeDie");
		Object.UnregisterEffectEvent(this, "CommandEndDomination");
		Object.UnregisterEffectEvent(this, "InterruptDomination");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie" || E.ID == "CommandEndDomination")
		{
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "InterruptDomination" && base.Object.FireEvent("ChainInterruptDomination"))
		{
			base.Object.RemoveEffect(this);
			return false;
		}
		return base.FireEvent(E);
	}
}

using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeDeathRemovalEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<BeforeDeathRemovalEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static BeforeDeathRemovalEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeDeathRemovalEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeDeathRemovalEvent()
	{
		base.ID = ID;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static BeforeDeathRemovalEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeDeathRemovalEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		BeforeDeathRemovalEvent beforeDeathRemovalEvent = FromPool();
		beforeDeathRemovalEvent.Dying = Dying;
		beforeDeathRemovalEvent.Killer = Killer;
		beforeDeathRemovalEvent.Weapon = Weapon;
		beforeDeathRemovalEvent.Projectile = Projectile;
		beforeDeathRemovalEvent.Accidental = Accidental;
		beforeDeathRemovalEvent.KillerText = KillerText;
		beforeDeathRemovalEvent.Reason = Reason;
		beforeDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
		return beforeDeathRemovalEvent;
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.HasRegisteredEvent("BeforeDeathRemoval"))
			{
				Event @event = Event.New("BeforeDeathRemoval");
				@event.SetParameter("Dying", Dying);
				@event.SetParameter("Killer", Killer);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("KillerText", KillerText);
				@event.SetParameter("Reason", Reason);
				@event.SetParameter("ThirdPersonReason", ThirdPersonReason);
				@event.SetFlag("Accidental", Accidental);
				flag = Dying.FireEvent(@event);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("BeforeDeathRemoval registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				BeforeDeathRemovalEvent e = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
				flag = Dying.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDeathRemoval MinEvent handling", x2);
		}
	}
}

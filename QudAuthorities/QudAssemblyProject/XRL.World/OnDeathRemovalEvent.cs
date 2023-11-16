using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OnDeathRemovalEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<OnDeathRemovalEvent> Pool;

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

	static OnDeathRemovalEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OnDeathRemovalEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OnDeathRemovalEvent()
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

	public static OnDeathRemovalEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static OnDeathRemovalEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		OnDeathRemovalEvent onDeathRemovalEvent = FromPool();
		onDeathRemovalEvent.Dying = Dying;
		onDeathRemovalEvent.Killer = Killer;
		onDeathRemovalEvent.Weapon = Weapon;
		onDeathRemovalEvent.Projectile = Projectile;
		onDeathRemovalEvent.Accidental = Accidental;
		onDeathRemovalEvent.KillerText = KillerText;
		onDeathRemovalEvent.Reason = Reason;
		onDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
		return onDeathRemovalEvent;
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.HasRegisteredEvent("OnDeathRemoval"))
			{
				Event @event = Event.New("OnDeathRemoval");
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
			MetricsManager.LogError("OnDeathRemoval registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				OnDeathRemovalEvent e = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
				flag = Dying.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("OnDeathRemoval MinEvent handling", x2);
		}
	}
}

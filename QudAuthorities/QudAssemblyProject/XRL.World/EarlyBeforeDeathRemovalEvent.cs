using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EarlyBeforeDeathRemovalEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<EarlyBeforeDeathRemovalEvent> Pool;

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

	static EarlyBeforeDeathRemovalEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EarlyBeforeDeathRemovalEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EarlyBeforeDeathRemovalEvent()
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

	public static EarlyBeforeDeathRemovalEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EarlyBeforeDeathRemovalEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		EarlyBeforeDeathRemovalEvent earlyBeforeDeathRemovalEvent = FromPool();
		earlyBeforeDeathRemovalEvent.Dying = Dying;
		earlyBeforeDeathRemovalEvent.Killer = Killer;
		earlyBeforeDeathRemovalEvent.Weapon = Weapon;
		earlyBeforeDeathRemovalEvent.Projectile = Projectile;
		earlyBeforeDeathRemovalEvent.Accidental = Accidental;
		earlyBeforeDeathRemovalEvent.KillerText = KillerText;
		earlyBeforeDeathRemovalEvent.Reason = Reason;
		earlyBeforeDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
		return earlyBeforeDeathRemovalEvent;
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.HasRegisteredEvent("EarlyBeforeDeathRemoval"))
			{
				Event @event = Event.New("EarlyBeforeDeathRemoval");
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
			MetricsManager.LogError("EarlyBeforeDeathRemoval registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				EarlyBeforeDeathRemovalEvent e = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
				flag = Dying.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("EarlyBeforeDeathRemoval MinEvent handling", x2);
		}
	}
}

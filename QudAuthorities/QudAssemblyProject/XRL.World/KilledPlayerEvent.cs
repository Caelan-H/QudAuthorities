using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class KilledPlayerEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<KilledPlayerEvent> Pool;

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

	static KilledPlayerEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(KilledPlayerEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public KilledPlayerEvent()
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

	public static KilledPlayerEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static KilledPlayerEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		KilledPlayerEvent killedPlayerEvent = FromPool();
		killedPlayerEvent.Dying = Dying;
		killedPlayerEvent.Killer = Killer;
		killedPlayerEvent.Weapon = Weapon;
		killedPlayerEvent.Projectile = Projectile;
		killedPlayerEvent.Accidental = Accidental;
		killedPlayerEvent.KillerText = KillerText;
		killedPlayerEvent.Reason = Reason;
		killedPlayerEvent.ThirdPersonReason = ThirdPersonReason;
		return killedPlayerEvent;
	}

	public static void Send(GameObject Dying, GameObject Killer, ref string Reason, ref string ThirdPersonReason, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Killer) && Dying.HasRegisteredEvent("KilledPlayer"))
			{
				Event @event = Event.New("KilledPlayer");
				@event.SetParameter("Object", Dying);
				@event.SetParameter("Killer", Killer);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("KillerText", KillerText);
				@event.SetParameter("Reason", Reason);
				@event.SetParameter("ThirdPersonReason", ThirdPersonReason);
				@event.SetFlag("Accidental", Accidental);
				flag = Killer.FireEvent(@event);
				Reason = @event.GetStringParameter("Reason");
				ThirdPersonReason = @event.GetStringParameter("ThirdPersonReason");
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("KilledPlayer registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(ref Killer) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				KilledPlayerEvent killedPlayerEvent = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
				flag = Killer.HandleEvent(killedPlayerEvent);
				Reason = killedPlayerEvent.Reason;
				ThirdPersonReason = killedPlayerEvent.ThirdPersonReason;
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("KilledPlayer MinEvent handling", x2);
		}
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		Send(Dying, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental, KillerText);
	}
}

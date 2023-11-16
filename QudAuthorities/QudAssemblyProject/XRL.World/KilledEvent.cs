using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class KilledEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<KilledEvent> Pool;

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

	static KilledEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(KilledEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public KilledEvent()
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

	public static KilledEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static KilledEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		KilledEvent killedEvent = FromPool();
		killedEvent.Dying = Dying;
		killedEvent.Killer = Killer;
		killedEvent.Weapon = Weapon;
		killedEvent.Projectile = Projectile;
		killedEvent.Accidental = Accidental;
		killedEvent.KillerText = KillerText;
		killedEvent.Reason = Reason;
		killedEvent.ThirdPersonReason = ThirdPersonReason;
		return killedEvent;
	}

	public static void Send(GameObject Dying, GameObject Killer, ref string Reason, ref string ThirdPersonReason, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Killer) && Killer.HasRegisteredEvent("Killed"))
			{
				Event @event = Event.New("Killed");
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
			MetricsManager.LogError("Killed registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(ref Killer) && Killer.WantEvent(ID, MinEvent.CascadeLevel))
			{
				KilledEvent killedEvent = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
				flag = Killer.HandleEvent(killedEvent);
				Reason = killedEvent.Reason;
				ThirdPersonReason = killedEvent.ThirdPersonReason;
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("Killed MinEvent handling", x2);
		}
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		Send(Dying, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental, KillerText);
	}
}

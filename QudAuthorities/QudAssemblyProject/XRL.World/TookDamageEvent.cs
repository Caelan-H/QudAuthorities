using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TookDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<TookDamageEvent> Pool;

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

	static TookDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TookDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TookDamageEvent()
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

	public static TookDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("TookDamage"))
		{
			Event @event = Event.New("TookDamage");
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Defender", Object);
			@event.SetParameter("Owner", Actor);
			@event.SetParameter("Attacker", Actor);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Weapon", Weapon);
			@event.SetParameter("Projectile", Projectile);
			@event.SetFlag("Indirect", Indirect);
			ParentEvent?.PreprocessChildEvent(@event);
			flag = Object.FireEvent(@event, ParentEvent);
			ParentEvent?.ProcessChildEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			TookDamageEvent tookDamageEvent = FromPool();
			tookDamageEvent.Damage = Damage;
			tookDamageEvent.Object = Object;
			tookDamageEvent.Actor = Actor;
			tookDamageEvent.Source = Source;
			tookDamageEvent.Weapon = Weapon;
			tookDamageEvent.Projectile = Projectile;
			tookDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(tookDamageEvent);
			flag = Object.HandleEvent(tookDamageEvent);
			ParentEvent?.ProcessChildEvent(tookDamageEvent);
		}
	}
}

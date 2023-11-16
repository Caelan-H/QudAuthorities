using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeTookDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<BeforeTookDamageEvent> Pool;

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

	static BeforeTookDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeTookDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeTookDamageEvent()
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

	public static BeforeTookDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BeforeTookDamage"))
		{
			Event @event = Event.New("BeforeTookDamage");
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Defender", Object);
			@event.SetParameter("Owner", Actor);
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
			BeforeTookDamageEvent beforeTookDamageEvent = FromPool();
			beforeTookDamageEvent.Damage = Damage;
			beforeTookDamageEvent.Object = Object;
			beforeTookDamageEvent.Actor = Actor;
			beforeTookDamageEvent.Source = Source;
			beforeTookDamageEvent.Weapon = Weapon;
			beforeTookDamageEvent.Projectile = Projectile;
			beforeTookDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(beforeTookDamageEvent);
			flag = Object.HandleEvent(beforeTookDamageEvent);
			ParentEvent?.ProcessChildEvent(beforeTookDamageEvent);
		}
	}
}

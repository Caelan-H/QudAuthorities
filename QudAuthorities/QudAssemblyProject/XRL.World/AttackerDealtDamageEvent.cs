using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AttackerDealtDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<AttackerDealtDamageEvent> Pool;

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

	static AttackerDealtDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AttackerDealtDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AttackerDealtDamageEvent()
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

	public static AttackerDealtDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("AttackerDealtDamage"))
		{
			Event @event = Event.New("AttackerDealtDamage");
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Object", Object);
			@event.SetParameter("Owner", Actor);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Weapon", Weapon);
			@event.SetParameter("Projectile", Projectile);
			@event.SetFlag("Indirect", Indirect);
			ParentEvent?.PreprocessChildEvent(@event);
			flag = Actor.FireEvent(@event, ParentEvent);
			ParentEvent?.ProcessChildEvent(@event);
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AttackerDealtDamageEvent attackerDealtDamageEvent = FromPool();
			attackerDealtDamageEvent.Damage = Damage;
			attackerDealtDamageEvent.Object = Object;
			attackerDealtDamageEvent.Actor = Actor;
			attackerDealtDamageEvent.Source = Source;
			attackerDealtDamageEvent.Weapon = Weapon;
			attackerDealtDamageEvent.Projectile = Projectile;
			attackerDealtDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(attackerDealtDamageEvent);
			flag = Actor.HandleEvent(attackerDealtDamageEvent);
			ParentEvent?.ProcessChildEvent(attackerDealtDamageEvent);
		}
	}
}

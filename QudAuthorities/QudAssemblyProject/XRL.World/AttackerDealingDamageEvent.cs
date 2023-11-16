using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AttackerDealingDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<AttackerDealingDamageEvent> Pool;

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

	static AttackerDealingDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AttackerDealingDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AttackerDealingDamageEvent()
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

	public static AttackerDealingDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("AttackerDealingDamage"))
		{
			Event @event = Event.New("AttackerDealingDamage");
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
			AttackerDealingDamageEvent attackerDealingDamageEvent = FromPool();
			attackerDealingDamageEvent.Damage = Damage;
			attackerDealingDamageEvent.Object = Object;
			attackerDealingDamageEvent.Actor = Actor;
			attackerDealingDamageEvent.Source = Source;
			attackerDealingDamageEvent.Weapon = Weapon;
			attackerDealingDamageEvent.Projectile = Projectile;
			attackerDealingDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(attackerDealingDamageEvent);
			flag = Actor.HandleEvent(attackerDealingDamageEvent);
			ParentEvent?.ProcessChildEvent(attackerDealingDamageEvent);
		}
		return flag;
	}
}

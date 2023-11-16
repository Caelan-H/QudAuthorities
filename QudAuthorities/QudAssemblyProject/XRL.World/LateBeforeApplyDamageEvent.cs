using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class LateBeforeApplyDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<LateBeforeApplyDamageEvent> Pool;

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

	static LateBeforeApplyDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(LateBeforeApplyDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public LateBeforeApplyDamageEvent()
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

	public static LateBeforeApplyDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("LateBeforeApplyDamage"))
		{
			Event @event = Event.New("LateBeforeApplyDamage");
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Object", Object);
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
			LateBeforeApplyDamageEvent lateBeforeApplyDamageEvent = FromPool();
			lateBeforeApplyDamageEvent.Damage = Damage;
			lateBeforeApplyDamageEvent.Object = Object;
			lateBeforeApplyDamageEvent.Actor = Actor;
			lateBeforeApplyDamageEvent.Source = Source;
			lateBeforeApplyDamageEvent.Weapon = Weapon;
			lateBeforeApplyDamageEvent.Projectile = Projectile;
			lateBeforeApplyDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(lateBeforeApplyDamageEvent);
			flag = Object.HandleEvent(lateBeforeApplyDamageEvent);
			ParentEvent?.ProcessChildEvent(lateBeforeApplyDamageEvent);
		}
		return flag;
	}
}
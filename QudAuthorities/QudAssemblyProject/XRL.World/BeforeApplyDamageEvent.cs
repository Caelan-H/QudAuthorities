using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeApplyDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<BeforeApplyDamageEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

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

	static BeforeApplyDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeApplyDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeApplyDamageEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static BeforeApplyDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BeforeApplyDamage"))
		{
			Event @event = Event.New("BeforeApplyDamage");
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
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			BeforeApplyDamageEvent beforeApplyDamageEvent = FromPool();
			beforeApplyDamageEvent.Damage = Damage;
			beforeApplyDamageEvent.Object = Object;
			beforeApplyDamageEvent.Actor = Actor;
			beforeApplyDamageEvent.Source = Source;
			beforeApplyDamageEvent.Weapon = Weapon;
			beforeApplyDamageEvent.Projectile = Projectile;
			beforeApplyDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(beforeApplyDamageEvent);
			flag = Object.HandleEvent(beforeApplyDamageEvent);
			ParentEvent?.ProcessChildEvent(beforeApplyDamageEvent);
		}
		return flag;
	}
}

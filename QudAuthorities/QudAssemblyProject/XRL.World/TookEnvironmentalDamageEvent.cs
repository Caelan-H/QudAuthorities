using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TookEnvironmentalDamageEvent : IDamageEvent
{
	public new static readonly int ID;

	private static List<TookEnvironmentalDamageEvent> Pool;

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

	static TookEnvironmentalDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TookEnvironmentalDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TookEnvironmentalDamageEvent()
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

	public static TookEnvironmentalDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("TookEnvironmentalDamage"))
		{
			Event @event = Event.New("TookEnvironmentalDamage");
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Defender", Object);
			@event.SetParameter("Owner", Actor);
			@event.SetParameter("Source", Source);
			@event.SetFlag("Indirect", Indirect);
			ParentEvent?.PreprocessChildEvent(@event);
			flag = Object.FireEvent(@event, ParentEvent);
			ParentEvent?.ProcessChildEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			TookEnvironmentalDamageEvent tookEnvironmentalDamageEvent = FromPool();
			tookEnvironmentalDamageEvent.Damage = Damage;
			tookEnvironmentalDamageEvent.Object = Object;
			tookEnvironmentalDamageEvent.Actor = Actor;
			tookEnvironmentalDamageEvent.Source = Source;
			tookEnvironmentalDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(tookEnvironmentalDamageEvent);
			flag = Object.HandleEvent(tookEnvironmentalDamageEvent);
			ParentEvent?.ProcessChildEvent(tookEnvironmentalDamageEvent);
		}
	}
}

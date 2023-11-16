using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeReplicatedEvent : IReplicationEvent
{
	public new static readonly int ID;

	private static List<CanBeReplicatedEvent> Pool;

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

	static CanBeReplicatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeReplicatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeReplicatedEvent()
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

	public static CanBeReplicatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanBeReplicatedEvent FromPool(GameObject Object, GameObject Actor, string Context = null)
	{
		CanBeReplicatedEvent canBeReplicatedEvent = FromPool();
		canBeReplicatedEvent.Object = Object;
		canBeReplicatedEvent.Actor = Actor;
		canBeReplicatedEvent.Context = Context;
		return canBeReplicatedEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, string Context = null)
	{
		bool result = true;
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("CanBeReplicated"))
		{
			Event @event = Event.New("CanBeReplicated");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Context", Context);
			result = Object.FireEvent(@event);
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			result = Object.HandleEvent(FromPool(Object, Actor));
		}
		return result;
	}
}

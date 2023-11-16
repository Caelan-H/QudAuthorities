using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class WasReplicatedEvent : IReplicationEvent
{
	public GameObject Replica;

	public new static readonly int ID;

	private static List<WasReplicatedEvent> Pool;

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

	static WasReplicatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(WasReplicatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public WasReplicatedEvent()
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

	public static WasReplicatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Replica = null;
		base.Reset();
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Replica, string Context = null)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("WasReplicated"))
		{
			Event @event = Event.New("WasReplicated");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Context", Context);
			@event.SetParameter("Replica", Replica);
			Object.FireEvent(@event);
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			WasReplicatedEvent wasReplicatedEvent = FromPool();
			wasReplicatedEvent.Object = Object;
			wasReplicatedEvent.Actor = Actor;
			wasReplicatedEvent.Replica = Replica;
			wasReplicatedEvent.Context = Context;
			Object.HandleEvent(wasReplicatedEvent);
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID;

	private static List<ObjectCreatedEvent> Pool;

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

	static ObjectCreatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectCreatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectCreatedEvent()
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

	public static ObjectCreatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Process(GameObject Object, string Context, ref GameObject ReplacementObject)
	{
		if (true && Object.HasRegisteredEvent("ObjectCreated"))
		{
			Event @event = Event.New("ObjectCreated");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Context", Context);
			@event.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(@event);
			ReplacementObject = @event.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ObjectCreatedEvent objectCreatedEvent = FromPool();
			objectCreatedEvent.Object = Object;
			objectCreatedEvent.Context = Context;
			objectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(objectCreatedEvent);
			ReplacementObject = objectCreatedEvent.ReplacementObject;
		}
	}
}

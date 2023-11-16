using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID;

	private static List<BeforeObjectCreatedEvent> Pool;

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

	static BeforeObjectCreatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeObjectCreatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeObjectCreatedEvent()
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

	public static BeforeObjectCreatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Process(GameObject Object, string Context, ref GameObject ReplacementObject)
	{
		if (true && Object.HasRegisteredEvent("BeforeObjectCreated"))
		{
			Event @event = Event.New("BeforeObjectCreated");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Context", Context);
			@event.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(@event);
			ReplacementObject = @event.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			BeforeObjectCreatedEvent beforeObjectCreatedEvent = FromPool();
			beforeObjectCreatedEvent.Object = Object;
			beforeObjectCreatedEvent.Context = Context;
			beforeObjectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(beforeObjectCreatedEvent);
			ReplacementObject = beforeObjectCreatedEvent.ReplacementObject;
		}
	}
}

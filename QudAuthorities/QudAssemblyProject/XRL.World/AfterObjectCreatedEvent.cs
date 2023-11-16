using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID;

	private static List<AfterObjectCreatedEvent> Pool;

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

	static AfterObjectCreatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterObjectCreatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterObjectCreatedEvent()
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

	public static AfterObjectCreatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Process(GameObject Object, string Context, ref GameObject ReplacementObject)
	{
		if (true && Object.HasRegisteredEvent("AfterObjectCreated"))
		{
			Event @event = Event.New("AfterObjectCreated");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Context", Context);
			@event.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(@event);
			ReplacementObject = @event.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterObjectCreatedEvent afterObjectCreatedEvent = FromPool();
			afterObjectCreatedEvent.Object = Object;
			afterObjectCreatedEvent.Context = Context;
			afterObjectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(afterObjectCreatedEvent);
			ReplacementObject = afterObjectCreatedEvent.ReplacementObject;
		}
	}
}

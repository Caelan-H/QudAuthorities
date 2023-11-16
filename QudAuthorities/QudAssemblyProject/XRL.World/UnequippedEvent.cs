using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UnequippedEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<UnequippedEvent> Pool;

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

	static UnequippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UnequippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UnequippedEvent()
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

	public static UnequippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Object, GameObject WasEquippedBy)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("Unequipped"))
		{
			Event @event = new Event("Unequipped");
			@event.SetParameter("UnequippingObject", WasEquippedBy);
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", WasEquippedBy);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			UnequippedEvent unequippedEvent = FromPool();
			unequippedEvent.Actor = WasEquippedBy;
			unequippedEvent.Item = Object;
			flag = Object.HandleEvent(unequippedEvent);
		}
	}
}

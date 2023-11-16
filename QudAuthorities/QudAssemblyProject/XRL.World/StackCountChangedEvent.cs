using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class StackCountChangedEvent : MinEvent
{
	public GameObject Object;

	public int OldValue;

	public int NewValue;

	public new static readonly int ID;

	private static List<StackCountChangedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static StackCountChangedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(StackCountChangedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public StackCountChangedEvent()
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

	public static StackCountChangedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Object = null;
		OldValue = 0;
		NewValue = 0;
		base.Reset();
	}

	public static void Send(GameObject Object, int OldValue, int NewValue)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("StackCountChanged"))
		{
			Event @event = Event.New("StackCountChanged");
			@event.SetParameter("Object", Object);
			@event.SetParameter("OldValue", OldValue);
			@event.SetParameter("NewValue", NewValue);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			StackCountChangedEvent stackCountChangedEvent = FromPool();
			stackCountChangedEvent.Object = Object;
			stackCountChangedEvent.OldValue = OldValue;
			stackCountChangedEvent.NewValue = NewValue;
			flag = Object.HandleEvent(stackCountChangedEvent);
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsOverloadableEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<IsOverloadableEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IsOverloadableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsOverloadableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsOverloadableEvent()
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

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static IsOverloadableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsOverloadable"))
		{
			Event @event = Event.New("IsOverloadable");
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			IsOverloadableEvent isOverloadableEvent = FromPool();
			isOverloadableEvent.Object = Object;
			flag = Object.HandleEvent(isOverloadableEvent);
		}
		return !flag;
	}
}

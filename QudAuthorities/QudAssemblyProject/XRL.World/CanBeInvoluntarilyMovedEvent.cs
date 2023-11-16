using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeInvoluntarilyMovedEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CanBeInvoluntarilyMovedEvent> Pool;

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

	static CanBeInvoluntarilyMovedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeInvoluntarilyMovedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeInvoluntarilyMovedEvent()
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

	public static CanBeInvoluntarilyMovedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanBeInvoluntarilyMovedEvent FromPool(GameObject Object)
	{
		CanBeInvoluntarilyMovedEvent canBeInvoluntarilyMovedEvent = FromPool();
		canBeInvoluntarilyMovedEvent.Object = Object;
		return canBeInvoluntarilyMovedEvent;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("CanBeInvoluntarilyMoved"))
		{
			Event @event = Event.New("CanBeInvoluntarilyMoved");
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object));
		}
		return flag;
	}
}

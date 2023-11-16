using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsRootedInPlaceEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<IsRootedInPlaceEvent> Pool;

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

	static IsRootedInPlaceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsRootedInPlaceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsRootedInPlaceEvent()
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

	public static IsRootedInPlaceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static IsRootedInPlaceEvent FromPool(GameObject Object)
	{
		IsRootedInPlaceEvent isRootedInPlaceEvent = FromPool();
		isRootedInPlaceEvent.Object = Object;
		return isRootedInPlaceEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsRootedInPlace"))
		{
			Event @event = Event.New("IsRootedInPlace");
			@event.SetParameter("Object", Object);
			if (!Object.FireEvent(@event))
			{
				return true;
			}
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return true;
		}
		return false;
	}
}

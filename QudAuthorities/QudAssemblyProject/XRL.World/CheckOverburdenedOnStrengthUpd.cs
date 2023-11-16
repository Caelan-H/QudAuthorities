using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckOverburdenedOnStrengthUpdateEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CheckOverburdenedOnStrengthUpdateEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckOverburdenedOnStrengthUpdateEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckOverburdenedOnStrengthUpdateEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckOverburdenedOnStrengthUpdateEvent()
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

	public static CheckOverburdenedOnStrengthUpdateEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckOverburdenedOnStrengthUpdateEvent FromPool(GameObject Object)
	{
		CheckOverburdenedOnStrengthUpdateEvent checkOverburdenedOnStrengthUpdateEvent = FromPool();
		checkOverburdenedOnStrengthUpdateEvent.Object = Object;
		return checkOverburdenedOnStrengthUpdateEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("CheckOverburdenedOnStrengthUpdate"))
		{
			Event @event = Event.New("CheckOverburdenedOnStrengthUpdate");
			@event.SetParameter("Object", Object);
			if (!Object.FireEvent(@event))
			{
				return false;
			}
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}

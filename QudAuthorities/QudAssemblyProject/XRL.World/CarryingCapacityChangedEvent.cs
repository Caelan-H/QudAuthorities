using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CarryingCapacityChangedEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CarryingCapacityChangedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CarryingCapacityChangedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CarryingCapacityChangedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CarryingCapacityChangedEvent()
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

	public static CarryingCapacityChangedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CarryingCapacityChangedEvent FromPool(GameObject Object)
	{
		CarryingCapacityChangedEvent carryingCapacityChangedEvent = FromPool();
		carryingCapacityChangedEvent.Object = Object;
		return carryingCapacityChangedEvent;
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}

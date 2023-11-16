using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class FrozeEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<FrozeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static FrozeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(FrozeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public FrozeEvent()
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

	public static FrozeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static FrozeEvent FromPool(GameObject Object)
	{
		FrozeEvent frozeEvent = FromPool();
		frozeEvent.Object = Object;
		return frozeEvent;
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
		if (Object.HasRegisteredEvent("Froze"))
		{
			Object.FireEvent(Event.New("Froze", "Object", Object));
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BootSequenceInitializedEvent : IBootSequenceEvent
{
	public new static readonly int ID;

	private static List<BootSequenceInitializedEvent> Pool;

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

	static BootSequenceInitializedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BootSequenceInitializedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BootSequenceInitializedEvent()
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

	public static BootSequenceInitializedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BootSequenceInitializedEvent FromPool(GameObject Object)
	{
		BootSequenceInitializedEvent bootSequenceInitializedEvent = FromPool();
		bootSequenceInitializedEvent.Object = Object;
		return bootSequenceInitializedEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceInitialized") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}

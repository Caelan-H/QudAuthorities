using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BootSequenceAbortedEvent : IBootSequenceEvent
{
	public new static readonly int ID;

	private static List<BootSequenceAbortedEvent> Pool;

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

	static BootSequenceAbortedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BootSequenceAbortedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BootSequenceAbortedEvent()
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

	public static BootSequenceAbortedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BootSequenceAbortedEvent FromPool(GameObject Object)
	{
		BootSequenceAbortedEvent bootSequenceAbortedEvent = FromPool();
		bootSequenceAbortedEvent.Object = Object;
		return bootSequenceAbortedEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceAborted") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}

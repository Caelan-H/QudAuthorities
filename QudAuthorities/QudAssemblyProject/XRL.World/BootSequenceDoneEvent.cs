using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BootSequenceDoneEvent : IBootSequenceEvent
{
	public new static readonly int ID;

	private static List<BootSequenceDoneEvent> Pool;

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

	static BootSequenceDoneEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BootSequenceDoneEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BootSequenceDoneEvent()
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

	public static BootSequenceDoneEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BootSequenceDoneEvent FromPool(GameObject Object)
	{
		BootSequenceDoneEvent bootSequenceDoneEvent = FromPool();
		bootSequenceDoneEvent.Object = Object;
		return bootSequenceDoneEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceDone") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}

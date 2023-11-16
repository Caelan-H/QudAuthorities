using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetExtrinsicValueEvent : IValueEvent
{
	public new static readonly int ID;

	private static List<GetExtrinsicValueEvent> Pool;

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

	static GetExtrinsicValueEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetExtrinsicValueEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetExtrinsicValueEvent()
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

	public static GetExtrinsicValueEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetExtrinsicValueEvent FromPool(GameObject Object, double Value)
	{
		GetExtrinsicValueEvent getExtrinsicValueEvent = FromPool();
		getExtrinsicValueEvent.Object = Object;
		getExtrinsicValueEvent.Value = Value;
		return getExtrinsicValueEvent;
	}

	public static GetExtrinsicValueEvent FromPool(IValueEvent PE)
	{
		GetExtrinsicValueEvent getExtrinsicValueEvent = FromPool();
		getExtrinsicValueEvent.Object = PE.Object;
		getExtrinsicValueEvent.Value = PE.Value;
		return getExtrinsicValueEvent;
	}
}

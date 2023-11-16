using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetExtrinsicWeightEvent : IWeightEvent
{
	public new static readonly int ID;

	private static List<GetExtrinsicWeightEvent> Pool;

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

	static GetExtrinsicWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetExtrinsicWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetExtrinsicWeightEvent()
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

	public static GetExtrinsicWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetExtrinsicWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		GetExtrinsicWeightEvent getExtrinsicWeightEvent = FromPool();
		getExtrinsicWeightEvent.Object = Object;
		getExtrinsicWeightEvent.BaseWeight = BaseWeight;
		getExtrinsicWeightEvent.Weight = Weight;
		return getExtrinsicWeightEvent;
	}

	public static GetExtrinsicWeightEvent FromPool(IWeightEvent PE)
	{
		GetExtrinsicWeightEvent getExtrinsicWeightEvent = FromPool();
		getExtrinsicWeightEvent.Object = PE.Object;
		getExtrinsicWeightEvent.BaseWeight = PE.BaseWeight;
		getExtrinsicWeightEvent.Weight = PE.Weight;
		return getExtrinsicWeightEvent;
	}
}

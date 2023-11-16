using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetIntrinsicWeightEvent : IWeightEvent
{
	public new static readonly int ID;

	private static List<GetIntrinsicWeightEvent> Pool;

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

	static GetIntrinsicWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetIntrinsicWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetIntrinsicWeightEvent()
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

	public static GetIntrinsicWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		GetIntrinsicWeightEvent getIntrinsicWeightEvent = FromPool();
		getIntrinsicWeightEvent.Object = Object;
		getIntrinsicWeightEvent.BaseWeight = BaseWeight;
		getIntrinsicWeightEvent.Weight = Weight;
		return getIntrinsicWeightEvent;
	}

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, double Weight)
	{
		GetIntrinsicWeightEvent getIntrinsicWeightEvent = FromPool();
		getIntrinsicWeightEvent.Object = Object;
		getIntrinsicWeightEvent.BaseWeight = Weight;
		getIntrinsicWeightEvent.Weight = Weight;
		return getIntrinsicWeightEvent;
	}

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, int Weight)
	{
		return FromPool(Object, (double)Weight);
	}
}

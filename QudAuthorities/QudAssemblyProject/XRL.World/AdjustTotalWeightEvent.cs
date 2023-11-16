using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AdjustTotalWeightEvent : IWeightEvent
{
	public new static readonly int ID;

	private static List<AdjustTotalWeightEvent> Pool;

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

	static AdjustTotalWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AdjustTotalWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AdjustTotalWeightEvent()
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

	public static AdjustTotalWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AdjustTotalWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		AdjustTotalWeightEvent adjustTotalWeightEvent = FromPool();
		adjustTotalWeightEvent.Object = Object;
		adjustTotalWeightEvent.BaseWeight = BaseWeight;
		adjustTotalWeightEvent.Weight = Weight;
		return adjustTotalWeightEvent;
	}

	public static AdjustTotalWeightEvent FromPool(IWeightEvent PE)
	{
		AdjustTotalWeightEvent adjustTotalWeightEvent = FromPool();
		adjustTotalWeightEvent.Object = PE.Object;
		adjustTotalWeightEvent.BaseWeight = PE.BaseWeight;
		adjustTotalWeightEvent.Weight = PE.Weight;
		return adjustTotalWeightEvent;
	}

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}
}

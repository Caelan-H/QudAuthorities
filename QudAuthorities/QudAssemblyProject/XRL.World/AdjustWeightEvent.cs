using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AdjustWeightEvent : IWeightEvent
{
	public new static readonly int ID;

	private static List<AdjustWeightEvent> Pool;

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

	static AdjustWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AdjustWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AdjustWeightEvent()
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

	public static AdjustWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AdjustWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		AdjustWeightEvent adjustWeightEvent = FromPool();
		adjustWeightEvent.Object = Object;
		adjustWeightEvent.BaseWeight = BaseWeight;
		adjustWeightEvent.Weight = Weight;
		return adjustWeightEvent;
	}

	public static AdjustWeightEvent FromPool(IWeightEvent PE)
	{
		AdjustWeightEvent adjustWeightEvent = FromPool();
		adjustWeightEvent.Object = PE.Object;
		adjustWeightEvent.BaseWeight = PE.BaseWeight;
		adjustWeightEvent.Weight = PE.Weight;
		return adjustWeightEvent;
	}

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}
}

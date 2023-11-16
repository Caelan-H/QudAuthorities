using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AdjustValueEvent : IValueEvent
{
	public new static readonly int ID;

	private static List<AdjustValueEvent> Pool;

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

	static AdjustValueEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AdjustValueEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AdjustValueEvent()
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

	public static AdjustValueEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AdjustValueEvent FromPool(GameObject Object, double Value)
	{
		AdjustValueEvent adjustValueEvent = FromPool();
		adjustValueEvent.Object = Object;
		adjustValueEvent.Value = Value;
		return adjustValueEvent;
	}

	public static AdjustValueEvent FromPool(IValueEvent PE)
	{
		AdjustValueEvent adjustValueEvent = FromPool();
		adjustValueEvent.Object = PE.Object;
		adjustValueEvent.Value = PE.Value;
		return adjustValueEvent;
	}

	public void AdjustValue(double Factor)
	{
		Value *= Factor;
	}
}

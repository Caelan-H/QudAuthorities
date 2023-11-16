using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetKineticResistanceEvent : MinEvent
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	private static List<GetKineticResistanceEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetKineticResistanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetKineticResistanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetKineticResistanceEvent()
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

	public static GetKineticResistanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetKineticResistanceEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetKineticResistanceEvent getKineticResistanceEvent = FromPool();
		getKineticResistanceEvent.Object = Object;
		getKineticResistanceEvent.Base = Base;
		getKineticResistanceEvent.PercentageIncrease = PercentageIncrease;
		getKineticResistanceEvent.LinearIncrease = LinearIncrease;
		getKineticResistanceEvent.PercentageReduction = PercentageReduction;
		getKineticResistanceEvent.LinearReduction = LinearReduction;
		return getKineticResistanceEvent;
	}

	public override void Reset()
	{
		Object = null;
		Base = 0;
		LinearIncrease = 0;
		PercentageIncrease = 0;
		LinearReduction = 0;
		PercentageReduction = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int num = Object.Weight + Object.GetIntProperty("Anchoring");
		int val;
		if (Object != null && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetKineticResistanceEvent getKineticResistanceEvent = FromPool(Object, num, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getKineticResistanceEvent);
			val = (num + getKineticResistanceEvent.LinearIncrease) * (100 + getKineticResistanceEvent.PercentageIncrease) * (100 - getKineticResistanceEvent.PercentageReduction) / 10000 - getKineticResistanceEvent.LinearReduction;
		}
		else
		{
			val = (num + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
		}
		return Math.Max(val, 0);
	}
}

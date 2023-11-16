using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSprintDurationEvent : MinEvent
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	private static List<GetSprintDurationEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetSprintDurationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSprintDurationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSprintDurationEvent()
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

	public static GetSprintDurationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetSprintDurationEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetSprintDurationEvent getSprintDurationEvent = FromPool();
		getSprintDurationEvent.Object = Object;
		getSprintDurationEvent.Base = Base;
		getSprintDurationEvent.PercentageIncrease = PercentageIncrease;
		getSprintDurationEvent.LinearIncrease = LinearIncrease;
		getSprintDurationEvent.PercentageReduction = PercentageReduction;
		getSprintDurationEvent.LinearReduction = LinearReduction;
		return getSprintDurationEvent;
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

	public static int GetFor(GameObject Object, int Base = 0, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int val;
		if (Object != null && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetSprintDurationEvent getSprintDurationEvent = FromPool(Object, Base, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getSprintDurationEvent);
			val = (Base + getSprintDurationEvent.LinearIncrease) * (100 + getSprintDurationEvent.PercentageIncrease) * (100 - getSprintDurationEvent.PercentageReduction) / 10000 - getSprintDurationEvent.LinearReduction;
		}
		else
		{
			val = (Base + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
		}
		return Math.Max(val, 0);
	}
}

using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSpringinessEvent : MinEvent
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	private static List<GetSpringinessEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetSpringinessEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSpringinessEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSpringinessEvent()
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

	public static GetSpringinessEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetSpringinessEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetSpringinessEvent getSpringinessEvent = FromPool();
		getSpringinessEvent.Object = Object;
		getSpringinessEvent.Base = Base;
		getSpringinessEvent.PercentageIncrease = PercentageIncrease;
		getSpringinessEvent.LinearIncrease = LinearIncrease;
		getSpringinessEvent.PercentageReduction = PercentageReduction;
		getSpringinessEvent.LinearReduction = LinearReduction;
		return getSpringinessEvent;
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
		int intProperty = Object.GetIntProperty("Springiness");
		int val;
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetSpringinessEvent getSpringinessEvent = FromPool(Object, intProperty, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getSpringinessEvent);
			val = (intProperty + getSpringinessEvent.LinearIncrease) * (100 + getSpringinessEvent.PercentageIncrease) * (100 - getSpringinessEvent.PercentageReduction) / 10000 - getSpringinessEvent.LinearReduction;
		}
		else
		{
			val = (intProperty + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
		}
		return Math.Max(val, 0);
	}
}

using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMaximumLiquidExposureEvent : MinEvent
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	private static List<GetMaximumLiquidExposureEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMaximumLiquidExposureEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetMaximumLiquidExposureEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMaximumLiquidExposureEvent()
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

	public static GetMaximumLiquidExposureEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMaximumLiquidExposureEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = FromPool();
		getMaximumLiquidExposureEvent.Object = Object;
		getMaximumLiquidExposureEvent.Base = Base;
		getMaximumLiquidExposureEvent.PercentageIncrease = PercentageIncrease;
		getMaximumLiquidExposureEvent.LinearIncrease = LinearIncrease;
		getMaximumLiquidExposureEvent.PercentageReduction = PercentageReduction;
		getMaximumLiquidExposureEvent.LinearReduction = LinearReduction;
		return getMaximumLiquidExposureEvent;
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
		int @base = GetBase(Object);
		int val;
		if (Object != null && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = FromPool(Object, @base, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getMaximumLiquidExposureEvent);
			val = (int)Math.Round((double)(@base + getMaximumLiquidExposureEvent.LinearIncrease) * (100.0 + (double)getMaximumLiquidExposureEvent.PercentageIncrease) * (100.0 - (double)getMaximumLiquidExposureEvent.PercentageReduction) / 10000.0) - getMaximumLiquidExposureEvent.LinearReduction;
		}
		else
		{
			val = (int)Math.Round((double)(@base + LinearIncrease) * (100.0 + (double)PercentageIncrease) * (100.0 - (double)PercentageReduction) / 10000.0) - LinearReduction;
		}
		return Math.Max(val, 0);
	}

	public static double GetDoubleFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		double doubleBase = GetDoubleBase(Object);
		double val;
		if (Object != null && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = FromPool(Object, (int)Math.Round(doubleBase), PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getMaximumLiquidExposureEvent);
			val = (doubleBase + (double)getMaximumLiquidExposureEvent.LinearIncrease) * (double)(100 + getMaximumLiquidExposureEvent.PercentageIncrease) * (double)(100 - getMaximumLiquidExposureEvent.PercentageReduction) / 10000.0 - (double)getMaximumLiquidExposureEvent.LinearReduction;
		}
		else
		{
			val = (doubleBase + (double)LinearIncrease) * (double)(100 + PercentageIncrease) * (double)(100 - PercentageReduction) / 10000.0 - (double)LinearReduction;
		}
		return Math.Max(val, 0.0);
	}

	public static int GetBase(GameObject obj)
	{
		if (obj == null)
		{
			return 0;
		}
		if (obj.HasTag("Creature"))
		{
			return Math.Max(obj.Stat("Strength") + obj.Stat("Toughness") + obj.GetConcreteBodyPartCount(), 1);
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return 0;
		}
		return (int)Math.Round(Math.Max(obj.GetIntrinsicWeight().DiminishingReturns(1.0), 1.0));
	}

	public static double GetDoubleBase(GameObject obj)
	{
		if (obj == null)
		{
			return 0.0;
		}
		if (obj.HasTag("Creature"))
		{
			return Math.Max(obj.Stat("Strength") + obj.Stat("Toughness") + obj.GetConcreteBodyPartCount(), 1);
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return 0.0;
		}
		return Math.Max(obj.GetIntrinsicWeight().DiminishingReturns(1.0), 1.0);
	}
}

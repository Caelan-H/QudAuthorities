using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetTradePerformanceEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Trader;

	public int BaseRating;

	public double LinearAdjustment;

	public double FactorAdjustment;

	public new static readonly int ID;

	private static List<GetTradePerformanceEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetTradePerformanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetTradePerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetTradePerformanceEvent()
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

	public static GetTradePerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetTradePerformanceEvent FromPool(GameObject Actor, GameObject Trader, int BaseRating, double LinearAdjustment = 0.0, double FactorAdjustment = 1.0)
	{
		GetTradePerformanceEvent getTradePerformanceEvent = FromPool();
		getTradePerformanceEvent.Actor = Actor;
		getTradePerformanceEvent.Trader = Trader;
		getTradePerformanceEvent.BaseRating = BaseRating;
		getTradePerformanceEvent.LinearAdjustment = LinearAdjustment;
		getTradePerformanceEvent.FactorAdjustment = FactorAdjustment;
		return getTradePerformanceEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		BaseRating = 0;
		LinearAdjustment = 0.0;
		FactorAdjustment = 1.0;
		base.Reset();
	}

	public static double GetFor(GameObject Actor, GameObject Trader)
	{
		if (Trader == null || Actor == null)
		{
			return 1.0;
		}
		if (!Actor.HasStat("Ego"))
		{
			return 0.25;
		}
		int num = Actor.StatMod("Ego");
		double num2 = 0.0;
		double num3 = 1.0;
		bool flag = true;
		if (flag && Actor.HasRegisteredEvent("GetTradePerformance"))
		{
			Event @event = Event.New("GetTradePerformance");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Trader", Trader);
			@event.SetParameter("BaseRating", num);
			@event.SetParameter("LinearAdjustment", num2);
			@event.SetParameter("FactorAdjustment", num3);
			if (!Actor.FireEvent(@event))
			{
				flag = false;
			}
			num2 = (double)@event.GetParameter("LinearAdjustment");
			num3 = (double)@event.GetParameter("FactorAdjustment");
		}
		if (flag && Actor.WantEvent(ID, CascadeLevel))
		{
			GetTradePerformanceEvent getTradePerformanceEvent = FromPool(Actor, Trader, num, num2, num3);
			if (!Actor.HandleEvent(getTradePerformanceEvent))
			{
				flag = false;
			}
			num2 = getTradePerformanceEvent.LinearAdjustment;
			num3 = getTradePerformanceEvent.FactorAdjustment;
		}
		return Math.Min(Math.Max((0.35 + 0.07 * ((double)num + num2)) * num3, 0.05), 0.95);
	}

	public static int GetRatingFor(GameObject Actor, GameObject Trader)
	{
		if (Trader == null || Actor == null)
		{
			return 0;
		}
		if (!Actor.HasStat("Ego"))
		{
			return -2;
		}
		int num = Actor.StatMod("Ego");
		double num2 = 0.0;
		double num3 = 1.0;
		bool flag = true;
		if (flag && Actor.HasRegisteredEvent("GetTradePerformance"))
		{
			Event @event = Event.New("GetTradePerformance");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Trader", Trader);
			@event.SetParameter("BaseRating", num);
			@event.SetParameter("LinearAdjustment", num2);
			@event.SetParameter("FactorAdjustment", num3);
			if (!Actor.FireEvent(@event))
			{
				flag = false;
			}
			num2 = (double)@event.GetParameter("LinearAdjustment");
			num3 = (double)@event.GetParameter("FactorAdjustment");
		}
		if (flag && Actor.WantEvent(ID, CascadeLevel))
		{
			GetTradePerformanceEvent getTradePerformanceEvent = FromPool(Actor, Trader, num, num2, num3);
			if (!Actor.HandleEvent(getTradePerformanceEvent))
			{
				flag = false;
			}
			num2 = getTradePerformanceEvent.LinearAdjustment;
			num3 = getTradePerformanceEvent.FactorAdjustment;
		}
		return (int)(((double)num + num2) * num3);
	}
}

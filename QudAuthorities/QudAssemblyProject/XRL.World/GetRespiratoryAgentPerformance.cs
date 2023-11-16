using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRespiratoryAgentPerformanceEvent : MinEvent
{
	public GameObject Object;

	public GameObject GasObject;

	public Gas Gas;

	public int BaseRating;

	public int LinearAdjustment;

	public int PercentageAdjustment;

	public bool WillAllowSave;

	public new static readonly int ID;

	private static List<GetRespiratoryAgentPerformanceEvent> Pool;

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

	static GetRespiratoryAgentPerformanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetRespiratoryAgentPerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetRespiratoryAgentPerformanceEvent()
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

	public static GetRespiratoryAgentPerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Object = null;
		GasObject = null;
		Gas = null;
		BaseRating = 0;
		LinearAdjustment = 0;
		PercentageAdjustment = 0;
		WillAllowSave = false;
		base.Reset();
	}

	public static int GetFor(GameObject Object, GameObject GasObject = null, Gas Gas = null, int? BaseRating = null, int LinearAdjustment = 0, int PercentageAdjustment = 0, bool WillAllowSave = false)
	{
		if (!GameObject.validate(ref Object))
		{
			return 1;
		}
		if (Gas == null && GasObject != null)
		{
			Gas = GasObject.GetPart("Gas") as Gas;
		}
		int num = BaseRating ?? Gas?.Density ?? 0;
		bool flag = true;
		if (flag)
		{
			bool flag2 = Object.HasRegisteredEvent("GetRespiratoryAgentPerformance");
			bool flag3 = GasObject?.HasRegisteredEvent("GetRespiratoryAgentPerformance") ?? false;
			if (flag2 || flag3)
			{
				Event @event = Event.New("GetRespiratoryAgentPerformance");
				@event.SetParameter("Object", Object);
				@event.SetParameter("GasObject", GasObject);
				@event.SetParameter("Gas", Gas);
				@event.SetParameter("BaseRating", num);
				@event.SetParameter("LinearAdjustment", LinearAdjustment);
				@event.SetParameter("PercentageAdjustment", PercentageAdjustment);
				@event.SetFlag("WillAllowSave", WillAllowSave);
				if (flag && flag2 && !Object.FireEvent(@event))
				{
					flag = false;
				}
				if (flag && flag3 && !GasObject.FireEvent(@event))
				{
					flag = false;
				}
				LinearAdjustment = @event.GetIntParameter("LinearAdjustment");
				PercentageAdjustment = @event.GetIntParameter("PercentageAdjustment");
			}
		}
		if (flag)
		{
			bool flag4 = Object.WantEvent(ID, CascadeLevel);
			bool flag5 = GasObject?.WantEvent(ID, CascadeLevel) ?? false;
			if (flag4 || flag5)
			{
				GetRespiratoryAgentPerformanceEvent getRespiratoryAgentPerformanceEvent = FromPool();
				getRespiratoryAgentPerformanceEvent.Object = Object;
				getRespiratoryAgentPerformanceEvent.GasObject = GasObject;
				getRespiratoryAgentPerformanceEvent.Gas = Gas;
				getRespiratoryAgentPerformanceEvent.BaseRating = num;
				getRespiratoryAgentPerformanceEvent.LinearAdjustment = LinearAdjustment;
				getRespiratoryAgentPerformanceEvent.PercentageAdjustment = PercentageAdjustment;
				getRespiratoryAgentPerformanceEvent.WillAllowSave = WillAllowSave;
				if (flag && flag4 && !Object.HandleEvent(getRespiratoryAgentPerformanceEvent))
				{
					flag = false;
				}
				if (flag && flag5 && !GasObject.HandleEvent(getRespiratoryAgentPerformanceEvent))
				{
					flag = false;
				}
				LinearAdjustment = getRespiratoryAgentPerformanceEvent.LinearAdjustment;
				PercentageAdjustment = getRespiratoryAgentPerformanceEvent.PercentageAdjustment;
			}
		}
		int num2 = num;
		if (LinearAdjustment != 0)
		{
			num2 += LinearAdjustment;
		}
		if (PercentageAdjustment != 0)
		{
			num2 = num2 * (100 + PercentageAdjustment) / 100;
		}
		return num2;
	}
}

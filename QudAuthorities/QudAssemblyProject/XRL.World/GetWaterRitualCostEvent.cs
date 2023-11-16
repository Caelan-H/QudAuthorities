using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetWaterRitualCostEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Target;

	public string Type;

	public int BaseCost;

	public int Cost;

	public new static readonly int ID;

	private static List<GetWaterRitualCostEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 3;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetWaterRitualCostEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("GetWaterRitualCostEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public GetWaterRitualCostEvent()
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

	public override void Reset()
	{
		Actor = null;
		Target = null;
		Type = null;
		BaseCost = 0;
		Cost = 0;
		base.Reset();
	}

	public static GetWaterRitualCostEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetWaterRitualCostEvent FromPool(GameObject Actor, GameObject Target, string Type, int BaseCost, int Cost)
	{
		GetWaterRitualCostEvent getWaterRitualCostEvent = FromPool();
		getWaterRitualCostEvent.Actor = Actor;
		getWaterRitualCostEvent.Target = Target;
		getWaterRitualCostEvent.Type = Type;
		getWaterRitualCostEvent.BaseCost = BaseCost;
		getWaterRitualCostEvent.Cost = Cost;
		return getWaterRitualCostEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int GetFor(GameObject Actor, GameObject Target, string Type, int BaseCost)
	{
		int num = BaseCost;
		if (Actor.HasRegisteredEvent("GetWaterRitualCost"))
		{
			Event @event = Event.New("GetWaterRitualCost", 2, 1, 2);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Target", Target);
			@event.SetParameter("Type", Type);
			@event.SetParameter("BaseCost", BaseCost);
			@event.SetParameter("Cost", num);
			if (!Actor.FireEvent(@event))
			{
				return @event.GetIntParameter("Cost");
			}
			num = @event.GetIntParameter("Cost");
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetWaterRitualCostEvent getWaterRitualCostEvent = FromPool(Actor, Target, Type, BaseCost, num);
			if (!Actor.HandleEvent(getWaterRitualCostEvent))
			{
				return getWaterRitualCostEvent.Cost;
			}
			num = getWaterRitualCostEvent.Cost;
		}
		return num;
	}
}

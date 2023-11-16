using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSwimmingPerformanceEvent : MinEvent
{
	public GameObject Actor;

	public int MoveSpeedPenalty;

	public new static readonly int ID;

	private static List<GetSwimmingPerformanceEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetSwimmingPerformanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSwimmingPerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSwimmingPerformanceEvent()
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

	public static GetSwimmingPerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetSwimmingPerformanceEvent FromPool(GameObject Actor, int MoveSpeedPenalty)
	{
		GetSwimmingPerformanceEvent getSwimmingPerformanceEvent = FromPool();
		getSwimmingPerformanceEvent.Actor = Actor;
		getSwimmingPerformanceEvent.MoveSpeedPenalty = MoveSpeedPenalty;
		return getSwimmingPerformanceEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		MoveSpeedPenalty = 0;
		base.Reset();
	}

	public static bool GetFor(GameObject Actor, ref int MoveSpeedPenalty)
	{
		bool flag = GameObject.validate(ref Actor);
		if (flag && Actor.HasRegisteredEvent("GetSwimmingPerformance"))
		{
			Event @event = Event.New("GetSwimmingPerformance", "Actor", Actor, "MoveSpeedPenalty", MoveSpeedPenalty);
			flag = Actor.FireEvent(@event);
			MoveSpeedPenalty = @event.GetIntParameter("MoveSpeedPenalty");
		}
		if (flag && Actor.WantEvent(ID, CascadeLevel))
		{
			GetSwimmingPerformanceEvent getSwimmingPerformanceEvent = FromPool(Actor, MoveSpeedPenalty);
			flag = Actor.HandleEvent(getSwimmingPerformanceEvent);
			MoveSpeedPenalty = getSwimmingPerformanceEvent.MoveSpeedPenalty;
		}
		return flag;
	}
}

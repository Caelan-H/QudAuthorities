using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetWadingPerformanceEvent : MinEvent
{
	public GameObject Actor;

	public int MoveSpeedPenalty;

	public new static readonly int ID;

	private static List<GetWadingPerformanceEvent> Pool;

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

	static GetWadingPerformanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetWadingPerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetWadingPerformanceEvent()
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

	public static GetWadingPerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetWadingPerformanceEvent FromPool(GameObject Actor, int MoveSpeedPenalty)
	{
		GetWadingPerformanceEvent getWadingPerformanceEvent = FromPool();
		getWadingPerformanceEvent.Actor = Actor;
		getWadingPerformanceEvent.MoveSpeedPenalty = MoveSpeedPenalty;
		return getWadingPerformanceEvent;
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
		if (Actor != null)
		{
			if (Actor.HasRegisteredEvent("GetWadingPerformance"))
			{
				Event @event = Event.New("GetWadingPerformance", "Actor", Actor, "MoveSpeedPenalty", MoveSpeedPenalty);
				bool num = Actor.FireEvent(@event);
				MoveSpeedPenalty = @event.GetIntParameter("MoveSpeedPenalty");
				if (!num)
				{
					return false;
				}
			}
			if (Actor.WantEvent(ID, CascadeLevel))
			{
				GetWadingPerformanceEvent getWadingPerformanceEvent = FromPool(Actor, MoveSpeedPenalty);
				bool num2 = Actor.HandleEvent(getWadingPerformanceEvent);
				MoveSpeedPenalty = getWadingPerformanceEvent.MoveSpeedPenalty;
				if (!num2)
				{
					return false;
				}
			}
		}
		return true;
	}
}

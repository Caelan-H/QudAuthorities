using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetFirefightingPerformanceEvent : MinEvent
{
	public const int FIREFIGHTING_BASE_PERFORMANCE = -100;

	public const int FIREFIGHTING_ROLLING_FACTOR = 2;

	public GameObject Actor;

	public GameObject Object;

	public int Result;

	public new static readonly int ID;

	private static List<GetFirefightingPerformanceEvent> Pool;

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

	static GetFirefightingPerformanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetFirefightingPerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetFirefightingPerformanceEvent()
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
		Object = null;
		Result = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetFirefightingPerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Actor, GameObject Object = null, bool Patting = false, bool Rolling = false)
	{
		int num = -100;
		if (Rolling)
		{
			num *= 2;
		}
		if (Object == null)
		{
			Object = Actor;
		}
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("GetFirefightingPerformance"))
		{
			Event @event = Event.New("GetFirefightingPerformance");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Object", Object);
			@event.SetParameter("Result", num);
			flag = Actor.FireEvent(@event);
			num = @event.GetIntParameter("Result");
		}
		if (flag && GameObject.validate(ref Object) && Object != Actor && Object.HasRegisteredEvent("GetFirefightingPerformance"))
		{
			Event event2 = Event.New("GetFirefightingPerformance");
			event2.SetParameter("Actor", Actor);
			event2.SetParameter("Object", Object);
			event2.SetParameter("Result", num);
			flag = Object.FireEvent(event2);
			num = event2.GetIntParameter("Result");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			GetFirefightingPerformanceEvent getFirefightingPerformanceEvent = FromPool();
			getFirefightingPerformanceEvent.Actor = Actor;
			getFirefightingPerformanceEvent.Object = Object;
			getFirefightingPerformanceEvent.Result = num;
			flag = Actor.HandleEvent(getFirefightingPerformanceEvent);
			num = getFirefightingPerformanceEvent.Result;
		}
		if (flag && GameObject.validate(ref Object) && Object != Actor && Object.WantEvent(ID, CascadeLevel))
		{
			GetFirefightingPerformanceEvent getFirefightingPerformanceEvent2 = FromPool();
			getFirefightingPerformanceEvent2.Actor = Actor;
			getFirefightingPerformanceEvent2.Object = Object;
			getFirefightingPerformanceEvent2.Result = num;
			flag = Object.HandleEvent(getFirefightingPerformanceEvent2);
			num = getFirefightingPerformanceEvent2.Result;
		}
		return num;
	}
}

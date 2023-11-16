using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AIBoredEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AIBoredEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static AIBoredEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AIBoredEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AIBoredEvent()
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
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static AIBoredEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Actor)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("AIBored"))
		{
			Event @event = Event.New("AIBored");
			@event.SetParameter("Actor", Actor);
			flag = Actor.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			AIBoredEvent aIBoredEvent = FromPool();
			aIBoredEvent.Actor = Actor;
			flag = Actor.HandleEvent(aIBoredEvent);
		}
		return flag;
	}
}

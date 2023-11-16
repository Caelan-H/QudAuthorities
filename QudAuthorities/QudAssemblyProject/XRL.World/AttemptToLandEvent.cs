using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AttemptToLandEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AttemptToLandEvent> Pool;

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

	static AttemptToLandEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AttemptToLandEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AttemptToLandEvent()
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

	public static AttemptToLandEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AttemptToLandEvent FromPool(GameObject Actor)
	{
		AttemptToLandEvent attemptToLandEvent = FromPool();
		attemptToLandEvent.Actor = Actor;
		return attemptToLandEvent;
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

	public static bool Check(GameObject Actor)
	{
		if (GameObject.validate(Actor) && Actor.HasRegisteredEvent("AttemptToLand") && !Actor.FireEvent(Event.New("AttemptToLand", "Actor", Actor)))
		{
			return true;
		}
		if (GameObject.validate(Actor) && Actor.WantEvent(ID, CascadeLevel) && !Actor.HandleEvent(FromPool(Actor)))
		{
			return true;
		}
		return false;
	}
}

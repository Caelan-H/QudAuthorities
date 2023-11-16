using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IdleQueryEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<IdleQueryEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IdleQueryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IdleQueryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IdleQueryEvent()
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

	public static IdleQueryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static IdleQueryEvent FromPool(GameObject Actor)
	{
		IdleQueryEvent idleQueryEvent = FromPool();
		idleQueryEvent.Actor = Actor;
		return idleQueryEvent;
	}

	public override void Reset()
	{
		Actor = null;
		base.Reset();
	}
}

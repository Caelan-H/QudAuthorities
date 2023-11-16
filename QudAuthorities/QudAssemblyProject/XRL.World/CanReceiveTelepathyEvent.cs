using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanReceiveTelepathyEvent : MinEvent
{
	public GameObject Object;

	public GameObject Actor;

	public new static readonly int ID;

	private static List<CanReceiveTelepathyEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanReceiveTelepathyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanReceiveTelepathyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanReceiveTelepathyEvent()
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

	public static CanReceiveTelepathyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanReceiveTelepathyEvent FromPool(GameObject Object, GameObject Actor)
	{
		CanReceiveTelepathyEvent canReceiveTelepathyEvent = FromPool();
		canReceiveTelepathyEvent.Object = Object;
		canReceiveTelepathyEvent.Actor = Actor;
		return canReceiveTelepathyEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Actor)))
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		Object = null;
		Actor = null;
		base.Reset();
	}
}

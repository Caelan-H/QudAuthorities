using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanSmartUseEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<CanSmartUseEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static CanSmartUseEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanSmartUseEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanSmartUseEvent()
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

	public static CanSmartUseEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanSmartUseEvent FromPool(GameObject Actor, GameObject Item)
	{
		CanSmartUseEvent canSmartUseEvent = FromPool();
		canSmartUseEvent.Actor = Actor;
		canSmartUseEvent.Item = Item;
		return canSmartUseEvent;
	}
}

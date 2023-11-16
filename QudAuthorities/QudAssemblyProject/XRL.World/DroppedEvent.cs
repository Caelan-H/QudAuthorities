using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DroppedEvent : IActOnItemEvent
{
	public bool Forced;

	public new static readonly int ID;

	private static List<DroppedEvent> Pool;

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

	static DroppedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(DroppedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public DroppedEvent()
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

	public new void Reset()
	{
		Forced = false;
		base.Reset();
	}

	public static DroppedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DroppedEvent FromPool(GameObject Actor, GameObject Item, bool Forced = false)
	{
		DroppedEvent droppedEvent = FromPool();
		droppedEvent.Actor = Actor;
		droppedEvent.Item = Item;
		droppedEvent.Forced = Forced;
		return droppedEvent;
	}
}

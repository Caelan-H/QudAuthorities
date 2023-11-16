using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TakenEvent : IActOnItemEvent
{
	public string Context;

	public new static readonly int ID;

	private static List<TakenEvent> Pool;

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

	static TakenEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TakenEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TakenEvent()
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
		Context = null;
		base.Reset();
	}

	public static TakenEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static TakenEvent FromPool(GameObject Actor, GameObject Item, string Context = null)
	{
		TakenEvent takenEvent = FromPool();
		takenEvent.Actor = Actor;
		takenEvent.Item = Item;
		takenEvent.Context = Context;
		return takenEvent;
	}
}

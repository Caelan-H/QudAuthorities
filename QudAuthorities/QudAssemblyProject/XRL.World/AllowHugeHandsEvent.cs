using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AllowHugeHandsEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<AllowHugeHandsEvent> Pool;

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

	static AllowHugeHandsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AllowHugeHandsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AllowHugeHandsEvent()
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

	public static AllowHugeHandsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AllowHugeHandsEvent FromPool(GameObject Actor, GameObject Item)
	{
		AllowHugeHandsEvent allowHugeHandsEvent = FromPool();
		allowHugeHandsEvent.Actor = Actor;
		allowHugeHandsEvent.Item = Item;
		return allowHugeHandsEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (Item.WantEvent(ID, MinEvent.CascadeLevel) && !Item.HandleEvent(FromPool(Actor, Item)))
		{
			return false;
		}
		if (!Item.FireEvent("AllowHugeHands"))
		{
			return false;
		}
		return true;
	}
}

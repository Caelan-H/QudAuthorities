using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AllowInventoryStackEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<AllowInventoryStackEvent> Pool;

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

	static AllowInventoryStackEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AllowInventoryStackEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AllowInventoryStackEvent()
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

	public static AllowInventoryStackEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AllowInventoryStackEvent FromPool(GameObject Actor, GameObject Item)
	{
		AllowInventoryStackEvent allowInventoryStackEvent = FromPool();
		allowInventoryStackEvent.Actor = Actor;
		allowInventoryStackEvent.Item = Item;
		return allowInventoryStackEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (GameObject.validate(ref Actor) && GameObject.validate(ref Item) && Actor.HasRegisteredEvent("AllowInventoryStack"))
		{
			Event @event = Event.New("AllowInventoryStack");
			@event.SetParameter("Object", Item);
			if (!Actor.FireEvent(@event))
			{
				return false;
			}
		}
		if (GameObject.validate(ref Actor) && GameObject.validate(ref Item) && Actor.WantEvent(ID, MinEvent.CascadeLevel) && !Actor.HandleEvent(FromPool(Actor, Item)))
		{
			return false;
		}
		return true;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PowerSwitchFlippedEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<PowerSwitchFlippedEvent> Pool;

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

	static PowerSwitchFlippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PowerSwitchFlippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PowerSwitchFlippedEvent()
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

	public static PowerSwitchFlippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PowerSwitchFlippedEvent FromPool(GameObject Actor, GameObject Item)
	{
		PowerSwitchFlippedEvent powerSwitchFlippedEvent = FromPool();
		powerSwitchFlippedEvent.Actor = Actor;
		powerSwitchFlippedEvent.Item = Item;
		return powerSwitchFlippedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item)
	{
		if (Item.HasRegisteredEvent("PowerSwitchFlipped"))
		{
			Event @event = Event.New("PowerSwitchFlipped");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Item", Item);
			if (!Item.FireEvent(@event))
			{
				return;
			}
		}
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Item.HandleEvent(FromPool(Actor, Item));
		}
	}
}

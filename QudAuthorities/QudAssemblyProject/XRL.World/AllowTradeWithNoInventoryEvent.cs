using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AllowTradeWithNoInventoryEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Trader;

	public new static readonly int ID;

	private static List<AllowTradeWithNoInventoryEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static AllowTradeWithNoInventoryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AllowTradeWithNoInventoryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AllowTradeWithNoInventoryEvent()
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
		Trader = null;
		base.Reset();
	}

	public static AllowTradeWithNoInventoryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Actor, GameObject Trader)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Trader) && Trader.HasRegisteredEvent("AllowTradeWithNoInventory"))
		{
			Event @event = Event.New("AllowTradeWithNoInventory");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Trader", Trader);
			flag = Trader.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Trader) && Trader.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AllowTradeWithNoInventoryEvent allowTradeWithNoInventoryEvent = FromPool();
			allowTradeWithNoInventoryEvent.Actor = Actor;
			allowTradeWithNoInventoryEvent.Trader = Trader;
			flag = Trader.HandleEvent(allowTradeWithNoInventoryEvent);
		}
		return !flag;
	}
}

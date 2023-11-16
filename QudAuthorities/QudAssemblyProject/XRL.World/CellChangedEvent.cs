using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CellChangedEvent : IActOnItemEvent
{
	public GameObject OldCell;

	public GameObject NewCell;

	public new static readonly int ID;

	private static List<CellChangedEvent> Pool;

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

	static CellChangedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CellChangedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CellChangedEvent()
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
		OldCell = null;
		NewCell = null;
		base.Reset();
	}

	public static CellChangedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CellChangedEvent FromPool(GameObject Actor, GameObject Item, GameObject OldCell, GameObject NewCell)
	{
		CellChangedEvent cellChangedEvent = FromPool();
		cellChangedEvent.Actor = Actor;
		cellChangedEvent.Item = Item;
		cellChangedEvent.OldCell = OldCell;
		cellChangedEvent.NewCell = NewCell;
		return cellChangedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, GameObject OldCell, GameObject NewCell)
	{
		if (Item.HasRegisteredEvent("CellChanged"))
		{
			Event @event = Event.New("CellChanged");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Item", Item);
			@event.SetParameter("OldCell", OldCell);
			@event.SetParameter("NewCell", NewCell);
			if (!Item.FireEvent(@event))
			{
				return;
			}
		}
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Item.HandleEvent(FromPool(Actor, Item, OldCell, NewCell));
		}
	}
}

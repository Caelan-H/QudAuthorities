using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AddedToInventoryEvent : IActOnItemEvent
{
	public bool Silent;

	public bool NoStack;

	public new static readonly int ID;

	private static List<AddedToInventoryEvent> Pool;

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

	static AddedToInventoryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AddedToInventoryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AddedToInventoryEvent()
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

	public static AddedToInventoryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Silent = false;
		NoStack = false;
		base.Reset();
	}

	public static void Send(GameObject Actor, GameObject Item, bool Silent = false, bool NoStack = false, IEvent ParentEvent = null)
	{
		bool flag = true;
		if (GameObject.validate(ref Item) && Item.HasRegisteredEvent("AddedToInventory"))
		{
			Event @event = Event.New("AddedToInventory");
			@event.SetParameter("TakingObject", Actor);
			@event.SetParameter("Object", Item);
			@event.SetFlag("NoStack", NoStack);
			@event.SetSilent(Silent);
			flag = Item.FireEvent(@event);
			ParentEvent?.ProcessChildEvent(@event);
		}
		if (flag && GameObject.validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AddedToInventoryEvent addedToInventoryEvent = FromPool();
			addedToInventoryEvent.Actor = Actor;
			addedToInventoryEvent.Item = Item;
			addedToInventoryEvent.Silent = Silent;
			addedToInventoryEvent.NoStack = NoStack;
			flag = Item.HandleEvent(addedToInventoryEvent);
			ParentEvent?.ProcessChildEvent(addedToInventoryEvent);
		}
	}
}

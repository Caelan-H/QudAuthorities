using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetCleaningItemsNearbyEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Item;

	public List<GameObject> Objects;

	public new static readonly int ID;

	private static List<GetCleaningItemsNearbyEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetCleaningItemsNearbyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetCleaningItemsNearbyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetCleaningItemsNearbyEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Item = null;
		Objects = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetCleaningItemsNearbyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetCleaningItemsNearbyEvent FromPool(GameObject Actor, GameObject Item, List<GameObject> Objects)
	{
		GetCleaningItemsNearbyEvent getCleaningItemsNearbyEvent = FromPool();
		getCleaningItemsNearbyEvent.Actor = Actor;
		getCleaningItemsNearbyEvent.Item = Item;
		getCleaningItemsNearbyEvent.Objects = Objects;
		return getCleaningItemsNearbyEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, ref List<GameObject> Objects)
	{
		Cell cell = Actor?.CurrentCell;
		if (cell == null)
		{
			return;
		}
		GetCleaningItemsNearbyEvent getCleaningItemsNearbyEvent = null;
		if (cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (getCleaningItemsNearbyEvent == null)
			{
				if (Objects == null)
				{
					Objects = new List<GameObject>();
				}
				getCleaningItemsNearbyEvent = FromPool(Actor, Item, Objects);
			}
			if (!cell.HandleEvent(getCleaningItemsNearbyEvent))
			{
				return;
			}
		}
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			if (!localAdjacentCell.WantEvent(ID, MinEvent.CascadeLevel))
			{
				continue;
			}
			if (getCleaningItemsNearbyEvent == null)
			{
				if (Objects == null)
				{
					Objects = new List<GameObject>();
				}
				getCleaningItemsNearbyEvent = FromPool(Actor, Item, Objects);
			}
			if (!localAdjacentCell.HandleEvent(getCleaningItemsNearbyEvent))
			{
				break;
			}
		}
	}
}

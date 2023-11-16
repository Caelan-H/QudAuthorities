using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckAnythingToCleanWithNearbyEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Item;

	public new static readonly int ID;

	private static List<CheckAnythingToCleanWithNearbyEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckAnythingToCleanWithNearbyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckAnythingToCleanWithNearbyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckAnythingToCleanWithNearbyEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Item = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static CheckAnythingToCleanWithNearbyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckAnythingToCleanWithNearbyEvent FromPool(GameObject Actor, GameObject Item)
	{
		CheckAnythingToCleanWithNearbyEvent checkAnythingToCleanWithNearbyEvent = FromPool();
		checkAnythingToCleanWithNearbyEvent.Actor = Actor;
		checkAnythingToCleanWithNearbyEvent.Item = Item;
		return checkAnythingToCleanWithNearbyEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		Cell cell = Actor?.CurrentCell;
		if (cell != null)
		{
			CheckAnythingToCleanWithNearbyEvent checkAnythingToCleanWithNearbyEvent = null;
			if (cell.WantEvent(ID, MinEvent.CascadeLevel))
			{
				if (checkAnythingToCleanWithNearbyEvent == null)
				{
					checkAnythingToCleanWithNearbyEvent = FromPool(Actor, Item);
				}
				if (!cell.HandleEvent(checkAnythingToCleanWithNearbyEvent))
				{
					return true;
				}
			}
			foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
			{
				if (localAdjacentCell.WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (checkAnythingToCleanWithNearbyEvent == null)
					{
						checkAnythingToCleanWithNearbyEvent = FromPool(Actor, Item);
					}
					if (!localAdjacentCell.HandleEvent(checkAnythingToCleanWithNearbyEvent))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}

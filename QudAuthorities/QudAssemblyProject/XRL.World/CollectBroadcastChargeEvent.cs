using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CollectBroadcastChargeEvent : MinEvent
{
	public GameObject Object;

	public Zone Zone;

	public Cell Cell;

	public int Charge;

	public int MultipleCharge;

	public new static readonly int ID;

	private static List<CollectBroadcastChargeEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CollectBroadcastChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CollectBroadcastChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CollectBroadcastChargeEvent()
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

	public static CollectBroadcastChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Object = null;
		Zone = null;
		Cell = null;
		Charge = 0;
		MultipleCharge = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Object, Zone Zone, Cell Cell, int Charge, int MultipleCharge = 1)
	{
		if (Zone != null && Zone.WantEvent(ID, CascadeLevel))
		{
			CollectBroadcastChargeEvent collectBroadcastChargeEvent = FromPool();
			collectBroadcastChargeEvent.Object = Object;
			collectBroadcastChargeEvent.Zone = Zone;
			collectBroadcastChargeEvent.Cell = Cell;
			collectBroadcastChargeEvent.Charge = Charge;
			collectBroadcastChargeEvent.MultipleCharge = MultipleCharge;
			Zone.HandleEvent(collectBroadcastChargeEvent);
			return collectBroadcastChargeEvent.Charge;
		}
		return Charge;
	}

	public static int GetFor(GameObject Object, int Charge, int MultipleCharge = 1)
	{
		Cell currentCell = Object.CurrentCell;
		if (currentCell != null)
		{
			return GetFor(Object, currentCell.ParentZone, currentCell, Charge, MultipleCharge);
		}
		return Charge;
	}
}

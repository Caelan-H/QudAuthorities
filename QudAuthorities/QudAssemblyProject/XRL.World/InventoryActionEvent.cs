using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class InventoryActionEvent : IActOnItemEvent
{
	public string Command;

	public bool OwnershipHandled;

	public bool Auto;

	public bool OverrideEnergyCost;

	public int EnergyCostOverride;

	public int MinimumCharge;

	public GameObject ObjectTarget;

	public Cell CellTarget;

	public Cell FromCell;

	public List<GameObject> Generated = new List<GameObject>();

	public new static readonly int ID;

	private static List<InventoryActionEvent> Pool;

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

	static InventoryActionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(InventoryActionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public InventoryActionEvent()
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
		Command = null;
		OwnershipHandled = false;
		Auto = false;
		OverrideEnergyCost = false;
		EnergyCostOverride = 0;
		MinimumCharge = 0;
		ObjectTarget = null;
		CellTarget = null;
		FromCell = null;
		Generated.Clear();
		base.Reset();
	}

	public static InventoryActionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static InventoryActionEvent FromPool(GameObject Actor, GameObject Item)
	{
		InventoryActionEvent inventoryActionEvent = FromPool();
		inventoryActionEvent.Actor = Actor;
		inventoryActionEvent.Item = Item;
		return inventoryActionEvent;
	}

	public static InventoryActionEvent FromPool(GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, int EnergyCostOverride = 0, int MinimumCharge = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null)
	{
		InventoryActionEvent inventoryActionEvent = FromPool(Actor, Item);
		inventoryActionEvent.Command = Command;
		inventoryActionEvent.Auto = Auto;
		inventoryActionEvent.OwnershipHandled = OwnershipHandled;
		inventoryActionEvent.OverrideEnergyCost = OverrideEnergyCost;
		inventoryActionEvent.EnergyCostOverride = EnergyCostOverride;
		inventoryActionEvent.MinimumCharge = MinimumCharge;
		inventoryActionEvent.ObjectTarget = ObjectTarget;
		inventoryActionEvent.CellTarget = CellTarget;
		inventoryActionEvent.FromCell = FromCell;
		inventoryActionEvent.Generated.Clear();
		return inventoryActionEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, int EnergyCostOverride = 0, int MinimumCharge = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			InventoryActionEvent inventoryActionEvent = FromPool(Actor, Item, Command, Auto, OwnershipHandled, OverrideEnergyCost, EnergyCostOverride, MinimumCharge, ObjectTarget, CellTarget, FromCell);
			if (!Object.HandleEvent(inventoryActionEvent))
			{
				return false;
			}
			if (!AfterInventoryActionEvent.Check(Object, inventoryActionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Check(out IEvent GeneratedEvent, GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, int EnergyCostOverride = 0, int MinimumCharge = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			InventoryActionEvent inventoryActionEvent = (InventoryActionEvent)(GeneratedEvent = FromPool(Actor, Item, Command, Auto, OwnershipHandled, OverrideEnergyCost, EnergyCostOverride, MinimumCharge, ObjectTarget, CellTarget, FromCell));
			if (!Object.HandleEvent(inventoryActionEvent))
			{
				return false;
			}
			if (!AfterInventoryActionEvent.Check(Object, inventoryActionEvent))
			{
				return false;
			}
		}
		else
		{
			GeneratedEvent = null;
		}
		return true;
	}

	public static bool Check(out InventoryActionEvent E, GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, int EnergyCostOverride = 0, int MinimumCharge = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			E = FromPool(Actor, Item, Command, Auto, OwnershipHandled, OverrideEnergyCost, EnergyCostOverride, MinimumCharge, ObjectTarget, CellTarget, FromCell);
			if (!Object.HandleEvent(E))
			{
				return false;
			}
			if (!AfterInventoryActionEvent.Check(Object, E))
			{
				return false;
			}
		}
		else
		{
			E = null;
		}
		return true;
	}
}

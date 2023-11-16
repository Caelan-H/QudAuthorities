using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterInventoryActionEvent : IActOnItemEvent
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

	private static List<AfterInventoryActionEvent> Pool;

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

	static AfterInventoryActionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterInventoryActionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterInventoryActionEvent()
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

	public static AfterInventoryActionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterInventoryActionEvent FromPool(GameObject Actor, GameObject Item)
	{
		AfterInventoryActionEvent afterInventoryActionEvent = FromPool();
		afterInventoryActionEvent.Actor = Actor;
		afterInventoryActionEvent.Item = Item;
		return afterInventoryActionEvent;
	}

	public static AfterInventoryActionEvent FromPool(GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, int EnergyCostOverride = 0, int MinimumCharge = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null)
	{
		AfterInventoryActionEvent afterInventoryActionEvent = FromPool(Actor, Item);
		afterInventoryActionEvent.Command = Command;
		afterInventoryActionEvent.Auto = Auto;
		afterInventoryActionEvent.OwnershipHandled = OwnershipHandled;
		afterInventoryActionEvent.OverrideEnergyCost = OverrideEnergyCost;
		afterInventoryActionEvent.EnergyCostOverride = EnergyCostOverride;
		afterInventoryActionEvent.MinimumCharge = MinimumCharge;
		afterInventoryActionEvent.ObjectTarget = ObjectTarget;
		afterInventoryActionEvent.CellTarget = CellTarget;
		afterInventoryActionEvent.FromCell = FromCell;
		afterInventoryActionEvent.Generated.Clear();
		return afterInventoryActionEvent;
	}

	public static bool Check(GameObject Object, InventoryActionEvent Source)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterInventoryActionEvent afterInventoryActionEvent = FromPool(Source.Actor, Source.Item, Source.Command, Source.Auto, Source.OwnershipHandled, Source.OverrideEnergyCost, Source.EnergyCostOverride, Source.MinimumCharge, Source.ObjectTarget, Source.CellTarget, Source.FromCell);
			bool num = Object.HandleEvent(afterInventoryActionEvent);
			Source.ProcessChildEvent(afterInventoryActionEvent);
			Source.Actor = afterInventoryActionEvent.Actor;
			Source.Item = afterInventoryActionEvent.Item;
			Source.Command = afterInventoryActionEvent.Command;
			Source.Auto = afterInventoryActionEvent.Auto;
			Source.OwnershipHandled = afterInventoryActionEvent.OwnershipHandled;
			Source.OverrideEnergyCost = afterInventoryActionEvent.OverrideEnergyCost;
			Source.EnergyCostOverride = afterInventoryActionEvent.EnergyCostOverride;
			Source.MinimumCharge = afterInventoryActionEvent.MinimumCharge;
			Source.ObjectTarget = afterInventoryActionEvent.ObjectTarget;
			Source.CellTarget = afterInventoryActionEvent.CellTarget;
			Source.FromCell = afterInventoryActionEvent.FromCell;
			if (!num)
			{
				return false;
			}
		}
		return true;
	}
}

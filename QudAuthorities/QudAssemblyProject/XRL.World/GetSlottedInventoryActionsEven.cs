using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSlottedInventoryActionsEvent : IInventoryActionsEvent
{
	public GameObject Slotted;

	public new static readonly int ID;

	private static List<GetSlottedInventoryActionsEvent> Pool;

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

	static GetSlottedInventoryActionsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSlottedInventoryActionsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSlottedInventoryActionsEvent()
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
		Slotted = null;
		base.Reset();
	}

	public static GetSlottedInventoryActionsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetSlottedInventoryActionsEvent FromPool(GameObject Slotted, GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetSlottedInventoryActionsEvent getSlottedInventoryActionsEvent = FromPool();
		getSlottedInventoryActionsEvent.Actor = Actor;
		getSlottedInventoryActionsEvent.Object = Object;
		getSlottedInventoryActionsEvent.Actions = Actions;
		return getSlottedInventoryActionsEvent;
	}

	public static GetSlottedInventoryActionsEvent FromPool(GameObject Slotted, IInventoryActionsEvent Parent)
	{
		return FromPool(Slotted, Parent.Actor, Parent.Object, Parent.Actions);
	}

	public static void Send(GameObject Slotted, IInventoryActionsEvent Parent)
	{
		if (GameObject.validate(ref Slotted) && Slotted.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetSlottedInventoryActionsEvent e = FromPool(Slotted, Parent);
			Slotted.HandleEvent(e);
			Parent.ProcessChildEvent(e);
		}
	}
}

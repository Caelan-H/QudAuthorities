using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetInventoryActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID;

	private static List<GetInventoryActionsEvent> Pool;

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

	static GetInventoryActionsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetInventoryActionsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetInventoryActionsEvent()
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

	public static GetInventoryActionsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetInventoryActionsEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetInventoryActionsEvent getInventoryActionsEvent = FromPool();
		getInventoryActionsEvent.Actor = Actor;
		getInventoryActionsEvent.Object = Object;
		getInventoryActionsEvent.Actions = Actions;
		return getInventoryActionsEvent;
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetInventoryActionsAlwaysEvent : IInventoryActionsEvent
{
	public new static readonly int ID;

	private static List<GetInventoryActionsAlwaysEvent> Pool;

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

	static GetInventoryActionsAlwaysEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetInventoryActionsAlwaysEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetInventoryActionsAlwaysEvent()
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

	public static GetInventoryActionsAlwaysEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetInventoryActionsAlwaysEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetInventoryActionsAlwaysEvent getInventoryActionsAlwaysEvent = FromPool();
		getInventoryActionsAlwaysEvent.Actor = Actor;
		getInventoryActionsAlwaysEvent.Object = Object;
		getInventoryActionsAlwaysEvent.Actions = Actions;
		return getInventoryActionsAlwaysEvent;
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}

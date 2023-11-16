using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OwnerGetInventoryActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID;

	private static List<OwnerGetInventoryActionsEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

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

	static OwnerGetInventoryActionsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OwnerGetInventoryActionsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OwnerGetInventoryActionsEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static OwnerGetInventoryActionsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static OwnerGetInventoryActionsEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		OwnerGetInventoryActionsEvent ownerGetInventoryActionsEvent = FromPool();
		ownerGetInventoryActionsEvent.Actor = Actor;
		ownerGetInventoryActionsEvent.Object = Object;
		ownerGetInventoryActionsEvent.Actions = Actions;
		return ownerGetInventoryActionsEvent;
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			Actor.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}

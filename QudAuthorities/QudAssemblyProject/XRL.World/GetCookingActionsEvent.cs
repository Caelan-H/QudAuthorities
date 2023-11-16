using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetCookingActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID;

	private static List<GetCookingActionsEvent> Pool;

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

	static GetCookingActionsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetCookingActionsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetCookingActionsEvent()
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

	public static GetCookingActionsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetCookingActionsEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetCookingActionsEvent getCookingActionsEvent = FromPool();
		getCookingActionsEvent.Actor = Actor;
		getCookingActionsEvent.Object = Object;
		getCookingActionsEvent.Actions = Actions;
		return getCookingActionsEvent;
	}

	public static void SendToActorAndObject(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) || Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetCookingActionsEvent e = FromPool(Actor, Object, Actions);
			Object.HandleEvent(e);
			Actor.HandleEvent(e);
		}
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterAfterThrownEvent : IActOnItemEvent
{
	public GameObject ApparentTarget;

	public new static readonly int ID;

	private static List<AfterAfterThrownEvent> Pool;

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

	static AfterAfterThrownEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterAfterThrownEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterAfterThrownEvent()
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
		ApparentTarget = null;
		base.Reset();
	}

	public static AfterAfterThrownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterAfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		AfterAfterThrownEvent afterAfterThrownEvent = FromPool();
		afterAfterThrownEvent.Actor = Actor;
		afterAfterThrownEvent.Item = Item;
		afterAfterThrownEvent.ApparentTarget = ApparentTarget;
		return afterAfterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.validate(ref Item) && Item.HasRegisteredEvent("AfterAfterThrown"))
		{
			Event @event = Event.New("AfterAfterThrown");
			@event.SetParameter("Owner", Actor);
			@event.SetParameter("Object", Item);
			@event.SetParameter("ApparentTarget", ApparentTarget);
			if (!Item.FireEvent(@event))
			{
				return false;
			}
		}
		if (GameObject.validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterAfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}

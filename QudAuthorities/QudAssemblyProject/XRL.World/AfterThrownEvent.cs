using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterThrownEvent : IActOnItemEvent
{
	public GameObject ApparentTarget;

	public new static readonly int ID;

	private static List<AfterThrownEvent> Pool;

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

	static AfterThrownEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterThrownEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterThrownEvent()
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

	public static AfterThrownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		AfterThrownEvent afterThrownEvent = FromPool();
		afterThrownEvent.Actor = Actor;
		afterThrownEvent.Item = Item;
		afterThrownEvent.ApparentTarget = ApparentTarget;
		return afterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.validate(ref Item) && Item.HasRegisteredEvent("AfterThrown"))
		{
			Event @event = Event.New("AfterThrown");
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
			AfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}

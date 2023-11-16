using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeAfterThrownEvent : IActOnItemEvent
{
	public GameObject ApparentTarget;

	public new static readonly int ID;

	private static List<BeforeAfterThrownEvent> Pool;

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

	static BeforeAfterThrownEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeAfterThrownEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeAfterThrownEvent()
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

	public static BeforeAfterThrownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeAfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		BeforeAfterThrownEvent beforeAfterThrownEvent = FromPool();
		beforeAfterThrownEvent.Actor = Actor;
		beforeAfterThrownEvent.Item = Item;
		beforeAfterThrownEvent.ApparentTarget = ApparentTarget;
		return beforeAfterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.validate(ref Item) && Item.HasRegisteredEvent("BeforeAfterThrown"))
		{
			Event @event = Event.New("BeforeAfterThrown");
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
			BeforeAfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}

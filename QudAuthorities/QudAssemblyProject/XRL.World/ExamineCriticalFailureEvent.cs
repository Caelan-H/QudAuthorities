using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ExamineCriticalFailureEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<ExamineCriticalFailureEvent> Pool;

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

	static ExamineCriticalFailureEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ExamineCriticalFailureEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ExamineCriticalFailureEvent()
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

	public static ExamineCriticalFailureEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ExamineCriticalFailureEvent FromPool(GameObject Actor, GameObject Item)
	{
		ExamineCriticalFailureEvent examineCriticalFailureEvent = FromPool();
		examineCriticalFailureEvent.Actor = Actor;
		examineCriticalFailureEvent.Item = Item;
		return examineCriticalFailureEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (Actor.HasRegisteredEvent("ExamineCriticalFailure") || Item.HasRegisteredEvent("ExamineCriticalFailure"))
		{
			Event @event = Event.New("ExamineCriticalFailure");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Item", Item);
			if (!Actor.FireEvent(@event) || !Item.FireEvent(@event))
			{
				return false;
			}
		}
		bool flag = Actor.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag2 = Item.WantEvent(ID, MinEvent.CascadeLevel);
		if (flag || flag2)
		{
			ExamineCriticalFailureEvent e = FromPool(Actor, Item);
			if ((flag && !Actor.HandleEvent(e)) || (flag2 && !Item.HandleEvent(e)))
			{
				return false;
			}
		}
		return true;
	}
}

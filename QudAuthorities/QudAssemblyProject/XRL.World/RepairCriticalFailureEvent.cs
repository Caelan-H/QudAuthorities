using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RepairCriticalFailureEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<RepairCriticalFailureEvent> Pool;

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

	static RepairCriticalFailureEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(RepairCriticalFailureEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public RepairCriticalFailureEvent()
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

	public static RepairCriticalFailureEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static RepairCriticalFailureEvent FromPool(GameObject Actor, GameObject Item)
	{
		RepairCriticalFailureEvent repairCriticalFailureEvent = FromPool();
		repairCriticalFailureEvent.Actor = Actor;
		repairCriticalFailureEvent.Item = Item;
		return repairCriticalFailureEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (Actor.HasRegisteredEvent("RepairCriticalFailure") || Item.HasRegisteredEvent("RepairCriticalFailure"))
		{
			Event @event = Event.New("RepairCriticalFailure");
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
			RepairCriticalFailureEvent e = FromPool(Actor, Item);
			if ((flag && !Actor.HandleEvent(e)) || (flag2 && !Item.HandleEvent(e)))
			{
				return false;
			}
		}
		return true;
	}
}

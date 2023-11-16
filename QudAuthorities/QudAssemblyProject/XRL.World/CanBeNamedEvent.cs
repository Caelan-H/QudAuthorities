using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeNamedEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<CanBeNamedEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

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

	static CanBeNamedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeNamedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeNamedEvent()
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

	public static CanBeNamedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanBeNamedEvent FromPool(GameObject Actor, GameObject Item)
	{
		CanBeNamedEvent canBeNamedEvent = FromPool();
		canBeNamedEvent.Actor = Actor;
		canBeNamedEvent.Item = Item;
		return canBeNamedEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag || flag2)
		{
			CanBeNamedEvent e = FromPool(Actor, Item);
			if (flag && !Actor.HandleEvent(e))
			{
				return false;
			}
			if (flag2 && !Item.HandleEvent(e))
			{
				return false;
			}
		}
		bool flag3 = Actor?.HasRegisteredEvent("CanBeNamed") ?? false;
		bool flag4 = Item?.HasRegisteredEvent("CanBeNamed") ?? false;
		if (flag3 || flag4)
		{
			Event e2 = Event.New("CanBeNamed", "Actor", Actor, "Item", Item);
			if (flag3 && !Actor.FireEvent(e2))
			{
				return false;
			}
			if (flag4 && !Item.FireEvent(e2))
			{
				return false;
			}
		}
		return true;
	}
}

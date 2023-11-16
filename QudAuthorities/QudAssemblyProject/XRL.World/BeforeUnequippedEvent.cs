using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeUnequippedEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<BeforeUnequippedEvent> Pool;

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

	static BeforeUnequippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeUnequippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeUnequippedEvent()
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

	public static BeforeUnequippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeUnequippedEvent FromPool(GameObject Actor, GameObject Item)
	{
		BeforeUnequippedEvent beforeUnequippedEvent = FromPool();
		beforeUnequippedEvent.Actor = Actor;
		beforeUnequippedEvent.Item = Item;
		return beforeUnequippedEvent;
	}

	public static void Send(GameObject Object, GameObject WasEquippedBy)
	{
		if (!GameObject.validate(ref Object))
		{
			return;
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(WasEquippedBy, Object));
			if (!GameObject.validate(ref Object))
			{
				return;
			}
		}
		if (Object.HasRegisteredEvent("BeforeUnequipped"))
		{
			Object.FireEvent(Event.New("BeforeUnequipped", "Actor", WasEquippedBy));
		}
	}
}

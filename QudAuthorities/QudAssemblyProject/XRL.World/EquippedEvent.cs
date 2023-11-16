using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EquippedEvent : IActOnItemEvent
{
	public BodyPart Part;

	public new static readonly int ID;

	private static List<EquippedEvent> Pool;

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

	static EquippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EquippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EquippedEvent()
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
		Part = null;
		base.Reset();
	}

	public static EquippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Actor, GameObject Item, BodyPart Part)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Item) && Item.HasRegisteredEvent("Equipped"))
		{
			Event @event = new Event("Equipped");
			@event.SetParameter("EquippingObject", Actor);
			@event.SetParameter("Object", Item);
			@event.SetParameter("BodyPart", Part);
			@event.SetParameter("Actor", Actor);
			flag = Item.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			EquippedEvent equippedEvent = FromPool();
			equippedEvent.Actor = Actor;
			equippedEvent.Item = Item;
			equippedEvent.Part = Part;
			flag = Item.HandleEvent(equippedEvent);
		}
	}
}

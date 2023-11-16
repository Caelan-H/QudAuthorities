using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EquipperEquippedEvent : IActOnItemEvent
{
	public BodyPart Part;

	public new static readonly int ID;

	private static List<EquipperEquippedEvent> Pool;

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

	static EquipperEquippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EquipperEquippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EquipperEquippedEvent()
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

	public static EquipperEquippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EquipperEquippedEvent FromPool(GameObject Actor, GameObject Item, BodyPart Part)
	{
		EquipperEquippedEvent equipperEquippedEvent = FromPool();
		equipperEquippedEvent.Actor = Actor;
		equipperEquippedEvent.Item = Item;
		equipperEquippedEvent.Part = Part;
		return equipperEquippedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, BodyPart Part)
	{
		if ((!GameObject.validate(ref Actor) || !Actor.WantEvent(ID, MinEvent.CascadeLevel) || Actor.HandleEvent(FromPool(Actor, Item, Part))) && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("EquipperEquipped"))
		{
			Event @event = new Event("EquipperEquipped");
			@event.SetParameter("Object", Item);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("BodyPart", Part);
			Actor.FireEvent(@event);
		}
	}
}

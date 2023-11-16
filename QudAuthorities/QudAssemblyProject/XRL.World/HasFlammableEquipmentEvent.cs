using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class HasFlammableEquipmentEvent : MinEvent
{
	public GameObject Object;

	public int Temperature;

	public new static readonly int ID;

	private static List<HasFlammableEquipmentEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static HasFlammableEquipmentEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(HasFlammableEquipmentEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public HasFlammableEquipmentEvent()
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
		Object = null;
		Temperature = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static HasFlammableEquipmentEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static HasFlammableEquipmentEvent FromPool(GameObject Object, int Temperature)
	{
		HasFlammableEquipmentEvent hasFlammableEquipmentEvent = FromPool();
		hasFlammableEquipmentEvent.Object = Object;
		hasFlammableEquipmentEvent.Temperature = Temperature;
		return hasFlammableEquipmentEvent;
	}

	public static bool Check(GameObject Object, int Temperature)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("HasFlammableEquipment"))
		{
			Event @event = Event.New("HasFlammableEquipment");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Temperature", Temperature);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Temperature));
		}
		return !flag;
	}
}

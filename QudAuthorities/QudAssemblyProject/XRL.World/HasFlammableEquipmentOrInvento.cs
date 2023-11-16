using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class HasFlammableEquipmentOrInventoryEvent : MinEvent
{
	public GameObject Object;

	public int Temperature;

	public new static readonly int ID;

	private static List<HasFlammableEquipmentOrInventoryEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 3;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static HasFlammableEquipmentOrInventoryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(HasFlammableEquipmentOrInventoryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public HasFlammableEquipmentOrInventoryEvent()
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

	public static HasFlammableEquipmentOrInventoryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static HasFlammableEquipmentOrInventoryEvent FromPool(GameObject Object, int Temperature)
	{
		HasFlammableEquipmentOrInventoryEvent hasFlammableEquipmentOrInventoryEvent = FromPool();
		hasFlammableEquipmentOrInventoryEvent.Object = Object;
		hasFlammableEquipmentOrInventoryEvent.Temperature = Temperature;
		return hasFlammableEquipmentOrInventoryEvent;
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

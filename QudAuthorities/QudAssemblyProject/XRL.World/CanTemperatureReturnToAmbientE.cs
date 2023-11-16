using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanTemperatureReturnToAmbientEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CanTemperatureReturnToAmbientEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanTemperatureReturnToAmbientEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanTemperatureReturnToAmbientEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanTemperatureReturnToAmbientEvent()
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
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanTemperatureReturnToAmbientEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanTemperatureReturnToAmbientEvent FromPool(GameObject Object)
	{
		CanTemperatureReturnToAmbientEvent canTemperatureReturnToAmbientEvent = FromPool();
		canTemperatureReturnToAmbientEvent.Object = Object;
		return canTemperatureReturnToAmbientEvent;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("CanTemperatureReturnToAmbient"))
		{
			Event @event = Event.New("CanTemperatureReturnToAmbient");
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object));
		}
		return flag;
	}
}

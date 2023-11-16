using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetFixedMissileSpreadEvent : MinEvent
{
	public GameObject Object;

	public int Spread;

	public new static readonly int ID;

	private static List<GetFixedMissileSpreadEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetFixedMissileSpreadEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetFixedMissileSpreadEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetFixedMissileSpreadEvent()
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
		Spread = 0;
		base.Reset();
	}

	public static GetFixedMissileSpreadEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool GetFor(GameObject Object, out int Spread)
	{
		Spread = 0;
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetFixedMissileSpread"))
		{
			Event @event = Event.New("GetFixedMissileSpread");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Spread", Spread);
			flag = Object.FireEvent(@event);
			Spread = @event.GetIntParameter("Spread");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetFixedMissileSpreadEvent getFixedMissileSpreadEvent = FromPool();
			getFixedMissileSpreadEvent.Object = Object;
			getFixedMissileSpreadEvent.Spread = Spread;
			flag = Object.HandleEvent(getFixedMissileSpreadEvent);
			Spread = getFixedMissileSpreadEvent.Spread;
		}
		return !flag;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPowerLoadLevelEvent : MinEvent
{
	public GameObject Object;

	public int Level;

	public new static readonly int ID;

	private static List<GetPowerLoadLevelEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetPowerLoadLevelEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPowerLoadLevelEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPowerLoadLevelEvent()
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
		Level = 0;
		base.Reset();
	}

	public static GetPowerLoadLevelEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Object, int Level = 100)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetPowerLoadLevel"))
		{
			Event @event = Event.New("GetPowerLoadLevel");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Level", Level);
			flag = Object.FireEvent(@event);
			Level = @event.GetIntParameter("Level");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetPowerLoadLevelEvent getPowerLoadLevelEvent = FromPool();
			getPowerLoadLevelEvent.Object = Object;
			getPowerLoadLevelEvent.Level = Level;
			flag = Object.HandleEvent(getPowerLoadLevelEvent);
			Level = getPowerLoadLevelEvent.Level;
		}
		return Level;
	}
}

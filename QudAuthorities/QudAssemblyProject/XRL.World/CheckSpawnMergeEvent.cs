using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckSpawnMergeEvent : MinEvent
{
	public GameObject Object;

	public GameObject Other;

	public new static readonly int ID;

	private static List<CheckSpawnMergeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckSpawnMergeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckSpawnMergeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckSpawnMergeEvent()
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

	public static CheckSpawnMergeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Object = null;
		Other = null;
		base.Reset();
	}

	public static bool Check(GameObject Object, GameObject Other)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("CheckSpawnMerge"))
		{
			Event @event = Event.New("CheckSpawnMerge");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Other", Other);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			CheckSpawnMergeEvent checkSpawnMergeEvent = FromPool();
			checkSpawnMergeEvent.Object = Object;
			checkSpawnMergeEvent.Other = Other;
			flag = Object.HandleEvent(checkSpawnMergeEvent);
		}
		return flag;
	}
}

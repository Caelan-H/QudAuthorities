using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsExplosiveEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<IsExplosiveEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IsExplosiveEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsExplosiveEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsExplosiveEvent()
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

	public static IsExplosiveEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsExplosive"))
		{
			Event @event = Event.New("IsExplosive");
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			IsExplosiveEvent isExplosiveEvent = FromPool();
			isExplosiveEvent.Object = Object;
			flag = Object.HandleEvent(isExplosiveEvent);
		}
		return !flag;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class HasBeenReadEvent : MinEvent
{
	public GameObject Object;

	public GameObject Actor;

	public new static readonly int ID;

	private static List<HasBeenReadEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static HasBeenReadEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(HasBeenReadEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public HasBeenReadEvent()
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
		Actor = null;
		base.Reset();
	}

	public static HasBeenReadEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("HasBeenRead"))
		{
			Event @event = Event.New("HasBeenRead");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", Actor);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			HasBeenReadEvent hasBeenReadEvent = FromPool();
			hasBeenReadEvent.Object = Object;
			hasBeenReadEvent.Actor = Actor;
			flag = Object.HandleEvent(hasBeenReadEvent);
		}
		return !flag;
	}
}

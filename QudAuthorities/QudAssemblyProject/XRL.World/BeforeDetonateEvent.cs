using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeDetonateEvent : MinEvent
{
	public GameObject Object;

	public GameObject Actor;

	public GameObject ApparentTarget;

	public bool Indirect;

	public new static readonly int ID;

	private static List<BeforeDetonateEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static BeforeDetonateEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeDetonateEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeDetonateEvent()
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
		ApparentTarget = null;
		Indirect = false;
		base.Reset();
	}

	public static BeforeDetonateEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BeforeDetonate"))
		{
			Event @event = Event.New("BeforeDetonate");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("ApparentTarget", ApparentTarget);
			@event.SetFlag("Indirect", Indirect);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			BeforeDetonateEvent beforeDetonateEvent = FromPool();
			beforeDetonateEvent.Object = Object;
			beforeDetonateEvent.Actor = Actor;
			beforeDetonateEvent.ApparentTarget = ApparentTarget;
			beforeDetonateEvent.Indirect = Indirect;
			flag = Object.HandleEvent(beforeDetonateEvent);
		}
		return flag;
	}
}

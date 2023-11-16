using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanAcceptObjectEvent : MinEvent
{
	public GameObject Object;

	public GameObject Holder;

	public GameObject Container;

	public new static readonly int ID;

	private static List<CanAcceptObjectEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanAcceptObjectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanAcceptObjectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanAcceptObjectEvent()
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
		Holder = null;
		Container = null;
		base.Reset();
	}

	public static CanAcceptObjectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Relevant(GameObject Container)
	{
		if (GameObject.validate(ref Container))
		{
			if (Container.HasRegisteredEvent("CanAcceptObject"))
			{
				return true;
			}
			if (Container.WantEvent(ID, MinEvent.CascadeLevel))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(GameObject Object, GameObject Holder, GameObject Container)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Container) && Container.HasRegisteredEvent("CanAcceptObject"))
		{
			Event @event = Event.New("CanAcceptObject");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Holder", Holder);
			@event.SetParameter("Container", Container);
			flag = Container.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Container) && Container.WantEvent(ID, MinEvent.CascadeLevel))
		{
			CanAcceptObjectEvent canAcceptObjectEvent = FromPool();
			canAcceptObjectEvent.Object = Object;
			canAcceptObjectEvent.Holder = Holder;
			canAcceptObjectEvent.Container = Container;
			flag = Container.HandleEvent(canAcceptObjectEvent);
		}
		return flag;
	}
}

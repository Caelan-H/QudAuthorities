using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsTrueKinEvent : MinEvent
{
	public GameObject Object;

	public bool IsTrueKin;

	public new static readonly int ID;

	private static List<IsTrueKinEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IsTrueKinEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsTrueKinEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsTrueKinEvent()
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
		IsTrueKin = false;
		base.Reset();
	}

	public static IsTrueKinEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static IsTrueKinEvent FromPool(GameObject Object, bool IsTrueKin)
	{
		IsTrueKinEvent isTrueKinEvent = FromPool();
		isTrueKinEvent.Object = Object;
		isTrueKinEvent.IsTrueKin = IsTrueKin;
		return isTrueKinEvent;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = (Object?.genotypeEntry?.IsTrueKin).GetValueOrDefault();
		bool flag2 = true;
		if (flag2 && GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsTrueKin"))
		{
			Event @event = Event.New("IsTrueKin");
			@event.SetParameter("Object", Object);
			@event.SetFlag("IsTrueKin", flag);
			flag2 = Object.FireEvent(@event);
			flag = @event.HasFlag("IsTrueKin");
		}
		if (flag2 && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			IsTrueKinEvent isTrueKinEvent = FromPool(Object, flag);
			flag2 = Object.HandleEvent(isTrueKinEvent);
			flag = isTrueKinEvent.IsTrueKin;
		}
		return flag;
	}
}

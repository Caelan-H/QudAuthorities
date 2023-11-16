using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeDismemberedEvent : MinEvent
{
	public GameObject Object;

	public string Attributes;

	public new static readonly int ID;

	private static List<CanBeDismemberedEvent> Pool;

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

	static CanBeDismemberedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeDismemberedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeDismemberedEvent()
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
		Attributes = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanBeDismemberedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, string Attributes = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("CanBeDismembered"))
		{
			Event @event = Event.New("CanBeDismembered");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Attributes", Attributes);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			CanBeDismemberedEvent canBeDismemberedEvent = FromPool();
			canBeDismemberedEvent.Object = Object;
			canBeDismemberedEvent.Attributes = Attributes;
			flag = Object.HandleEvent(canBeDismemberedEvent);
		}
		return flag;
	}
}

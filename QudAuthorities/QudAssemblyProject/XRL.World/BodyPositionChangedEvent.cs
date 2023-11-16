using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BodyPositionChangedEvent : MinEvent
{
	public GameObject Object;

	public string To;

	public bool Involuntary;

	public new static readonly int ID;

	private static List<BodyPositionChangedEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static BodyPositionChangedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BodyPositionChangedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BodyPositionChangedEvent()
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
		To = null;
		Involuntary = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static BodyPositionChangedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BodyPositionChangedEvent FromPool(GameObject Object, string To = null, bool Involuntary = false)
	{
		BodyPositionChangedEvent bodyPositionChangedEvent = FromPool();
		bodyPositionChangedEvent.Object = Object;
		bodyPositionChangedEvent.To = To;
		bodyPositionChangedEvent.Involuntary = Involuntary;
		return bodyPositionChangedEvent;
	}

	public static void Send(GameObject Object, string To = null, bool Involuntary = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BodyPositionChanged"))
		{
			Event @event = Event.New("BodyPositionChanged");
			@event.SetParameter("Object", Object);
			@event.SetParameter("To", To);
			@event.SetFlag("Involuntary", Involuntary);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, To, Involuntary));
		}
	}
}

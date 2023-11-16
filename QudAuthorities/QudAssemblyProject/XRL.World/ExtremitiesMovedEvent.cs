using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ExtremitiesMovedEvent : MinEvent
{
	public GameObject Object;

	public string To;

	public bool Involuntary;

	public new static readonly int ID;

	private static List<ExtremitiesMovedEvent> Pool;

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

	static ExtremitiesMovedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ExtremitiesMovedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ExtremitiesMovedEvent()
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

	public static ExtremitiesMovedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ExtremitiesMovedEvent FromPool(GameObject Object, string To = null, bool Involuntary = false)
	{
		ExtremitiesMovedEvent extremitiesMovedEvent = FromPool();
		extremitiesMovedEvent.Object = Object;
		extremitiesMovedEvent.To = To;
		extremitiesMovedEvent.Involuntary = Involuntary;
		return extremitiesMovedEvent;
	}

	public static void Send(GameObject Object, string To = null, bool Involuntary = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("ExtremitiesMoved"))
		{
			Event @event = Event.New("ExtremitiesMoved");
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

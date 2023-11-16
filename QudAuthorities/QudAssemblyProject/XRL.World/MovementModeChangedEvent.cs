using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class MovementModeChangedEvent : MinEvent
{
	public GameObject Object;

	public string To;

	public bool Involuntary;

	public new static readonly int ID;

	private static List<MovementModeChangedEvent> Pool;

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

	static MovementModeChangedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(MovementModeChangedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public MovementModeChangedEvent()
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

	public static MovementModeChangedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static MovementModeChangedEvent FromPool(GameObject Object, string To = null, bool Involuntary = false)
	{
		MovementModeChangedEvent movementModeChangedEvent = FromPool();
		movementModeChangedEvent.Object = Object;
		movementModeChangedEvent.To = To;
		movementModeChangedEvent.Involuntary = Involuntary;
		return movementModeChangedEvent;
	}

	public static void Send(GameObject Object, string To = null, bool Involuntary = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("MovementModeChanged"))
		{
			Event @event = Event.New("MovementModeChanged");
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

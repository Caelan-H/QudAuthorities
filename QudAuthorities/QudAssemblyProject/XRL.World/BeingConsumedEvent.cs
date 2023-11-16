using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeingConsumedEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public new static readonly int ID;

	private static List<BeingConsumedEvent> Pool;

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

	static BeingConsumedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeingConsumedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeingConsumedEvent()
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
		Actor = null;
		Object = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static BeingConsumedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Actor, GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BeingConsumed"))
		{
			Event @event = Event.New("BeingConsumed");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			BeingConsumedEvent beingConsumedEvent = FromPool();
			beingConsumedEvent.Actor = Actor;
			beingConsumedEvent.Object = Object;
			flag = Object.HandleEvent(beingConsumedEvent);
		}
	}
}

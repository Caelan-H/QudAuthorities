using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetActivationPhaseEvent : MinEvent
{
	public GameObject Object;

	public int Phase;

	public new static readonly int ID;

	private static List<GetActivationPhaseEvent> Pool;

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

	static GetActivationPhaseEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetActivationPhaseEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetActivationPhaseEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetActivationPhaseEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Object, int Phase = 0)
	{
		if (Phase == 0 && GameObject.validate(ref Object))
		{
			Phase = Object.GetPhase();
		}
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetActivationPhase"))
		{
			Event @event = Event.New("GetActivationPhase");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Phase", Phase);
			flag = Object.FireEvent(@event);
			Phase = @event.GetIntParameter("Phase");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GetActivationPhaseEvent getActivationPhaseEvent = FromPool();
			getActivationPhaseEvent.Object = Object;
			getActivationPhaseEvent.Phase = Phase;
			flag = Object.HandleEvent(getActivationPhaseEvent);
			Phase = getActivationPhaseEvent.Phase;
		}
		return Phase;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsSensableAsPsychicEvent : MinEvent
{
	public GameObject Object;

	public bool Sensable;

	public new static readonly int ID;

	private static List<IsSensableAsPsychicEvent> Pool;

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

	static IsSensableAsPsychicEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsSensableAsPsychicEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsSensableAsPsychicEvent()
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
		Sensable = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static IsSensableAsPsychicEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object)
	{
		bool flag = false;
		bool flag2 = true;
		if (flag2 && GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsSensableAsPsychic"))
		{
			Event @event = Event.New("IsSensableAsPsychic");
			@event.SetParameter("Object", Object);
			@event.SetFlag("Sensable", flag);
			flag2 = Object.FireEvent(@event);
			flag = @event.HasFlag("Sensable");
		}
		if (flag2 && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			IsSensableAsPsychicEvent isSensableAsPsychicEvent = FromPool();
			isSensableAsPsychicEvent.Object = Object;
			isSensableAsPsychicEvent.Sensable = flag;
			flag2 = Object.HandleEvent(isSensableAsPsychicEvent);
			flag = isSensableAsPsychicEvent.Sensable;
		}
		return flag;
	}
}

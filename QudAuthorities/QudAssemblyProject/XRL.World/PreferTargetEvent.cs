using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PreferTargetEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Target1;

	public GameObject Target2;

	public int Result;

	public new static readonly int ID;

	private static List<PreferTargetEvent> Pool;

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

	static PreferTargetEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PreferTargetEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PreferTargetEvent()
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
		Target1 = null;
		Target2 = null;
		Result = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static PreferTargetEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int Check(GameObject Actor, GameObject Target1, GameObject Target2)
	{
		int num = 0;
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("PreferTarget"))
		{
			Event @event = Event.New("PreferTarget");
			@event.SetParameter("Target1", Target1);
			@event.SetParameter("Target2", Target2);
			@event.SetParameter("Result", num);
			flag = Actor.FireEvent(@event);
			num = @event.GetIntParameter("Result");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			PreferTargetEvent preferTargetEvent = FromPool();
			preferTargetEvent.Target1 = Target1;
			preferTargetEvent.Target2 = Target2;
			preferTargetEvent.Result = num;
			flag = Actor.HandleEvent(preferTargetEvent);
			num = preferTargetEvent.Result;
		}
		return num;
	}
}

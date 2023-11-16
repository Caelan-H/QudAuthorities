using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OkayToDamageEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public new static readonly int ID;

	private static List<OkayToDamageEvent> Pool;

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

	static OkayToDamageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OkayToDamageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OkayToDamageEvent()
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

	public static OkayToDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, GameObject Actor, out bool WasWanted)
	{
		WasWanted = false;
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("OkayToDamage"))
		{
			WasWanted = true;
			Event @event = Event.New("OkayToDamage");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Object", Object);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			WasWanted = true;
			OkayToDamageEvent okayToDamageEvent = FromPool();
			okayToDamageEvent.Actor = Actor;
			okayToDamageEvent.Object = Object;
			flag = Object.HandleEvent(okayToDamageEvent);
		}
		return flag;
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		bool WasWanted;
		return Check(Object, Actor, out WasWanted);
	}
}

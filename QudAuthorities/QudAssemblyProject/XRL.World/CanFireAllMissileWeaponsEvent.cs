using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanFireAllMissileWeaponsEvent : MinEvent
{
	public GameObject Actor;

	public List<GameObject> MissileWeapons;

	public new static readonly int ID;

	private static List<CanFireAllMissileWeaponsEvent> Pool;

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

	static CanFireAllMissileWeaponsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanFireAllMissileWeaponsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanFireAllMissileWeaponsEvent()
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
		MissileWeapons = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanFireAllMissileWeaponsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanFireAllMissileWeaponsEvent FromPool(GameObject Actor)
	{
		CanFireAllMissileWeaponsEvent canFireAllMissileWeaponsEvent = FromPool();
		canFireAllMissileWeaponsEvent.Actor = Actor;
		return canFireAllMissileWeaponsEvent;
	}

	public static bool Check(GameObject Actor, List<GameObject> MissileWeapons = null)
	{
		if (MissileWeapons == null && Actor != null)
		{
			MissileWeapons = Actor.GetMissileWeapons();
		}
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("CanFireAllMissileWeapons"))
		{
			Event @event = Event.New("CanFireAllMissileWeapons");
			@event.SetParameter("Actor", Actor);
			flag = Actor.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor));
		}
		return !flag;
	}
}

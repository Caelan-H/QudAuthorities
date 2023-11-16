using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetProjectileObjectEvent : MinEvent
{
	public GameObject Ammo;

	public GameObject Launcher;

	public GameObject Projectile;

	public new static readonly int ID;

	private static List<GetProjectileObjectEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetProjectileObjectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetProjectileObjectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetProjectileObjectEvent()
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

	public static GetProjectileObjectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Ammo = null;
		Launcher = null;
		Projectile = null;
		base.Reset();
	}

	public static GameObject GetFor(GameObject Ammo, GameObject Launcher)
	{
		bool flag = true;
		GameObject gameObject = null;
		if (flag && GameObject.validate(ref Ammo) && Ammo.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetProjectileObjectEvent getProjectileObjectEvent = FromPool();
			getProjectileObjectEvent.Ammo = Ammo;
			getProjectileObjectEvent.Launcher = Launcher;
			getProjectileObjectEvent.Projectile = gameObject;
			flag = Ammo.HandleEvent(getProjectileObjectEvent);
			gameObject = getProjectileObjectEvent.Projectile;
		}
		if (flag && GameObject.validate(ref Ammo) && Ammo.HasRegisteredEvent("GetProjectileObject"))
		{
			Event @event = Event.New("GetProjectileObject");
			@event.SetParameter("Ammo", Ammo);
			@event.SetParameter("Launcher", Launcher);
			@event.SetParameter("Projectile", gameObject);
			flag = Ammo.FireEvent(@event);
			gameObject = @event.GetGameObjectParameter("Projectile");
		}
		return gameObject;
	}
}

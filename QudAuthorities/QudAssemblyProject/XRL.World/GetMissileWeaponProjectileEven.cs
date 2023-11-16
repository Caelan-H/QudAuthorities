using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMissileWeaponProjectileEvent : MinEvent
{
	public GameObject Launcher;

	public GameObject Projectile;

	public string Blueprint;

	public new static readonly int ID;

	private static List<GetMissileWeaponProjectileEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMissileWeaponProjectileEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetMissileWeaponProjectileEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMissileWeaponProjectileEvent()
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

	public static GetMissileWeaponProjectileEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMissileWeaponProjectileEvent FromPool(GameObject Launcher)
	{
		GetMissileWeaponProjectileEvent getMissileWeaponProjectileEvent = FromPool();
		getMissileWeaponProjectileEvent.Launcher = Launcher;
		return getMissileWeaponProjectileEvent;
	}

	public override void Reset()
	{
		Launcher = null;
		Projectile = null;
		Blueprint = null;
		base.Reset();
	}

	public static bool GetFor(GameObject Launcher, ref GameObject Projectile, ref string Blueprint)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Launcher) && Launcher.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMissileWeaponProjectileEvent getMissileWeaponProjectileEvent = FromPool();
			getMissileWeaponProjectileEvent.Launcher = Launcher;
			getMissileWeaponProjectileEvent.Projectile = Projectile;
			getMissileWeaponProjectileEvent.Blueprint = Blueprint;
			flag = Launcher.HandleEvent(getMissileWeaponProjectileEvent);
			Projectile = getMissileWeaponProjectileEvent.Projectile;
			Blueprint = getMissileWeaponProjectileEvent.Blueprint;
		}
		if (flag && GameObject.validate(ref Launcher) && Launcher.HasRegisteredEvent("GetMissileWeaponProjectile"))
		{
			Event @event = Event.New("GetMissileWeaponProjectile");
			@event.SetParameter("Launcher", Launcher);
			@event.SetParameter("Projectile", Projectile);
			@event.SetParameter("Blueprint", Blueprint);
			flag = Launcher.FireEvent(@event);
			Projectile = @event.GetGameObjectParameter("Projectile");
			Blueprint = @event.GetStringParameter("Blueprint");
		}
		return flag;
	}
}

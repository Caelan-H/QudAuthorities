using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMissileWeaponPerformanceEvent : MinEvent
{
	private class Perf
	{
		public int BasePenetration;

		public int PenetrationCap;

		public string BaseDamage;

		public string Attributes;

		public bool PenetrateCreatures;

		public bool PenetrateWalls;

		public bool Quiet;
	}

	public GameObject Subject;

	public GameObject Actor;

	public GameObject Launcher;

	public GameObject Projectile;

	public int BasePenetration;

	public int PenetrationCap;

	public string BaseDamage;

	public string Attributes;

	public string DamageColor;

	public bool PenetrateCreatures;

	public bool PenetrateWalls;

	public bool Quiet;

	public DieRoll DamageRoll;

	public bool Active;

	public new static readonly int ID;

	private static List<GetMissileWeaponPerformanceEvent> Pool;

	private static int PoolCounter;

	private static Dictionary<string, Perf> ProjectilePerformance;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMissileWeaponPerformanceEvent()
	{
		ProjectilePerformance = new Dictionary<string, Perf>(8);
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetMissileWeaponPerformanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMissileWeaponPerformanceEvent()
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

	public static GetMissileWeaponPerformanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMissileWeaponPerformanceEvent FromPool(GameObject Actor, GameObject Launcher, GameObject Projectile = null, int? BasePenetration = null, int? PenetrationCap = null, string BaseDamage = null, string Attributes = null, string DamageColor = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool? Quiet = null, DieRoll DamageRoll = null, string ProjectileBlueprint = null, bool Active = false)
	{
		GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = FromPool();
		getMissileWeaponPerformanceEvent.Actor = Actor;
		getMissileWeaponPerformanceEvent.Launcher = Launcher;
		if (Projectile == null && ProjectileBlueprint == null && Launcher != null)
		{
			GetMissileWeaponProjectileEvent.GetFor(Launcher, ref Projectile, ref ProjectileBlueprint);
		}
		getMissileWeaponPerformanceEvent.Projectile = Projectile;
		if (BaseDamage == null || !BasePenetration.HasValue || !PenetrationCap.HasValue || Attributes == null || !PenetrateCreatures.HasValue || !PenetrateWalls.HasValue || !Quiet.HasValue)
		{
			if (Projectile != null)
			{
				if (Projectile.GetPart("Projectile") is Projectile projectile)
				{
					int num = BasePenetration.GetValueOrDefault();
					if (!BasePenetration.HasValue)
					{
						num = projectile.BasePenetration;
						BasePenetration = num;
						if (Actor != null)
						{
							MissileWeapon missileWeapon = Launcher?.GetPart("MissileWeapon") as MissileWeapon;
							if (!string.IsNullOrEmpty(missileWeapon.ProjectilePenetrationStat))
							{
								BasePenetration += Actor.StatMod(missileWeapon.ProjectilePenetrationStat);
							}
						}
					}
					if (!PenetrationCap.HasValue)
					{
						PenetrationCap = num + projectile.StrengthPenetration;
					}
					if (BaseDamage == null)
					{
						BaseDamage = projectile.BaseDamage;
					}
					if (Attributes == null)
					{
						Attributes = projectile.Attributes;
					}
					if (!PenetrateCreatures.HasValue)
					{
						PenetrateCreatures = projectile.PenetrateCreatures;
					}
					if (!PenetrateWalls.HasValue)
					{
						PenetrateWalls = projectile.PenetrateWalls;
					}
					if (!Quiet.HasValue)
					{
						Quiet = projectile.Quiet;
					}
				}
			}
			else if (ProjectileBlueprint != null)
			{
				if (!ProjectilePerformance.TryGetValue(ProjectileBlueprint, out var value))
				{
					value = new Perf();
					if (GameObject.createSample(ProjectileBlueprint).GetPart("Projectile") is Projectile projectile2)
					{
						value.BasePenetration = projectile2.BasePenetration;
						value.PenetrationCap = projectile2.BasePenetration + projectile2.StrengthPenetration;
						value.BaseDamage = projectile2.BaseDamage;
						value.Attributes = projectile2.Attributes;
						value.PenetrateCreatures = projectile2.PenetrateCreatures;
						value.PenetrateWalls = projectile2.PenetrateWalls;
						value.Quiet = projectile2.Quiet;
					}
				}
				if (!BasePenetration.HasValue)
				{
					BasePenetration = value.BasePenetration;
					if (Actor != null)
					{
						MissileWeapon missileWeapon2 = Launcher?.GetPart("MissileWeapon") as MissileWeapon;
						if (!string.IsNullOrEmpty(missileWeapon2.ProjectilePenetrationStat))
						{
							BasePenetration += Actor.StatMod(missileWeapon2.ProjectilePenetrationStat);
						}
					}
				}
				if (!PenetrationCap.HasValue)
				{
					PenetrationCap = value.PenetrationCap;
				}
				if (BaseDamage == null)
				{
					BaseDamage = value.BaseDamage;
				}
				if (Attributes == null)
				{
					Attributes = value.Attributes;
				}
				if (!PenetrateCreatures.HasValue)
				{
					PenetrateCreatures = value.PenetrateCreatures;
				}
				if (!PenetrateWalls.HasValue)
				{
					PenetrateWalls = value.PenetrateWalls;
				}
				if (!Quiet.HasValue)
				{
					Quiet = value.Quiet;
				}
			}
		}
		if (Attributes != null && Attributes.Contains("Psionic") && getMissileWeaponPerformanceEvent.Actor != null)
		{
			int num2 = getMissileWeaponPerformanceEvent.Actor.StatMod("Ego");
			BasePenetration = BasePenetration.GetValueOrDefault() + num2;
			PenetrationCap = PenetrationCap.GetValueOrDefault() + num2;
		}
		getMissileWeaponPerformanceEvent.Subject = null;
		getMissileWeaponPerformanceEvent.BaseDamage = BaseDamage;
		getMissileWeaponPerformanceEvent.BasePenetration = BasePenetration.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.PenetrationCap = PenetrationCap ?? getMissileWeaponPerformanceEvent.BasePenetration;
		if (getMissileWeaponPerformanceEvent.BasePenetration > getMissileWeaponPerformanceEvent.PenetrationCap)
		{
			getMissileWeaponPerformanceEvent.BasePenetration = getMissileWeaponPerformanceEvent.PenetrationCap;
		}
		getMissileWeaponPerformanceEvent.Attributes = Attributes;
		getMissileWeaponPerformanceEvent.DamageColor = DamageColor;
		getMissileWeaponPerformanceEvent.PenetrateCreatures = PenetrateCreatures.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.PenetrateWalls = PenetrateWalls.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.Quiet = Quiet.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.DamageRoll = DamageRoll;
		getMissileWeaponPerformanceEvent.Active = Active;
		return getMissileWeaponPerformanceEvent;
	}

	public DieRoll GetDamageRoll()
	{
		if (DamageRoll == null && BaseDamage != null)
		{
			DamageRoll = new DieRoll(BaseDamage);
		}
		return DamageRoll;
	}

	public DieRoll GetPossiblyCachedDamageRoll()
	{
		return DamageRoll ?? BaseDamage.GetCachedDieRoll();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Subject = null;
		Actor = null;
		Launcher = null;
		Projectile = null;
		BaseDamage = null;
		BasePenetration = 0;
		Attributes = null;
		DamageColor = null;
		PenetrateCreatures = false;
		DamageRoll = null;
		Active = false;
		base.Reset();
	}

	public string GetDamageColor()
	{
		if (!string.IsNullOrEmpty(DamageColor))
		{
			return DamageColor;
		}
		return Damage.GetDamageColor(Attributes);
	}

	public static GetMissileWeaponPerformanceEvent GetFor(GameObject Actor, GameObject Launcher, GameObject Projectile = null, int? BasePenetration = null, int? PenetrationCap = null, string BaseDamage = null, string Attributes = null, string DamageColor = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool? Quiet = null, DieRoll DamageRoll = null, string ProjectileBlueprint = null, bool Active = false)
	{
		GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = FromPool(Actor, Launcher, Projectile, BasePenetration, PenetrationCap, BaseDamage, Attributes, DamageColor, PenetrateCreatures, PenetrateWalls, Quiet, DamageRoll, ProjectileBlueprint, Active);
		Projectile = getMissileWeaponPerformanceEvent.Projectile;
		bool flag = true;
		if (flag && GameObject.validate(ref Launcher) && Launcher.WantEvent(ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Launcher;
			if (!Launcher.HandleEvent(getMissileWeaponPerformanceEvent))
			{
				flag = false;
			}
		}
		if (flag && GameObject.validate(ref Projectile) && Projectile.WantEvent(ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Projectile;
			if (!Projectile.HandleEvent(getMissileWeaponPerformanceEvent))
			{
				flag = false;
			}
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Actor;
			if (!Actor.HandleEvent(getMissileWeaponPerformanceEvent))
			{
				flag = false;
			}
		}
		return getMissileWeaponPerformanceEvent;
	}
}

using System.Collections.Generic;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ProjectileMovingEvent : MinEvent
{
	public GameObject Attacker;

	public GameObject Launcher;

	public GameObject Projectile;

	public GameObject Defender;

	public Cell Cell;

	public Cell TargetCell;

	public List<Point> Path;

	public int PathIndex = -1;

	public ScreenBuffer ScreenBuffer;

	public bool Throw;

	public new static readonly int ID;

	private static List<ProjectileMovingEvent> Pool;

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

	static ProjectileMovingEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ProjectileMovingEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ProjectileMovingEvent()
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

	public static ProjectileMovingEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ProjectileMovingEvent FromPool(GameObject Attacker = null, GameObject Launcher = null, GameObject Projectile = null, GameObject Defender = null, Cell Cell = null, Cell TargetCell = null, List<Point> Path = null, int PathIndex = -1, ScreenBuffer ScreenBuffer = null, bool Throw = false)
	{
		ProjectileMovingEvent projectileMovingEvent = FromPool();
		projectileMovingEvent.Attacker = Attacker;
		projectileMovingEvent.Launcher = Launcher;
		projectileMovingEvent.Projectile = Projectile;
		projectileMovingEvent.Defender = Defender;
		projectileMovingEvent.Cell = Cell;
		projectileMovingEvent.TargetCell = TargetCell;
		projectileMovingEvent.Path = Path;
		projectileMovingEvent.PathIndex = PathIndex;
		projectileMovingEvent.ScreenBuffer = ScreenBuffer;
		projectileMovingEvent.Throw = Throw;
		return projectileMovingEvent;
	}

	public override void Reset()
	{
		Attacker = null;
		Launcher = null;
		Projectile = null;
		Defender = null;
		Cell = null;
		TargetCell = null;
		Path = null;
		PathIndex = -1;
		ScreenBuffer = null;
		Throw = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}
}

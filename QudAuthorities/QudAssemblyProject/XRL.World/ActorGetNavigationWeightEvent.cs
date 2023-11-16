using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ActorGetNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID;

	private static List<ActorGetNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static ActorGetNavigationWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ActorGetNavigationWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ActorGetNavigationWeightEvent()
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

	public static ActorGetNavigationWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(Cell Cell, GameObject Actor, ref bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		if (Weight >= 100)
		{
			return Weight;
		}
		if (Cell != null && GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Zone.UnpackNav(Nav, out var Smart, out var Burrower, out var Autoexploring, out var Flying, out var WallWalker, out var IgnoresWalls, out var Swimming, out var Slimewalking, out var Aquatic, out var Polypwalking, out var Strutwalking, out var Juggernaut, out var Reefer);
			ActorGetNavigationWeightEvent actorGetNavigationWeightEvent = FromPool();
			actorGetNavigationWeightEvent.Cell = Cell;
			actorGetNavigationWeightEvent.Actor = Actor;
			actorGetNavigationWeightEvent.Object = null;
			actorGetNavigationWeightEvent.Uncacheable = Uncacheable;
			actorGetNavigationWeightEvent.Weight = Weight;
			actorGetNavigationWeightEvent.PriorWeight = Weight;
			actorGetNavigationWeightEvent.Smart = Smart;
			actorGetNavigationWeightEvent.Burrower = Burrower;
			actorGetNavigationWeightEvent.Autoexploring = Autoexploring;
			actorGetNavigationWeightEvent.Flying = Flying;
			actorGetNavigationWeightEvent.WallWalker = WallWalker;
			actorGetNavigationWeightEvent.IgnoresWalls = IgnoresWalls;
			actorGetNavigationWeightEvent.Swimming = Swimming;
			actorGetNavigationWeightEvent.Slimewalking = Slimewalking;
			actorGetNavigationWeightEvent.Aquatic = Aquatic;
			actorGetNavigationWeightEvent.Polypwalking = Polypwalking;
			actorGetNavigationWeightEvent.Strutwalking = Strutwalking;
			actorGetNavigationWeightEvent.Juggernaut = Juggernaut;
			actorGetNavigationWeightEvent.Reefer = Reefer;
			Actor.HandleEvent(actorGetNavigationWeightEvent);
			Weight = actorGetNavigationWeightEvent.Weight;
			Uncacheable = actorGetNavigationWeightEvent.Uncacheable;
		}
		return Weight;
	}
}

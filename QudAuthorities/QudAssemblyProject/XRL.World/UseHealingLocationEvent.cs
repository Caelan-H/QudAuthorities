using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UseHealingLocationEvent : MinEvent
{
	public GameObject Actor;

	public Cell Cell;

	public new static readonly int ID;

	private static List<UseHealingLocationEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static UseHealingLocationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UseHealingLocationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UseHealingLocationEvent()
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

	public static UseHealingLocationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static UseHealingLocationEvent FromPool(GameObject Actor, Cell Cell)
	{
		UseHealingLocationEvent useHealingLocationEvent = FromPool();
		useHealingLocationEvent.Actor = Actor;
		useHealingLocationEvent.Cell = Cell;
		return useHealingLocationEvent;
	}

	public override void Reset()
	{
		Actor = null;
		Cell = null;
		base.Reset();
	}

	public static void Send(GameObject Actor, Cell Cell)
	{
		Cell.HandleEvent(FromPool(Actor, Cell));
	}
}

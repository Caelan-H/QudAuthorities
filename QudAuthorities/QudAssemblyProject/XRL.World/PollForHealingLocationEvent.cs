using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PollForHealingLocationEvent : MinEvent
{
	public GameObject Actor;

	public Cell Cell;

	public int Value;

	public bool First;

	public new static readonly int ID;

	private static List<PollForHealingLocationEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static PollForHealingLocationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PollForHealingLocationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PollForHealingLocationEvent()
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

	public static PollForHealingLocationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PollForHealingLocationEvent FromPool(GameObject Actor, Cell Cell, bool First = false)
	{
		PollForHealingLocationEvent pollForHealingLocationEvent = FromPool();
		pollForHealingLocationEvent.Actor = Actor;
		pollForHealingLocationEvent.Cell = Cell;
		pollForHealingLocationEvent.Value = 0;
		pollForHealingLocationEvent.First = First;
		return pollForHealingLocationEvent;
	}

	public override void Reset()
	{
		Actor = null;
		Cell = null;
		Value = 0;
		First = false;
		base.Reset();
	}

	public static int GetFor(GameObject Actor, Cell Cell, bool First = false)
	{
		if (Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PollForHealingLocationEvent pollForHealingLocationEvent = FromPool(Actor, Cell, First);
			Cell.HandleEvent(pollForHealingLocationEvent);
			return pollForHealingLocationEvent.Value;
		}
		return 0;
	}
}

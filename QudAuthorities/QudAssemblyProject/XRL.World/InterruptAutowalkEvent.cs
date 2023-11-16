using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class InterruptAutowalkEvent : MinEvent
{
	public GameObject Actor;

	public Cell Cell;

	public GameObject IndicateObject;

	public Cell IndicateCell;

	public new static readonly int ID;

	private static List<InterruptAutowalkEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static InterruptAutowalkEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(InterruptAutowalkEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public InterruptAutowalkEvent()
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

	public static InterruptAutowalkEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static InterruptAutowalkEvent FromPool(GameObject Actor, Cell Cell)
	{
		InterruptAutowalkEvent interruptAutowalkEvent = FromPool();
		interruptAutowalkEvent.Actor = Actor;
		interruptAutowalkEvent.Cell = Cell;
		return interruptAutowalkEvent;
	}

	public override void Reset()
	{
		Actor = null;
		Cell = null;
		IndicateObject = null;
		IndicateCell = null;
		base.Reset();
	}

	public static bool Check(GameObject Actor, Cell Cell, out GameObject IndicateObject, out Cell IndicateCell)
	{
		IndicateObject = null;
		IndicateCell = null;
		if (Cell == null)
		{
			return true;
		}
		bool flag = true;
		if (flag && Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			InterruptAutowalkEvent interruptAutowalkEvent = FromPool();
			interruptAutowalkEvent.Actor = Actor;
			interruptAutowalkEvent.Cell = Cell;
			flag = Cell.HandleEvent(interruptAutowalkEvent);
			IndicateObject = interruptAutowalkEvent.IndicateObject;
			IndicateCell = interruptAutowalkEvent.IndicateCell;
		}
		return !flag;
	}

	public static bool Check(GameObject Actor, Cell Cell)
	{
		GameObject IndicateObject;
		Cell IndicateCell;
		return Check(Actor, Cell, out IndicateObject, out IndicateCell);
	}
}

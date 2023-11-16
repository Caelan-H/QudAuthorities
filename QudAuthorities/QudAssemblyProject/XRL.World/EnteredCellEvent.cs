using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EnteredCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<EnteredCellEvent> Pool;

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

	static EnteredCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EnteredCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EnteredCellEvent()
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

	public static EnteredCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnteredCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		EnteredCellEvent enteredCellEvent = FromPool();
		enteredCellEvent.Object = Object;
		enteredCellEvent.Cell = Cell;
		enteredCellEvent.Forced = Forced;
		enteredCellEvent.System = System;
		enteredCellEvent.IgnoreGravity = IgnoreGravity;
		enteredCellEvent.NoStack = NoStack;
		enteredCellEvent.Direction = Direction;
		enteredCellEvent.Type = Type;
		enteredCellEvent.Dragging = Dragging;
		return enteredCellEvent;
	}
}

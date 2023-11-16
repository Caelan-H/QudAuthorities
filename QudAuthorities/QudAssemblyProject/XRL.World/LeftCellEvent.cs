using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class LeftCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<LeftCellEvent> Pool;

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

	static LeftCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(LeftCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public LeftCellEvent()
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

	public static LeftCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeftCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		LeftCellEvent leftCellEvent = FromPool();
		leftCellEvent.Object = Object;
		leftCellEvent.Cell = Cell;
		leftCellEvent.Forced = Forced;
		leftCellEvent.System = System;
		leftCellEvent.IgnoreGravity = IgnoreGravity;
		leftCellEvent.NoStack = NoStack;
		leftCellEvent.Direction = Direction;
		leftCellEvent.Type = Type;
		leftCellEvent.Dragging = Dragging;
		return leftCellEvent;
	}
}

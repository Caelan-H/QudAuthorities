using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class LeaveCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<LeaveCellEvent> Pool;

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

	static LeaveCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(LeaveCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public LeaveCellEvent()
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

	public static LeaveCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeaveCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		LeaveCellEvent leaveCellEvent = FromPool();
		leaveCellEvent.Object = Object;
		leaveCellEvent.Cell = Cell;
		leaveCellEvent.Forced = Forced;
		leaveCellEvent.System = System;
		leaveCellEvent.IgnoreGravity = IgnoreGravity;
		leaveCellEvent.NoStack = NoStack;
		leaveCellEvent.Direction = Direction;
		leaveCellEvent.Type = Type;
		leaveCellEvent.Dragging = Dragging;
		return leaveCellEvent;
	}
}

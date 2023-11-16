using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class LeavingCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<LeavingCellEvent> Pool;

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

	static LeavingCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(LeavingCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public LeavingCellEvent()
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

	public static LeavingCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeavingCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		LeavingCellEvent leavingCellEvent = FromPool();
		leavingCellEvent.Object = Object;
		leavingCellEvent.Cell = Cell;
		leavingCellEvent.Forced = Forced;
		leavingCellEvent.System = System;
		leavingCellEvent.IgnoreGravity = IgnoreGravity;
		leavingCellEvent.NoStack = NoStack;
		leavingCellEvent.Direction = Direction;
		leavingCellEvent.Type = Type;
		leavingCellEvent.Dragging = Dragging;
		return leavingCellEvent;
	}
}

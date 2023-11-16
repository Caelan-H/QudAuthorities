using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EnteringCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<EnteringCellEvent> Pool;

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

	static EnteringCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EnteringCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EnteringCellEvent()
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

	public static EnteringCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnteringCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		EnteringCellEvent enteringCellEvent = FromPool();
		enteringCellEvent.Object = Object;
		enteringCellEvent.Cell = Cell;
		enteringCellEvent.Forced = Forced;
		enteringCellEvent.System = System;
		enteringCellEvent.IgnoreGravity = IgnoreGravity;
		enteringCellEvent.NoStack = NoStack;
		enteringCellEvent.Direction = Direction;
		enteringCellEvent.Type = Type;
		enteringCellEvent.Dragging = Dragging;
		return enteringCellEvent;
	}
}

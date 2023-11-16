using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EnterCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<EnterCellEvent> Pool;

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

	static EnterCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EnterCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EnterCellEvent()
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

	public static EnterCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnterCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		EnterCellEvent enterCellEvent = FromPool();
		enterCellEvent.Object = Object;
		enterCellEvent.Cell = Cell;
		enterCellEvent.Forced = Forced;
		enterCellEvent.System = System;
		enterCellEvent.IgnoreGravity = IgnoreGravity;
		enterCellEvent.NoStack = NoStack;
		enterCellEvent.Direction = Direction;
		enterCellEvent.Type = Type;
		enterCellEvent.Dragging = Dragging;
		return enterCellEvent;
	}
}

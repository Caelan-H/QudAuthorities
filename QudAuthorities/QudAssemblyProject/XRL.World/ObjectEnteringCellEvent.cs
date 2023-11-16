using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectEnteringCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<ObjectEnteringCellEvent> Pool;

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

	static ObjectEnteringCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectEnteringCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectEnteringCellEvent()
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

	public static ObjectEnteringCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectEnteringCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		ObjectEnteringCellEvent objectEnteringCellEvent = FromPool();
		objectEnteringCellEvent.Object = Object;
		objectEnteringCellEvent.Cell = Cell;
		objectEnteringCellEvent.Forced = Forced;
		objectEnteringCellEvent.System = System;
		objectEnteringCellEvent.IgnoreGravity = IgnoreGravity;
		objectEnteringCellEvent.NoStack = NoStack;
		objectEnteringCellEvent.Direction = Direction;
		objectEnteringCellEvent.Type = Type;
		objectEnteringCellEvent.Dragging = Dragging;
		return objectEnteringCellEvent;
	}
}

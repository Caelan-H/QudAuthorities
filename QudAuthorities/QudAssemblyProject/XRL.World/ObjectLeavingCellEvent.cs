using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectLeavingCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<ObjectLeavingCellEvent> Pool;

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

	static ObjectLeavingCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectLeavingCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectLeavingCellEvent()
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

	public static ObjectLeavingCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectLeavingCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		ObjectLeavingCellEvent objectLeavingCellEvent = FromPool();
		objectLeavingCellEvent.Object = Object;
		objectLeavingCellEvent.Cell = Cell;
		objectLeavingCellEvent.Forced = Forced;
		objectLeavingCellEvent.System = System;
		objectLeavingCellEvent.IgnoreGravity = IgnoreGravity;
		objectLeavingCellEvent.NoStack = NoStack;
		objectLeavingCellEvent.Direction = Direction;
		objectLeavingCellEvent.Type = Type;
		objectLeavingCellEvent.Dragging = Dragging;
		return objectLeavingCellEvent;
	}
}

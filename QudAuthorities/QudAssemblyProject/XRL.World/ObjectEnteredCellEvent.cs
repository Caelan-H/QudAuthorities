using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectEnteredCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<ObjectEnteredCellEvent> Pool;

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

	static ObjectEnteredCellEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectEnteredCellEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectEnteredCellEvent()
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

	public static ObjectEnteredCellEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectEnteredCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		ObjectEnteredCellEvent objectEnteredCellEvent = FromPool();
		objectEnteredCellEvent.Object = Object;
		objectEnteredCellEvent.Cell = Cell;
		objectEnteredCellEvent.Forced = Forced;
		objectEnteredCellEvent.System = System;
		objectEnteredCellEvent.IgnoreGravity = IgnoreGravity;
		objectEnteredCellEvent.NoStack = NoStack;
		objectEnteredCellEvent.Direction = Direction;
		objectEnteredCellEvent.Type = Type;
		objectEnteredCellEvent.Dragging = Dragging;
		return objectEnteredCellEvent;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectStoppedFlyingEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<ObjectStoppedFlyingEvent> Pool;

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

	static ObjectStoppedFlyingEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectStoppedFlyingEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectStoppedFlyingEvent()
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

	public static ObjectStoppedFlyingEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectStoppedFlyingEvent FromPool(GameObject Object, Cell Cell)
	{
		ObjectStoppedFlyingEvent objectStoppedFlyingEvent = FromPool();
		objectStoppedFlyingEvent.Object = Object;
		objectStoppedFlyingEvent.Cell = Cell;
		objectStoppedFlyingEvent.Forced = false;
		objectStoppedFlyingEvent.System = false;
		objectStoppedFlyingEvent.IgnoreGravity = false;
		objectStoppedFlyingEvent.NoStack = false;
		objectStoppedFlyingEvent.Direction = null;
		objectStoppedFlyingEvent.Type = null;
		objectStoppedFlyingEvent.Dragging = null;
		return objectStoppedFlyingEvent;
	}

	public static bool SendFor(GameObject Object)
	{
		ObjectStoppedFlyingEvent objectStoppedFlyingEvent = FromPool(Object, Object.CurrentCell);
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(objectStoppedFlyingEvent))
		{
			return false;
		}
		if (objectStoppedFlyingEvent.Cell != null && objectStoppedFlyingEvent.Cell.WantEvent(ID, MinEvent.CascadeLevel) && !objectStoppedFlyingEvent.Cell.HandleEvent(objectStoppedFlyingEvent))
		{
			return false;
		}
		return true;
	}
}

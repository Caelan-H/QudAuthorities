using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectStartedFlyingEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<ObjectStartedFlyingEvent> Pool;

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

	static ObjectStartedFlyingEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectStartedFlyingEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectStartedFlyingEvent()
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

	public static ObjectStartedFlyingEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectStartedFlyingEvent FromPool(GameObject Object, Cell Cell)
	{
		ObjectStartedFlyingEvent objectStartedFlyingEvent = FromPool();
		objectStartedFlyingEvent.Object = Object;
		objectStartedFlyingEvent.Cell = Cell;
		objectStartedFlyingEvent.Forced = false;
		objectStartedFlyingEvent.System = false;
		objectStartedFlyingEvent.IgnoreGravity = false;
		objectStartedFlyingEvent.NoStack = false;
		objectStartedFlyingEvent.Direction = null;
		objectStartedFlyingEvent.Type = null;
		objectStartedFlyingEvent.Dragging = null;
		return objectStartedFlyingEvent;
	}

	public static bool SendFor(GameObject Object)
	{
		ObjectStartedFlyingEvent objectStartedFlyingEvent = FromPool(Object, Object.CurrentCell);
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(objectStartedFlyingEvent))
		{
			return false;
		}
		if (objectStartedFlyingEvent.Cell != null && objectStartedFlyingEvent.Cell.WantEvent(ID, MinEvent.CascadeLevel) && !objectStartedFlyingEvent.Cell.HandleEvent(objectStartedFlyingEvent))
		{
			return false;
		}
		return true;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class FellDownEvent : IObjectCellInteractionEvent
{
	public Cell FromCell;

	public int Distance;

	public new static readonly int ID;

	private static List<FellDownEvent> Pool;

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

	static FellDownEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(FellDownEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public FellDownEvent()
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

	public override void Reset()
	{
		Distance = 0;
		FromCell = null;
		base.Reset();
	}

	public static FellDownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static FellDownEvent FromPool(GameObject Object, Cell Cell, Cell FromCell, int Distance)
	{
		FellDownEvent fellDownEvent = FromPool();
		fellDownEvent.Object = Object;
		fellDownEvent.Cell = Cell;
		fellDownEvent.Forced = false;
		fellDownEvent.System = false;
		fellDownEvent.IgnoreGravity = false;
		fellDownEvent.NoStack = false;
		fellDownEvent.Direction = "D";
		fellDownEvent.Type = null;
		fellDownEvent.Dragging = null;
		fellDownEvent.Distance = Distance;
		return fellDownEvent;
	}

	public static void Send(GameObject Object, Cell Cell, Cell FromCell, int Distance)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("FellDown"))
		{
			Event @event = Event.New("FellDown");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Cell", Cell);
			@event.SetParameter("FromCell", FromCell);
			@event.SetParameter("Distance", Distance);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Cell, FromCell, Distance));
		}
	}
}

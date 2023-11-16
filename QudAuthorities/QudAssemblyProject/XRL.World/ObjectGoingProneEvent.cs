using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ObjectGoingProneEvent : IObjectCellInteractionEvent
{
	public bool Voluntary;

	public new static readonly int ID;

	private static List<ObjectGoingProneEvent> Pool;

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

	static ObjectGoingProneEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ObjectGoingProneEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ObjectGoingProneEvent()
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
		Voluntary = false;
		base.Reset();
	}

	public static ObjectGoingProneEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ObjectGoingProneEvent FromPool(GameObject Object, Cell Cell, bool Voluntary = false)
	{
		ObjectGoingProneEvent objectGoingProneEvent = FromPool();
		objectGoingProneEvent.Object = Object;
		objectGoingProneEvent.Cell = Cell;
		objectGoingProneEvent.Forced = !Voluntary;
		objectGoingProneEvent.System = false;
		objectGoingProneEvent.IgnoreGravity = false;
		objectGoingProneEvent.NoStack = false;
		objectGoingProneEvent.Direction = ".";
		objectGoingProneEvent.Type = null;
		objectGoingProneEvent.Dragging = null;
		objectGoingProneEvent.Voluntary = Voluntary;
		return objectGoingProneEvent;
	}

	public static void Send(GameObject Object, Cell Cell, bool Voluntary = false)
	{
		if (Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Cell.HandleEvent(FromPool(Object, Cell, Voluntary));
		}
		if (Cell.HasObjectWithRegisteredEvent("ObjectGoingProne"))
		{
			Cell.FireEvent(Event.New("ObjectGoingProne", "Object", Object, "Cell", Cell, "Voluntary", Voluntary ? 1 : 0));
		}
	}
}

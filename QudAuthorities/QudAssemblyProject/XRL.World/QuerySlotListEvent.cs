using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class QuerySlotListEvent : MinEvent
{
	public GameObject Subject;

	public GameObject Object;

	public List<BodyPart> SlotList = new List<BodyPart>();

	public new static readonly int ID;

	private static List<QuerySlotListEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static QuerySlotListEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(QuerySlotListEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public QuerySlotListEvent()
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
		Subject = null;
		Object = null;
		SlotList.Clear();
		base.Reset();
	}

	public static QuerySlotListEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static List<BodyPart> GetFor(GameObject Subject, GameObject Object)
	{
		bool flag = true;
		QuerySlotListEvent querySlotListEvent = FromPool();
		querySlotListEvent.Subject = Subject;
		querySlotListEvent.Object = Object;
		querySlotListEvent.SlotList.Clear();
		if (flag && GameObject.validate(ref Subject) && Subject.HasRegisteredEvent("QuerySlotList"))
		{
			Event @event = Event.New("QuerySlotList");
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Object", Object);
			@event.SetParameter("SlotList", querySlotListEvent.SlotList);
			flag = Subject.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Subject) && Subject.WantEvent(ID, MinEvent.CascadeLevel))
		{
			flag = Subject.HandleEvent(querySlotListEvent);
		}
		return querySlotListEvent.SlotList;
	}
}

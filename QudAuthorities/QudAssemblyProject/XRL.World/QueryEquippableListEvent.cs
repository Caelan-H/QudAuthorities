using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class QueryEquippableListEvent : IActOnItemEvent
{
	public List<GameObject> List = new List<GameObject>();

	public string SlotType;

	public new static readonly int ID;

	private static List<QueryEquippableListEvent> Pool;

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

	static QueryEquippableListEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(QueryEquippableListEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public QueryEquippableListEvent()
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

	public static QueryEquippableListEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static QueryEquippableListEvent FromPool(GameObject Actor, GameObject Item, string SlotType)
	{
		QueryEquippableListEvent queryEquippableListEvent = FromPool();
		queryEquippableListEvent.Actor = Actor;
		queryEquippableListEvent.Item = Item;
		queryEquippableListEvent.SlotType = SlotType;
		return queryEquippableListEvent;
	}

	public override void Reset()
	{
		List.Clear();
		SlotType = null;
		base.Reset();
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetContentsEvent : MinEvent
{
	public GameObject Object;

	public List<GameObject> Objects = new List<GameObject>();

	public new static readonly int ID;

	private static List<GetContentsEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetContentsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetContentsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetContentsEvent()
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

	public static GetContentsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetContentsEvent FromPool(GameObject Object)
	{
		GetContentsEvent getContentsEvent = FromPool();
		getContentsEvent.Object = Object;
		getContentsEvent.Objects.Clear();
		return getContentsEvent;
	}

	public override void Reset()
	{
		Object = null;
		Objects.Clear();
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static List<GameObject> GetFor(GameObject Object)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (Object.WantEvent(ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(Object);
			Object.HandleEvent(getContentsEvent);
			list.AddRange(getContentsEvent.Objects);
		}
		return list;
	}

	public static List<GameObject> GetFor(Cell C)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (C.WantEvent(ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(null);
			C.HandleEvent(getContentsEvent);
			list.AddRange(getContentsEvent.Objects);
		}
		return list;
	}

	public static List<GameObject> GetFor(Zone Z)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (Z.WantEvent(ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(null);
			Z.HandleEvent(getContentsEvent);
			list.AddRange(getContentsEvent.Objects);
		}
		return list;
	}
}

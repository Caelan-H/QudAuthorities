using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class QueryDrawEvent : MinEvent
{
	public GameObject Object;

	public int Draw;

	public bool BroadcastDrawDone;

	public int BroadcastDraw;

	public int HighTransmitRate;

	public new static readonly int ID;

	private static List<QueryDrawEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static QueryDrawEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(QueryDrawEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public QueryDrawEvent()
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

	public static QueryDrawEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static QueryDrawEvent FromPool(GameObject Object)
	{
		QueryDrawEvent queryDrawEvent = FromPool();
		queryDrawEvent.Object = Object;
		queryDrawEvent.Draw = 0;
		queryDrawEvent.BroadcastDrawDone = false;
		queryDrawEvent.BroadcastDraw = 0;
		queryDrawEvent.HighTransmitRate = 0;
		return queryDrawEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Object = null;
		Draw = 0;
		BroadcastDrawDone = false;
		BroadcastDraw = 0;
		HighTransmitRate = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Object)
	{
		if (Object != null && Object.WantEvent(ID, CascadeLevel))
		{
			QueryDrawEvent queryDrawEvent = FromPool(Object);
			Object.HandleEvent(queryDrawEvent);
			return queryDrawEvent.Draw;
		}
		return 0;
	}

	public static int GetFor(List<GameObject> Objects)
	{
		QueryDrawEvent queryDrawEvent = null;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				GameObject gameObject = Objects[i];
				if (gameObject.WantEvent(ID, CascadeLevel))
				{
					if (queryDrawEvent == null)
					{
						queryDrawEvent = FromPool(gameObject);
					}
					else
					{
						queryDrawEvent.Object = gameObject;
					}
					gameObject.HandleEvent(queryDrawEvent);
				}
			}
		}
		return queryDrawEvent?.Draw ?? 0;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class QueryBroadcastDrawEvent : MinEvent
{
	public Zone Zone;

	public int Draw;

	public new static readonly int ID;

	private static List<QueryBroadcastDrawEvent> Pool;

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

	static QueryBroadcastDrawEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(QueryBroadcastDrawEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public QueryBroadcastDrawEvent()
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

	public static QueryBroadcastDrawEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static QueryBroadcastDrawEvent FromPool(Zone Zone)
	{
		QueryBroadcastDrawEvent queryBroadcastDrawEvent = FromPool();
		queryBroadcastDrawEvent.Zone = Zone;
		queryBroadcastDrawEvent.Draw = 0;
		return queryBroadcastDrawEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Zone = null;
		Draw = 0;
		base.Reset();
	}

	public static int GetFor(Zone Z)
	{
		if (Z != null && Z.WantEvent(ID, CascadeLevel))
		{
			QueryBroadcastDrawEvent queryBroadcastDrawEvent = FromPool(Z);
			Z.HandleEvent(queryBroadcastDrawEvent);
			return queryBroadcastDrawEvent.Draw;
		}
		return 0;
	}
}

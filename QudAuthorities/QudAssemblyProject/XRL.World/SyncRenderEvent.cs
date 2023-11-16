using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class SyncRenderEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<SyncRenderEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static SyncRenderEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(SyncRenderEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public SyncRenderEvent()
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

	public static SyncRenderEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static SyncRenderEvent FromPool(GameObject Object)
	{
		SyncRenderEvent syncRenderEvent = FromPool();
		syncRenderEvent.Object = Object;
		return syncRenderEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}
}

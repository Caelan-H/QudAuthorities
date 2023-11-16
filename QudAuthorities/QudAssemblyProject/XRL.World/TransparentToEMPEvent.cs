using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TransparentToEMPEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<TransparentToEMPEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static TransparentToEMPEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TransparentToEMPEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TransparentToEMPEvent()
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

	public static TransparentToEMPEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static TransparentToEMPEvent FromPool(GameObject Object)
	{
		TransparentToEMPEvent transparentToEMPEvent = FromPool();
		transparentToEMPEvent.Object = Object;
		return transparentToEMPEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}
}

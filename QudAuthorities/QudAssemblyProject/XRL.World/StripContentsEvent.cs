using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class StripContentsEvent : MinEvent
{
	public GameObject Object;

	public bool KeepNatural;

	public bool Silent;

	public new static readonly int ID;

	private static List<StripContentsEvent> Pool;

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

	static StripContentsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(StripContentsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public StripContentsEvent()
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

	public static StripContentsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static StripContentsEvent FromPool(GameObject Object, bool KeepNatural = false, bool Silent = false)
	{
		StripContentsEvent stripContentsEvent = FromPool();
		stripContentsEvent.Object = Object;
		stripContentsEvent.KeepNatural = KeepNatural;
		stripContentsEvent.Silent = Silent;
		return stripContentsEvent;
	}

	public override void Reset()
	{
		Object = null;
		KeepNatural = false;
		Silent = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}
}

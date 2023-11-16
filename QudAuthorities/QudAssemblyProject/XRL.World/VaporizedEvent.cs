using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class VaporizedEvent : MinEvent
{
	public GameObject Object;

	public GameObject By;

	public new static readonly int ID;

	private static List<VaporizedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static VaporizedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(VaporizedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public VaporizedEvent()
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

	public static VaporizedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static VaporizedEvent FromPool(GameObject Object, GameObject By = null)
	{
		VaporizedEvent vaporizedEvent = FromPool();
		vaporizedEvent.Object = Object;
		vaporizedEvent.By = By;
		return vaporizedEvent;
	}

	public override void Reset()
	{
		Object = null;
		By = null;
		base.Reset();
	}

	public static bool Check(GameObject Object, GameObject By = null)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, By)))
		{
			return false;
		}
		if (Object.HasRegisteredEvent("Vaporized") && !Object.FireEvent(Event.New("Vaporized", "Object", Object, "By", By)))
		{
			return false;
		}
		return true;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ContainsEvent : MinEvent
{
	public GameObject Container;

	public GameObject Object;

	public new static readonly int ID;

	private static List<ContainsEvent> Pool;

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

	static ContainsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ContainsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ContainsEvent()
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

	public static ContainsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ContainsEvent FromPool(GameObject Container, GameObject Object)
	{
		ContainsEvent containsEvent = FromPool();
		containsEvent.Container = Container;
		containsEvent.Object = Object;
		return containsEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Container = null;
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Container, GameObject Object)
	{
		if (Container.WantEvent(ID, CascadeLevel) && !Container.HandleEvent(FromPool(Container, Object)))
		{
			return true;
		}
		return false;
	}
}

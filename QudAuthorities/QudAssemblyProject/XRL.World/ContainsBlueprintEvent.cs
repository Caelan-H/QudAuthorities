using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ContainsBlueprintEvent : MinEvent
{
	public GameObject Container;

	public string Blueprint;

	public GameObject Object;

	public new static readonly int ID;

	private static List<ContainsBlueprintEvent> Pool;

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

	static ContainsBlueprintEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ContainsBlueprintEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ContainsBlueprintEvent()
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

	public static ContainsBlueprintEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ContainsBlueprintEvent FromPool(GameObject Container, string Blueprint)
	{
		ContainsBlueprintEvent containsBlueprintEvent = FromPool();
		containsBlueprintEvent.Container = Container;
		containsBlueprintEvent.Blueprint = Blueprint;
		containsBlueprintEvent.Object = null;
		return containsBlueprintEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Container = null;
		Blueprint = null;
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Container, string Blueprint)
	{
		if (Container.WantEvent(ID, CascadeLevel) && !Container.HandleEvent(FromPool(Container, Blueprint)))
		{
			return true;
		}
		return false;
	}

	public static GameObject Find(GameObject Container, string Blueprint)
	{
		if (Container.WantEvent(ID, CascadeLevel))
		{
			ContainsBlueprintEvent containsBlueprintEvent = FromPool(Container, Blueprint);
			if (!Container.HandleEvent(containsBlueprintEvent))
			{
				return containsBlueprintEvent.Object;
			}
		}
		return null;
	}
}

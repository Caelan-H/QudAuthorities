using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ContainsAnyBlueprintEvent : MinEvent
{
	public GameObject Container;

	public List<string> Blueprints;

	public GameObject Object;

	public new static readonly int ID;

	private static List<ContainsAnyBlueprintEvent> Pool;

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

	static ContainsAnyBlueprintEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ContainsAnyBlueprintEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ContainsAnyBlueprintEvent()
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

	public static ContainsAnyBlueprintEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ContainsAnyBlueprintEvent FromPool(GameObject Container, List<string> Blueprints)
	{
		ContainsAnyBlueprintEvent containsAnyBlueprintEvent = FromPool();
		containsAnyBlueprintEvent.Container = Container;
		containsAnyBlueprintEvent.Blueprints = Blueprints;
		containsAnyBlueprintEvent.Object = null;
		return containsAnyBlueprintEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Container = null;
		Blueprints = null;
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Container, List<string> Blueprints)
	{
		if (Container.WantEvent(ID, CascadeLevel) && !Container.HandleEvent(FromPool(Container, Blueprints)))
		{
			return true;
		}
		return false;
	}

	public static GameObject Find(GameObject Container, List<string> Blueprints)
	{
		if (Container.WantEvent(ID, CascadeLevel))
		{
			ContainsAnyBlueprintEvent containsAnyBlueprintEvent = FromPool(Container, Blueprints);
			if (!Container.HandleEvent(containsAnyBlueprintEvent))
			{
				return containsAnyBlueprintEvent.Object;
			}
		}
		return null;
	}
}

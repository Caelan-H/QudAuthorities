using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AdjustVisibilityRadiusEvent : MinEvent
{
	public GameObject Object;

	public int Radius;

	public new static readonly int ID;

	private static List<AdjustVisibilityRadiusEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static AdjustVisibilityRadiusEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AdjustVisibilityRadiusEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AdjustVisibilityRadiusEvent()
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

	public override void Reset()
	{
		Object = null;
		Radius = 0;
		base.Reset();
	}

	public static AdjustVisibilityRadiusEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AdjustVisibilityRadiusEvent FromPool(GameObject Object, int Radius)
	{
		AdjustVisibilityRadiusEvent adjustVisibilityRadiusEvent = FromPool();
		adjustVisibilityRadiusEvent.Object = Object;
		adjustVisibilityRadiusEvent.Radius = Radius;
		return adjustVisibilityRadiusEvent;
	}

	public static int GetFor(GameObject Actor, int Radius = 80)
	{
		if (Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AdjustVisibilityRadiusEvent adjustVisibilityRadiusEvent = FromPool(Actor, Radius);
			Actor.HandleEvent(adjustVisibilityRadiusEvent);
			Radius = adjustVisibilityRadiusEvent.Radius;
		}
		return Radius;
	}
}

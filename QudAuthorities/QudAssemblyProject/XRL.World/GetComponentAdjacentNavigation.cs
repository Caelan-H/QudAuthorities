using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetComponentAdjacentNavigationWeightEvent : IAdjacentNavigationWeightEvent
{
	public new static readonly int ID;

	private static List<GetComponentAdjacentNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static GetComponentAdjacentNavigationWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetComponentAdjacentNavigationWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetComponentAdjacentNavigationWeightEvent()
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

	public static GetComponentAdjacentNavigationWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetComponentAdjacentNavigationWeightEvent FromPool(INavigationWeightEvent Source)
	{
		GetComponentAdjacentNavigationWeightEvent getComponentAdjacentNavigationWeightEvent = FromPool();
		Source.ApplyTo(getComponentAdjacentNavigationWeightEvent);
		return getComponentAdjacentNavigationWeightEvent;
	}

	public static void Process(GameObject Object, INavigationWeightEvent ParentEvent)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetComponentAdjacentNavigationWeightEvent getComponentAdjacentNavigationWeightEvent = FromPool(ParentEvent);
			getComponentAdjacentNavigationWeightEvent.Object = Object;
			getComponentAdjacentNavigationWeightEvent.PriorWeight = getComponentAdjacentNavigationWeightEvent.Weight;
			Object.HandleEvent(getComponentAdjacentNavigationWeightEvent);
			getComponentAdjacentNavigationWeightEvent.ApplyTo(ParentEvent);
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetComponentNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID;

	private static List<GetComponentNavigationWeightEvent> Pool;

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

	static GetComponentNavigationWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetComponentNavigationWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetComponentNavigationWeightEvent()
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

	public static GetComponentNavigationWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetComponentNavigationWeightEvent FromPool(INavigationWeightEvent Source)
	{
		GetComponentNavigationWeightEvent getComponentNavigationWeightEvent = FromPool();
		Source.ApplyTo(getComponentNavigationWeightEvent);
		return getComponentNavigationWeightEvent;
	}

	public static void Process(GameObject Object, INavigationWeightEvent ParentEvent)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetComponentNavigationWeightEvent getComponentNavigationWeightEvent = FromPool(ParentEvent);
			getComponentNavigationWeightEvent.Object = Object;
			getComponentNavigationWeightEvent.PriorWeight = getComponentNavigationWeightEvent.Weight;
			Object.HandleEvent(getComponentNavigationWeightEvent);
			getComponentNavigationWeightEvent.ApplyTo(ParentEvent);
		}
	}
}

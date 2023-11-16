using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMaxCarriedWeightEvent : IWeightEvent
{
	public new static readonly int ID;

	private static List<GetMaxCarriedWeightEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

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

	static GetMaxCarriedWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("GetMaxCarriedWeightEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMaxCarriedWeightEvent()
	{
		base.ID = ID;
	}

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetMaxCarriedWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMaxCarriedWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		GetMaxCarriedWeightEvent getMaxCarriedWeightEvent = FromPool();
		getMaxCarriedWeightEvent.Object = Object;
		getMaxCarriedWeightEvent.BaseWeight = BaseWeight;
		getMaxCarriedWeightEvent.Weight = Weight;
		return getMaxCarriedWeightEvent;
	}

	public static GetMaxCarriedWeightEvent FromPool(GameObject Object, double Weight)
	{
		GetMaxCarriedWeightEvent getMaxCarriedWeightEvent = FromPool();
		getMaxCarriedWeightEvent.Object = Object;
		getMaxCarriedWeightEvent.BaseWeight = Weight;
		getMaxCarriedWeightEvent.Weight = Weight;
		return getMaxCarriedWeightEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int GetFor(GameObject Object, double BaseWeight)
	{
		double num = BaseWeight;
		if (Object != null)
		{
			if (Object.HasRegisteredEvent("GetMaxWeight"))
			{
				Event @event = Event.New("GetMaxWeight", "Weight", (int)num);
				if (!Object.FireEvent(@event))
				{
					return @event.GetIntParameter("Weight");
				}
				num = @event.GetIntParameter("Weight");
			}
			if (Object.WantEvent(ID, CascadeLevel))
			{
				GetMaxCarriedWeightEvent getMaxCarriedWeightEvent = FromPool(Object, BaseWeight, num);
				if (!Object.HandleEvent(getMaxCarriedWeightEvent))
				{
					return (int)getMaxCarriedWeightEvent.Weight;
				}
				num = getMaxCarriedWeightEvent.Weight;
			}
		}
		return (int)num;
	}
}

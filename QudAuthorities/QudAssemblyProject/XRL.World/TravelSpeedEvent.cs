using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TravelSpeedEvent : ITravelEvent
{
	public new static readonly int ID;

	private static List<TravelSpeedEvent> Pool;

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

	static TravelSpeedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TravelSpeedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TravelSpeedEvent()
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

	public static TravelSpeedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("TravelSpeed"))
		{
			Event @event = Event.New("TravelSpeed");
			@event.SetParameter("Object", Actor);
			@event.SetParameter("TravelClass", TravelClass);
			@event.SetParameter("PercentageBonus", PercentageBonus);
			flag = Actor.FireEvent(@event);
			PercentageBonus = @event.GetIntParameter("PercentageBonus");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			TravelSpeedEvent travelSpeedEvent = FromPool();
			travelSpeedEvent.Actor = Actor;
			travelSpeedEvent.TravelClass = TravelClass;
			travelSpeedEvent.PercentageBonus = PercentageBonus;
			flag = Actor.HandleEvent(travelSpeedEvent);
			PercentageBonus = travelSpeedEvent.PercentageBonus;
		}
		return PercentageBonus;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetLostChanceEvent : ITravelEvent
{
	public int DefaultLimit;

	public bool OverrideDefaultLimit;

	public new static readonly int ID;

	private static List<GetLostChanceEvent> Pool;

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

	static GetLostChanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetLostChanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetLostChanceEvent()
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
		OverrideDefaultLimit = false;
		base.Reset();
	}

	public static GetLostChanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0, int DefaultLimit = 95, bool OverrideDefaultLimit = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("GetLostChance"))
		{
			Event @event = Event.New("GetLostChance");
			@event.SetParameter("Object", Actor);
			@event.SetParameter("TravelClass", TravelClass);
			@event.SetParameter("PercentageBonus", PercentageBonus);
			@event.SetFlag("OverrideDefaultLimit", OverrideDefaultLimit);
			flag = Actor.FireEvent(@event);
			PercentageBonus = @event.GetIntParameter("PercentageBonus");
			OverrideDefaultLimit = @event.HasFlag("OverrideDefaultLimit");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			GetLostChanceEvent getLostChanceEvent = FromPool();
			getLostChanceEvent.Actor = Actor;
			getLostChanceEvent.TravelClass = TravelClass;
			getLostChanceEvent.PercentageBonus = PercentageBonus;
			flag = Actor.HandleEvent(getLostChanceEvent);
			PercentageBonus = getLostChanceEvent.PercentageBonus;
			OverrideDefaultLimit = getLostChanceEvent.OverrideDefaultLimit;
		}
		if (!OverrideDefaultLimit && PercentageBonus > DefaultLimit)
		{
			PercentageBonus = DefaultLimit;
		}
		return PercentageBonus;
	}
}

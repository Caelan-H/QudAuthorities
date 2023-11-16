using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EncounterChanceEvent : ITravelEvent
{
	public EncounterEntry Encounter;

	public new static readonly int ID;

	private static List<EncounterChanceEvent> Pool;

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

	static EncounterChanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EncounterChanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EncounterChanceEvent()
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
		Encounter = null;
		base.Reset();
	}

	public static EncounterChanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0, EncounterEntry Encounter = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("EncounterChance"))
		{
			Event @event = Event.New("EncounterChance");
			@event.SetParameter("Object", Actor);
			@event.SetParameter("TravelClass", TravelClass);
			@event.SetParameter("PercentageBonus", PercentageBonus);
			@event.SetParameter("Encounter", Encounter);
			flag = Actor.FireEvent(@event);
			PercentageBonus = @event.GetIntParameter("PercentageBonus");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			EncounterChanceEvent encounterChanceEvent = FromPool();
			encounterChanceEvent.Actor = Actor;
			encounterChanceEvent.TravelClass = TravelClass;
			encounterChanceEvent.PercentageBonus = PercentageBonus;
			encounterChanceEvent.Encounter = Encounter;
			flag = Actor.HandleEvent(encounterChanceEvent);
			PercentageBonus = encounterChanceEvent.PercentageBonus;
		}
		return PercentageBonus;
	}
}

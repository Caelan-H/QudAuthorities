using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRunningBehaviorEvent : MinEvent
{
	public GameObject Actor;

	public string AbilityName;

	public string AbilityDescription;

	public string Verb;

	public string EffectDisplayName;

	public string EffectMessageName;

	public int EffectDuration;

	public bool SpringingEffective;

	public int Priority;

	public new static readonly int ID;

	private static List<GetRunningBehaviorEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetRunningBehaviorEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetRunningBehaviorEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetRunningBehaviorEvent()
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
		Actor = null;
		AbilityName = null;
		AbilityDescription = null;
		Verb = null;
		EffectDisplayName = null;
		EffectMessageName = null;
		EffectDuration = 0;
		SpringingEffective = false;
		Priority = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetRunningBehaviorEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetRunningBehaviorEvent FromPool(GameObject Actor, string AbilityName, string AbilityDescription, string Verb, string EffectDisplayName, string EffectMessageName, int EffectDuration, bool SpringingEffective, int Priority)
	{
		GetRunningBehaviorEvent getRunningBehaviorEvent = FromPool();
		getRunningBehaviorEvent.Actor = Actor;
		getRunningBehaviorEvent.AbilityName = AbilityName;
		getRunningBehaviorEvent.AbilityDescription = AbilityDescription;
		getRunningBehaviorEvent.Verb = Verb;
		getRunningBehaviorEvent.EffectDisplayName = EffectDisplayName;
		getRunningBehaviorEvent.EffectMessageName = EffectMessageName;
		getRunningBehaviorEvent.EffectDuration = EffectDuration;
		getRunningBehaviorEvent.SpringingEffective = SpringingEffective;
		getRunningBehaviorEvent.Priority = Priority;
		return getRunningBehaviorEvent;
	}

	public static void Retrieve(GameObject Actor, out string AbilityName, out string AbilityDescription, out string Verb, out string EffectDisplayName, out string EffectMessageName, out int EffectDuration, out bool SpringingEffective)
	{
		AbilityName = null;
		AbilityDescription = null;
		Verb = null;
		EffectDisplayName = null;
		EffectMessageName = null;
		EffectDuration = 0;
		SpringingEffective = false;
		bool flag = true;
		int num = 0;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("GetRunningBehavior"))
		{
			Event @event = Event.New("GetRunningBehavior");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("AbilityName", AbilityName);
			@event.SetParameter("AbilityDescription", AbilityDescription);
			@event.SetParameter("Verb", Verb);
			@event.SetParameter("EffectDisplayName", EffectDisplayName);
			@event.SetParameter("EffectMessageName", EffectMessageName);
			@event.SetParameter("EffectDuration", EffectDuration);
			@event.SetFlag("SpringingEffective", SpringingEffective);
			@event.SetParameter("Priority", num);
			flag = Actor.FireEvent(@event);
			AbilityName = @event.GetStringParameter("AbilityName");
			AbilityDescription = @event.GetStringParameter("AbilityDescription");
			Verb = @event.GetStringParameter("Verb");
			EffectDisplayName = @event.GetStringParameter("EffectDisplayName");
			EffectMessageName = @event.GetStringParameter("EffectMessageName");
			EffectDuration = @event.GetIntParameter("EffectDuration");
			SpringingEffective = @event.HasFlag("SpringingEffective");
			num = @event.GetIntParameter("Priority");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			GetRunningBehaviorEvent getRunningBehaviorEvent = FromPool(Actor, AbilityName, AbilityDescription, Verb, EffectDisplayName, EffectMessageName, EffectDuration, SpringingEffective, num);
			flag = Actor.HandleEvent(getRunningBehaviorEvent);
			AbilityName = getRunningBehaviorEvent.AbilityName;
			AbilityDescription = getRunningBehaviorEvent.AbilityDescription;
			Verb = getRunningBehaviorEvent.Verb;
			EffectDisplayName = getRunningBehaviorEvent.EffectDisplayName;
			EffectMessageName = getRunningBehaviorEvent.EffectMessageName;
			EffectDuration = getRunningBehaviorEvent.EffectDuration;
			SpringingEffective = getRunningBehaviorEvent.SpringingEffective;
			num = getRunningBehaviorEvent.Priority;
		}
	}
}

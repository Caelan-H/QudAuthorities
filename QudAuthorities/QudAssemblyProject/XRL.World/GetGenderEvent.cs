using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetGenderEvent : MinEvent
{
	public GameObject Object;

	public string Name;

	public bool AsIfKnown;

	public new static readonly int ID;

	private static List<GetGenderEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetGenderEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetGenderEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetGenderEvent()
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

	public static GetGenderEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetGenderEvent FromPool(GameObject Object, string Name, bool AsIfKnown = false)
	{
		GetGenderEvent getGenderEvent = FromPool();
		getGenderEvent.Object = Object;
		getGenderEvent.Name = Name;
		getGenderEvent.AsIfKnown = AsIfKnown;
		return getGenderEvent;
	}

	public override void Reset()
	{
		Object = null;
		Name = null;
		AsIfKnown = false;
		base.Reset();
	}

	public static string GetFor(GameObject Object, string Name, bool AsIfKnown = false)
	{
		if (Object.HasRegisteredEvent("GetGender"))
		{
			Event @event = Event.New("GetGender", "Object", Object, "Name", Name, "AsIfKnown", AsIfKnown ? 1 : 0);
			Object.FireEvent(@event);
			Name = @event.GetStringParameter("Name");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetGenderEvent getGenderEvent = FromPool(Object, Name, AsIfKnown);
			Object.HandleEvent(getGenderEvent);
			Name = getGenderEvent.Name;
		}
		return Name;
	}
}

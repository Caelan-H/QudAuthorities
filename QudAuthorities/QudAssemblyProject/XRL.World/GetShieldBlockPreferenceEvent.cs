using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetShieldBlockPreferenceEvent : MinEvent
{
	public GameObject Shield;

	public GameObject Attacker;

	public GameObject Defender;

	public int Preference;

	public new static readonly int ID;

	private static List<GetShieldBlockPreferenceEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetShieldBlockPreferenceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetShieldBlockPreferenceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetShieldBlockPreferenceEvent()
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

	public static GetShieldBlockPreferenceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetShieldBlockPreferenceEvent FromPool(GameObject Shield, GameObject Attacker, GameObject Defender, int Preference)
	{
		GetShieldBlockPreferenceEvent getShieldBlockPreferenceEvent = FromPool();
		getShieldBlockPreferenceEvent.Shield = Shield;
		getShieldBlockPreferenceEvent.Attacker = Attacker;
		getShieldBlockPreferenceEvent.Defender = Defender;
		getShieldBlockPreferenceEvent.Preference = Preference;
		return getShieldBlockPreferenceEvent;
	}

	public override void Reset()
	{
		Shield = null;
		Attacker = null;
		Defender = null;
		Preference = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Shield, GameObject Attacker = null, GameObject Defender = null, int Preference = 0)
	{
		if (GameObject.validate(ref Shield))
		{
			if (Shield.HasRegisteredEvent("GetShieldBlockPreference"))
			{
				Event @event = Event.New("GetShieldBlockPreference");
				@event.SetParameter("Shield", Shield);
				@event.SetParameter("Attacker", Attacker);
				@event.SetParameter("Defender", Defender);
				@event.SetParameter("Preference", Preference);
				bool num = Shield.FireEvent(@event);
				Preference = @event.GetIntParameter("Preference");
				if (!num)
				{
					return Preference;
				}
			}
			if (Shield.WantEvent(ID, MinEvent.CascadeLevel))
			{
				GetShieldBlockPreferenceEvent getShieldBlockPreferenceEvent = FromPool(Shield, Attacker, Defender, Preference);
				Shield.HandleEvent(getShieldBlockPreferenceEvent);
				Preference = getShieldBlockPreferenceEvent.Preference;
				return Preference;
			}
		}
		return Preference;
	}
}

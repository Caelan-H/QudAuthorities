using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetCriticalThresholdEvent : MinEvent
{
	public const int BASE_THRESHOLD = 20;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public GameObject Projectile;

	public string Skill;

	public int Threshold = 20;

	public new static readonly int ID;

	private static List<GetCriticalThresholdEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetCriticalThresholdEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetCriticalThresholdEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetCriticalThresholdEvent()
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

	public static GetCriticalThresholdEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetCriticalThresholdEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Projectile, string Skill, int Threshold)
	{
		GetCriticalThresholdEvent getCriticalThresholdEvent = FromPool();
		getCriticalThresholdEvent.Attacker = Attacker;
		getCriticalThresholdEvent.Defender = Defender;
		getCriticalThresholdEvent.Weapon = Weapon;
		getCriticalThresholdEvent.Projectile = Projectile;
		getCriticalThresholdEvent.Skill = Skill;
		getCriticalThresholdEvent.Threshold = Threshold;
		return getCriticalThresholdEvent;
	}

	public override void Reset()
	{
		Attacker = null;
		Defender = null;
		Weapon = null;
		Projectile = null;
		Threshold = 20;
		base.Reset();
	}

	public static int GetFor(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Projectile = null, string Skill = null, int Threshold = 20)
	{
		bool flag = Attacker?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag2 = Defender?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag3 = Weapon?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag4 = Projectile?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		if (flag || flag2 || flag3 || flag4)
		{
			Event @event = Event.New("GetCriticalThreshold");
			@event.SetParameter("Attacker", Attacker);
			@event.SetParameter("Defender", Defender);
			@event.SetParameter("Weapon", Weapon);
			@event.SetParameter("Projectile", Projectile);
			@event.SetParameter("Skill", Skill);
			@event.SetParameter("Threshold", Threshold);
			if (flag)
			{
				Attacker.FireEvent(@event);
			}
			if (flag2)
			{
				Defender.FireEvent(@event);
			}
			if (flag3)
			{
				Weapon.FireEvent(@event);
			}
			if (flag4)
			{
				Projectile.FireEvent(@event);
			}
			Threshold = @event.GetIntParameter("Threshold");
		}
		bool flag5 = Attacker?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		bool flag6 = Defender?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		bool flag7 = Weapon?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		bool flag8 = Projectile?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		if (flag5 || flag6 || flag7 || flag8)
		{
			GetCriticalThresholdEvent getCriticalThresholdEvent = FromPool(Attacker, Defender, Weapon, Projectile, Skill, Threshold);
			if (flag5)
			{
				Attacker.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag6)
			{
				Defender.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag7)
			{
				Weapon.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag8)
			{
				Projectile.HandleEvent(getCriticalThresholdEvent);
			}
			Threshold = getCriticalThresholdEvent.Threshold;
		}
		return Threshold;
	}
}

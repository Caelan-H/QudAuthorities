using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DefendMeleeHitEvent : MinEvent
{
	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public Damage Damage;

	public int Result;

	public new static readonly int ID;

	private static List<DefendMeleeHitEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 3;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static DefendMeleeHitEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(DefendMeleeHitEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public DefendMeleeHitEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static DefendMeleeHitEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DefendMeleeHitEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, Damage Damage, int Result)
	{
		DefendMeleeHitEvent defendMeleeHitEvent = FromPool();
		defendMeleeHitEvent.Attacker = Attacker;
		defendMeleeHitEvent.Defender = Defender;
		defendMeleeHitEvent.Weapon = Weapon;
		defendMeleeHitEvent.Damage = Damage;
		defendMeleeHitEvent.Result = Result;
		return defendMeleeHitEvent;
	}

	public override void Reset()
	{
		Attacker = null;
		Defender = null;
		Weapon = null;
		Damage = null;
		Result = 0;
		base.Reset();
	}

	public static void Send(GameObject Attacker, GameObject Defender, GameObject Weapon, Damage Damage, int Result)
	{
		if ((!Defender.WantEvent(ID, CascadeLevel) || Defender.HandleEvent(FromPool(Attacker, Defender, Weapon, Damage, Result))) && Defender.HasRegisteredEvent("DefendMeleeHit"))
		{
			Event @event = Event.New("DefendMeleeHit");
			@event.SetParameter("Attacker", Attacker);
			@event.SetParameter("Defender", Defender);
			@event.SetParameter("Weapon", Weapon);
			@event.SetParameter("Damage", Damage);
			@event.SetParameter("Result", Result);
			Defender.FireEvent(@event);
		}
	}
}

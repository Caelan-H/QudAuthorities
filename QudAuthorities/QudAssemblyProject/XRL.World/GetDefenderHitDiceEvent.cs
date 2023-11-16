using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetDefenderHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID;

	private static List<GetDefenderHitDiceEvent> Pool;

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

	static GetDefenderHitDiceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetDefenderHitDiceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetDefenderHitDiceEvent()
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

	public static GetDefenderHitDiceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetDefenderHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetDefenderHitDiceEvent getDefenderHitDiceEvent = FromPool();
		getDefenderHitDiceEvent.Attacker = Attacker;
		getDefenderHitDiceEvent.Defender = Defender;
		getDefenderHitDiceEvent.Weapon = Weapon;
		getDefenderHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getDefenderHitDiceEvent.AV = AV;
		getDefenderHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getDefenderHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetDefenderHitDice", Defender, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}

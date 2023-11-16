using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetAttackerHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID;

	private static List<GetAttackerHitDiceEvent> Pool;

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

	static GetAttackerHitDiceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetAttackerHitDiceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetAttackerHitDiceEvent()
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

	public static GetAttackerHitDiceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetAttackerHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetAttackerHitDiceEvent getAttackerHitDiceEvent = FromPool();
		getAttackerHitDiceEvent.Attacker = Attacker;
		getAttackerHitDiceEvent.Defender = Defender;
		getAttackerHitDiceEvent.Weapon = Weapon;
		getAttackerHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getAttackerHitDiceEvent.AV = AV;
		getAttackerHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getAttackerHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetAttackerHitDice", Attacker, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}

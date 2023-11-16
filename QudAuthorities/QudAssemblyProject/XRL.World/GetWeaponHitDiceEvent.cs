using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetWeaponHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID;

	private static List<GetWeaponHitDiceEvent> Pool;

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

	static GetWeaponHitDiceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetWeaponHitDiceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetWeaponHitDiceEvent()
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

	public static GetWeaponHitDiceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetWeaponHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetWeaponHitDiceEvent getWeaponHitDiceEvent = FromPool();
		getWeaponHitDiceEvent.Attacker = Attacker;
		getWeaponHitDiceEvent.Defender = Defender;
		getWeaponHitDiceEvent.Weapon = Weapon;
		getWeaponHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getWeaponHitDiceEvent.AV = AV;
		getWeaponHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getWeaponHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetWeaponHitDice", Weapon, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}

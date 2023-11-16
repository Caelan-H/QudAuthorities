using System.Collections.Generic;

namespace XRL.World;

public class MentalAttackEvent : IMentalAttackEvent
{
	private static List<MentalAttackEvent> Pool;

	private static int PoolCounter;

	static MentalAttackEvent()
	{
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("MentalAttackEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static MentalAttackEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static MentalAttackEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source = null, string Command = null, string Dice = null, int Type = 0, int Magnitude = int.MinValue)
	{
		MentalAttackEvent mentalAttackEvent = FromPool();
		mentalAttackEvent.Attacker = Attacker;
		mentalAttackEvent.Defender = Defender;
		mentalAttackEvent.Source = Source;
		mentalAttackEvent.Command = Command;
		mentalAttackEvent.Dice = Dice;
		mentalAttackEvent.Type = Type;
		mentalAttackEvent.Magnitude = Magnitude;
		return mentalAttackEvent;
	}
}

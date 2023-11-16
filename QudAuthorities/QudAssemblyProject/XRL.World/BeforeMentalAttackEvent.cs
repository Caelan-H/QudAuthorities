using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<BeforeMentalAttackEvent> Pool;

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

	static BeforeMentalAttackEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("BeforeMentalAttackEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeMentalAttackEvent()
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

	public static BeforeMentalAttackEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		BeforeMentalAttackEvent beforeMentalAttackEvent = FromPool();
		PE.ApplyTo(beforeMentalAttackEvent);
		return beforeMentalAttackEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeforeMentalAttackEvent beforeMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(beforeMentalAttackEvent, PE);
			PE.SetFrom(beforeMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("BeforeMentalAttack"))
		{
			Event @event = Event.New("BeforeMentalAttack");
			PE.ApplyTo(@event);
			flag = PE.Attacker.FireEvent(@event);
			PE.SetFrom(@event);
		}
		return flag;
	}
}

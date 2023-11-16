using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<AfterMentalAttackEvent> Pool;

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

	static AfterMentalAttackEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("AfterMentalAttackEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterMentalAttackEvent()
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

	public static AfterMentalAttackEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		AfterMentalAttackEvent afterMentalAttackEvent = FromPool();
		PE.ApplyTo(afterMentalAttackEvent);
		return afterMentalAttackEvent;
	}

	public static void Send(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			AfterMentalAttackEvent afterMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(afterMentalAttackEvent, PE);
			PE.SetFrom(afterMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("AfterMentalAttack"))
		{
			Event @event = Event.New("AfterMentalAttack");
			PE.ApplyTo(@event);
			flag = PE.Attacker.FireEvent(@event);
			PE.SetFrom(@event);
		}
	}
}

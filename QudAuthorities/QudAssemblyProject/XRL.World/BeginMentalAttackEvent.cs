using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeginMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<BeginMentalAttackEvent> Pool;

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

	static BeginMentalAttackEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("BeginMentalAttackEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public BeginMentalAttackEvent()
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

	public static BeginMentalAttackEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeginMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		BeginMentalAttackEvent beginMentalAttackEvent = FromPool();
		PE.ApplyTo(beginMentalAttackEvent);
		return beginMentalAttackEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeginMentalAttackEvent beginMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(beginMentalAttackEvent, PE);
			PE.SetFrom(beginMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("BeginMentalAttack"))
		{
			Event @event = Event.New("BeginMentalAttack");
			PE.ApplyTo(@event);
			flag = PE.Attacker.FireEvent(@event);
			PE.SetFrom(@event);
		}
		return flag;
	}
}

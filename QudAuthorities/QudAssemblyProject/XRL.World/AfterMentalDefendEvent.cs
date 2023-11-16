using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<AfterMentalDefendEvent> Pool;

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

	static AfterMentalDefendEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("AfterMentalDefendEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterMentalDefendEvent()
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

	public static AfterMentalDefendEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		AfterMentalDefendEvent afterMentalDefendEvent = FromPool();
		PE.ApplyTo(afterMentalDefendEvent);
		return afterMentalDefendEvent;
	}

	public static void Send(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			AfterMentalDefendEvent afterMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(afterMentalDefendEvent, PE);
			PE.SetFrom(afterMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("AfterMentalDefend"))
		{
			Event @event = Event.New("AfterMentalDefend");
			PE.ApplyTo(@event);
			flag = PE.Defender.FireEvent(@event);
			PE.SetFrom(@event);
		}
	}
}

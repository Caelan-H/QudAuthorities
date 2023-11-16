using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<BeforeMentalDefendEvent> Pool;

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

	static BeforeMentalDefendEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("BeforeMentalDefendEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeMentalDefendEvent()
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

	public static BeforeMentalDefendEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		BeforeMentalDefendEvent beforeMentalDefendEvent = FromPool();
		PE.ApplyTo(beforeMentalDefendEvent);
		return beforeMentalDefendEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeforeMentalDefendEvent beforeMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(beforeMentalDefendEvent, PE);
			PE.SetFrom(beforeMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("BeforeMentalDefend"))
		{
			Event @event = Event.New("BeforeMentalDefend");
			PE.ApplyTo(@event);
			flag = PE.Defender.FireEvent(@event);
			PE.SetFrom(@event);
		}
		return flag;
	}
}

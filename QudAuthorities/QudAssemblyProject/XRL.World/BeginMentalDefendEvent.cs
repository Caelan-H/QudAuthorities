using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeginMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID;

	private static List<BeginMentalDefendEvent> Pool;

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

	static BeginMentalDefendEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("BeginMentalDefendEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public BeginMentalDefendEvent()
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

	public static BeginMentalDefendEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeginMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		BeginMentalDefendEvent beginMentalDefendEvent = FromPool();
		PE.ApplyTo(beginMentalDefendEvent);
		return beginMentalDefendEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeginMentalDefendEvent beginMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(beginMentalDefendEvent, PE);
			PE.SetFrom(beginMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("BeginMentalDefend"))
		{
			Event @event = Event.New("BeginMentalDefend");
			PE.ApplyTo(@event);
			flag = PE.Defender.FireEvent(@event);
			PE.SetFrom(@event);
		}
		return flag;
	}
}

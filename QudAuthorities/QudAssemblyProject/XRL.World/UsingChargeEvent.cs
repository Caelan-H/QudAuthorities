using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UsingChargeEvent : IChargeConsumptionEvent
{
	public new static readonly int ID;

	private static List<UsingChargeEvent> Pool;

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

	static UsingChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UsingChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UsingChargeEvent()
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

	public static UsingChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true, int PowerLoadLevel = 100)
	{
		if (Wanted(Object))
		{
			UsingChargeEvent usingChargeEvent = FromPool();
			usingChargeEvent.Source = Source;
			usingChargeEvent.Amount = Amount;
			usingChargeEvent.StartingAmount = Amount;
			usingChargeEvent.Multiple = Multiple;
			usingChargeEvent.GridMask = GridMask;
			usingChargeEvent.Forced = Forced;
			usingChargeEvent.LiveOnly = LiveOnly;
			usingChargeEvent.IncludeTransient = IncludeTransient;
			usingChargeEvent.IncludeBiological = IncludeBiological;
			usingChargeEvent.PowerLoadLevel = PowerLoadLevel;
			Process(Object, usingChargeEvent);
		}
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("UsingCharge");
		}
		return true;
	}

	public static void Process(GameObject Object, UsingChargeEvent E)
	{
		if (E.CheckRegisteredEvent(Object, "UsingCharge"))
		{
			Object.HandleEvent(E);
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RechargeAvailableEvent : IInitialChargeProductionEvent
{
	public new static readonly int ID;

	private static List<RechargeAvailableEvent> Pool;

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

	static RechargeAvailableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(RechargeAvailableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public RechargeAvailableEvent()
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

	public static RechargeAvailableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static RechargeAvailableEvent FromPool(IChargeEvent From)
	{
		RechargeAvailableEvent rechargeAvailableEvent = FromPool();
		rechargeAvailableEvent.Source = From.Source;
		rechargeAvailableEvent.Amount = From.Amount;
		rechargeAvailableEvent.StartingAmount = From.Amount;
		rechargeAvailableEvent.Multiple = From.Multiple;
		rechargeAvailableEvent.GridMask = From.GridMask;
		rechargeAvailableEvent.Forced = From.Forced;
		rechargeAvailableEvent.LiveOnly = From.LiveOnly;
		rechargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		rechargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return rechargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool();
			rechargeAvailableEvent.Source = Source;
			rechargeAvailableEvent.Amount = Amount;
			rechargeAvailableEvent.StartingAmount = Amount;
			rechargeAvailableEvent.Multiple = Multiple;
			rechargeAvailableEvent.GridMask = GridMask;
			rechargeAvailableEvent.Forced = Forced;
			rechargeAvailableEvent.LiveOnly = false;
			rechargeAvailableEvent.IncludeTransient = IncludeTransient;
			rechargeAvailableEvent.IncludeBiological = IncludeBiological;
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(out RechargeAvailableEvent E, GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			E = FromPool();
			E.Source = Source;
			E.Amount = Amount;
			E.StartingAmount = Amount;
			E.Multiple = Multiple;
			E.GridMask = GridMask;
			E.Forced = Forced;
			E.LiveOnly = false;
			E.IncludeTransient = IncludeTransient;
			E.IncludeBiological = IncludeBiological;
			Process(Object, E);
			return E.Used;
		}
		E = null;
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool(From);
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From, int Amount)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool(From);
			rechargeAvailableEvent.Amount = Amount;
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("RechargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, RechargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "RechargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

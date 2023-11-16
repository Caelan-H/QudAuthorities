using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class FinishRechargeAvailableEvent : IFinalChargeProductionEvent
{
	public new static readonly int ID;

	private static List<FinishRechargeAvailableEvent> Pool;

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

	static FinishRechargeAvailableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(FinishRechargeAvailableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public FinishRechargeAvailableEvent()
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

	public static FinishRechargeAvailableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static FinishRechargeAvailableEvent FromPool(IChargeEvent From)
	{
		FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool();
		finishRechargeAvailableEvent.Source = From.Source;
		finishRechargeAvailableEvent.Amount = From.Amount;
		finishRechargeAvailableEvent.StartingAmount = From.StartingAmount;
		finishRechargeAvailableEvent.Multiple = From.Multiple;
		finishRechargeAvailableEvent.GridMask = From.GridMask;
		finishRechargeAvailableEvent.Forced = From.Forced;
		finishRechargeAvailableEvent.LiveOnly = From.LiveOnly;
		finishRechargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		finishRechargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return finishRechargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool();
			finishRechargeAvailableEvent.Source = Source;
			finishRechargeAvailableEvent.Amount = Amount;
			finishRechargeAvailableEvent.StartingAmount = Amount;
			finishRechargeAvailableEvent.Multiple = Multiple;
			finishRechargeAvailableEvent.GridMask = GridMask;
			finishRechargeAvailableEvent.Forced = Forced;
			finishRechargeAvailableEvent.LiveOnly = false;
			finishRechargeAvailableEvent.IncludeTransient = IncludeTransient;
			finishRechargeAvailableEvent.IncludeBiological = IncludeBiological;
			Process(Object, finishRechargeAvailableEvent);
			return finishRechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool(From);
			Process(Object, finishRechargeAvailableEvent);
			return finishRechargeAvailableEvent.Used;
		}
		return From.Used;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("FinishRechargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, FinishRechargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "FinishRechargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class FinishChargeAvailableEvent : IFinalChargeProductionEvent
{
	public new static readonly int ID;

	private static List<FinishChargeAvailableEvent> Pool;

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

	static FinishChargeAvailableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(FinishChargeAvailableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public FinishChargeAvailableEvent()
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

	public static FinishChargeAvailableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static FinishChargeAvailableEvent FromPool(IChargeEvent From)
	{
		FinishChargeAvailableEvent finishChargeAvailableEvent = FromPool();
		finishChargeAvailableEvent.Source = From.Source;
		finishChargeAvailableEvent.Amount = From.Amount;
		finishChargeAvailableEvent.StartingAmount = From.StartingAmount;
		finishChargeAvailableEvent.Multiple = From.Multiple;
		finishChargeAvailableEvent.GridMask = From.GridMask;
		finishChargeAvailableEvent.Forced = From.Forced;
		finishChargeAvailableEvent.LiveOnly = From.LiveOnly;
		finishChargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		finishChargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return finishChargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			FinishChargeAvailableEvent finishChargeAvailableEvent = FromPool();
			finishChargeAvailableEvent.Source = Source;
			finishChargeAvailableEvent.Amount = Amount;
			finishChargeAvailableEvent.StartingAmount = Amount;
			finishChargeAvailableEvent.Multiple = Multiple;
			finishChargeAvailableEvent.GridMask = GridMask;
			finishChargeAvailableEvent.Forced = Forced;
			finishChargeAvailableEvent.LiveOnly = false;
			finishChargeAvailableEvent.IncludeTransient = IncludeTransient;
			finishChargeAvailableEvent.IncludeBiological = IncludeBiological;
			Process(Object, finishChargeAvailableEvent);
			return finishChargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			FinishChargeAvailableEvent finishChargeAvailableEvent = FromPool(From);
			Process(Object, finishChargeAvailableEvent);
			return finishChargeAvailableEvent.Used;
		}
		return From.Used;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("FinishChargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, FinishChargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "FinishChargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

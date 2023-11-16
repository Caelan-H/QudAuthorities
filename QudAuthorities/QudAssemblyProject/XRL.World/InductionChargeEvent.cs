using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class InductionChargeEvent : IInitialChargeProductionEvent
{
	public new static readonly int ID;

	private static List<InductionChargeEvent> Pool;

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

	static InductionChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(InductionChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public InductionChargeEvent()
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

	public static InductionChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static InductionChargeEvent FromPool(IChargeEvent From)
	{
		InductionChargeEvent inductionChargeEvent = FromPool();
		inductionChargeEvent.Source = From.Source;
		inductionChargeEvent.Amount = From.Amount;
		inductionChargeEvent.StartingAmount = From.Amount;
		inductionChargeEvent.Multiple = From.Multiple;
		inductionChargeEvent.GridMask = From.GridMask;
		inductionChargeEvent.Forced = From.Forced;
		inductionChargeEvent.LiveOnly = From.LiveOnly;
		inductionChargeEvent.IncludeTransient = From.IncludeTransient;
		inductionChargeEvent.IncludeBiological = From.IncludeBiological;
		return inductionChargeEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			InductionChargeEvent inductionChargeEvent = FromPool();
			inductionChargeEvent.Source = Source;
			inductionChargeEvent.Amount = Amount;
			inductionChargeEvent.StartingAmount = Amount;
			inductionChargeEvent.Multiple = Multiple;
			inductionChargeEvent.GridMask = GridMask;
			inductionChargeEvent.Forced = Forced;
			inductionChargeEvent.LiveOnly = false;
			inductionChargeEvent.IncludeTransient = IncludeTransient;
			inductionChargeEvent.IncludeBiological = IncludeBiological;
			Process(Object, inductionChargeEvent);
			return inductionChargeEvent.Used;
		}
		return 0;
	}

	public static int Send(out InductionChargeEvent E, GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
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

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("InductionCharge");
		}
		return true;
	}

	public static bool Process(GameObject Object, InductionChargeEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "InductionCharge"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UseChargeEvent : IChargeConsumptionEvent
{
	public const int PASSES = 2;

	public new static readonly int ID;

	private static List<UseChargeEvent> Pool;

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

	static UseChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UseChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UseChargeEvent()
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

	public static UseChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true, int PowerLoadLevel = 100)
	{
		if (Wanted(Object))
		{
			UseChargeEvent useChargeEvent = FromPool();
			useChargeEvent.Source = Source;
			useChargeEvent.Amount = Amount;
			useChargeEvent.Multiple = Multiple;
			useChargeEvent.GridMask = GridMask;
			useChargeEvent.Forced = Forced;
			useChargeEvent.LiveOnly = LiveOnly;
			useChargeEvent.IncludeTransient = IncludeTransient;
			useChargeEvent.IncludeBiological = IncludeBiological;
			useChargeEvent.PowerLoadLevel = PowerLoadLevel;
			if (!Process(Object, useChargeEvent))
			{
				ChargeUsedEvent.Send(Object, useChargeEvent.Source, Amount - useChargeEvent.Amount, Amount, useChargeEvent.Multiple, useChargeEvent.GridMask, useChargeEvent.Forced, useChargeEvent.LiveOnly, useChargeEvent.IncludeTransient, useChargeEvent.IncludeBiological, PowerLoadLevel);
				return true;
			}
		}
		return false;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("UseCharge");
		}
		return true;
	}

	public static bool Process(GameObject Object, UseChargeEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "UseCharge"))
		{
			return false;
		}
		for (int i = 1; i <= 2; i++)
		{
			E.Pass = i;
			if (!Object.HandleEvent(E))
			{
				return false;
			}
		}
		return true;
	}
}

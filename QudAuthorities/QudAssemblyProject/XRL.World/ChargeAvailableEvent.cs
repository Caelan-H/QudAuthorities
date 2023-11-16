using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ChargeAvailableEvent : IInitialChargeProductionEvent
{
	public new static readonly int ID;

	private static List<ChargeAvailableEvent> Pool;

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

	static ChargeAvailableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ChargeAvailableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ChargeAvailableEvent()
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

	public static ChargeAvailableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false)
	{
		if (Wanted(Object))
		{
			ChargeAvailableEvent chargeAvailableEvent = FromPool();
			chargeAvailableEvent.Source = Source;
			chargeAvailableEvent.Amount = Amount;
			chargeAvailableEvent.StartingAmount = Amount;
			chargeAvailableEvent.Multiple = Multiple;
			chargeAvailableEvent.GridMask = GridMask;
			chargeAvailableEvent.Forced = Forced;
			chargeAvailableEvent.LiveOnly = false;
			Process(Object, chargeAvailableEvent);
			return chargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(out ChargeAvailableEvent E, GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false)
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
			return Object.HasRegisteredEvent("ChargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, ChargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "ChargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class QueryRechargeStorageEvent : IChargeStorageEvent
{
	public new static readonly int ID;

	private static List<QueryRechargeStorageEvent> Pool;

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

	static QueryRechargeStorageEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(QueryRechargeStorageEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public QueryRechargeStorageEvent()
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

	public static QueryRechargeStorageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static QueryRechargeStorageEvent FromPool(IChargeStorageEvent From)
	{
		QueryRechargeStorageEvent queryRechargeStorageEvent = FromPool();
		queryRechargeStorageEvent.Source = From.Source;
		queryRechargeStorageEvent.Amount = From.Amount;
		queryRechargeStorageEvent.StartingAmount = From.StartingAmount;
		queryRechargeStorageEvent.Multiple = From.Multiple;
		queryRechargeStorageEvent.GridMask = From.GridMask;
		queryRechargeStorageEvent.Forced = From.Forced;
		queryRechargeStorageEvent.LiveOnly = From.LiveOnly;
		queryRechargeStorageEvent.IncludeTransient = From.IncludeTransient;
		queryRechargeStorageEvent.IncludeBiological = From.IncludeBiological;
		queryRechargeStorageEvent.Transient = From.Transient;
		queryRechargeStorageEvent.UnlimitedTransient = From.UnlimitedTransient;
		return queryRechargeStorageEvent;
	}

	public static int Retrieve(GameObject Object, GameObject Source, bool IncludeTransient = false, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			QueryRechargeStorageEvent queryRechargeStorageEvent = FromPool();
			queryRechargeStorageEvent.Source = Source;
			queryRechargeStorageEvent.Amount = 0;
			queryRechargeStorageEvent.StartingAmount = 0;
			queryRechargeStorageEvent.Multiple = 1;
			queryRechargeStorageEvent.GridMask = GridMask;
			queryRechargeStorageEvent.Forced = Forced;
			queryRechargeStorageEvent.LiveOnly = LiveOnly;
			queryRechargeStorageEvent.IncludeTransient = IncludeTransient;
			queryRechargeStorageEvent.IncludeBiological = IncludeBiological;
			Process(Object, queryRechargeStorageEvent);
			return Result(queryRechargeStorageEvent);
		}
		return 0;
	}

	public static int Retrieve(out QueryRechargeStorageEvent E, GameObject Object, GameObject Source, bool IncludeTransient = false, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			E = FromPool();
			E.Source = Source;
			E.Amount = 0;
			E.StartingAmount = 0;
			E.Multiple = 1;
			E.GridMask = GridMask;
			E.Forced = Forced;
			E.LiveOnly = LiveOnly;
			E.IncludeTransient = IncludeTransient;
			E.IncludeBiological = IncludeBiological;
			Process(Object, E);
			return Result(E);
		}
		E = null;
		return 0;
	}

	public static int Retrieve(GameObject Object, IChargeStorageEvent From)
	{
		if (Wanted(Object))
		{
			QueryRechargeStorageEvent e = FromPool(From);
			Process(Object, e);
			return Result(e);
		}
		return 0;
	}

	public static int Retrieve(out QueryRechargeStorageEvent E, GameObject Object, IChargeStorageEvent From)
	{
		if (Wanted(Object))
		{
			E = FromPool(From);
			Process(Object, E);
			return Result(E);
		}
		E = null;
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("QueryRechargeStorage");
		}
		return true;
	}

	public static bool Process(GameObject Object, QueryRechargeStorageEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "QueryRechargeStorage"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}

	public static int Result(QueryRechargeStorageEvent E)
	{
		if (E.IncludeTransient)
		{
			if (E.UnlimitedTransient)
			{
				return int.MaxValue;
			}
			return E.Amount;
		}
		return E.Amount - E.Transient;
	}
}

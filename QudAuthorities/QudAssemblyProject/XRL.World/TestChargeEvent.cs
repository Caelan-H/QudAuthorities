using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TestChargeEvent : IChargeConsumptionEvent
{
	public new static readonly int ID;

	private static List<TestChargeEvent> Pool;

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

	static TestChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TestChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TestChargeEvent()
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

	public static TestChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			TestChargeEvent testChargeEvent = FromPool();
			testChargeEvent.Source = Source;
			testChargeEvent.Amount = Amount;
			testChargeEvent.StartingAmount = Amount;
			testChargeEvent.Multiple = Multiple;
			testChargeEvent.GridMask = GridMask;
			testChargeEvent.Forced = Forced;
			testChargeEvent.LiveOnly = LiveOnly;
			testChargeEvent.IncludeTransient = IncludeTransient;
			testChargeEvent.IncludeBiological = IncludeBiological;
			return !Process(Object, testChargeEvent);
		}
		return false;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("TestCharge");
		}
		return true;
	}

	public static bool Process(GameObject Object, TestChargeEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "TestCharge"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}

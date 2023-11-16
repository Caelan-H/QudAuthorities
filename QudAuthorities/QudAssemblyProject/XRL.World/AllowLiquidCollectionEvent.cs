using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AllowLiquidCollectionEvent : ILiquidEvent
{
	public new static readonly int ID;

	private static List<AllowLiquidCollectionEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 0;

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

	static AllowLiquidCollectionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AllowLiquidCollectionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AllowLiquidCollectionEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static AllowLiquidCollectionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AllowLiquidCollectionEvent FromPool(GameObject Actor, string Liquid = "water")
	{
		AllowLiquidCollectionEvent allowLiquidCollectionEvent = FromPool();
		allowLiquidCollectionEvent.Actor = Actor;
		allowLiquidCollectionEvent.Liquid = Liquid;
		allowLiquidCollectionEvent.LiquidVolume = null;
		allowLiquidCollectionEvent.Drams = 0;
		allowLiquidCollectionEvent.Skip = null;
		allowLiquidCollectionEvent.SkipList = null;
		allowLiquidCollectionEvent.Filter = null;
		allowLiquidCollectionEvent.Auto = false;
		allowLiquidCollectionEvent.ImpureOkay = false;
		allowLiquidCollectionEvent.SafeOnly = false;
		return allowLiquidCollectionEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, string Liquid = "water")
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			return Object.HandleEvent(FromPool(Actor, Liquid));
		}
		return true;
	}
}

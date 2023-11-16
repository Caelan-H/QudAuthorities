using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class WantsLiquidCollectionEvent : ILiquidEvent
{
	public new static readonly int ID;

	private static List<WantsLiquidCollectionEvent> Pool;

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

	static WantsLiquidCollectionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(WantsLiquidCollectionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public WantsLiquidCollectionEvent()
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

	public static WantsLiquidCollectionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static WantsLiquidCollectionEvent FromPool(GameObject Actor, string Liquid = "water")
	{
		WantsLiquidCollectionEvent wantsLiquidCollectionEvent = FromPool();
		wantsLiquidCollectionEvent.Actor = Actor;
		wantsLiquidCollectionEvent.Liquid = Liquid;
		wantsLiquidCollectionEvent.LiquidVolume = null;
		wantsLiquidCollectionEvent.Drams = 0;
		wantsLiquidCollectionEvent.Skip = null;
		wantsLiquidCollectionEvent.SkipList = null;
		wantsLiquidCollectionEvent.Filter = null;
		wantsLiquidCollectionEvent.Auto = false;
		wantsLiquidCollectionEvent.ImpureOkay = false;
		wantsLiquidCollectionEvent.SafeOnly = false;
		return wantsLiquidCollectionEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, string Liquid = "water")
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			return !Object.HandleEvent(FromPool(Actor, Liquid));
		}
		return false;
	}
}

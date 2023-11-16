using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetFreeDramsEvent : ILiquidEvent
{
	public new static readonly int ID;

	private static List<GetFreeDramsEvent> Pool;

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

	static GetFreeDramsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetFreeDramsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetFreeDramsEvent()
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

	public static GetFreeDramsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetFreeDramsEvent FromPool(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false)
	{
		GetFreeDramsEvent getFreeDramsEvent = FromPool();
		getFreeDramsEvent.Actor = Actor;
		getFreeDramsEvent.Liquid = Liquid;
		getFreeDramsEvent.LiquidVolume = null;
		getFreeDramsEvent.Drams = 0;
		getFreeDramsEvent.Skip = Skip;
		getFreeDramsEvent.SkipList = SkipList;
		getFreeDramsEvent.Filter = Filter;
		getFreeDramsEvent.Auto = false;
		getFreeDramsEvent.ImpureOkay = ImpureOkay;
		getFreeDramsEvent.SafeOnly = false;
		return getFreeDramsEvent;
	}

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetFreeDramsEvent getFreeDramsEvent = FromPool(Actor, Liquid, Skip, SkipList, Filter, ImpureOkay);
			Actor.HandleEvent(getFreeDramsEvent);
			return getFreeDramsEvent.Drams;
		}
		return 0;
	}
}

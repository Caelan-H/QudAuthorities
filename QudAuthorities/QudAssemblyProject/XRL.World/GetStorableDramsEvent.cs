using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetStorableDramsEvent : ILiquidEvent
{
	public new static readonly int ID;

	private static List<GetStorableDramsEvent> Pool;

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

	static GetStorableDramsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetStorableDramsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetStorableDramsEvent()
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

	public static GetStorableDramsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetStorableDramsEvent FromPool(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		GetStorableDramsEvent getStorableDramsEvent = FromPool();
		getStorableDramsEvent.Actor = Actor;
		getStorableDramsEvent.Liquid = Liquid;
		getStorableDramsEvent.LiquidVolume = LiquidVolume;
		getStorableDramsEvent.Drams = 0;
		getStorableDramsEvent.Skip = Skip;
		getStorableDramsEvent.SkipList = SkipList;
		getStorableDramsEvent.Filter = Filter;
		getStorableDramsEvent.Auto = false;
		getStorableDramsEvent.ImpureOkay = false;
		getStorableDramsEvent.SafeOnly = SafeOnly;
		return getStorableDramsEvent;
	}

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetStorableDramsEvent getStorableDramsEvent = FromPool(Actor, Liquid, Skip, SkipList, Filter, SafeOnly, LiquidVolume);
			Actor.HandleEvent(getStorableDramsEvent);
			return getStorableDramsEvent.Drams;
		}
		return 0;
	}
}

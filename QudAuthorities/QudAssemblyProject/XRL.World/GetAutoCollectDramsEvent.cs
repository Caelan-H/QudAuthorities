using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetAutoCollectDramsEvent : ILiquidEvent
{
	public new static readonly int ID;

	private static List<GetAutoCollectDramsEvent> Pool;

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

	static GetAutoCollectDramsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetAutoCollectDramsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetAutoCollectDramsEvent()
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

	public static GetAutoCollectDramsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetAutoCollectDramsEvent FromPool(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null)
	{
		GetAutoCollectDramsEvent getAutoCollectDramsEvent = FromPool();
		getAutoCollectDramsEvent.Actor = Actor;
		getAutoCollectDramsEvent.Liquid = Liquid;
		getAutoCollectDramsEvent.LiquidVolume = null;
		getAutoCollectDramsEvent.Drams = 0;
		getAutoCollectDramsEvent.Skip = Skip;
		getAutoCollectDramsEvent.SkipList = SkipList;
		getAutoCollectDramsEvent.Filter = null;
		getAutoCollectDramsEvent.Auto = false;
		getAutoCollectDramsEvent.ImpureOkay = false;
		getAutoCollectDramsEvent.SafeOnly = false;
		return getAutoCollectDramsEvent;
	}

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetAutoCollectDramsEvent getAutoCollectDramsEvent = FromPool(Actor, Liquid, Skip, SkipList);
			Actor.HandleEvent(getAutoCollectDramsEvent);
			return getAutoCollectDramsEvent.Drams;
		}
		return 0;
	}
}

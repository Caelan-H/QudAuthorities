using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UseDramsEvent : ILiquidEvent
{
	public const int PASSES = 3;

	public List<GameObject> TrackContainers;

	public bool Drinking;

	public int Pass;

	public new static readonly int ID;

	private static List<UseDramsEvent> Pool;

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

	static UseDramsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UseDramsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UseDramsEvent()
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

	public override void Reset()
	{
		TrackContainers = null;
		Drinking = false;
		Pass = 0;
		base.Reset();
	}

	public static UseDramsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static UseDramsEvent FromPool(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false, List<GameObject> TrackContainers = null, bool Drinking = false)
	{
		UseDramsEvent useDramsEvent = FromPool();
		useDramsEvent.Actor = Actor;
		useDramsEvent.Liquid = Liquid;
		useDramsEvent.LiquidVolume = null;
		useDramsEvent.Drams = Drams;
		useDramsEvent.Skip = Skip;
		useDramsEvent.SkipList = SkipList;
		useDramsEvent.Filter = Filter;
		useDramsEvent.Auto = false;
		useDramsEvent.ImpureOkay = ImpureOkay;
		useDramsEvent.SafeOnly = false;
		useDramsEvent.TrackContainers = TrackContainers;
		useDramsEvent.Drinking = Drinking;
		useDramsEvent.Pass = 0;
		return useDramsEvent;
	}

	public static bool Check(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false, List<GameObject> TrackContainers = null, bool Drinking = false)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			UseDramsEvent useDramsEvent = FromPool(Actor, Liquid, Drams, Skip, SkipList, Filter, ImpureOkay, TrackContainers, Drinking);
			for (int i = 1; i <= 3; i++)
			{
				useDramsEvent.Pass = i;
				if (!Actor.HandleEvent(useDramsEvent))
				{
					return false;
				}
				if (useDramsEvent.Drams <= 0)
				{
					break;
				}
			}
		}
		return true;
	}
}

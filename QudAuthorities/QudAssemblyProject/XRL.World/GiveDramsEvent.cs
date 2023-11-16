using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GiveDramsEvent : ILiquidEvent
{
	public const int PASSES = 5;

	public int Pass;

	public List<GameObject> StoredIn = new List<GameObject>();

	public new static readonly int ID;

	private static List<GiveDramsEvent> Pool;

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

	static GiveDramsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GiveDramsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GiveDramsEvent()
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
		Pass = 0;
		StoredIn.Clear();
		base.Reset();
	}

	public static GiveDramsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GiveDramsEvent FromPool(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool Auto = false, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		GiveDramsEvent giveDramsEvent = FromPool();
		giveDramsEvent.Actor = Actor;
		giveDramsEvent.Liquid = Liquid;
		giveDramsEvent.LiquidVolume = LiquidVolume;
		giveDramsEvent.Drams = Drams;
		giveDramsEvent.Skip = Skip;
		giveDramsEvent.SkipList = SkipList;
		giveDramsEvent.Filter = Filter;
		giveDramsEvent.Auto = Auto;
		giveDramsEvent.ImpureOkay = false;
		giveDramsEvent.SafeOnly = SafeOnly;
		giveDramsEvent.Pass = 0;
		giveDramsEvent.StoredIn.Clear();
		return giveDramsEvent;
	}

	public static bool Check(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool Auto = false, List<GameObject> StoredIn = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		bool result = true;
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GiveDramsEvent giveDramsEvent = FromPool(Actor, Liquid, Drams, Skip, SkipList, Filter, Auto, SafeOnly, LiquidVolume);
			for (int i = 1; i <= 5; i++)
			{
				giveDramsEvent.Pass = i;
				if (!Actor.HandleEvent(giveDramsEvent))
				{
					result = false;
					break;
				}
				if (giveDramsEvent.Drams <= 0)
				{
					break;
				}
			}
			StoredIn?.AddRange(giveDramsEvent.StoredIn);
		}
		return result;
	}
}

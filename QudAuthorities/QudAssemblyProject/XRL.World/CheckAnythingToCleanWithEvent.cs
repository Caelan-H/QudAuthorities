using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckAnythingToCleanWithEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Item;

	public new static readonly int ID;

	private static List<CheckAnythingToCleanWithEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 7;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckAnythingToCleanWithEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckAnythingToCleanWithEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckAnythingToCleanWithEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Item = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static CheckAnythingToCleanWithEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckAnythingToCleanWithEvent FromPool(GameObject Actor, GameObject Item)
	{
		CheckAnythingToCleanWithEvent checkAnythingToCleanWithEvent = FromPool();
		checkAnythingToCleanWithEvent.Actor = Actor;
		checkAnythingToCleanWithEvent.Item = Item;
		return checkAnythingToCleanWithEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel) && !Actor.HandleEvent(FromPool(Actor, Item)))
		{
			return true;
		}
		if (CheckAnythingToCleanWithNearbyEvent.Check(Actor, Item))
		{
			return true;
		}
		return false;
	}
}

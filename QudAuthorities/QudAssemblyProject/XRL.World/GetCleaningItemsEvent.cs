using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetCleaningItemsEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Item;

	public List<GameObject> Objects = new List<GameObject>();

	public new static readonly int ID;

	private static List<GetCleaningItemsEvent> Pool;

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

	static GetCleaningItemsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetCleaningItemsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetCleaningItemsEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Item = null;
		Objects.Clear();
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetCleaningItemsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetCleaningItemsEvent FromPool(GameObject Actor, GameObject Item)
	{
		GetCleaningItemsEvent getCleaningItemsEvent = FromPool();
		getCleaningItemsEvent.Actor = Actor;
		getCleaningItemsEvent.Item = Item;
		getCleaningItemsEvent.Objects.Clear();
		return getCleaningItemsEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static List<GameObject> GetFor(GameObject Actor, GameObject Item)
	{
		GetCleaningItemsEvent getCleaningItemsEvent = null;
		List<GameObject> Objects = null;
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			if (getCleaningItemsEvent == null)
			{
				getCleaningItemsEvent = FromPool(Actor, Item);
				Objects = getCleaningItemsEvent.Objects;
			}
			Actor.HandleEvent(getCleaningItemsEvent);
		}
		GetCleaningItemsNearbyEvent.Send(Actor, Item, ref Objects);
		return Objects;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeTradedEvent : MinEvent
{
	public GameObject Object;

	public GameObject Holder;

	public GameObject OtherParty;

	public float CostMultiple;

	public new static readonly int ID;

	private static List<CanBeTradedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanBeTradedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeTradedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeTradedEvent()
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
		Object = null;
		Holder = null;
		OtherParty = null;
		CostMultiple = 0f;
		base.Reset();
	}

	public static CanBeTradedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanBeTradedEvent FromPool(GameObject Object, GameObject Holder, GameObject OtherParty, float CostMultiple = 1f)
	{
		CanBeTradedEvent canBeTradedEvent = FromPool();
		canBeTradedEvent.Object = Object;
		canBeTradedEvent.Holder = Holder;
		canBeTradedEvent.OtherParty = OtherParty;
		canBeTradedEvent.CostMultiple = CostMultiple;
		return canBeTradedEvent;
	}

	public static bool Check(GameObject Object, GameObject Holder, GameObject OtherParty, float CostMultiple = 1f)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Holder, OtherParty, CostMultiple)))
		{
			return false;
		}
		return true;
	}
}

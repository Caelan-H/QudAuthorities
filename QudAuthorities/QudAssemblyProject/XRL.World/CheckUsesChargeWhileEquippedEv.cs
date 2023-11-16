using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckUsesChargeWhileEquippedEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CheckUsesChargeWhileEquippedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckUsesChargeWhileEquippedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckUsesChargeWhileEquippedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckUsesChargeWhileEquippedEvent()
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

	public static CheckUsesChargeWhileEquippedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckUsesChargeWhileEquippedEvent FromPool(GameObject Object)
	{
		CheckUsesChargeWhileEquippedEvent checkUsesChargeWhileEquippedEvent = FromPool();
		checkUsesChargeWhileEquippedEvent.Object = Object;
		return checkUsesChargeWhileEquippedEvent;
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Object)
	{
		if (!Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			return false;
		}
		if (Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}

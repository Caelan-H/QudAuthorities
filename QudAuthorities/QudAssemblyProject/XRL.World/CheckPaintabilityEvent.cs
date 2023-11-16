using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckPaintabilityEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CheckPaintabilityEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckPaintabilityEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckPaintabilityEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckPaintabilityEvent()
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

	public static CheckPaintabilityEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckPaintabilityEvent FromPool(GameObject Object)
	{
		CheckPaintabilityEvent checkPaintabilityEvent = FromPool();
		checkPaintabilityEvent.Object = Object;
		return checkPaintabilityEvent;
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}

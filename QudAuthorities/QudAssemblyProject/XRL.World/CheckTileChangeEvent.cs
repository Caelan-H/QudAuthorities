using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckTileChangeEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<CheckTileChangeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckTileChangeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckTileChangeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckTileChangeEvent()
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

	public static CheckTileChangeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckTileChangeEvent FromPool(GameObject Object)
	{
		CheckTileChangeEvent checkTileChangeEvent = FromPool();
		checkTileChangeEvent.Object = Object;
		return checkTileChangeEvent;
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

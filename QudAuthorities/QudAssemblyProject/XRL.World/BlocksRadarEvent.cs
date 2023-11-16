using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BlocksRadarEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<BlocksRadarEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static BlocksRadarEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BlocksRadarEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BlocksRadarEvent()
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
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static BlocksRadarEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BlocksRadarEvent FromPool(GameObject Object)
	{
		BlocksRadarEvent blocksRadarEvent = FromPool();
		blocksRadarEvent.Object = Object;
		return blocksRadarEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("BlocksRadar"))
		{
			Event @event = Event.New("BlocksRadar");
			@event.SetParameter("Object", Object);
			if (!Object.FireEvent(@event))
			{
				return true;
			}
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return true;
		}
		return false;
	}

	public static bool Check(Cell C)
	{
		for (int num = C.Objects.Count - 1; num >= 0; num--)
		{
			if (Check(C.Objects[num]))
			{
				return true;
			}
		}
		return false;
	}
}

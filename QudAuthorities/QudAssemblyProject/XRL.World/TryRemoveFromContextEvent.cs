using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class TryRemoveFromContextEvent : IRemoveFromContextEvent
{
	public new static readonly int ID;

	private static List<TryRemoveFromContextEvent> Pool;

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

	static TryRemoveFromContextEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(TryRemoveFromContextEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public TryRemoveFromContextEvent()
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

	public static TryRemoveFromContextEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, IEvent ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object))
		{
			TryRemoveFromContextEvent tryRemoveFromContextEvent = FromPool();
			tryRemoveFromContextEvent.Object = Object;
			flag = Object.HandleEvent(tryRemoveFromContextEvent);
			ParentEvent?.ProcessChildEvent(tryRemoveFromContextEvent);
		}
		return flag;
	}
}

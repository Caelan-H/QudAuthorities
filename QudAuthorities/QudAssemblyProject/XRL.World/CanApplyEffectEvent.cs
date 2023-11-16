using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanApplyEffectEvent : IEffectCheckEvent
{
	public new static readonly int ID;

	private static List<CanApplyEffectEvent> Pool;

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

	static CanApplyEffectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanApplyEffectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanApplyEffectEvent()
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

	public static CanApplyEffectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject obj, string Name, int Duration = 0)
	{
		bool flag = true;
		if (flag && obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			CanApplyEffectEvent canApplyEffectEvent = FromPool();
			canApplyEffectEvent.Name = Name;
			canApplyEffectEvent.Duration = Duration;
			flag = obj.HandleEvent(canApplyEffectEvent);
		}
		return flag;
	}
}

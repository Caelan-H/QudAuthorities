using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AwardingXPEvent : IXPEvent
{
	public new static readonly int ID;

	private static List<AwardingXPEvent> Pool;

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

	static AwardingXPEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AwardingXPEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AwardingXPEvent()
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

	public static AwardingXPEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(IXPEvent ParentEvent)
	{
		if (ParentEvent.Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardingXPEvent awardingXPEvent = FromPool();
			ParentEvent.ApplyTo(awardingXPEvent);
			if (!awardingXPEvent.Actor.HandleEvent(awardingXPEvent))
			{
				return false;
			}
			awardingXPEvent.ApplyTo(ParentEvent);
		}
		return true;
	}
}

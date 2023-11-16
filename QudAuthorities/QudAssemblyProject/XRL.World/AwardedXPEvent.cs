using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AwardedXPEvent : IXPEvent
{
	public new static readonly int ID;

	private static List<AwardedXPEvent> Pool;

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

	static AwardedXPEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AwardedXPEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AwardedXPEvent()
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

	public static AwardedXPEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(IXPEvent ParentEvent, int Amount)
	{
		if (ParentEvent.Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardedXPEvent awardedXPEvent = FromPool();
			ParentEvent.ApplyTo(awardedXPEvent);
			awardedXPEvent.Amount = Amount;
			awardedXPEvent.Actor.HandleEvent(awardedXPEvent);
		}
	}
}

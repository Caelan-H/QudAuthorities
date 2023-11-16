using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OwnerGetUnknownShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID;

	private static List<OwnerGetUnknownShortDescriptionEvent> Pool;

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

	static OwnerGetUnknownShortDescriptionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OwnerGetUnknownShortDescriptionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OwnerGetUnknownShortDescriptionEvent()
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

	public static OwnerGetUnknownShortDescriptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static OwnerGetUnknownShortDescriptionEvent FromPool(string Base, string Context = null, bool AsIfKnown = false)
	{
		OwnerGetUnknownShortDescriptionEvent ownerGetUnknownShortDescriptionEvent = FromPool();
		ownerGetUnknownShortDescriptionEvent.Context = Context;
		ownerGetUnknownShortDescriptionEvent.AsIfKnown = AsIfKnown;
		ownerGetUnknownShortDescriptionEvent.Base.Append(Base);
		return ownerGetUnknownShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "OwnerGetUnknownShortDescription";
	}
}

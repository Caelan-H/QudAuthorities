using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OwnerGetShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID;

	private static List<OwnerGetShortDescriptionEvent> Pool;

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

	static OwnerGetShortDescriptionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OwnerGetShortDescriptionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OwnerGetShortDescriptionEvent()
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

	public static OwnerGetShortDescriptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static OwnerGetShortDescriptionEvent FromPool(string Base, string Context = null, bool AsIfKnown = false)
	{
		OwnerGetShortDescriptionEvent ownerGetShortDescriptionEvent = FromPool();
		ownerGetShortDescriptionEvent.Context = Context;
		ownerGetShortDescriptionEvent.AsIfKnown = AsIfKnown;
		ownerGetShortDescriptionEvent.Base.Append(Base);
		return ownerGetShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "OwnerGetShortDescription";
	}
}

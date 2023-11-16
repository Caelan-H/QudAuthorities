using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetUnknownShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID;

	private static List<GetUnknownShortDescriptionEvent> Pool;

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

	static GetUnknownShortDescriptionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetUnknownShortDescriptionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetUnknownShortDescriptionEvent()
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

	public static GetUnknownShortDescriptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetUnknownShortDescriptionEvent FromPool(GameObject Object, string Base, string Context = null, bool AsIfKnown = false)
	{
		GetUnknownShortDescriptionEvent getUnknownShortDescriptionEvent = FromPool();
		getUnknownShortDescriptionEvent.Object = Object;
		getUnknownShortDescriptionEvent.Context = Context;
		getUnknownShortDescriptionEvent.AsIfKnown = AsIfKnown;
		getUnknownShortDescriptionEvent.Base.Append(Base);
		return getUnknownShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "GetUnknownShortDescription";
	}
}

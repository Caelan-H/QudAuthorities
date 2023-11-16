using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID;

	private static List<GetShortDescriptionEvent> Pool;

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

	static GetShortDescriptionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetShortDescriptionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetShortDescriptionEvent()
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

	public static GetShortDescriptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetShortDescriptionEvent FromPool(GameObject Object, string Base, string Context = null, bool AsIfKnown = false)
	{
		GetShortDescriptionEvent getShortDescriptionEvent = FromPool();
		getShortDescriptionEvent.Object = Object;
		getShortDescriptionEvent.Context = Context;
		getShortDescriptionEvent.AsIfKnown = AsIfKnown;
		getShortDescriptionEvent.Base.Append(Base);
		return getShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "GetShortDescription";
	}
}

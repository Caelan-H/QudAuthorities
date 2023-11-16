using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PartSupportEvent : MinEvent
{
	public GameObject Actor;

	public string Type;

	public IPart Part;

	public IPart Skip;

	public new static readonly int ID;

	private static List<PartSupportEvent> Pool;

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

	static PartSupportEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PartSupportEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PartSupportEvent()
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
		Actor = null;
		Type = null;
		Part = null;
		Skip = null;
		base.Reset();
	}

	public static PartSupportEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PartSupportEvent FromPool(GameObject Actor, string Type, IPart Part, IPart Skip = null)
	{
		PartSupportEvent partSupportEvent = FromPool();
		partSupportEvent.Actor = Actor;
		partSupportEvent.Type = Type;
		partSupportEvent.Part = Part;
		partSupportEvent.Skip = Skip;
		return partSupportEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Actor, string Type, IPart Part, IPart Skip = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("PartSupport"))
		{
			Event @event = Event.New("PartSupport");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Type", Type);
			@event.SetParameter("Part", Part);
			@event.SetParameter("Skip", Skip);
			flag = Actor.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor, Type, Part, Skip));
		}
		return !flag;
	}

	public static bool Check(NeedPartSupportEvent PE, IPart Part)
	{
		return Check(PE.Actor, PE.Type, Part, PE.Skip);
	}
}

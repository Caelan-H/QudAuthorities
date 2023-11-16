using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class NeedPartSupportEvent : MinEvent
{
	public GameObject Actor;

	public string Type;

	public IPart Skip;

	public new static readonly int ID;

	private static List<NeedPartSupportEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static NeedPartSupportEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(NeedPartSupportEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public NeedPartSupportEvent()
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
		base.Reset();
	}

	public static NeedPartSupportEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static NeedPartSupportEvent FromPool(GameObject Actor, string Type, IPart Skip = null)
	{
		NeedPartSupportEvent needPartSupportEvent = FromPool();
		needPartSupportEvent.Actor = Actor;
		needPartSupportEvent.Type = Type;
		needPartSupportEvent.Skip = Skip;
		return needPartSupportEvent;
	}

	public static void Send(GameObject Actor, string Type, IPart Skip = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(Actor) && Actor.HasRegisteredEvent("NeedPartSupport"))
		{
			Event @event = Event.New("NeedPartSupport");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Type", Type);
			@event.SetParameter("Skip", Skip);
			flag = Actor.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor, Type, Skip));
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EffectRemovedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID;

	private static List<EffectRemovedEvent> Pool;

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

	static EffectRemovedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EffectRemovedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EffectRemovedEvent()
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

	public static EffectRemovedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EffectRemovedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectRemovedEvent effectRemovedEvent = FromPool();
		effectRemovedEvent.Name = Name;
		effectRemovedEvent.Effect = Effect;
		effectRemovedEvent.Actor = Actor;
		effectRemovedEvent.Duration = Effect.Duration;
		return effectRemovedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(FromPool(Name, Effect, Actor));
		}
		if (obj.HasRegisteredEvent("EffectRemoved"))
		{
			obj.FireEvent(Event.New("EffectRemoved", "Effect", Effect));
		}
	}
}

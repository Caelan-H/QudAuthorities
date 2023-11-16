using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EffectAppliedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID;

	private static List<EffectAppliedEvent> Pool;

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

	static EffectAppliedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EffectAppliedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EffectAppliedEvent()
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

	public static EffectAppliedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EffectAppliedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectAppliedEvent effectAppliedEvent = FromPool();
		effectAppliedEvent.Name = Name;
		effectAppliedEvent.Effect = Effect;
		effectAppliedEvent.Actor = Actor;
		effectAppliedEvent.Duration = Effect.Duration;
		return effectAppliedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(FromPool(Name, Effect, Actor));
		}
		if (obj.HasRegisteredEvent("EffectApplied"))
		{
			obj.FireEvent(Event.New("EffectApplied", "Effect", Effect));
		}
	}
}

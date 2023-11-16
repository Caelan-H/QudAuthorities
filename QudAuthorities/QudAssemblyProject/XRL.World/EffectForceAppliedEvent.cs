using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EffectForceAppliedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID;

	private static List<EffectForceAppliedEvent> Pool;

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

	static EffectForceAppliedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EffectForceAppliedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EffectForceAppliedEvent()
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

	public static EffectForceAppliedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EffectForceAppliedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectForceAppliedEvent effectForceAppliedEvent = FromPool();
		effectForceAppliedEvent.Name = Name;
		effectForceAppliedEvent.Effect = Effect;
		effectForceAppliedEvent.Actor = Actor;
		effectForceAppliedEvent.Duration = Effect.Duration;
		return effectForceAppliedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(FromPool(Name, Effect, Actor));
		}
		if (obj.HasRegisteredEvent("EffectForceApplied"))
		{
			obj.FireEvent(Event.New("EffectForceApplied", "Effect", Effect));
		}
	}
}

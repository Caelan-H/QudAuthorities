using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ApplyEffectEvent : IActualEffectCheckEvent
{
	public new static readonly int ID;

	private static List<ApplyEffectEvent> Pool;

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

	static ApplyEffectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ApplyEffectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ApplyEffectEvent()
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

	public static ApplyEffectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ApplyEffectEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		ApplyEffectEvent applyEffectEvent = FromPool();
		applyEffectEvent.Name = Name;
		applyEffectEvent.Effect = Effect;
		applyEffectEvent.Actor = Actor;
		applyEffectEvent.Duration = Effect.Duration;
		return applyEffectEvent;
	}

	public static bool Check(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (!Effect.CanBeAppliedTo(obj))
		{
			return false;
		}
		if (obj.WantEvent(ID, MinEvent.CascadeLevel) && !obj.HandleEvent(FromPool(Name, Effect, Actor)))
		{
			return false;
		}
		if (obj.HasRegisteredEvent("ApplyEffect") && !obj.FireEvent(Event.New("ApplyEffect", "Object", obj, "Effect", Effect, "Owner", Actor)))
		{
			return false;
		}
		return true;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ForceApplyEffectEvent : IActualEffectCheckEvent
{
	public new static readonly int ID;

	private static List<ForceApplyEffectEvent> Pool;

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

	static ForceApplyEffectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ForceApplyEffectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ForceApplyEffectEvent()
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

	public static ForceApplyEffectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ForceApplyEffectEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		ForceApplyEffectEvent forceApplyEffectEvent = FromPool();
		forceApplyEffectEvent.Name = Name;
		forceApplyEffectEvent.Effect = Effect;
		forceApplyEffectEvent.Actor = Actor;
		forceApplyEffectEvent.Duration = Effect.Duration;
		return forceApplyEffectEvent;
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
		if (obj.HasRegisteredEvent("ForceApplyEffect") && !obj.FireEvent(Event.New("ForceApplyEffect", "Object", obj, "Effect", Effect, "Owner", Actor)))
		{
			return false;
		}
		return true;
	}
}

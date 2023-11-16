using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Effects;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RealityStabilizeEvent : MinEvent
{
	public RealityStabilized Effect;

	public GameObject Object;

	public bool Projecting;

	public bool Relevant;

	public bool CanDestroy;

	public new static readonly int ID;

	private static List<RealityStabilizeEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RealityStabilizeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(RealityStabilizeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public RealityStabilizeEvent()
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

	public static RealityStabilizeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static RealityStabilizeEvent FromPool(RealityStabilized Effect, GameObject Object, bool Projecting = false)
	{
		RealityStabilizeEvent realityStabilizeEvent = FromPool();
		realityStabilizeEvent.Effect = Effect;
		realityStabilizeEvent.Object = Object;
		realityStabilizeEvent.Projecting = Projecting;
		realityStabilizeEvent.Relevant = false;
		realityStabilizeEvent.CanDestroy = false;
		return realityStabilizeEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Effect = null;
		Object = null;
		Projecting = false;
		Relevant = false;
		CanDestroy = false;
		base.Reset();
	}

	public bool Check(bool CanDestroy = false)
	{
		Relevant = true;
		if (CanDestroy)
		{
			this.CanDestroy = true;
		}
		return Effect.RandomlyTakeEffect();
	}

	public static void Send(RealityStabilized Effect, GameObject Object, bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		if (Object.WantEvent(ID, CascadeLevel))
		{
			RealityStabilizeEvent realityStabilizeEvent = FromPool(Effect, Object, Projecting);
			Object.HandleEvent(realityStabilizeEvent);
			Relevant = realityStabilizeEvent.Relevant;
			CanDestroy = realityStabilizeEvent.CanDestroy;
		}
		else
		{
			Relevant = false;
			CanDestroy = false;
		}
	}
}

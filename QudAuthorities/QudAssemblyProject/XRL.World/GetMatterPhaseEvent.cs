using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMatterPhaseEvent : MinEvent
{
	public GameObject Object;

	public int MatterPhase;

	public new static readonly int ID;

	private static List<GetMatterPhaseEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMatterPhaseEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetMatterPhaseEvent()
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

	public static GetMatterPhaseEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMatterPhaseEvent FromPool(GameObject Object, int Base)
	{
		GetMatterPhaseEvent getMatterPhaseEvent = FromPool();
		getMatterPhaseEvent.Object = Object;
		getMatterPhaseEvent.MatterPhase = Base;
		return getMatterPhaseEvent;
	}

	public override void Reset()
	{
		Object = null;
		MatterPhase = 0;
		base.Reset();
	}

	public void MinMatterPhase(int MatterPhase)
	{
		if (this.MatterPhase < MatterPhase)
		{
			this.MatterPhase = MatterPhase;
		}
	}

	public static int GetFor(GameObject Object, int Base = 1)
	{
		if (Object != null && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMatterPhaseEvent getMatterPhaseEvent = FromPool(Object, Base);
			Object.HandleEvent(getMatterPhaseEvent);
			return getMatterPhaseEvent.MatterPhase;
		}
		return Base;
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EnvironmentalUpdateEvent : MinEvent
{
	public new static readonly int ID;

	private static List<EnvironmentalUpdateEvent> Pool;

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

	static EnvironmentalUpdateEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(EnvironmentalUpdateEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public EnvironmentalUpdateEvent()
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

	public static EnvironmentalUpdateEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static void Send(GameObject Object)
	{
		Object.FireEvent("EnvironmentalUpdate");
		if (Object.WantEvent(ID, CascadeLevel))
		{
			Object.HandleEvent(FromPool());
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetIntrinsicValueEvent : IValueEvent
{
	public new static readonly int ID;

	private static List<GetIntrinsicValueEvent> Pool;

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

	static GetIntrinsicValueEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetIntrinsicValueEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetIntrinsicValueEvent()
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

	public static GetIntrinsicValueEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetIntrinsicValueEvent FromPool(GameObject Object, double Value)
	{
		GetIntrinsicValueEvent getIntrinsicValueEvent = FromPool();
		getIntrinsicValueEvent.Object = Object;
		getIntrinsicValueEvent.Value = Value;
		return getIntrinsicValueEvent;
	}

	public static GetIntrinsicValueEvent FromPool(GameObject Object)
	{
		GetIntrinsicValueEvent getIntrinsicValueEvent = FromPool();
		getIntrinsicValueEvent.Object = Object;
		if (Object.GetPart("Commerce") is Commerce commerce)
		{
			getIntrinsicValueEvent.Value = commerce.Value;
		}
		return getIntrinsicValueEvent;
	}
}

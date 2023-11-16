using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetInventoryCategoryEvent : MinEvent
{
	public GameObject Object;

	public string Category = "";

	public new static readonly int ID;

	private static List<GetInventoryCategoryEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetInventoryCategoryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetInventoryCategoryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetInventoryCategoryEvent()
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

	public static GetInventoryCategoryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetInventoryCategoryEvent FromPool(GameObject Object, string Category = "")
	{
		GetInventoryCategoryEvent getInventoryCategoryEvent = FromPool();
		getInventoryCategoryEvent.Object = Object;
		getInventoryCategoryEvent.Category = Category;
		return getInventoryCategoryEvent;
	}

	public override void Reset()
	{
		Object = null;
		Category = "";
		base.Reset();
	}
}

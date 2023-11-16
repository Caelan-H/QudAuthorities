using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ReplaceInContextEvent : MinEvent
{
	public GameObject Object;

	public GameObject Replacement;

	public new static readonly int ID;

	private static List<ReplaceInContextEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ReplaceInContextEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ReplaceInContextEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ReplaceInContextEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Object = null;
		Replacement = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static ReplaceInContextEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ReplaceInContextEvent FromPool(GameObject Object, GameObject Replacement)
	{
		ReplaceInContextEvent replaceInContextEvent = FromPool();
		replaceInContextEvent.Object = Object;
		replaceInContextEvent.Replacement = Replacement;
		return replaceInContextEvent;
	}

	public static void Send(GameObject Object, GameObject Replacement)
	{
		if (GameObject.validate(ref Object))
		{
			Object.HandleEvent(FromPool(Object, Replacement));
		}
	}
}

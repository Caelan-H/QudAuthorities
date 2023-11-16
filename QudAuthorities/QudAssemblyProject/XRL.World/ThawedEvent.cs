using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ThawedEvent : MinEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<ThawedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ThawedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ThawedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ThawedEvent()
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

	public static ThawedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ThawedEvent FromPool(GameObject Object)
	{
		ThawedEvent thawedEvent = FromPool();
		thawedEvent.Object = Object;
		return thawedEvent;
	}

	public override void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
		if (Object.HasRegisteredEvent("Thawed"))
		{
			Object.FireEvent(Event.New("Thawed", "Object", Object));
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetExtraPhysicalFeaturesEvent : MinEvent
{
	public GameObject Object;

	public List<string> Features;

	public new static readonly int ID;

	private static List<GetExtraPhysicalFeaturesEvent> Pool;

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

	static GetExtraPhysicalFeaturesEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetExtraPhysicalFeaturesEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetExtraPhysicalFeaturesEvent()
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

	public override void Reset()
	{
		Object = null;
		Features = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetExtraPhysicalFeaturesEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetExtraPhysicalFeaturesEvent FromPool(GameObject Object, List<string> Features)
	{
		GetExtraPhysicalFeaturesEvent getExtraPhysicalFeaturesEvent = FromPool();
		getExtraPhysicalFeaturesEvent.Object = Object;
		getExtraPhysicalFeaturesEvent.Features = Features;
		return getExtraPhysicalFeaturesEvent;
	}

	public static void Send(GameObject Object, List<string> Features)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetExtraPhysicalFeatures"))
		{
			Event @event = Event.New("GetExtraPhysicalFeatures");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Features", Features);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Features));
		}
	}
}

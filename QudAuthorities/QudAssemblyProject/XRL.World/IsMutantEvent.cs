using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsMutantEvent : MinEvent
{
	public GameObject Object;

	public bool IsMutant;

	public new static readonly int ID;

	private static List<IsMutantEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IsMutantEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsMutantEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsMutantEvent()
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
		IsMutant = false;
		base.Reset();
	}

	public static IsMutantEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object)
	{
		bool flag = Object?.genotypeEntry?.IsMutant ?? Object.IsCreature;
		bool flag2 = true;
		if (flag2 && GameObject.validate(ref Object) && Object.HasRegisteredEvent("IsMutant"))
		{
			Event @event = Event.New("IsMutant");
			@event.SetParameter("Object", Object);
			@event.SetFlag("IsMutant", flag);
			flag2 = Object.FireEvent(@event);
			flag = @event.HasFlag("IsMutant");
		}
		if (flag2 && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			IsMutantEvent isMutantEvent = FromPool();
			isMutantEvent.Object = Object;
			isMutantEvent.IsMutant = flag;
			flag2 = Object.HandleEvent(isMutantEvent);
			flag = isMutantEvent.IsMutant;
		}
		return flag;
	}
}

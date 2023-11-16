using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class WasDerivedFromEvent : IDerivationEvent
{
	public GameObject Derivation;

	public new static readonly int ID;

	private static List<WasDerivedFromEvent> Pool;

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

	static WasDerivedFromEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(WasDerivedFromEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public WasDerivedFromEvent()
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

	public static WasDerivedFromEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Derivation = null;
		base.Reset();
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Derivation, string Context = null)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("WasDerivedFrom"))
		{
			Event @event = Event.New("WasDerivedFrom");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Context", Context);
			@event.SetParameter("Derivation", Derivation);
			Object.FireEvent(@event);
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			WasDerivedFromEvent wasDerivedFromEvent = FromPool();
			wasDerivedFromEvent.Object = Object;
			wasDerivedFromEvent.Actor = Actor;
			wasDerivedFromEvent.Derivation = Derivation;
			wasDerivedFromEvent.Context = Context;
			Object.HandleEvent(wasDerivedFromEvent);
		}
	}
}

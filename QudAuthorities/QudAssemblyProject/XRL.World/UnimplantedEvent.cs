using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class UnimplantedEvent : IActOnItemEvent
{
	public GameObject Implantee;

	public BodyPart Part;

	public new static readonly int ID;

	private static List<UnimplantedEvent> Pool;

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

	static UnimplantedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(UnimplantedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public UnimplantedEvent()
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
		Implantee = null;
		Part = null;
		base.Reset();
	}

	public static UnimplantedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Implantee, GameObject Implant, BodyPart Part, GameObject Actor = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Implant) && Implant.HasRegisteredEvent("Unimplanted"))
		{
			Event @event = new Event("Unimplanted");
			@event.SetParameter("Actor", Actor ?? Implantee);
			@event.SetParameter("Implantee", Implantee);
			@event.SetParameter("Object", Implantee);
			@event.SetParameter("Implant", Implant);
			@event.SetParameter("BodyPart", Part);
			flag = Implant.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Implant) && Implant.WantEvent(ID, MinEvent.CascadeLevel))
		{
			UnimplantedEvent unimplantedEvent = FromPool();
			unimplantedEvent.Actor = Actor ?? Implantee;
			unimplantedEvent.Implantee = Implantee;
			unimplantedEvent.Item = Implant;
			unimplantedEvent.Part = Part;
			flag = Implant.HandleEvent(unimplantedEvent);
		}
	}
}

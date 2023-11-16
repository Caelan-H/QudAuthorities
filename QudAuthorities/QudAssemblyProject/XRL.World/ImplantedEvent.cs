using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ImplantedEvent : IActOnItemEvent
{
	public GameObject Implantee;

	public BodyPart Part;

	public bool ForDeepCopy;

	public new static readonly int ID;

	private static List<ImplantedEvent> Pool;

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

	static ImplantedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ImplantedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ImplantedEvent()
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
		ForDeepCopy = false;
		base.Reset();
	}

	public static ImplantedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Implantee, GameObject Implant, BodyPart Part, GameObject Actor = null, bool ForDeepCopy = false)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Implant) && Implant.HasRegisteredEvent("Implanted"))
		{
			Event @event = new Event("Implanted");
			@event.SetParameter("Implantee", Implantee);
			@event.SetParameter("Actor", Actor ?? Implantee);
			@event.SetParameter("Object", Implantee);
			@event.SetParameter("Implant", Implant);
			@event.SetParameter("BodyPart", Part);
			@event.SetFlag("ForDeepCopy", ForDeepCopy);
			flag = Implant.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Implant) && Implant.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ImplantedEvent implantedEvent = FromPool();
			implantedEvent.Actor = Actor ?? Implantee;
			implantedEvent.Implantee = Implantee;
			implantedEvent.Item = Implant;
			implantedEvent.Part = Part;
			implantedEvent.ForDeepCopy = ForDeepCopy;
			flag = Implant.HandleEvent(implantedEvent);
		}
	}
}

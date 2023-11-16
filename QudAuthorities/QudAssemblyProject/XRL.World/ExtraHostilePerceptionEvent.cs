using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ExtraHostilePerceptionEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Hostile;

	public string PerceiveVerb;

	public bool TreatAsVisible;

	public new static readonly int ID;

	private static List<ExtraHostilePerceptionEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ExtraHostilePerceptionEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ExtraHostilePerceptionEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ExtraHostilePerceptionEvent()
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
		Actor = null;
		Hostile = null;
		PerceiveVerb = null;
		TreatAsVisible = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static ExtraHostilePerceptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Actor, out GameObject Hostile, out string PerceiveVerb, out bool TreatAsVisible)
	{
		Hostile = null;
		PerceiveVerb = "perceive";
		TreatAsVisible = false;
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("ExtraHostilePerception"))
		{
			Event @event = Event.New("ExtraHostilePerception");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Hostile", Hostile);
			@event.SetParameter("PerceiveVerb", PerceiveVerb);
			@event.SetFlag("TreatAsVisible", TreatAsVisible);
			flag = Actor.FireEvent(@event);
			Hostile = @event.GetGameObjectParameter("Hostile");
			PerceiveVerb = @event.GetStringParameter("PerceiveVerb");
			TreatAsVisible = @event.HasFlag("TreatAsVisible");
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			ExtraHostilePerceptionEvent extraHostilePerceptionEvent = FromPool();
			extraHostilePerceptionEvent.Actor = Actor;
			extraHostilePerceptionEvent.Hostile = Hostile;
			extraHostilePerceptionEvent.PerceiveVerb = PerceiveVerb;
			extraHostilePerceptionEvent.TreatAsVisible = TreatAsVisible;
			flag = Actor.HandleEvent(extraHostilePerceptionEvent);
			Hostile = extraHostilePerceptionEvent.Hostile;
			PerceiveVerb = extraHostilePerceptionEvent.PerceiveVerb;
			TreatAsVisible = extraHostilePerceptionEvent.TreatAsVisible;
		}
		return Hostile != null;
	}
}

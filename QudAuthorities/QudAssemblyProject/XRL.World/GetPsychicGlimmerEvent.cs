using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPsychicGlimmerEvent : MinEvent
{
	public GameObject Subject;

	public int Base;

	public int Level;

	public new static readonly int ID;

	private static List<GetPsychicGlimmerEvent> Pool;

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

	static GetPsychicGlimmerEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPsychicGlimmerEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPsychicGlimmerEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetPsychicGlimmerEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Subject = null;
		Base = 0;
		Level = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Subject, int Base = 0)
	{
		int num = Base;
		bool flag = true;
		if (flag && Subject != null && Subject.HasRegisteredEvent("GetPsychicGlimmer"))
		{
			Event @event = Event.New("GetPsychicGlimmer");
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Base", Base);
			@event.SetParameter("Level", num);
			flag = Subject.FireEvent(@event);
			num = @event.GetIntParameter("Level");
		}
		if (flag && Subject != null && Subject.WantEvent(ID, CascadeLevel))
		{
			GetPsychicGlimmerEvent getPsychicGlimmerEvent = FromPool();
			getPsychicGlimmerEvent.Subject = Subject;
			getPsychicGlimmerEvent.Base = Base;
			getPsychicGlimmerEvent.Level = num;
			flag = Subject.HandleEvent(getPsychicGlimmerEvent);
			num = getPsychicGlimmerEvent.Level;
		}
		return Math.Max(num, 0);
	}
}

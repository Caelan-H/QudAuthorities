using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RepairedEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Subject;

	public GameObject Tool;

	public BaseSkill Skill;

	public int MaxRepairTier;

	public new static readonly int ID;

	private static List<RepairedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RepairedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(RepairedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public RepairedEvent()
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
		Subject = null;
		Tool = null;
		Skill = null;
		MaxRepairTier = 0;
		base.Reset();
	}

	public static RepairedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Actor = null, GameObject Subject = null, GameObject Tool = null, BaseSkill Skill = null, int MaxRepairTier = 0)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Subject) && Subject.HasRegisteredEvent("Repaired"))
		{
			Event @event = Event.New("Repaired");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Tool", Tool);
			@event.SetParameter("Skill", Skill);
			@event.SetParameter("MaxRepairTier", MaxRepairTier);
			flag = Subject.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Subject) && Subject.WantEvent(ID, MinEvent.CascadeLevel))
		{
			RepairedEvent repairedEvent = FromPool();
			repairedEvent.Actor = Actor;
			repairedEvent.Subject = Subject;
			repairedEvent.Tool = Tool;
			repairedEvent.Skill = Skill;
			repairedEvent.MaxRepairTier = MaxRepairTier;
			flag = Subject.HandleEvent(repairedEvent);
		}
	}
}

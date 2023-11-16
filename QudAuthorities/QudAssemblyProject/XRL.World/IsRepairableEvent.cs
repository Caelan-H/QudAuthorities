using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsRepairableEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Subject;

	public GameObject Tool;

	public BaseSkill Skill;

	public int? MaxRepairTier;

	public new static readonly int ID;

	private static List<IsRepairableEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static IsRepairableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsRepairableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsRepairableEvent()
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
		MaxRepairTier = null;
		base.Reset();
	}

	public static IsRepairableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Actor = null, GameObject Subject = null, GameObject Tool = null, BaseSkill Skill = null, int? MaxRepairTier = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Subject) && Subject.HasRegisteredEvent("IsRepairable"))
		{
			Event @event = Event.New("IsRepairable");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Tool", Tool);
			@event.SetParameter("Skill", Skill);
			@event.SetParameter("MaxRepairTier", MaxRepairTier);
			flag = Subject.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Subject) && Subject.WantEvent(ID, MinEvent.CascadeLevel))
		{
			IsRepairableEvent isRepairableEvent = FromPool();
			isRepairableEvent.Actor = Actor;
			isRepairableEvent.Subject = Subject;
			isRepairableEvent.Tool = Tool;
			isRepairableEvent.Skill = Skill;
			isRepairableEvent.MaxRepairTier = MaxRepairTier;
			flag = Subject.HandleEvent(isRepairableEvent);
		}
		return !flag;
	}
}

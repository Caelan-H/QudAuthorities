using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterAddSkillEvent : MinEvent
{
	public GameObject Actor;

	public BaseSkill Skill;

	public new static readonly int ID;

	private static List<AfterAddSkillEvent> Pool;

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

	static AfterAddSkillEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterAddSkillEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterAddSkillEvent()
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

	public static AfterAddSkillEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterAddSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		AfterAddSkillEvent afterAddSkillEvent = FromPool();
		afterAddSkillEvent.Actor = Actor;
		afterAddSkillEvent.Skill = Skill;
		return afterAddSkillEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Skill = null;
		base.Reset();
	}

	public static void Send(GameObject Actor, BaseSkill Skill)
	{
		if (!GameObject.validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("AfterAddSkill"))
		{
			Event e = Event.New("AfterAddSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			AfterAddSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

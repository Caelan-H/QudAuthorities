using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeAddSkillEvent : MinEvent
{
	public GameObject Actor;

	public BaseSkill Skill;

	public new static readonly int ID;

	private static List<BeforeAddSkillEvent> Pool;

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

	static BeforeAddSkillEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeAddSkillEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeAddSkillEvent()
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

	public static BeforeAddSkillEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeAddSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		BeforeAddSkillEvent beforeAddSkillEvent = FromPool();
		beforeAddSkillEvent.Actor = Actor;
		beforeAddSkillEvent.Skill = Skill;
		return beforeAddSkillEvent;
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
		if (Actor.HasRegisteredEvent("BeforeAddSkill"))
		{
			Event e = Event.New("BeforeAddSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			BeforeAddSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

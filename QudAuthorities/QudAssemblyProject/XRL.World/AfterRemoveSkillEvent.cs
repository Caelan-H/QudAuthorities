using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterRemoveSkillEvent : MinEvent
{
	public GameObject Actor;

	public BaseSkill Skill;

	public new static readonly int ID;

	private static List<AfterRemoveSkillEvent> Pool;

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

	static AfterRemoveSkillEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AfterRemoveSkillEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AfterRemoveSkillEvent()
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

	public static AfterRemoveSkillEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AfterRemoveSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		AfterRemoveSkillEvent afterRemoveSkillEvent = FromPool();
		afterRemoveSkillEvent.Actor = Actor;
		afterRemoveSkillEvent.Skill = Skill;
		return afterRemoveSkillEvent;
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
		if (Actor.HasRegisteredEvent("AfterRemoveSkill"))
		{
			Event e = Event.New("AfterRemoveSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			AfterRemoveSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

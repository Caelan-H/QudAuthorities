using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeRemoveSkillEvent : MinEvent
{
	public GameObject Actor;

	public BaseSkill Skill;

	public new static readonly int ID;

	private static List<BeforeRemoveSkillEvent> Pool;

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

	static BeforeRemoveSkillEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeRemoveSkillEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeRemoveSkillEvent()
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

	public static BeforeRemoveSkillEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeRemoveSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		BeforeRemoveSkillEvent beforeRemoveSkillEvent = FromPool();
		beforeRemoveSkillEvent.Actor = Actor;
		beforeRemoveSkillEvent.Skill = Skill;
		return beforeRemoveSkillEvent;
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
		if (Actor.HasRegisteredEvent("BeforeRemoveSkill"))
		{
			Event e = Event.New("BeforeRemoveSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			BeforeRemoveSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

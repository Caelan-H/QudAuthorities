using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningGreedEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AuthorityAwakeningGreedEvent> Pool;

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

	static AuthorityAwakeningGreedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningGreedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AuthorityAwakeningGreedEvent()
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

	public static AuthorityAwakeningGreedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AuthorityAwakeningGreedEvent FromPool(GameObject Actor)
	{
        AuthorityAwakeningGreedEvent authorityAwakeningGreedEvent = FromPool();
        authorityAwakeningGreedEvent.Actor = Actor;
		
		return authorityAwakeningGreedEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		
		base.Reset();
	}

	public static void Send(GameObject Actor)
	{
		if (!GameObject.validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("AuthorityAwakeningGreedEvent"))
		{


			Event e = Event.New("AuthorityAwakeningGreedEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
            AuthorityAwakeningGreedEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

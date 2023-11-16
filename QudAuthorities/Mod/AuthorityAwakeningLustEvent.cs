using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningLustEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AuthorityAwakeningLustEvent> Pool;

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

	static AuthorityAwakeningLustEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningLustEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AuthorityAwakeningLustEvent()
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

	public static AuthorityAwakeningLustEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AuthorityAwakeningLustEvent FromPool(GameObject Actor)
	{
        AuthorityAwakeningLustEvent authorityAwakeningLustEvent = FromPool();
        authorityAwakeningLustEvent.Actor = Actor;
		
		return authorityAwakeningLustEvent;
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
		if (Actor.HasRegisteredEvent("AuthorityAwakeningLustEvent"))
		{


			Event e = Event.New("AuthorityAwakeningLustEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
            AuthorityAwakeningLustEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

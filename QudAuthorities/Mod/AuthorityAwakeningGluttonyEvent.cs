using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningGluttonyEvent : MinEvent
{
	public GameObject Actor;
	
	public new static readonly int ID;

	private static List<AuthorityAwakeningGluttonyEvent> Pool;

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

	static AuthorityAwakeningGluttonyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningGluttonyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AuthorityAwakeningGluttonyEvent()
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

	public static AuthorityAwakeningGluttonyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AuthorityAwakeningGluttonyEvent FromPool(GameObject Actor)
	{
        AuthorityAwakeningGluttonyEvent authorityAwakeningGluttonyEvent = FromPool();
        authorityAwakeningGluttonyEvent.Actor = Actor;
		
		return authorityAwakeningGluttonyEvent;
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
		if (Actor.HasRegisteredEvent("AuthorityAwakeningGluttonyEvent"))
		{


			Event e = Event.New("AuthorityAwakeningGluttonyEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
            AuthorityAwakeningGluttonyEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

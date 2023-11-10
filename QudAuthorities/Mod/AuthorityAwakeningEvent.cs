using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AuthorityAwakeningEvent> Pool;

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

	static AuthorityAwakeningEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AuthorityAwakeningEvent()
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

	public static AuthorityAwakeningEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AuthorityAwakeningEvent FromPool(GameObject Actor)
	{
        AuthorityAwakeningEvent authorityAwakeningEvent = FromPool();
        authorityAwakeningEvent.Actor = Actor;
		
		return authorityAwakeningEvent;
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

	public static void Send(GameObject Actor, string Witchfactor)
	{
		if (!GameObject.validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("AuthorityAwakeningEvent"))
		{


			Event e = Event.New("AuthorityAwakeningEvent", "Actor", Actor, "Witchfactor", Witchfactor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
            AuthorityAwakeningEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

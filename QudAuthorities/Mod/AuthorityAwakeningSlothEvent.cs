using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningSlothEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<AuthorityAwakeningSlothEvent> Pool;

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

	static AuthorityAwakeningSlothEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningSlothEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AuthorityAwakeningSlothEvent()
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

	public static AuthorityAwakeningSlothEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AuthorityAwakeningSlothEvent FromPool(GameObject Actor)
	{
        AuthorityAwakeningSlothEvent authorityAwakeningSlothEvent = FromPool();
        authorityAwakeningSlothEvent.Actor = Actor;
		
		return authorityAwakeningSlothEvent;
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
		if (Actor.HasRegisteredEvent("AuthorityAwakeningSlothEvent"))
		{


			Event e = Event.New("AuthorityAwakeningSlothEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
            AuthorityAwakeningSlothEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

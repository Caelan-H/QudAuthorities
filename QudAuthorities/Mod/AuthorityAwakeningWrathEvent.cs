using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningWrathEvent : MinEvent
{
    public GameObject Actor;

    public new static readonly int ID;

    private static List<AuthorityAwakeningWrathEvent> Pool;

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

    static AuthorityAwakeningWrathEvent()
    {
        ID = MinEvent.AllocateID();
        MinEvent.RegisterPoolReset(ResetPool);
        MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningWrathEvent).Name, () => (Pool != null) ? Pool.Count : 0);
    }

    public AuthorityAwakeningWrathEvent()
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

    public static AuthorityAwakeningWrathEvent FromPool()
    {
        return MinEvent.FromPool(ref Pool, ref PoolCounter);
    }

    public static AuthorityAwakeningWrathEvent FromPool(GameObject Actor)
    {
        AuthorityAwakeningWrathEvent authorityAwakeningWrathEvent = FromPool();
        authorityAwakeningWrathEvent.Actor = Actor;

        return authorityAwakeningWrathEvent;
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
        if (Actor.HasRegisteredEvent("AuthorityAwakeningWrathEvent"))
        {


            Event e = Event.New("AuthorityAwakeningWrathEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
            {
                return;
            }
        }
        if (Actor.WantEvent(ID, CascadeLevel))
        {
            AuthorityAwakeningWrathEvent e2 = FromPool(Actor);
            Actor.HandleEvent(e2);
        }
    }
}

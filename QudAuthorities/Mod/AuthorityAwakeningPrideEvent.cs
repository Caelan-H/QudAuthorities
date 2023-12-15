using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class AuthorityAwakeningPrideEvent : MinEvent
{
    public GameObject Actor;

    public new static readonly int ID;

    private static List<AuthorityAwakeningPrideEvent> Pool;

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

    static AuthorityAwakeningPrideEvent()
    {
        ID = MinEvent.AllocateID();
        MinEvent.RegisterPoolReset(ResetPool);
        MinEvent.RegisterPoolCount(typeof(AuthorityAwakeningPrideEvent).Name, () => (Pool != null) ? Pool.Count : 0);
    }

    public AuthorityAwakeningPrideEvent()
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

    public static AuthorityAwakeningPrideEvent FromPool()
    {
        return MinEvent.FromPool(ref Pool, ref PoolCounter);
    }

    public static AuthorityAwakeningPrideEvent FromPool(GameObject Actor)
    {
        AuthorityAwakeningPrideEvent authorityAwakeningPrideEvent = FromPool();
        authorityAwakeningPrideEvent.Actor = Actor;

        return authorityAwakeningPrideEvent;
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
        if (Actor.HasRegisteredEvent("AuthorityAwakeningPrideEvent"))
        {


            Event e = Event.New("AuthorityAwakeningPrideEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
            {
                return;
            }
        }
        if (Actor.WantEvent(ID, CascadeLevel))
        {
            AuthorityAwakeningPrideEvent e2 = FromPool(Actor);
            Actor.HandleEvent(e2);
        }
    }
}

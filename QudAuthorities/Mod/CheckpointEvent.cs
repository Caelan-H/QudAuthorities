using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckpointEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<CheckpointEvent> Pool;

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

	static CheckpointEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckpointEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckpointEvent()
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

	public static CheckpointEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckpointEvent FromPool(GameObject Actor)
	{
		CheckpointEvent checkpointEvent= FromPool();
        checkpointEvent.Actor = Actor;
		
		return checkpointEvent;
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
		if (Actor.HasRegisteredEvent("CheckpointEvent"))
		{


			Event e = Event.New("CheckpointEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			CheckpointEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Skill;

using XRL.World;

[GenerateMinEventDispatchPartials]
public class PlayerTurnPassedEvent : MinEvent
{
	public GameObject Actor;

	public new static readonly int ID;

	private static List<PlayerTurnPassedEvent> Pool;

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

	static PlayerTurnPassedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PlayerTurnPassedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PlayerTurnPassedEvent()
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

	public static PlayerTurnPassedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PlayerTurnPassedEvent FromPool(GameObject Actor)
	{
		PlayerTurnPassedEvent playerTurnPassedEvent= FromPool();
        
		
		return playerTurnPassedEvent;
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
		if (Actor.HasRegisteredEvent("PlayerTurnPassedEvent"))
		{


			Event e = Event.New("PlayerTurnPassedEvent", "Actor", Actor);

            if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			PlayerTurnPassedEvent e2 = FromPool(Actor);
			Actor.HandleEvent(e2);
		}
	}
}

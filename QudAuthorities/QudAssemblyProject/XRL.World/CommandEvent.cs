using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CommandEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Target;

	public Cell TargetCell;

	public string Command;

	public new static readonly int ID;

	private static List<CommandEvent> Pool;

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

	static CommandEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CommandEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CommandEvent()
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

	public override void Reset()
	{
		Actor = null;
		Target = null;
		TargetCell = null;
		Command = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CommandEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CommandEvent FromPool(GameObject Actor, string Command, GameObject Target = null, Cell TargetCell = null)
	{
		CommandEvent commandEvent = FromPool();
		commandEvent.Actor = Actor;
		commandEvent.Target = Target;
		commandEvent.TargetCell = TargetCell;
		commandEvent.Command = Command;
		return commandEvent;
	}

	public static bool Send(GameObject Actor, string Command, ref bool InterfaceExitRequested, GameObject Target = null, Cell TargetCell = null, GameObject Handler = null)
	{
		if (Handler == null)
		{
			Handler = Actor;
		}
		bool flag = true;
		if (flag && GameObject.validate(ref Actor) && Actor.HasRegisteredEvent(Command))
		{
			Event @event = Event.New(Command);
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Target", Target);
			@event.SetParameter("TargetCell", TargetCell);
			@event.SetParameter("Command", Command);
			flag = Handler.FireEvent(@event);
			if (@event.InterfaceExitRequested())
			{
				InterfaceExitRequested = true;
			}
		}
		if (flag && GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			CommandEvent commandEvent = FromPool(Actor, Command, Target, TargetCell);
			flag = Handler.HandleEvent(commandEvent);
			if (commandEvent.InterfaceExitRequested())
			{
				InterfaceExitRequested = true;
			}
		}
		return flag;
	}

	public static bool Send(GameObject Actor, string Command, GameObject Target = null, Cell TargetCell = null, GameObject Handler = null)
	{
		bool InterfaceExitRequested = false;
		return Send(Actor, Command, ref InterfaceExitRequested, Target, TargetCell, Handler);
	}
}

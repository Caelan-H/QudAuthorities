using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CommandSmartUseEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<CommandSmartUseEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static CommandSmartUseEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CommandSmartUseEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CommandSmartUseEvent()
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

	public static CommandSmartUseEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CommandSmartUseEvent FromPool(GameObject Actor, GameObject Item)
	{
		CommandSmartUseEvent commandSmartUseEvent = FromPool();
		commandSmartUseEvent.Actor = Actor;
		commandSmartUseEvent.Item = Item;
		return commandSmartUseEvent;
	}
}

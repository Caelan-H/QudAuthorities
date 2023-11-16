using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CommandSmartUseEarlyEvent : IActOnItemEvent
{
	public new static readonly int ID;

	private static List<CommandSmartUseEarlyEvent> Pool;

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

	static CommandSmartUseEarlyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CommandSmartUseEarlyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CommandSmartUseEarlyEvent()
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

	public static CommandSmartUseEarlyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CommandSmartUseEarlyEvent FromPool(GameObject Actor, GameObject Item)
	{
		CommandSmartUseEarlyEvent commandSmartUseEarlyEvent = FromPool();
		commandSmartUseEarlyEvent.Actor = Actor;
		commandSmartUseEarlyEvent.Item = Item;
		return commandSmartUseEarlyEvent;
	}
}

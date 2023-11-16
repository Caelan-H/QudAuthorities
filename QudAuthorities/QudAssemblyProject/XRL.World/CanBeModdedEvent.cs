using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanBeModdedEvent : IActOnItemEvent
{
	public string ModName;

	public new static readonly int ID;

	private static List<CanBeModdedEvent> Pool;

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

	static CanBeModdedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanBeModdedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanBeModdedEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		ModName = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static CanBeModdedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanBeModdedEvent FromPool(GameObject Actor, GameObject Item, string ModName)
	{
		CanBeModdedEvent canBeModdedEvent = FromPool();
		canBeModdedEvent.Actor = Actor;
		canBeModdedEvent.Item = Item;
		canBeModdedEvent.ModName = ModName;
		return canBeModdedEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, string ModName)
	{
		bool flag = Actor?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		if (flag || flag2)
		{
			CanBeModdedEvent e = FromPool(Actor, Item, ModName);
			if (flag && !Actor.HandleEvent(e))
			{
				return false;
			}
			if (flag2 && !Item.HandleEvent(e))
			{
				return false;
			}
		}
		bool flag3 = Actor?.HasRegisteredEvent("CanBeModded") ?? false;
		bool flag4 = Item?.HasRegisteredEvent("CanBeModded") ?? false;
		if (flag3 || flag4)
		{
			Event e2 = Event.New("CanBeModded", "Actor", Actor, "Item", Item, "Mod", ModName);
			if (flag3 && !Actor.FireEvent(e2))
			{
				return false;
			}
			if (flag4 && !Item.FireEvent(e2))
			{
				return false;
			}
		}
		return true;
	}
}

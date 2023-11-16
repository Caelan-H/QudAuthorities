using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DropOnDeathEvent : IObjectCellInteractionEvent
{
	public bool WasEquipped;

	public new static readonly int ID;

	private static List<DropOnDeathEvent> Pool;

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

	static DropOnDeathEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(DropOnDeathEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public DropOnDeathEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		base.Reset();
		WasEquipped = false;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static DropOnDeathEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DropOnDeathEvent FromPool(GameObject Object, Cell Cell, bool WasEquipped = false, bool Forced = true, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = ".", string Type = "DropOnDeath")
	{
		DropOnDeathEvent dropOnDeathEvent = FromPool();
		dropOnDeathEvent.Object = Object;
		dropOnDeathEvent.Cell = Cell;
		dropOnDeathEvent.Forced = Forced;
		dropOnDeathEvent.IgnoreGravity = IgnoreGravity;
		dropOnDeathEvent.NoStack = NoStack;
		dropOnDeathEvent.Direction = Direction;
		dropOnDeathEvent.Type = Type;
		dropOnDeathEvent.Dragging = null;
		dropOnDeathEvent.WasEquipped = WasEquipped;
		return dropOnDeathEvent;
	}

	public static bool Check(GameObject Object, Cell Cell, bool WasEquipped = false, bool Forced = true, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = ".", string Type = "DropOnDeath")
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Cell, WasEquipped, Forced, System, IgnoreGravity, NoStack: false, Direction, Type)))
		{
			return false;
		}
		return true;
	}
}

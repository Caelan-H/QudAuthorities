using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GravitationEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID;

	private static List<GravitationEvent> Pool;

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

	static GravitationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GravitationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GravitationEvent()
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

	public static GravitationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GravitationEvent FromPool(GameObject Object, Cell Cell)
	{
		GravitationEvent gravitationEvent = FromPool();
		gravitationEvent.Object = Object;
		gravitationEvent.Cell = Cell;
		gravitationEvent.Forced = true;
		gravitationEvent.System = false;
		gravitationEvent.IgnoreGravity = false;
		gravitationEvent.NoStack = false;
		gravitationEvent.Direction = "D";
		gravitationEvent.Type = "Gravitation";
		gravitationEvent.Dragging = null;
		return gravitationEvent;
	}

	public static void Check(GameObject Object, Cell Cell)
	{
		if (Cell != null && GameObject.validate(ref Object) && Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Cell.HandleEvent(FromPool(Object, Cell));
		}
	}
}

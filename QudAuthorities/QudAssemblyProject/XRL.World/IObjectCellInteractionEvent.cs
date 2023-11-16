using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IObjectCellInteractionEvent : MinEvent
{
	public GameObject Object;

	public Cell Cell;

	public bool Forced;

	public bool System;

	public bool IgnoreGravity;

	public bool NoStack;

	public string Direction;

	public string Type;

	public GameObject Dragging;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override void Reset()
	{
		Object = null;
		Cell = null;
		Forced = false;
		System = false;
		IgnoreGravity = false;
		NoStack = false;
		Direction = null;
		Type = null;
		Dragging = null;
		base.Reset();
	}
}

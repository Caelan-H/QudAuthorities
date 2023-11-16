using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class SuspendingEvent : MinEvent
{
	public new static readonly int ID;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static SuspendingEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public SuspendingEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}
}

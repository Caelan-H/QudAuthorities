using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IRenderEvent : MinEvent
{
	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}
}

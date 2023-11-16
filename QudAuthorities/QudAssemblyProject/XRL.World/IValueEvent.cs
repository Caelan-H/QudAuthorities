using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IValueEvent : MinEvent
{
	public GameObject Object;

	public double Value = 0.01;

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
		Value = 0.01;
		base.Reset();
	}
}

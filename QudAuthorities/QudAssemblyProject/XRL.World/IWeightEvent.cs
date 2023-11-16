using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IWeightEvent : MinEvent
{
	public GameObject Object;

	public double BaseWeight;

	public double Weight;

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
		BaseWeight = 0.0;
		Weight = 0.0;
		base.Reset();
	}
}

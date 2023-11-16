using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IObjectCreationEvent : MinEvent
{
	public GameObject ReplacementObject;

	public GameObject Object;

	public string Context;

	public GameObject ActiveObject => ReplacementObject ?? Object;

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
		Context = null;
		ReplacementObject = null;
		base.Reset();
	}
}

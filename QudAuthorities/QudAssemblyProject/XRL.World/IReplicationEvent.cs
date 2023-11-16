using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IReplicationEvent : MinEvent
{
	public GameObject Object;

	public GameObject Actor;

	public string Context;

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
		Actor = null;
		Context = null;
		base.Reset();
	}
}

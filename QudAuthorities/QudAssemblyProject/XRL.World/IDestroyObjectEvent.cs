using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IDestroyObjectEvent : MinEvent
{
	public GameObject Object;

	public bool Obliterate;

	public bool Silent;

	public string Reason;

	public string ThirdPersonReason;

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
		Obliterate = false;
		Silent = false;
		Reason = null;
		ThirdPersonReason = null;
		base.Reset();
	}
}

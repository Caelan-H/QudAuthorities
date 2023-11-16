using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeRenderEvent : IRenderEvent
{
	public new static readonly int ID;

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

	static BeforeRenderEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public BeforeRenderEvent()
	{
		base.ID = ID;
	}
}

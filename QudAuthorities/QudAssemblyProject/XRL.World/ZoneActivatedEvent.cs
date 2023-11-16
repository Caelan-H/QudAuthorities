using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ZoneActivatedEvent : IZoneEvent
{
	public new static readonly int ID;

	public new static int CascadeLevel => 15;

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

	static ZoneActivatedEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public ZoneActivatedEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}
}

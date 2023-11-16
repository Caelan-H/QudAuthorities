using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IChargeStorageEvent : IChargeEvent
{
	public int Transient;

	public bool UnlimitedTransient;

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

	public override void Reset()
	{
		Transient = 0;
		UnlimitedTransient = false;
		base.Reset();
	}

	public override Event GenerateRegisteredEvent(string ID)
	{
		Event @event = base.GenerateRegisteredEvent(ID);
		@event.SetParameter("Transient", Transient);
		@event.SetFlag("UnlimitedTransient", UnlimitedTransient);
		return @event;
	}

	public override void SyncFromRegisteredEvent(Event E, bool AllFields = false)
	{
		base.SyncFromRegisteredEvent(E, AllFields);
		Transient = E.GetIntParameter("Transient");
		UnlimitedTransient = E.HasFlag("UnlimitedTransient");
	}
}

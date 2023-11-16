using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IChargeEvent : MinEvent
{
	public GameObject Source;

	public int Amount;

	public int StartingAmount;

	public int Multiple = 1;

	public int Pass;

	public long GridMask;

	public bool Forced;

	public bool LiveOnly;

	public bool IncludeTransient = true;

	public bool IncludeBiological = true;

	public int? PowerLoadLevel;

	public int Used
	{
		get
		{
			return StartingAmount - Amount;
		}
		set
		{
			Amount = StartingAmount - value;
		}
	}

	public new static int CascadeLevel => 4;

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

	public override void Reset()
	{
		Source = null;
		Amount = 0;
		StartingAmount = 0;
		Multiple = 1;
		Pass = 0;
		GridMask = 0L;
		Forced = false;
		LiveOnly = false;
		IncludeTransient = true;
		IncludeBiological = true;
		PowerLoadLevel = null;
		base.Reset();
	}

	public virtual Event GenerateRegisteredEvent(string ID)
	{
		Event @event = Event.New(ID);
		@event.SetParameter("Source", Source);
		@event.SetParameter("Charge", Amount);
		@event.SetParameter("StartingCharge", StartingAmount);
		@event.SetParameter("MultipleCharge", Multiple);
		@event.SetParameter("GridMask", GridMask);
		@event.SetFlag("Forced", Forced);
		@event.SetFlag("LiveOnly", LiveOnly);
		@event.SetFlag("IncludeTransient", IncludeTransient);
		@event.SetFlag("IncludeBiological", IncludeBiological);
		@event.SetParameter("PowerLoadLevel", PowerLoadLevel ?? 100);
		return @event;
	}

	public virtual void SyncFromRegisteredEvent(Event E, bool AllFields = false)
	{
		Amount = E.GetIntParameter("Charge");
		if (AllFields)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Source");
			if (gameObjectParameter != null)
			{
				Source = gameObjectParameter;
			}
			StartingAmount = E.GetIntParameter("StartingCharge");
			Multiple = E.GetIntParameter("MultipleCharge");
			GridMask = E.GetIntParameter("GridMask");
			Forced = E.HasFlag("Forced");
			LiveOnly = E.HasFlag("LiveOnly");
			IncludeTransient = E.HasFlag("IncludeTransient");
			IncludeBiological = E.HasFlag("IncludeBiological");
			PowerLoadLevel = E.GetIntParameter("PowerLoadLevel");
		}
	}

	public bool SendRegisteredEvent(GameObject Object, string ID)
	{
		if (Object.HasRegisteredEvent(ID))
		{
			return Object.FireEvent(GenerateRegisteredEvent(ID));
		}
		return true;
	}

	public bool CheckRegisteredEvent(GameObject Object, string ID)
	{
		if (Object.HasRegisteredEvent(ID))
		{
			Event e = GenerateRegisteredEvent(ID);
			bool result = Object.FireEvent(e);
			SyncFromRegisteredEvent(e);
			return result;
		}
		return true;
	}
}

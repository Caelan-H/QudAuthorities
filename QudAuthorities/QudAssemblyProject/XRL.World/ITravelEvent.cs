using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class ITravelEvent : MinEvent
{
	public GameObject Actor;

	public string TravelClass;

	public int PercentageBonus;

	public Dictionary<string, int> ApplicationTracking;

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

	public override void Reset()
	{
		Actor = null;
		TravelClass = null;
		PercentageBonus = 0;
		ApplicationTracking?.Clear();
		base.Reset();
	}

	public int GetApplied(string Application)
	{
		if (ApplicationTracking != null && !string.IsNullOrEmpty(Application) && ApplicationTracking.TryGetValue(Application, out var value))
		{
			return value;
		}
		return 0;
	}

	public void SetApplied(string Application, int Value)
	{
		if (!string.IsNullOrEmpty(Application))
		{
			if (ApplicationTracking == null)
			{
				ApplicationTracking = new Dictionary<string, int>();
			}
			ApplicationTracking[Application] = Value;
		}
	}
}

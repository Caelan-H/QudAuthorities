using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Grounded : Effect
{
	public Grounded()
	{
		base.DisplayName = "{{w|grounded}}";
	}

	public Grounded(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 83886336;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Cannot fly.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsCreature)
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyGrounded")) || !ApplyEffectEvent.Check(Object, "Grounded", this))
		{
			return false;
		}
		Flight.Fall(Object);
		return true;
	}
}

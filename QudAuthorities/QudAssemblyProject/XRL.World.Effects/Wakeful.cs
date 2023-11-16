using System;

namespace XRL.World.Effects;

[Serializable]
public class Wakeful : Effect
{
	public Wakeful()
	{
		base.DisplayName = "{{W|wakeful}}";
	}

	public Wakeful(int Duration)
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
		return 83886082;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		if (Asleep.UsesSleepMode(base.Object))
		{
			return "{{W|safe mode}}";
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		if (Asleep.UsesSleepMode(base.Object))
		{
			return "Cannot be put in sleep mode.";
		}
		return "Cannot fall asleep.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart("Brain"))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyWakeful")) || !ApplyEffectEvent.Check(Object, "Wakeful", this))
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyInvoluntarySleep");
		Object.RegisterEffectEvent(this, "CanApplyInvoluntarySleep");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyInvoluntarySleep");
		Object.UnregisterEffectEvent(this, "CanApplyInvoluntarySleep");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyInvoluntarySleep" || E.ID == "ApplyInvoluntarySleep")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}

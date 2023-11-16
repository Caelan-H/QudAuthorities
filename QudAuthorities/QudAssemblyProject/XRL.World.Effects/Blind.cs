using System;

namespace XRL.World.Effects;

[Serializable]
public class Blind : Effect
{
	public Blind()
	{
		base.DisplayName = "{{K|blind}}";
	}

	public Blind(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDetails()
	{
		return "Can't see.";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117448704;
	}

	public override bool Apply(GameObject Object)
	{
		return Object.FireEvent(Event.New("ApplyBlind", "Duration", base.Duration));
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (base.Duration > 0 && base.Object.IsPlayer())
		{
			base.Object.CurrentCell?.ParentZone.ClearLightMap();
			return false;
		}
		return base.HandleEvent(E);
	}
}

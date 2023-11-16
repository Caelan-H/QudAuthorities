using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Luminous : Effect
{
	public Luminous()
	{
	}

	public Luminous(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 83886144;
	}

	public override string GetDescription()
	{
		return "{{m|phosphorescent}}";
	}

	public override string GetDetails()
	{
		return "Radiates light in radius 3.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("The glow dims until it's extinguished.");
		}
		base.Remove(Object);
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
		if (base.Duration > 0)
		{
			AddLight(2);
		}
		return base.HandleEvent(E);
	}
}

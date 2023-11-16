using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class ShieldWall : Effect
{
	public ShieldWall()
	{
		base.DisplayName = "shield wall";
	}

	public ShieldWall(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67108992;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{g|shield wall}}";
	}

	public override string GetDetails()
	{
		return "Blocks all incoming melee attacks.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("ShieldWall"))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyShieldWall", "Effect", this)))
		{
			return false;
		}
		DidX("raise", Object.its + " shield", "!", null, Object);
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 21 && num < 31)
		{
			E.Tile = null;
			E.RenderString = "\u0004";
			E.ColorString = "&B";
		}
		return true;
	}
}

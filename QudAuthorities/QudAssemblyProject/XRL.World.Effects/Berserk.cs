using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Berserk : Effect
{
	public Berserk()
	{
		base.DisplayName = "berserk";
	}

	public Berserk(int Duration)
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
		return 67108866;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "100% chance to dismember with axes.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0 && base.Duration != 9999 && base.Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(base.Duration.Things("turn remains", "turns remain") + " until your berserker rage ends.");
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 45 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}
}

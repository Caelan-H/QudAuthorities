using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Suppressed : Effect
{
	public Suppressed()
	{
		base.DisplayName = "suppressed";
	}

	public Suppressed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 100663424;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Can't move.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsPotentiallyMobile())
		{
			return false;
		}
		if (!Object.FireEvent("ApplySuppressed"))
		{
			return false;
		}
		if (Object.GetEffect("Suppressed") is Suppressed suppressed && base.Duration > suppressed.Duration)
		{
			suppressed.Duration = base.Duration;
		}
		if (Object.IsPlayer())
		{
			DidX("are", "suppressed", "!", null, null, Object);
		}
		Object.ParticleText("*suppressed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == LeaveCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeaveCellEvent E)
	{
		if (base.Duration > 0 && !E.Forced && E.Type != "Teleporting")
		{
			base.Object.UseEnergy(1000, "Suppression");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.Tile = null;
				E.RenderString = "\u000f";
				E.ColorString = "&C^K";
			}
		}
		return true;
	}
}

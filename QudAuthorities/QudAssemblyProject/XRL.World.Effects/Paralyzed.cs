using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Paralyzed : Effect
{
	public int DVPenalty;

	public int SaveTarget;

	public Paralyzed()
	{
		base.DisplayName = "{{C|paralyzed}}";
	}

	public Paralyzed(int Duration, int SaveTarget)
		: this()
	{
		this.SaveTarget = SaveTarget;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool SameAs(Effect e)
	{
		Paralyzed paralyzed = e as Paralyzed;
		if (paralyzed.DVPenalty != DVPenalty)
		{
			return false;
		}
		if (paralyzed.SaveTarget != SaveTarget)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		if (DVPenalty < 0)
		{
			return "Can't move or attack.\n" + DVPenalty + " DV";
		}
		return "Can't move or attack.\nDV set to 0.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Paralyzed"))
		{
			Paralyzed paralyzed = Object.GetEffect("Paralyzed") as Paralyzed;
			if (base.Duration > paralyzed.Duration)
			{
				paralyzed.Duration = base.Duration;
			}
			return true;
		}
		if (!Object.FireEvent("ApplyParalyze"))
		{
			return false;
		}
		ApplyStats();
		DidX("are", "paralyzed", "!", null, null, Object);
		Object.ParticleText("*paralyzed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		int combatDV = Stats.GetCombatDV(base.Object);
		if (combatDV > 0)
		{
			DVPenalty += combatDV;
			base.StatShifter.SetStatShift(base.Object, "DV", -DVPenalty);
		}
		else
		{
			DVPenalty = 0;
		}
	}

	private void UnapplyStats()
	{
		DVPenalty = 0;
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You are {{C|paralyzed}}.");
			}
			E.PreventAction = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == base.Object && !E.Mental)
		{
			E.Message = base.Object.T() + base.Object.Is + " utterly unresponsive.";
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMod");
		Object.RegisterEffectEvent(this, "CanMoveExtremities");
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMod");
		Object.UnregisterEffectEvent(this, "CanMoveExtremities");
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 15 && num < 30)
		{
			E.Tile = null;
			E.RenderString = "X";
			E.ColorString = "&C^c";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			if (base.Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanChangeMovementMode" || E.ID == "CanMoveExtremities")
		{
			if (base.Duration > 0 && !E.HasFlag("Involuntary"))
			{
				if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.Show("You are {{C|paralyzed}}!");
				}
				return false;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}

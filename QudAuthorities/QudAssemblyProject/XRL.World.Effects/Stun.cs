using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Stun : Effect
{
	public int DVPenalty;

	public int SaveTarget = 15;

	public bool bDontStunIfPlayer;

	public Stun()
	{
		base.DisplayName = "{{C|stunned}}";
	}

	public Stun(int Duration, int SaveTarget)
		: this()
	{
		this.SaveTarget = SaveTarget;
		base.Duration = Duration;
	}

	public Stun(int Duration, int SaveTarget, bool bDontStunIfPlayer)
		: this(Duration, SaveTarget)
	{
		this.bDontStunIfPlayer = bDontStunIfPlayer;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{C|stunned}}";
	}

	public override string GetDetails()
	{
		if (DVPenalty < 0)
		{
			return "Can't take actions.\n" + DVPenalty + " DV";
		}
		return "Can't take actions.\nDV set to 0.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.pBrain == null)
		{
			return false;
		}
		if (Object.GetEffect("Stun") is Stun stun)
		{
			if (Object.IsPlayer() && bDontStunIfPlayer)
			{
				base.Duration = 0;
				return false;
			}
			stun.Duration += base.Duration;
			if (stun.SaveTarget < SaveTarget)
			{
				stun.SaveTarget = SaveTarget;
			}
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Stun", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyStun"))
		{
			return false;
		}
		DidX("are", "stunned", "!", null, null, Object);
		Object.ParticleText("*stunned*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		Object.ForfeitTurn();
		ApplyStats();
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
			DVPenalty = combatDV;
			base.StatShifter.SetStatShift("DV", -DVPenalty);
		}
		else
		{
			DVPenalty = 0;
		}
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
		DVPenalty = 0;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == base.Object)
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = base.Object.Poss("mind") + " is in disarray.";
			}
			else
			{
				E.Message = base.Object.T() + base.Object.GetVerb("don't") + " seem to understand you.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 10 && num < 25)
			{
				E.Tile = null;
				E.RenderString = "!";
				E.ColorString = "&C^c";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (base.Duration > 0)
			{
				if (!base.Object.MakeSave("Toughness", SaveTarget, null, null, "Stun"))
				{
					DidX("remain", "stunned", "!", null, null, base.Object);
					if (base.Object.IsPlayer())
					{
						XRLCore.Core.RenderDelay(500);
					}
					else
					{
						base.Object.ParticleText("*remains stunned*", IComponent<GameObject>.ConsequentialColorChar(null, base.Object));
					}
					base.Object.ForfeitTurn();
					if (base.Duration != 9999)
					{
						base.Duration--;
					}
					return false;
				}
				base.Object.ParticleText("*made save vs. stun*", IComponent<GameObject>.ConsequentialColorChar(base.Object));
				base.Duration = 0;
			}
		}
		else if (E.ID == "IsMobile")
		{
			if (base.Duration > 0)
			{
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

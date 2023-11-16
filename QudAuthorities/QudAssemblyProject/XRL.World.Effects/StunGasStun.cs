using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class StunGasStun : Effect
{
	public int Density = 20;

	public int SpeedPenalty;

	public StunGasStun()
	{
		base.Duration = 1;
		base.DisplayName = GetDescription();
	}

	public StunGasStun(int _Density)
		: this()
	{
		Density = _Density;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 117440520;
	}

	public override string GetDescription()
	{
		return "{{C|stunned by gas}}";
	}

	public override string GetDetails()
	{
		if (Density > 60)
		{
			return "(very dense) Can't take actions.";
		}
		if (Density > 40)
		{
			return "(dense) -60% Quickness";
		}
		return "(light) -30% Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart("Brain"))
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyStunGasStun"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "StunGasStun", this))
		{
			return false;
		}
		return Object.FireEvent("ApplyStunGasStun");
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CommandTakeAction");
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CommandTakeAction");
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 10 && num < 25)
			{
				if (Density > 60)
				{
					E.Tile = null;
					E.RenderString = "!";
					E.ColorString = "&C^c";
				}
				else if (SpeedPenalty > 0)
				{
					E.Tile = null;
					E.RenderString = "\u0019";
					E.ColorString = "&C^c";
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTakeAction")
		{
			if (base.Duration > 0)
			{
				base.Duration = 0;
				if (base.Object.FireEvent("ApplyStun"))
				{
					if (Density > 60)
					{
						DidX("are", "stunned", "!", null, null, base.Object);
						base.Object.ForfeitTurn();
						base.Object.ParticleText("*stunned*", IComponent<GameObject>.ConsequentialColorChar(null, base.Object));
						return false;
					}
					if (Density > 40)
					{
						if (SpeedPenalty != 0)
						{
							UnapplyStats();
						}
						SpeedPenalty = base.Object.Stat("Speed") * 6 / 10;
						ApplyStats();
					}
					else
					{
						if (SpeedPenalty != 0)
						{
							UnapplyStats();
						}
						SpeedPenalty = base.Object.Stat("Speed") * 3 / 10;
						ApplyStats();
					}
				}
			}
		}
		else if (E.ID == "IsMobile")
		{
			if (base.Duration > 0 && Density > 60)
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

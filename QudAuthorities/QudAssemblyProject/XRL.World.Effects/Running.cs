using System;
using System.Text;
using XRL.Core;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Running : Effect
{
	public bool PenaltyApplied;

	public string MessageName;

	public bool SpringingEffective;

	public int MovespeedBonus;

	public Running()
	{
		base.DisplayName = "sprinting";
	}

	public Running(int Duration = 0, string DisplayName = "sprinting", string MessageName = "sprinting", bool SpringingEffective = true)
		: this()
	{
		base.Duration = Duration;
		base.DisplayName = DisplayName;
		this.MessageName = MessageName;
		this.SpringingEffective = SpringingEffective;
	}

	public override int GetEffectType()
	{
		return 218103936;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!base.Object.HasSkill("Tactics_Hurdle"))
		{
			stringBuilder.Compound("-5 DV", '\n');
		}
		stringBuilder.Compound($"Moves at {GetMovespeedMultiplier()}X the normal speed. (+{MovespeedBonus} move speed)", '\n');
		if (!IsEnhanced())
		{
			if (base.Object.HasSkill("Pistol_SlingAndRun"))
			{
				stringBuilder.Compound("Reduced accuracy with missile weapons, except pistols.", '\n');
			}
			else
			{
				stringBuilder.Compound("Reduced accuracy with missile weapons.", '\n');
			}
			stringBuilder.Compound("-10 to hit in melee combat.", '\n').Compound("Is ended by attacking in melee, by effects that interfere with movement, and by most other actions that have action costs, other than using physical mutations.", '\n');
		}
		if (base.Duration == 9999)
		{
			stringBuilder.Compound("Indefinite duration.", '\n');
		}
		else
		{
			stringBuilder.Compound(base.Duration + " " + ((base.Duration == 1) ? "round" : "rounds") + " left.", '\n');
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffectByClass("Running"))
		{
			return false;
		}
		if (!Object.CanChangeMovementMode(MessageName))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyRunning", "Effect", this)))
		{
			return false;
		}
		base.StatShifter.DefaultDisplayName = base.DisplayName;
		Object.MovementModeChanged(MessageName);
		DidX("begin", MessageName, "!");
		if (!Object.HasSkill("Tactics_Hurdle") && Object.HasStat("DV"))
		{
			base.StatShifter.SetStatShift("DV", -5);
			PenaltyApplied = true;
		}
		RecalcMovespeedBonus();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("stop", MessageName);
		base.StatShifter.RemoveStatShifts(Object);
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EndActionEvent.ID)
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (base.Object != null && base.Object.OnWorldMap())
			{
				base.Duration = 0;
			}
			else
			{
				RecalcMovespeedBonus();
				if (base.Duration != 9999 && base.Object.GetIntProperty("SkatesEquipped") <= 0)
				{
					base.Duration--;
				}
			}
			RecalcMovespeedBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndActionEvent E)
	{
		RecalcMovespeedBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && !E.Type.Contains("Movement") && E.Type.Contains("Physical") && !IsEnhanced() && E.Amount >= 500)
		{
			base.Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AttackerRollMeleeToHit");
		Object.RegisterEffectEvent(this, "BeginAttack");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AttackerRollMeleeToHit");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "BeginAttack");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 45)
			{
				E.Tile = null;
				E.RenderString = "\u001a";
				E.ColorString = "&G";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginAttack")
		{
			if (!IsEnhanced())
			{
				base.Duration = 0;
			}
		}
		else if (E.ID == "AttackerRollMeleeToHit")
		{
			if (!IsEnhanced())
			{
				E.SetParameter("Result", E.GetIntParameter("Result") - 10);
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (!IsEnhanced() || E.GetStringParameter("To") == "Frozen")
			{
				base.Object.RemoveEffect(this);
			}
			else
			{
				RecalcMovespeedBonus();
			}
		}
		return base.FireEvent(E);
	}

	public static bool IsEnhanced(GameObject Object)
	{
		if (Object == null)
		{
			return false;
		}
		return Object.GetIntProperty("EnhancedSprint") > 0;
	}

	public bool IsEnhanced()
	{
		return IsEnhanced(base.Object);
	}

	public void RecalcMovespeedBonus()
	{
		if (base.Duration > 0)
		{
			base.StatShifter.RemoveStatShift(base.Object, "MoveSpeed");
			int num = 100 - base.Object.Stat("MoveSpeed") + 100;
			MovespeedBonus = (int)((float)num * (GetMovespeedMultiplier() - 1f));
			base.StatShifter.SetStatShift("MoveSpeed", -MovespeedBonus);
		}
	}

	public float GetMovespeedMultiplier()
	{
		float num = ((!base.Object.HasEffect("Springing") || !SpringingEffective) ? 2f : 3f);
		Wings part = base.Object.GetPart<Wings>();
		if (part != null)
		{
			num *= 1f + part.SprintingMoveSpeedBonus(part.Level);
		}
		return num;
	}
}

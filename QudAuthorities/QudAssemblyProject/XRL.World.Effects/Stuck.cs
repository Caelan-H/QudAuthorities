using System;
using System.Text;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Stuck : Effect
{
	public const string DEFAULT_TEXT = "stuck";

	public const int DEFAULT_SAVE_TARGET = 15;

	public const string DEFAULT_SAVE_VS = "Stuck Restraint";

	public const string DEFAULT_DEFENDING_SAVE_BONUS_VS = "Move,Knockback";

	public const float DEFAULT_DEFENDING_SAVE_BONUS_FACTOR = 0.25f;

	public const float DEFAULT_KINETIC_RESISTANCE_LINEAR_BONUS_FACTOR = 1f;

	public const float DEFAULT_KINETIC_RESISTANCE_PERCENTAGE_BONUS_FACTOR = 1f;

	public GameObject DestroyOnBreak;

	public string Text = "stuck";

	public int _SaveTarget = 15;

	public string SaveVs = "Stuck Restraint";

	public string DependsOn;

	public string DefendingSaveBonusVs = "Move,Knockback";

	public float DefendingSaveBonusFactor = 0.25f;

	public float KineticResistanceLinearBonusFactor = 1f;

	public float KineticResistancePercentageBonusFactor = 1f;

	public int DefendingSaveBonus;

	public int KineticResistanceLinearBonus;

	public int KineticResistancePercentageBonus;

	public int SaveTarget
	{
		get
		{
			return _SaveTarget;
		}
		set
		{
			_SaveTarget = value;
			BonusSetup();
		}
	}

	public Stuck()
	{
		base.DisplayName = "stuck";
		BonusSetup();
	}

	public Stuck(int Duration, int SaveTarget = 15, string SaveVs = "Stuck Restraint", GameObject DestroyOnBreak = null, string Text = "stuck", string DependsOn = null, string DefendingSaveBonusVs = "Move,Knockback", float DefendingSaveBonusDivisor = 0.25f, float KineticResistanceLinearBonus = 1f, float KineticResistancePercentageBonus = 1f)
		: this()
	{
		base.Duration = Duration;
		this.SaveTarget = SaveTarget;
		this.SaveVs = SaveVs;
		this.DestroyOnBreak = DestroyOnBreak;
		this.Text = Text;
		this.DependsOn = DependsOn;
		this.DefendingSaveBonusVs = DefendingSaveBonusVs;
		DefendingSaveBonusFactor = DefendingSaveBonusFactor;
		KineticResistanceLinearBonusFactor = KineticResistanceLinearBonus;
		KineticResistancePercentageBonusFactor = KineticResistancePercentageBonus;
		BonusSetup();
	}

	private void BonusSetup()
	{
		DefendingSaveBonus = (int)Math.Round((float)SaveTarget * DefendingSaveBonusFactor);
		KineticResistanceLinearBonus = (int)Math.Round((float)SaveTarget * KineticResistanceLinearBonusFactor);
		KineticResistancePercentageBonus = (int)Math.Round((float)SaveTarget * KineticResistancePercentageBonusFactor);
	}

	public override int GetEffectType()
	{
		return 117440640;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		Stuck stuck = e as Stuck;
		if (stuck.DestroyOnBreak != DestroyOnBreak)
		{
			return false;
		}
		if (stuck.Text != Text)
		{
			return false;
		}
		if (stuck.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (stuck.SaveVs != SaveVs)
		{
			return false;
		}
		if (stuck.DependsOn != DependsOn)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusVs != DefendingSaveBonusVs)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusFactor != DefendingSaveBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonusFactor != KineticResistanceLinearBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonusFactor != KineticResistancePercentageBonusFactor)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusFactor != DefendingSaveBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonusFactor != KineticResistanceLinearBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonusFactor != KineticResistancePercentageBonusFactor)
		{
			return false;
		}
		if (stuck.DefendingSaveBonus != DefendingSaveBonus)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonus != KineticResistanceLinearBonus)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonus != KineticResistancePercentageBonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EndTurnEvent.ID && ID != GetKineticResistanceEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.Flying)
		{
			E.Uncacheable = true;
			E.MinWeight(100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			CheckDependsOn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckDependsOn(Immediate: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckDependsOn(Immediate: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.validate(ref DestroyOnBreak);
		if (base.Duration > 0)
		{
			CheckDependsOn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (DefendingSaveBonus != 0 && SavingThrows.Applicable(DefendingSaveBonusVs, E.Vs))
		{
			E.Roll += DefendingSaveBonus;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetKineticResistanceEvent E)
	{
		if (KineticResistanceLinearBonus > 0)
		{
			E.LinearIncrease += KineticResistanceLinearBonus;
		}
		else if (KineticResistanceLinearBonus < 0)
		{
			E.LinearReduction += -KineticResistanceLinearBonus;
		}
		if (KineticResistancePercentageBonus > 0)
		{
			E.PercentageIncrease += KineticResistancePercentageBonus;
		}
		else if (KineticResistancePercentageBonus < 0)
		{
			E.PercentageReduction += -KineticResistancePercentageBonus;
		}
		return base.HandleEvent(E);
	}

	public void CheckDependsOn(bool Immediate = false)
	{
		if (string.IsNullOrEmpty(DependsOn))
		{
			return;
		}
		GameObject obj = GameObject.findById(DependsOn);
		if (!GameObject.validate(ref obj) || obj.IsInGraveyard() || !obj.InSameOrAdjacentCellTo(base.Object) || !obj.PhaseMatches(base.Object))
		{
			if (Immediate)
			{
				base.Object.RemoveEffect(this);
			}
			else
			{
				base.Duration = 0;
			}
		}
	}

	public void CheckDependsOn(string Invalidate, bool Immediate = false)
	{
		if (!string.IsNullOrEmpty(DependsOn) && Invalidate == DependsOn)
		{
			if (Immediate)
			{
				base.Object.RemoveEffect(this);
			}
			else
			{
				base.Duration = 0;
			}
		}
		else
		{
			CheckDependsOn(Immediate);
		}
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Stuck where ").Append(base.Object.it).Append(base.Object.Is)
			.Append('.');
		SavingThrows.AppendSaveBonusDescription(stringBuilder, DefendingSaveBonus, DefendingSaveBonusVs);
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("Energy") || Object.GetMatterPhase() > 1)
		{
			return false;
		}
		if (!Object.CanChangeMovementMode("Stuck", ShowMessage: false, Involuntary: true))
		{
			return false;
		}
		if (Object.FireEvent("ApplyStuck"))
		{
			DidX("are", Text, "!", null, null, Object);
			Object.ParticleText("*" + Text + "*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			Object.ForfeitTurn();
			Object.MovementModeChanged(null, Involuntary: true);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.validate(ref DestroyOnBreak))
		{
			DestroyOnBreak.Destroy();
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginAttack");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "CheckStuck");
		Object.RegisterEffectEvent(this, "IsMobile");
		Object.RegisterEffectEvent(this, "LeaveCell");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginAttack");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "CheckStuck");
		Object.UnregisterEffectEvent(this, "IsMobile");
		Object.UnregisterEffectEvent(this, "LeaveCell");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			if (base.Duration > 0 && !base.Object.IsTryingToJoinPartyLeader())
			{
				return false;
			}
		}
		else if (E.ID == "LeaveCell" || E.ID == "BeginAttack")
		{
			if (E.ID == "LeaveCell" && (E.HasFlag("Forced") || E.GetStringParameter("Type") == "Teleporting" || base.Object.IsTryingToJoinPartyLeader()))
			{
				base.Object.RemoveEffect(this);
			}
			else if (base.Duration > 0)
			{
				if (base.Object.MakeSave("Strength", SaveTarget - base.Object.GetIntProperty("Stable"), null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, GameObject.findById(DependsOn)))
				{
					DidX("break", "free", "!", null, base.Object);
					base.Object.RemoveEffect(this);
				}
				else
				{
					if (base.Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are {{|" + Text + "}}!");
					}
					if (!E.HasParameter("Dragging") && !E.HasFlag("Forced"))
					{
						base.Object.UseEnergy(1000);
					}
					if (E.ID == "LeaveCell")
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "CanChangeMovementMode" || E.ID == "CanChangeBodyPosition")
		{
			if (base.Duration > 0)
			{
				if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.Show("You are {{|" + Text + "}}!");
				}
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (base.Duration > 0)
			{
				base.Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "CheckStuck")
		{
			string stringParameter = E.GetStringParameter("Invalidate");
			if (!string.IsNullOrEmpty(stringParameter))
			{
				CheckDependsOn(stringParameter, Immediate: true);
			}
			else
			{
				CheckDependsOn(Immediate: true);
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.Tile = "terrain/sw_web.bmp";
				E.RenderString = "\u000f";
				E.ColorString = "&Y^K";
			}
		}
		return true;
	}
}

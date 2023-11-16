using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Submerged : Effect
{
	public Guid EndAbilityID = Guid.Empty;

	public int MovePenalty;

	public int HiddenDifficulty = 20;

	public Submerged()
	{
		base.Duration = 1;
		base.DisplayName = "{{B|submerged}}";
	}

	public Submerged(int MovePenalty = 0, int HiddenDifficulty = 20)
		: this()
	{
		this.MovePenalty = MovePenalty;
		this.HiddenDifficulty = HiddenDifficulty;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Staying underwater.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Submerged"))
		{
			return false;
		}
		GameObject gameObject = Object.CurrentCell?.GetAquaticSupportFor(Object);
		if (gameObject == null)
		{
			return false;
		}
		DidX("submerge", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		EndAbilityID = AddMyActivatedAbility("Surface", "CommandEndSubmerged", "Maneuvers", null, "\u0018");
		Object.LiquidSplash(gameObject.LiquidVolume?.GetPrimaryLiquid());
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		try
		{
			Hidden hidden = Object.GetPart("Hidden") as Hidden;
			GameObject gameObject = Object.CurrentCell?.GetAquaticSupportFor(Object);
			if (gameObject != null)
			{
				hidden?.Reveal(Silent: true);
				DidXToY("emerge", "from", gameObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
				Object.LiquidSplash(gameObject.LiquidVolume?.GetPrimaryLiquid());
			}
			else if (hidden != null)
			{
				hidden.Silent = false;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Submerged::Remove", x);
		}
		RemoveMyActivatedAbility(ref EndAbilityID);
		UnapplyChanges();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EarlyBeforeBeginTakeActionEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == LeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeavingCellEvent E)
	{
		Surface();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "AIAttackMelee");
		Object.RegisterEffectEvent(this, "AIAttackRange");
		Object.RegisterEffectEvent(this, "AICanAttackMelee");
		Object.RegisterEffectEvent(this, "AICanAttackRange");
		Object.RegisterEffectEvent(this, "AILookForTarget");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDirectMove");
		Object.RegisterEffectEvent(this, "BeforeTeleport");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "CanMoveExtremities");
		Object.RegisterEffectEvent(this, "CommandEndSubmerged");
		Object.RegisterEffectEvent(this, "FiringMissile");
		Object.RegisterEffectEvent(this, "LateBeforeApplyDamage");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		Object.RegisterEffectEvent(this, "PerformMeleeAttack");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "AIAttackMelee");
		Object.UnregisterEffectEvent(this, "AIAttackRange");
		Object.UnregisterEffectEvent(this, "AICanAttackMelee");
		Object.UnregisterEffectEvent(this, "AICanAttackRange");
		Object.UnregisterEffectEvent(this, "AILookForTarget");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDirectMove");
		Object.UnregisterEffectEvent(this, "BeforeTeleport");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "CanMoveExtremities");
		Object.UnregisterEffectEvent(this, "CommandEndSubmerged");
		Object.UnregisterEffectEvent(this, "FiringMissile");
		Object.UnregisterEffectEvent(this, "LateBeforeApplyDamage");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		Object.UnregisterEffectEvent(this, "PerformMeleeAttack");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AILookForTarget" || E.ID == "AIAttackMelee" || E.ID == "AIAttackRange" || E.ID == "AICanAttackMelee" || E.ID == "AICanAttackRange")
		{
			if (base.Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeginAttack" || E.ID == "CanFireMissileWeapon")
		{
			if (base.Duration > 0)
			{
				if (base.Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that while submerged.");
				}
				return false;
			}
		}
		else if (E.ID == "LateBeforeApplyDamage")
		{
			if (base.Duration > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
				Damage damage = E.GetParameter("Damage") as Damage;
				if (gameObjectParameter != null && gameObjectParameter != base.Object && damage != null && !damage.HasAttribute("Bleeding"))
				{
					DidX("are", "forced to the surface", null, null, null, base.Object);
					base.Object.RemoveEffect(this);
					return false;
				}
			}
		}
		else if (E.ID == "BeforeDirectMove" || E.ID == "BeforeTeleport" || E.ID == "PerformMeleeAttack" || E.ID == "FiringMissile")
		{
			if (base.Duration > 0)
			{
				base.Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanChangeMovementMode" || E.ID == "CanMoveExtremities")
		{
			if (base.Duration > 0 && !E.HasFlag("Involuntary"))
			{
				string stringParameter = E.GetStringParameter("To");
				if (stringParameter != "Swimming" && stringParameter != "Wading" && E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that while submerged.");
				}
				return false;
			}
		}
		else if (E.ID == "CommandEndSubmerged")
		{
			Surface();
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			string stringParameter2 = E.GetStringParameter("To");
			if (stringParameter2 != "Swimming" && stringParameter2 != "Wading")
			{
				Surface();
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", MovePenalty);
		if (!(base.Object.GetPart("Hidden") is Hidden hidden))
		{
			Hidden p = new Hidden(HiddenDifficulty, Silent: true);
			base.Object.AddPart(p);
		}
		else
		{
			hidden.Difficulty = HiddenDifficulty;
			hidden.Silent = true;
			hidden.Found = false;
		}
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
		if (base.Object.GetPart("Hidden") is Hidden hidden)
		{
			hidden.Reveal();
		}
	}

	private bool Validate()
	{
		if (CanSubmerge())
		{
			return true;
		}
		base.Object.RemoveEffect(this);
		return false;
	}

	public void Surface()
	{
		base.Object.RemoveEffect(this);
	}

	public bool CanSubmerge()
	{
		return CanSubmerge(base.Object);
	}

	public static bool CanSubmerge(GameObject Object)
	{
		return CanSubmergeIn(Object, Object?.CurrentCell);
	}

	public static bool CanSubmergeIn(GameObject Object, Cell C)
	{
		if (C != null && GameObject.validate(ref Object))
		{
			return C.HasAquaticSupportFor(Object);
		}
		return false;
	}
}

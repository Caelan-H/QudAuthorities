using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Burrowed : Effect
{
	public Guid EndAbilityID = Guid.Empty;

	public int MovePenalty;

	public Burrowed()
	{
		base.Duration = 1;
		base.DisplayName = "{{w|burrowed}}";
	}

	public Burrowed(int MSPenalty)
		: this()
	{
		MovePenalty = MSPenalty;
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
		return "Traveling underground.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Burrowed"))
		{
			return false;
		}
		DidX("burrow", "into the ground");
		EndAbilityID = AddMyActivatedAbility("Stop Burrowing", "CommandEndBurrowing", "Physical Mutation", null, "\u0018");
		Object.DustPuff();
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		RemoveMyActivatedAbility(ref EndAbilityID);
		Object.DustPuff();
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		base.Object.pRender.Visible = false;
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", MovePenalty);
	}

	private void UnapplyChanges()
	{
		base.Object.pRender.Visible = true;
		base.StatShifter.RemoveStatShifts(base.Object);
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
		Object.RegisterEffectEvent(this, "CommandEndBurrowing");
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
		Object.UnregisterEffectEvent(this, "CommandEndBurrowing");
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
					Popup.ShowFail("You cannot do that while burrowed.");
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
					DidX("are", "forced to the surface");
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
				if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that while burrowed.");
				}
				return false;
			}
		}
		else if (E.ID == "CommandEndBurrowing" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			DidX("emerge", "from the ground");
			base.Object.RemoveEffect(this);
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
}

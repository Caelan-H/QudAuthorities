using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, the damage roll used when the device is charged
///             is increased by the standard power load bonus, i.e. 2 for the standard
///             overload power load of 400.
///             </remarks>
[Serializable]
public class Gaslight : IPoweredPart
{
	public string ChargedName = "1";

	public string UnchargedName = "0";

	public int ChargedPenetrationBonus = 4;

	public int UnchargedPenetrationBonus;

	public string ChargedDamage = "1d6";

	public string UnchargedDamage = "1d2";

	public string ChargedSkill = "ShortBlades";

	public string UnchargedSkill = "Cudgel";

	public bool Active;

	public Gaslight()
	{
		ChargeUse = 10;
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		Gaslight gaslight = p as Gaslight;
		if (gaslight.ChargedName != ChargedName)
		{
			return false;
		}
		if (gaslight.UnchargedName != UnchargedName)
		{
			return false;
		}
		if (gaslight.ChargedPenetrationBonus != ChargedPenetrationBonus)
		{
			return false;
		}
		if (gaslight.UnchargedPenetrationBonus != UnchargedPenetrationBonus)
		{
			return false;
		}
		if (gaslight.ChargedDamage != ChargedDamage)
		{
			return false;
		}
		if (gaslight.UnchargedDamage != UnchargedDamage)
		{
			return false;
		}
		if (gaslight.ChargedSkill != ChargedSkill)
		{
			return false;
		}
		if (gaslight.UnchargedSkill != UnchargedSkill)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		SyncActiveState();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		SyncActiveState();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		SyncActiveState();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != DamageConstantAdjustedEvent.ID && ID != DamageDieSizeAdjustedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != GetDisplayNameEvent.ID && ID != ModificationAppliedEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DamageConstantAdjustedEvent E)
	{
		ChargedDamage = DieRoll.AdjustResult(ChargedDamage, E.Amount);
		if (Active)
		{
			MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
			if (part != null)
			{
				part.BaseDamage = ChargedDamage;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DamageDieSizeAdjustedEvent E)
	{
		ChargedDamage = DieRoll.AdjustDieSize(ChargedDamage, E.Amount);
		if (Active)
		{
			MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
			if (part != null)
			{
				part.BaseDamage = ChargedDamage;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		SyncActiveState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		SyncActiveState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SyncActiveState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.AsIfKnown && E.DB.PrimaryBase == UnchargedName)
		{
			E.ReplacePrimaryBase(ChargedName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModificationAppliedEvent E)
	{
		if (Active)
		{
			Deactivate();
			Activate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ParentObject.pRender?.DisplayName == ChargedName)
		{
			ParentObject.pRender.DisplayName = UnchargedName;
		}
		MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
		if (part != null)
		{
			part.PenBonus = UnchargedPenetrationBonus;
			part.BaseDamage = UnchargedDamage;
			part.Skill = UnchargedSkill;
		}
		Active = false;
		SyncActiveState();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GetWeaponPenModifier");
		base.Register(Object);
	}

	public void Activate()
	{
		if (Active)
		{
			return;
		}
		if (ParentObject.pRender?.DisplayName == UnchargedName)
		{
			ParentObject.pRender.DisplayName = ChargedName;
		}
		MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
		if (part != null)
		{
			part.PenBonus += ChargedPenetrationBonus - UnchargedPenetrationBonus;
			part.BaseDamage = ChargedDamage;
			int num = MyPowerLoadBonus();
			if (num != 0)
			{
				part.BaseDamage = DieRoll.AdjustResult(part.BaseDamage, num);
			}
			part.Skill = ChargedSkill;
		}
		Active = true;
	}

	public void Deactivate()
	{
		if (Active)
		{
			if (ParentObject.pRender?.DisplayName == ChargedName)
			{
				ParentObject.pRender.DisplayName = UnchargedName;
			}
			MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
			if (part != null)
			{
				part.PenBonus -= ChargedPenetrationBonus - UnchargedPenetrationBonus;
				part.BaseDamage = UnchargedDamage;
				part.Skill = UnchargedSkill;
			}
			Active = false;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetWeaponPenModifier")
		{
			ParentObject.UseCharge(ChargeUse, LiveOnly: false, 0L);
			SyncActiveState();
		}
		return base.FireEvent(E);
	}

	public void SyncActiveState()
	{
		if (Active)
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				Deactivate();
			}
		}
		else if (!IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Activate();
		}
	}
}

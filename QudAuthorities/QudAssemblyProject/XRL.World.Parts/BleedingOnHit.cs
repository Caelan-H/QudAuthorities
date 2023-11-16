using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BleedingOnHit : IActivePart
{
	public string Amount = "1d2";

	public int SaveTarget = 20;

	public string RequireDamageAttribute;

	public bool SelfOnly = true;

	public bool Stack;

	public BleedingOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		BleedingOnHit bleedingOnHit = p as BleedingOnHit;
		if (bleedingOnHit.Amount != Amount)
		{
			return false;
		}
		if (bleedingOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (bleedingOnHit.RequireDamageAttribute != RequireDamageAttribute)
		{
			return false;
		}
		if (bleedingOnHit.SelfOnly != SelfOnly)
		{
			return false;
		}
		if (bleedingOnHit.Stack != Stack)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public void CheckApply(Event E)
	{
		if ((string.IsNullOrEmpty(RequireDamageAttribute) || (E.GetParameter("Damage") is Damage damage && damage.HasAttribute(RequireDamageAttribute))) && !IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			E.GetGameObjectParameter("Defender").ApplyEffect(new Bleeding(Amount, SaveTarget, gameObjectParameter, Stack));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WieldedWeaponHit")
		{
			if (!SelfOnly)
			{
				CheckApply(E);
			}
		}
		else if (E.ID == "WeaponHit")
		{
			if (SelfOnly)
			{
				CheckApply(E);
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject")?.RegisterPartEvent(this, "WieldedWeaponHit");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject")?.UnregisterPartEvent(this, "WieldedWeaponHit");
		}
		return base.FireEvent(E);
	}
}

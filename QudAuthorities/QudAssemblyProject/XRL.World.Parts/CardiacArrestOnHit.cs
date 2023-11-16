using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CardiacArrestOnHit : IActivePart
{
	public int Chance = 100;

	public int SaveTarget = 20;

	public string SaveAttribute = "Toughness";

	public string SaveVs = "CardiacArrest CardiacArrestInduction";

	public string RequireDamageAttribute;

	public CardiacArrestOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		CardiacArrestOnHit cardiacArrestOnHit = p as CardiacArrestOnHit;
		if (cardiacArrestOnHit.Chance != Chance)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveAttribute != SaveAttribute)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveVs != SaveVs)
		{
			return false;
		}
		if (cardiacArrestOnHit.RequireDamageAttribute != RequireDamageAttribute)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "LauncherProjectileHit");
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		Object.RegisterPartEvent(this, "WeaponPseudoThrowHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public void CheckApply(Event E)
	{
		if ((!string.IsNullOrEmpty(RequireDamageAttribute) && (!(E.GetParameter("Damage") is Damage damage) || !damage.HasAttribute(RequireDamageAttribute))) || IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
		GameObject parentObject = ParentObject;
		GameObject subject = gameObjectParameter2;
		GameObject projectile = gameObjectParameter3;
		if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part CardiacArrestOnHit Activation", Chance, subject, projectile).in100())
		{
			ConsumeCharge();
			if (CanApplyEffectEvent.Check(gameObjectParameter2, "CardiacArrest") && (string.IsNullOrEmpty(SaveAttribute) || !gameObjectParameter2.MakeSave(SaveAttribute, SaveTarget, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject)))
			{
				gameObjectParameter2.ApplyEffect(new CardiacArrest());
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" || E.ID == "ProjectileHit" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}
}

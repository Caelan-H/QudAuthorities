using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is not by default, chance to activate is increased by a
///             percentage equal to ((power load - 100) / 10), i.e. 30% for
///             the standard overload power load of 400, and damage is increased
///             by the standard power load bonus, i.e. 2 for the standard overload
///             power load of 400.
///             </remarks>
[Serializable]
public class ElementalDamage : IActivePart
{
	public int Chance = 100;

	public string Damage = "1d4";

	public string Attributes = "Heat";

	public ElementalDamage()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		ElementalDamage elementalDamage = p as ElementalDamage;
		if (elementalDamage.Chance != Chance)
		{
			return false;
		}
		if (elementalDamage.Damage != Damage)
		{
			return false;
		}
		if (elementalDamage.Attributes != Attributes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (XRL.World.Damage.IsColdDamage(Attributes))
		{
			E.Add("ice", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int num = MyPowerLoadBonus();
		string text = Damage;
		if (num != 0)
		{
			text = DieRoll.AdjustResult(text, num);
		}
		int effectiveChance = GetEffectiveChance();
		E.Postfix.AppendRules("Causes " + text + " " + Attributes.ToLower() + " damage on hit" + ((effectiveChance < 100) ? (" " + effectiveChance + "% of the time") : "") + ".");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AdjustWeaponScore");
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			int num = MyPowerLoadLevel();
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			if (GetEffectiveChance(num, gameObjectParameter, gameObjectParameter2, gameObjectParameter3).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				string message = ((gameObjectParameter3 != null && !IComponent<GameObject>.TerseMessages) ? ("from " + gameObjectParameter.poss(gameObjectParameter3) + ".") : "from %t attack.");
				int amount = Damage.RollCached() + IComponent<GameObject>.PowerLoadBonus(num);
				string attributes = Attributes;
				GameObject owner = gameObjectParameter;
				GameObject source = gameObjectParameter3;
				string showDamageType = Attributes.ToLower() + " damage";
				if (gameObjectParameter2.TakeDamage(amount, message, attributes, null, null, owner, null, source, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, ShowForInanimate: false, SilentIfNoDamage: false, 5, showDamageType))
				{
					E.SetFlag("DidSpecialEffect", State: true);
				}
				if (!string.IsNullOrEmpty(Attributes))
				{
					if (Attributes.Contains("Umbral"))
					{
						gameObjectParameter2.ParticleBlip("&K-");
						gameObjectParameter2.Acidsplatter();
					}
					if (XRL.World.Damage.ContainsAcidDamage(Attributes))
					{
						gameObjectParameter2.ParticleBlip("&G*");
						gameObjectParameter2.Acidsplatter();
					}
					if (XRL.World.Damage.ContainsElectricDamage(Attributes))
					{
						gameObjectParameter2.ParticleBlip("&W*");
						gameObjectParameter2.Sparksplatter();
					}
					if (XRL.World.Damage.ContainsColdDamage(Attributes))
					{
						gameObjectParameter2.ParticleBlip("&C*");
						gameObjectParameter2.Icesplatter();
					}
					if (XRL.World.Damage.ContainsHeatDamage(Attributes))
					{
						gameObjectParameter2.ParticleBlip("&C*");
						gameObjectParameter2.Firesplatter();
					}
				}
			}
		}
		else if (E.ID == "AdjustWeaponScore" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int @for = GetSpecialEffectChanceEvent.GetFor(E.GetGameObjectParameter("User"), ParentObject, "Part ElementalDamage Activation", Chance);
			int num2 = MyPowerLoadBonus();
			int num3 = ((Damage.RollMinCached() + num2) * 2 + (Damage.RollMaxCached() + num2)) * 2;
			if (@for < 100)
			{
				num3 = num3 * @for / 100;
			}
			E.SetParameter("Score", E.GetIntParameter("Score") + num3);
		}
		return base.FireEvent(E);
	}

	public int GetEffectiveChance(int? PowerLoadLevel = null, GameObject Attacker = null, GameObject Defender = null, GameObject Weapon = null)
	{
		int num = Chance;
		int num2 = IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel(), 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Part ElementalDamage Activation", num, Defender);
	}
}

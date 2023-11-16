using System;
using System.Text;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MeleeWeapon : IPart
{
	public const int BONUS_CAP_UNLIMITED = 999;

	public int MaxStrengthBonus;

	public int PenBonus;

	public int HitBonus;

	public string BaseDamage = "5";

	public int Ego;

	public string Skill = "Cudgel";

	public string Stat = "Strength";

	public string Slot = "Hand";

	public string Attributes;

	[NonSerialized]
	private static Event eIsAdaptivePenetrationActive = new Event("IsAdaptivePenetrationActive", "Bonus", 0);

	public override bool SameAs(IPart p)
	{
		MeleeWeapon meleeWeapon = p as MeleeWeapon;
		if (meleeWeapon.MaxStrengthBonus != MaxStrengthBonus)
		{
			return false;
		}
		if (meleeWeapon.PenBonus != PenBonus)
		{
			return false;
		}
		if (meleeWeapon.HitBonus != HitBonus)
		{
			return false;
		}
		if (meleeWeapon.BaseDamage != BaseDamage)
		{
			return false;
		}
		if (meleeWeapon.Ego != Ego)
		{
			return false;
		}
		if (meleeWeapon.Skill != Skill)
		{
			return false;
		}
		if (meleeWeapon.Stat != Stat)
		{
			return false;
		}
		if (meleeWeapon.Slot != Slot)
		{
			return false;
		}
		if (meleeWeapon.Attributes != Attributes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EquippedEvent.ID || Ego == 0) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			if (ID == UnequippedEvent.ID)
			{
				return Ego != 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		base.StatShifter.SetStatShift(E.Actor, "Ego", Ego);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		base.StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (ParentObject.HasTag("ShowMeleeWeaponStats") && E.Understood())
		{
			E.AddTag(GetSimplifiedStats(Options.ShowDetailedWeaponStats), -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ParentObject.HasTag("ShowMeleeWeaponStats"))
		{
			if (Ego != 0)
			{
				E.Postfix.AppendRules(((Ego > 0) ? "+" : "") + Ego + " Ego");
			}
			if (HitBonus != 0)
			{
				E.Postfix.AppendRules(((HitBonus > 0) ? "+" : "") + HitBonus + " To-Hit");
			}
			E.Postfix.AppendRules(GetDetailedStats());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (!E.List.Contains(ParentObject) && E.SlotType == Slot)
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (Ego > 0 || MaxStrengthBonus >= 5)
		{
			E.Add("might", 1);
		}
		if (Stat == "Intelligence")
		{
			E.Add("scholarship", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool AttackFromPart(BodyPart Part)
	{
		if (Part.PreferedPrimary)
		{
			return true;
		}
		if (string.IsNullOrEmpty(Slot))
		{
			return true;
		}
		if (Part.Type == null)
		{
			return true;
		}
		if (!(Slot == Part.Type))
		{
			return Slot == Part.VariantType;
		}
		return true;
	}

	public string GetDetailedStats()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (MaxStrengthBonus > 0 || PenBonus > 0)
		{
			stringBuilder.Compound(Stat, "\n").Append(" Bonus Cap: ");
			if (MaxStrengthBonus == 999)
			{
				stringBuilder.Append("no limit");
			}
			else
			{
				stringBuilder.Append(MaxStrengthBonus);
			}
		}
		string text = Skills.GetGenericSkill(Skill)?.GetWeaponCriticalDescription();
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.Compound("Weapon Class: " + text, "\n");
		}
		return stringBuilder.ToString();
	}

	public bool IsAdaptivePenetrationActive()
	{
		eIsAdaptivePenetrationActive.SetParameter("Bonus", 0);
		return !ParentObject.FireEvent(eIsAdaptivePenetrationActive);
	}

	public bool CheckAdaptivePenetration(out int Bonus)
	{
		Bonus = 0;
		eIsAdaptivePenetrationActive.SetParameter("Bonus", 0);
		if (ParentObject.FireEvent(eIsAdaptivePenetrationActive))
		{
			return false;
		}
		Bonus = eIsAdaptivePenetrationActive.GetIntParameter("Bonus");
		return true;
	}

	public void GetNormalPenetration(GameObject who, out int BasePenetration, out int StatMod)
	{
		BasePenetration = PenBonus;
		StatMod = 0;
		if ((Skill == "LongBlades" || Skill == "ShortBlades") && who != null && who.HasPart("LongBladesCore") && who.HasEffect("LongbladeStance_Aggressive") && who.GetPart<LongBladesCore>().IsPrimaryBladeEquipped())
		{
			if (who.HasPart("LongBladesImprovedAggressiveStance"))
			{
				BasePenetration += 2;
			}
			else
			{
				BasePenetration++;
			}
		}
		if (who != null && who.Statistics.ContainsKey(Stat) && !HasTag("WeaponIgnoreStrength"))
		{
			StatMod = Math.Min(MaxStrengthBonus, who.Statistics[Stat].Modifier);
		}
	}

	public int GetNormalPenetration(GameObject who)
	{
		GetNormalPenetration(who, out var BasePenetration, out var StatMod);
		return BasePenetration + StatMod;
	}

	public string GetSimplifiedStats(bool WithStrCap)
	{
		GameObject who = ParentObject.Equipped ?? IComponent<GameObject>.ThePlayer;
		StringBuilder stringBuilder = Event.NewStringBuilder().Append("{{c|").Append('\u001a')
			.Append("}}");
		GetNormalPenetration(who, out var BasePenetration, out var StatMod);
		if (CheckAdaptivePenetration(out var Bonus))
		{
			stringBuilder.Append('÷');
			int num = BasePenetration + Bonus;
			if (num > 0)
			{
				stringBuilder.Append('+').Append(num);
			}
			else if (num < 0)
			{
				stringBuilder.Append(num);
			}
		}
		else
		{
			stringBuilder.Append(BasePenetration + StatMod + 4);
			if (WithStrCap)
			{
				if (MaxStrengthBonus == 999)
				{
					stringBuilder.Append("{{K|/").Append('ì').Append("}}");
				}
				else
				{
					stringBuilder.Append("{{K|/").Append(BasePenetration + MaxStrengthBonus + 4).Append("}}");
				}
			}
		}
		stringBuilder.Append(" {{r|").Append('\u0003').Append("}}")
			.Append(BaseDamage);
		return stringBuilder.ToString();
	}

	public bool AdjustDamageDieSize(int Amount)
	{
		BaseDamage = DieRoll.AdjustDieSize(BaseDamage, Amount);
		DamageDieSizeAdjustedEvent.Send(ParentObject, Amount);
		return true;
	}

	public bool AdjustDamage(int Amount)
	{
		BaseDamage = DieRoll.AdjustResult(BaseDamage, Amount);
		DamageConstantAdjustedEvent.Send(ParentObject, Amount);
		return true;
	}

	public bool AdjustBonusCap(int Amount)
	{
		if (MaxStrengthBonus == 999)
		{
			return false;
		}
		MaxStrengthBonus += Amount;
		return true;
	}

	public bool IsEquippedOnPrimary()
	{
		if (ParentObject == null)
		{
			return false;
		}
		return ParentObject.Equipped?.HasEquippedOnPrimary(ParentObject) ?? false;
	}
}

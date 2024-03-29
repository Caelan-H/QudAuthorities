using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Armor : IPart
{
	public string WornOn = "Body";

	public int _AV;

	public int _DV;

	public int MA;

	public int Acid;

	public int Elec;

	public int Cold;

	public int Heat;

	public int Strength;

	public int Agility;

	public int Toughness;

	public int Intelligence;

	public int Ego;

	public int Willpower;

	public int ToHit;

	public int SpeedPenalty;

	public int SpeedBonus;

	public int CarryBonus;

	public int AVApplied;

	public int DVApplied;

	public bool AVAveraged;

	public bool DVAveraged;

	public bool BonusApplied;

	public int AV
	{
		get
		{
			return _AV;
		}
		set
		{
			if (value != _AV)
			{
				_AV = value;
				RecalculateArmor(null, null, Cascade: true);
			}
		}
	}

	public int DV
	{
		get
		{
			return _DV;
		}
		set
		{
			if (value != _DV)
			{
				_DV = value;
				RecalculateArmor(null, null, Cascade: true);
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		Armor armor = p as Armor;
		if (armor.WornOn != WornOn)
		{
			return false;
		}
		if (armor.AV != AV)
		{
			return false;
		}
		if (armor.DV != DV)
		{
			return false;
		}
		if (armor.MA != MA)
		{
			return false;
		}
		if (armor.Acid != Acid)
		{
			return false;
		}
		if (armor.Elec != Elec)
		{
			return false;
		}
		if (armor.Cold != Cold)
		{
			return false;
		}
		if (armor.Heat != Heat)
		{
			return false;
		}
		if (armor.Strength != Strength)
		{
			return false;
		}
		if (armor.Agility != Agility)
		{
			return false;
		}
		if (armor.Toughness != Toughness)
		{
			return false;
		}
		if (armor.Intelligence != Intelligence)
		{
			return false;
		}
		if (armor.Ego != Ego)
		{
			return false;
		}
		if (armor.Willpower != Willpower)
		{
			return false;
		}
		if (armor.ToHit != ToHit)
		{
			return false;
		}
		if (armor.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		if (armor.SpeedBonus != SpeedBonus)
		{
			return false;
		}
		if (armor.CarryBonus != CarryBonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public bool RecalculateArmor(GameObject who = null, BodyPart onPart = null, bool Cascade = false)
	{
		if (who == null)
		{
			if (ParentObject != null)
			{
				who = ParentObject.Equipped;
			}
			if (who == null)
			{
				return false;
			}
		}
		if (onPart == null)
		{
			onPart = who.FindEquippedObject(ParentObject);
			if (onPart == null)
			{
				return false;
			}
		}
		Body body = who.Body;
		if (body == null)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		if (ParentObject.IsWorn(onPart))
		{
			if (onPart.VariantTypeModel().NoArmorAveraging == true)
			{
				AVAveraged = false;
				DVAveraged = false;
				num = this.AV;
				num2 = this.DV;
			}
			else
			{
				GameObject First = null;
				int Count = 0;
				int AV = 0;
				int DV = 0;
				string forType = ((WornOn == "*") ? onPart.Type : WornOn);
				body.GetTypeArmorInfo(forType, ref First, ref Count, ref AV, ref DV);
				if (Cascade && First != null)
				{
					if (ParentObject == First)
					{
						body.RecalculateTypeArmorExcept(forType, ParentObject);
					}
					else if (First.GetPart("Armor") is Armor armor)
					{
						armor.RecalculateArmor();
					}
				}
				int num3 = ((Count > 0) ? ((Count > 1) ? ((int)Math.Round((double)AV / (double)Count)) : AV) : 0);
				int num4 = ((Count > 0) ? ((Count > 1) ? ((int)Math.Round((double)DV / (double)Count)) : DV) : 0);
				AVAveraged = this.AV != 0 && num3 != AV;
				DVAveraged = this.DV != 0 && num4 != DV;
				num = ((ParentObject == First) ? num3 : 0);
				num2 = ((ParentObject == First) ? num4 : 0);
			}
		}
		else
		{
			AVAveraged = false;
			DVAveraged = false;
			num = 0;
			num2 = 0;
		}
		if (num != AVApplied)
		{
			if (AVApplied > 0)
			{
				who.GetStat("AV").Bonus -= AVApplied;
			}
			else if (AVApplied < 0)
			{
				who.GetStat("AV").Penalty -= -AVApplied;
			}
			if (num > 0)
			{
				who.GetStat("AV").Bonus += num;
			}
			else if (num < 0)
			{
				who.GetStat("AV").Penalty += -num;
			}
			AVApplied = num;
		}
		if (num2 != DVApplied)
		{
			if (DVApplied > 0)
			{
				who.GetStat("DV").Bonus -= DVApplied;
			}
			else if (DVApplied < 0)
			{
				who.GetStat("DV").Penalty -= -DVApplied;
			}
			if (num2 > 0)
			{
				who.GetStat("DV").Bonus += num2;
			}
			else if (num2 < 0)
			{
				who.GetStat("DV").Penalty += -num2;
			}
			DVApplied = num2;
		}
		if (num == 0)
		{
			return num2 != 0;
		}
		return true;
	}

	public bool RemoveArmor(GameObject who = null)
	{
		if (who == null)
		{
			if (ParentObject == null || ParentObject.pPhysics == null)
			{
				return false;
			}
			who = ParentObject.Equipped;
			if (who == null)
			{
				return false;
			}
		}
		if (AVApplied != 0)
		{
			if (AVApplied > 0)
			{
				who.Statistics["AV"].Bonus -= AVApplied;
			}
			else
			{
				who.Statistics["AV"].Penalty -= -AVApplied;
			}
			AVApplied = 0;
		}
		if (DVApplied != 0)
		{
			if (DVApplied > 0)
			{
				who.Statistics["DV"].Bonus -= DVApplied;
			}
			else
			{
				who.Statistics["DV"].Penalty -= -DVApplied;
			}
			DVApplied = 0;
		}
		Body body = who.Body;
		if (body != null)
		{
			if (WornOn == "*")
			{
				body.RecalculateArmor();
			}
			else
			{
				body.RecalculateTypeArmor(WornOn);
			}
		}
		AVAveraged = false;
		DVAveraged = false;
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustTotalWeightEvent.ID && (ID != GetMaxCarriedWeightEvent.ID || CarryBonus == 0) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustTotalWeightEvent E)
	{
		BodyPart bodyPart = ParentObject.EquippedOn();
		if (bodyPart != null && !bodyPart.Contact)
		{
			E.AdjustWeight(0.0);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		if (BonusApplied)
		{
			E.AdjustWeight((100.0 + (double)CarryBonus) / 100.0);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (ParentObject.HasTagOrProperty("CannotEquip"))
		{
			return base.HandleEvent(E);
		}
		if (E.List.Contains(ParentObject))
		{
			if (WornOn == "Floating Nearby" && E.SlotType != WornOn)
			{
				E.List.Remove(ParentObject);
				return false;
			}
			return base.HandleEvent(E);
		}
		if (WornOn == "*" || E.SlotType == WornOn)
		{
			E.List.Add(ParentObject);
		}
		else if (WornOn == "Floating Nearby")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		GameObject actor = E.Actor;
		RecalculateArmor(actor, null, Cascade: true);
		if (UpdateStatShifts(actor) && CarryBonus != 0)
		{
			CarryingCapacityChangedEvent.Send(actor);
		}
		actor.RegisterPartEvent(this, "RollMeleeToHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		GameObject actor = E.Actor;
		RemoveArmor(actor);
		if (BonusApplied)
		{
			BonusApplied = false;
			base.StatShifter.RemoveStatShifts(actor);
			if (CarryBonus != 0)
			{
				CarryingCapacityChangedEvent.Send(actor);
			}
		}
		actor.UnregisterPartEvent(this, "RollMeleeToHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("ArmorStatsOverride");
			if (propertyOrTag != null)
			{
				E.AddTag(propertyOrTag, -40);
			}
			else
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{b|").Append('\u0004').Append("}}");
				if (AVAveraged)
				{
					stringBuilder.Append((AV > 0) ? "{{r|" : "{{g|");
				}
				stringBuilder.Append(AV);
				if (AVAveraged)
				{
					stringBuilder.Append("}}");
				}
				stringBuilder.Append(" {{K|").Append('\t').Append("}}");
				if (DVAveraged)
				{
					stringBuilder.Append((DV > 0) ? "{{r|" : "{{g|");
				}
				stringBuilder.Append(DV);
				if (DVAveraged)
				{
					stringBuilder.Append("}}");
				}
				E.AddTag(stringBuilder.ToString(), -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		StringBuilder postfix = E.Postfix;
		DescribeBonus(postfix, Heat, "Heat Resistance", 'R', 'r');
		DescribeBonus(postfix, Cold, "Cold Resistance", 'C', 'c');
		DescribeBonus(postfix, Elec, "Electrical Resistance", 'W', 'w');
		DescribeBonus(postfix, Acid, "Acid Resistance", 'G', 'g');
		DescribeBonus(postfix, MA, "MA");
		DescribeBonus(postfix, Strength, "Strength");
		DescribeBonus(postfix, Agility, "Agility");
		DescribeBonus(postfix, Toughness, "Toughness");
		DescribeBonus(postfix, Intelligence, "Intelligence");
		DescribeBonus(postfix, Willpower, "Willpower");
		DescribeBonus(postfix, Ego, "Ego");
		DescribeBonus(postfix, SpeedBonus - SpeedPenalty, "Quickness");
		DescribeBonus(postfix, CarryBonus, "carry capacity", 'C', 'R', isPerc: true);
		DescribeBonus(postfix, ToHit, "To-Hit");
		if (AVAveraged && DVAveraged)
		{
			if (AV > 0 && DV > 0)
			{
				postfix.AppendRules("This item's AV and DV bonuses are being averaged across all body parts of the same type.");
			}
			else if (AV < 0 && DV < 0)
			{
				postfix.AppendRules("This item's AV and DV penalties are being averaged across all body parts of the same type.");
			}
			else
			{
				postfix.AppendRules("This item's AV and DV modifiers are being averaged across all body parts of the same type.");
			}
		}
		else if (AVAveraged)
		{
			if (AV > 0)
			{
				postfix.AppendRules("This item's AV bonus is being averaged across all body parts of the same type.");
			}
			else
			{
				postfix.AppendRules("This item's AV penalty is being averaged across all body parts of the same type.");
			}
		}
		else if (DVAveraged)
		{
			if (DV > 0)
			{
				postfix.AppendRules("This item's DV bonus is being averaged across all body parts of the same type.");
			}
			else
			{
				postfix.AppendRules("This item's DV penalty is being averaged across all body parts of the same type.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (AV + DV >= 8 || Strength > 0)
		{
			E.Add("might", 1);
		}
		if (SpeedBonus - SpeedPenalty > 0)
		{
			E.Add("time", 1);
		}
		if (Intelligence > 0)
		{
			E.Add("scholarship", 1);
		}
		if (CarryBonus > 0)
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RollMeleeToHit" && ToHit != 0 && ParentObject.IsWorn())
		{
			E.SetParameter("Result", E.GetIntParameter("Result") + ToHit);
		}
		return base.FireEvent(E);
	}

	public bool UpdateStatShifts(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return false;
			}
		}
		if (!ParentObject.IsWorn())
		{
			base.StatShifter.RemoveStatShifts();
			BonusApplied = false;
			return false;
		}
		base.StatShifter.SetStatShift(who, "MA", MA);
		base.StatShifter.SetStatShift(who, "Speed", SpeedBonus - SpeedPenalty);
		base.StatShifter.SetStatShift(who, "Ego", Ego);
		base.StatShifter.SetStatShift(who, "Intelligence", Intelligence);
		base.StatShifter.SetStatShift(who, "Agility", Agility);
		base.StatShifter.SetStatShift(who, "Toughness", Toughness);
		base.StatShifter.SetStatShift(who, "Strength", Strength);
		base.StatShifter.SetStatShift(who, "Willpower", Willpower);
		base.StatShifter.SetStatShift(who, "ElectricResistance", Elec);
		base.StatShifter.SetStatShift(who, "AcidResistance", Acid);
		base.StatShifter.SetStatShift(who, "HeatResistance", Heat);
		base.StatShifter.SetStatShift(who, "ColdResistance", Cold);
		BonusApplied = true;
		return true;
	}

	private void DescribeBonus(StringBuilder SB, int amount, string what, char posColor = 'C', char negColor = 'R', bool isPerc = false)
	{
		if (amount != 0 && ShouldDescribeStatBonusEvent.Check(ParentObject, this, what, amount))
		{
			SB.Append("\n{{");
			if (amount > 0)
			{
				SB.Append(posColor).Append('|').Append('+');
			}
			else
			{
				SB.Append(negColor).Append('|');
			}
			SB.Append(amount);
			if (isPerc)
			{
				SB.Append('%');
			}
			SB.Append(' ').Append(what);
			SB.Append("}}");
		}
	}
}

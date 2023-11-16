using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to be
///             true, which it is not by default, the standard power load bonus (i.e.
///             2 for the standard overload power load of 400) is treated as an
///             additional stat bonus to attempt to distribute according to the
///             pattern established by the configured stat bonuses.  If there is only
///             one stat being modified, the bonus is added to that stat.  If a number
///             of stats equal to the bonus is being modified, each is increased by 1.
///             Otherwise, the configured bonuses are sorted first by descending order
///             of bonus size, then alphabetically, and the bonus amount is distributed
///             by moving down this list adding one at a time, starting over at the top
///             if the end is reached, until the bonus is exhausted.
///             </remarks>
[Serializable]
public class EquipStatBoost : IActivePart
{
	public string Boosts = "";

	[NonSerialized]
	public Dictionary<string, int> _BonusList;

	public bool Applied;

	public EquipStatBoost()
	{
		base.IsTechScannable = true;
		WorksOnEquipper = true;
	}

	public EquipStatBoost(string Boosts)
		: this()
	{
		this.Boosts = Boosts;
	}

	public static void AppendBoostOnEquip(GameObject obj, string boost, string nameForStatus = null, bool techScan = false)
	{
		EquipStatBoost equipStatBoost = null;
		if (obj.HasPart("EquipStatBoost"))
		{
			equipStatBoost = obj.GetPart<EquipStatBoost>();
			equipStatBoost.AddBonuses(boost);
		}
		else
		{
			equipStatBoost = new EquipStatBoost(boost);
			equipStatBoost.DescribeStatusForProperty = null;
			if (!techScan && equipStatBoost.IsTechScannable)
			{
				equipStatBoost.IsTechScannable = false;
			}
			obj.AddPart(equipStatBoost);
		}
		if (equipStatBoost.NameForStatus == null && nameForStatus != null)
		{
			equipStatBoost.NameForStatus = nameForStatus;
		}
	}

	public void AddBonuses(string Spec)
	{
		Dictionary<string, int> bonusList = GetBonusList(ForceRebuild: false, Unmodified: true);
		Dictionary<string, int> dictionary = DetermineBonusList(Spec, Unmodified: true);
		foreach (string key in dictionary.Keys)
		{
			if (bonusList.ContainsKey(key))
			{
				bonusList[key] += dictionary[key];
			}
			else
			{
				bonusList.Add(key, dictionary[key]);
			}
		}
		Boosts = BonusesToString(bonusList);
	}

	private Dictionary<string, int> DetermineBonusList(string Spec, bool Unmodified = false)
	{
		if (string.IsNullOrEmpty(Spec))
		{
			return new Dictionary<string, int>(1);
		}
		string[] array = Spec.Split(';');
		Dictionary<string, int> Result = new Dictionary<string, int>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] array3 = text.Split(':');
				if (Result.ContainsKey(array3[0]))
				{
					Result[array3[0]] += Convert.ToInt32(array3[1]);
				}
				else
				{
					Result.Add(array3[0], Convert.ToInt32(array3[1]));
				}
			}
		}
		int num = ((!Unmodified && IsPowerLoadSensitive) ? MyPowerLoadBonus() : 0);
		if (num > 0 && Result.Count > 0)
		{
			if (Result.Count == 1)
			{
				using List<string>.Enumerator enumerator = Result.Keys.ToList().GetEnumerator();
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					Result[current] += num;
				}
			}
			else if (Result.Count == num)
			{
				foreach (string item in Result.Keys.ToList())
				{
					Result[item]++;
				}
			}
			else
			{
				List<string> list = new List<string>(Result.Keys);
				list.Sort(delegate(string a, string b)
				{
					int num2 = Result[a].CompareTo(Result[b]);
					return (num2 != 0) ? (-num2) : a.CompareTo(b);
				});
				while (num > 0)
				{
					foreach (string item2 in list)
					{
						Result[item2]++;
						num--;
						if (num <= 0)
						{
							break;
						}
					}
				}
			}
		}
		return Result;
	}

	private static string BonusesToString(Dictionary<string, int> Bonuses)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = 0;
		foreach (KeyValuePair<string, int> Bonuse in Bonuses)
		{
			if (num++ > 0)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append(Bonuse.Key).Append(':').Append(Bonuse.Value);
		}
		return stringBuilder.ToString();
	}

	public Dictionary<string, int> GetBonusList(bool ForceRebuild = false, bool Unmodified = false)
	{
		if (Unmodified)
		{
			return DetermineBonusList(Boosts, Unmodified: true);
		}
		if (ForceRebuild || _BonusList == null)
		{
			_BonusList = DetermineBonusList(Boosts);
		}
		return _BonusList;
	}

	public void Apply(GameObject Object)
	{
		if (Applied)
		{
			return;
		}
		foreach (KeyValuePair<string, int> bonus in GetBonusList())
		{
			base.StatShifter.SetStatShift(Object, bonus.Key, bonus.Value);
		}
		Applied = true;
	}

	public void UnapplyEffects(GameObject Object)
	{
		if (Applied && Object != null)
		{
			base.StatShifter.RemoveStatShifts();
			Applied = false;
		}
	}

	public override bool SameAs(IPart p)
	{
		if ((p as EquipStatBoost).Boosts != Boosts)
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
		if (Applied && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
		CheckApplyEffects();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (Applied && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
		CheckApplyEffects();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (Applied && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
		CheckApplyEffects();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EquippedEvent.ID && ID != GetItemElementsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ModificationAppliedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ModificationAppliedEvent E)
	{
		UnapplyEffects(ParentObject.Equipped);
		GetBonusList(ForceRebuild: true);
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		foreach (KeyValuePair<string, int> bonus in GetBonusList())
		{
			E.Postfix.Append("\n{{rules|");
			Statistic.GetStatAdjustDescription(E.Postfix, bonus.Key, bonus.Value);
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		foreach (KeyValuePair<string, int> bonus in GetBonusList())
		{
			if (bonus.Key == "Strength")
			{
				if (bonus.Value > 0)
				{
					E.Add("might", bonus.Value.DiminishingReturns(1.0));
				}
			}
			else if (bonus.Key == "Intelligence")
			{
				if (bonus.Value > 0)
				{
					E.Add("scholarship", bonus.Value.DiminishingReturns(1.0));
				}
			}
			else if (bonus.Key == "Quickness")
			{
				if (bonus.Value != 0)
				{
					E.Add("time", Math.Abs(bonus.Value).DiminishingReturns(1.0));
				}
			}
			else if (bonus.Key == "MoveSpeed" && bonus.Value < 0)
			{
				E.Add("travel", Math.Abs(bonus.Value).DiminishingReturns(1.0));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public void CheckApplyEffects(GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject.Equipped;
			if (obj == null)
			{
				return;
			}
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			UnapplyEffects(obj);
		}
		else
		{
			Apply(obj);
		}
	}
}

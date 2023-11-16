using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class Statistic
{
	public class IntBox
	{
		public int i;

		public IntBox(int _i)
		{
			i = _i;
		}
	}

	[Serializable]
	public struct StatShift
	{
		public Guid ID;

		public int Amount;

		public string DisplayName;

		public bool BaseValue;
	}

	[NonSerialized]
	public static List<string> Attributes = new List<string>(6) { "Strength", "Agility", "Toughness", "Intelligence", "Willpower", "Ego" };

	[NonSerialized]
	public static List<string> MentalStats = new List<string>(4) { "Ego", "Intelligence", "Willpower", "MA" };

	[NonSerialized]
	public static List<string> InverseBenefitStats = new List<string>(1) { "MoveSpeed" };

	[NonSerialized]
	public static Dictionary<string, string> StatDisplayNames = new Dictionary<string, string>
	{
		{ "AcidResistance", "acid resistance" },
		{ "ColdResistance", "cold resistance" },
		{ "ElectricResistance", "electric resistance" },
		{ "HeatResistance", "heat resistance" },
		{ "Hitpoints", "hit points" },
		{ "MoveSpeed", "move speed" },
		{ "Speed", "quickness" }
	};

	[NonSerialized]
	public static Dictionary<string, string> StatShortNames = new Dictionary<string, string>
	{
		{ "MoveSpeed", "MS" },
		{ "Speed", "QN" },
		{ "Hitpoints", "HP" }
	};

	public GameObject Owner;

	public string Name = "";

	public string sValue = "";

	public int Boost;

	public static Dictionary<string, IntBox> _Max = new Dictionary<string, IntBox>();

	public static Dictionary<string, IntBox> _Min = new Dictionary<string, IntBox>();

	public int _Value;

	[NonSerialized]
	private string StatChangeID;

	[NonSerialized]
	private Event eStatChange;

	public int _Bonus;

	public int _Penalty;

	public List<StatShift> Shifts;

	public string ValueOrSValue
	{
		get
		{
			if (sValue != "")
			{
				return sValue;
			}
			return Value.ToString();
		}
	}

	public int Max
	{
		get
		{
			if (_Max.TryGetValue(Name, out var value))
			{
				return value.i;
			}
			return 30;
		}
		set
		{
			if (!_Max.ContainsKey(Name))
			{
				_Max.Add(Name, new IntBox(value));
			}
			else if (_Max[Name].i != value)
			{
				_Max[Name] = new IntBox(value);
			}
		}
	}

	public int Min
	{
		get
		{
			if (_Min.TryGetValue(Name, out var value))
			{
				return value.i;
			}
			return 0;
		}
		set
		{
			if (!_Min.ContainsKey(Name))
			{
				_Min.Add(Name, new IntBox(value));
			}
			else if (_Max[Name].i != value)
			{
				_Min[Name] = new IntBox(value);
			}
		}
	}

	public int BaseValue
	{
		get
		{
			return _Value;
		}
		set
		{
			if (_Value != value)
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Value = value;
				NotifyChange(value2, baseValue, "BaseValue");
			}
		}
	}

	public int Modifier => Stat.GetScoreModifier(Value);

	public int Value
	{
		get
		{
			int num = _Value + _Bonus - _Penalty;
			if (num < Min)
			{
				return Min;
			}
			if (num > Max)
			{
				return Max;
			}
			return num;
		}
	}

	public int Bonus
	{
		get
		{
			return _Bonus;
		}
		set
		{
			if (_Bonus != Math.Max(0, value))
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Bonus = value;
				if (_Bonus < 0)
				{
					_Bonus = 0;
				}
				NotifyChange(value2, baseValue, "Bonus");
			}
		}
	}

	public int Penalty
	{
		get
		{
			return _Penalty;
		}
		set
		{
			if (_Penalty != Math.Max(0, value))
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Penalty = value;
				if (_Penalty < 0)
				{
					_Penalty = 0;
				}
				NotifyChange(value2, baseValue, "Penalty");
			}
		}
	}

	public static bool IsMental(string Stat)
	{
		return MentalStats.Contains(Stat);
	}

	public static bool IsInverseBenefit(string Stat)
	{
		return InverseBenefitStats.Contains(Stat);
	}

	public static string GetStatShortName(string stat)
	{
		if (!StatShortNames.TryGetValue(stat, out var value))
		{
			return stat.Substring(0, 2).ToUpper();
		}
		return value;
	}

	public static string GetStatDisplayName(string Stat)
	{
		if (!StatDisplayNames.TryGetValue(Stat, out var value))
		{
			return Stat;
		}
		return value;
	}

	public string GetDisplayValue()
	{
		if (InverseBenefitStats.Contains(Name))
		{
			return (200 - Value).ToString();
		}
		return Value.ToString();
	}

	public static void GetStatAdjustDescription(StringBuilder SB, string Stat, int Adjust)
	{
		if (IsInverseBenefit(Stat))
		{
			Adjust = -Adjust;
		}
		if (Adjust > 0)
		{
			SB.Append('+');
		}
		SB.Append(Adjust).Append(' ').Append(GetStatDisplayName(Stat));
	}

	public static string GetStatAdjustDescription(string Stat, int Adjust)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetStatAdjustDescription(stringBuilder, Stat, Adjust);
		return stringBuilder.ToString();
	}

	private static int StatisticComparer(string a, string b)
	{
		if (a == b)
		{
			return 0;
		}
		int num = Attributes.IndexOf(a);
		int num2 = Attributes.IndexOf(b);
		if (num != -1)
		{
			if (num2 != -1)
			{
				return num.CompareTo(num2);
			}
			return -1;
		}
		if (num2 != -1)
		{
			return 1;
		}
		return a.CompareTo(b);
	}

	public static void SortStatistics(List<string> list)
	{
		list.Sort(StatisticComparer);
	}

	public Statistic()
	{
	}

	public Statistic(Statistic Source)
		: this()
	{
		Name = Source.Name;
		Min = Source.Min;
		Max = Source.Max;
		Penalty = Source.Penalty;
		Bonus = Source.Bonus;
		BaseValue = Source.BaseValue;
		Owner = Source.Owner;
		sValue = Source.sValue;
		Boost = Source.Boost;
		if (Source.Shifts != null)
		{
			Shifts = new List<StatShift>();
			{
				foreach (StatShift shift in Source.Shifts)
				{
					Shifts.Add(new StatShift
					{
						ID = Guid.NewGuid(),
						Amount = shift.Amount,
						DisplayName = shift.DisplayName,
						BaseValue = shift.BaseValue
					});
				}
				return;
			}
		}
		Shifts = null;
	}

	public Statistic(string name, int min, int max, int val, GameObject parent)
	{
		Name = name;
		Min = min;
		Max = max;
		Owner = parent;
		Penalty = 0;
		Bonus = 0;
		BaseValue = val;
		Boost = 0;
	}

	public void BoostStat(int Amount)
	{
		if (Amount > 0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.25 * (double)Amount);
		}
		else if (Amount < 0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.2 * (double)Amount);
		}
	}

	public void BoostStat(double Amount)
	{
		if (Amount > 0.0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.25 * Amount);
		}
		else if (Amount < 0.0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.2 * Amount);
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(Name);
		Writer.Write(_Bonus);
		Writer.Write(_Penalty);
		Writer.Write(_Value);
		Writer.Write(Shifts);
	}

	public static Statistic Load(SerializationReader Reader, GameObject Owner)
	{
		return new Statistic
		{
			Owner = Owner,
			Name = Reader.ReadString(),
			_Bonus = Reader.ReadInt32(),
			_Penalty = Reader.ReadInt32(),
			_Value = Reader.ReadInt32(),
			Shifts = Reader.ReadList<StatShift>()
		};
	}

	public bool SameAs(Statistic S)
	{
		if (Name != S.Name)
		{
			return false;
		}
		if (Min != S.Min)
		{
			return false;
		}
		if (Max != S.Max)
		{
			return false;
		}
		if (Penalty != S.Penalty)
		{
			return false;
		}
		if (Bonus != S.Bonus)
		{
			return false;
		}
		if (Value != S.Value)
		{
			return false;
		}
		return true;
	}

	public void NotifyChange(int OldValue, int OldBaseValue, string Type)
	{
		if (Owner == null)
		{
			return;
		}
		if (StatChangeID == null)
		{
			StatChangeID = "StatChange_" + Name;
		}
		if (Owner.HasRegisteredEvent(StatChangeID))
		{
			if (eStatChange == null)
			{
				eStatChange = new Event(StatChangeID, 0, 2, 4);
			}
			eStatChange.SetParameter("Stat", Name);
			eStatChange.SetParameter("OldValue", OldValue);
			eStatChange.SetParameter("NewValue", Value);
			eStatChange.SetParameter("OldBaseValue", OldBaseValue);
			eStatChange.SetParameter("NewBaseValue", BaseValue);
			eStatChange.SetParameter("Type", Type);
			Owner.FireEvent(eStatChange);
		}
		if (Owner.WantEvent(StatChangeEvent.ID, MinEvent.CascadeLevel))
		{
			StatChangeEvent e = StatChangeEvent.FromPool(Owner, Name, Type, OldValue, Value, OldBaseValue, BaseValue, this);
			Owner.HandleEvent(e);
		}
	}

	public override string ToString()
	{
		return Name + ": " + Value;
	}

	public string GetDisplayName()
	{
		return GetStatDisplayName(Name);
	}

	public StatShift GetShift(Guid shiftId)
	{
		return Shifts.Find((StatShift s) => s.ID == shiftId);
	}

	public Guid AddShift(int amount, string DisplayName, bool baseValue = false)
	{
		Guid guid = Guid.NewGuid();
		StatShift statShift = default(StatShift);
		statShift.ID = guid;
		statShift.Amount = amount;
		statShift.DisplayName = DisplayName;
		statShift.BaseValue = baseValue;
		StatShift item = statShift;
		if (Shifts == null)
		{
			Shifts = new List<StatShift>(1);
		}
		if (Options.DebugStatShift && The.Game != null)
		{
			The.Game.Player.Messages.Add($"DEBUG: NEW STAT SHIFT - {Owner.DisplayNameOnly}'s {Name} was shifted {item.Amount} by the {item.DisplayName}.");
		}
		Shifts.Add(item);
		if (baseValue)
		{
			BaseValue += amount;
		}
		else if (amount > 0)
		{
			Bonus += amount;
		}
		else
		{
			Penalty += -amount;
		}
		return guid;
	}

	public void RemoveShift(Guid id)
	{
		if (Shifts == null)
		{
			return;
		}
		int num = Shifts.FindIndex((StatShift s) => s.ID == id);
		if (num != -1)
		{
			StatShift statShift = Shifts[num];
			if (Options.DebugStatShift && The.Game != null)
			{
				The.Game.Player.Messages.Add($"DEBUG: REMOVED STAT SHIFT - {Owner.DisplayNameOnlyDirect}'s {Name} was shifted {statShift.Amount} by the {statShift.DisplayName}.");
			}
			if (statShift.BaseValue)
			{
				BaseValue -= statShift.Amount;
			}
			if (statShift.Amount > 0)
			{
				Bonus -= statShift.Amount;
			}
			else
			{
				Penalty -= -statShift.Amount;
			}
			Shifts.RemoveAt(num);
			if (Shifts.Count == 0 && !Owner.IsPlayerControlled())
			{
				Shifts = null;
			}
		}
	}

	public bool UpdateShift(Guid id, int newAmount)
	{
		if (Shifts == null)
		{
			return false;
		}
		int num = Shifts.FindIndex((StatShift s) => s.ID == id);
		if (num == -1)
		{
			return false;
		}
		StatShift item = Shifts[num];
		int amount = item.Amount;
		int num2 = newAmount - amount;
		if (Options.DebugStatShift)
		{
			XRLCore.Core.Game.Player.Messages.Add($"DEBUG: UPDATED STAT SHIFT - {Owner.DebugName}'s {Name} shift was {amount}, now {newAmount}, change {num2}, from {item.DisplayName}.");
		}
		if (num2 == 0)
		{
			return true;
		}
		if (item.BaseValue)
		{
			BaseValue += num2;
		}
		else if (newAmount == 0)
		{
			if (amount > 0)
			{
				Bonus -= amount;
			}
			else
			{
				Penalty -= -amount;
			}
		}
		else if (amount == 0 || amount >= 0 == newAmount >= 0)
		{
			if (newAmount >= 0)
			{
				Bonus += num2;
			}
			else
			{
				Penalty += -num2;
			}
		}
		else
		{
			if (newAmount > 0)
			{
				Bonus += newAmount;
			}
			if (amount > 0)
			{
				Bonus -= amount;
			}
			else
			{
				Penalty -= -amount;
			}
			if (newAmount <= 0)
			{
				Penalty += -newAmount;
			}
		}
		item.Amount = newAmount;
		Shifts.RemoveAt(num);
		Shifts.Add(item);
		return true;
	}
}

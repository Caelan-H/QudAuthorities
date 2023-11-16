using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World;

public class StatShifter
{
	public string DefaultDisplayName;

	public GameObject Owner;

	public Dictionary<string, Dictionary<string, Guid>> ActiveShifts = new Dictionary<string, Dictionary<string, Guid>>();

	public void Save(SerializationWriter writer)
	{
		writer.Write(DefaultDisplayName);
		writer.WriteObject(ActiveShifts);
	}

	public static StatShifter Load(SerializationReader reader, GameObject Owner)
	{
		return new StatShifter(Owner)
		{
			DefaultDisplayName = reader.ReadString(),
			ActiveShifts = (reader.ReadObject() as Dictionary<string, Dictionary<string, Guid>>)
		};
	}

	public StatShifter(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public StatShifter(GameObject Owner, string DefaultDisplayName)
	{
		this.Owner = Owner;
		this.DefaultDisplayName = DefaultDisplayName;
	}

	public bool SetStatShift(string statName, int amount, bool baseValue = false)
	{
		return SetStatShift(Owner, statName, amount, baseValue);
	}

	/// <summary>Get the value of the stat shift (optional base value) applied by this object</summary>
	public int GetStatShift(string statNAme, bool baseValue = false)
	{
		return GetStatShift(Owner, statNAme, baseValue);
	}

	public int GetStatShift(GameObject target, string statName, bool baseValue = false)
	{
		if (target == null || !target.IsValid() || !target.HasStat(statName))
		{
			return 0;
		}
		if (!ActiveShifts.TryGetValue(target.id, out var value))
		{
			return 0;
		}
		string key = statName + (baseValue ? ":base" : "");
		if (!value.TryGetValue(key, out var value2))
		{
			return 0;
		}
		return target.Statistics[statName].GetShift(value2).Amount;
	}

	/// <summary>Shift a stat on target object by an amount, further calls to SetStatShift with the same stat will
	///             undo the previous shift, and set to the new amount.</summary>
	public bool SetStatShift(GameObject target, string statName, int amount, bool baseValue = false)
	{
		if (target == null || !target.IsValid() || !target.HasStat(statName))
		{
			return false;
		}
		if (!ActiveShifts.TryGetValue(target.id, out var value))
		{
			value = new Dictionary<string, Guid>();
			ActiveShifts.Add(target.id, value);
		}
		Statistic statistic = target.Statistics[statName];
		string displayName = DefaultDisplayName;
		if (target != Owner)
		{
			displayName = ((!string.IsNullOrEmpty(DefaultDisplayName)) ? (Grammar.MakePossessive(Owner.ShortDisplayName) + " " + DefaultDisplayName) : Owner.ShortDisplayName);
		}
		string text = statName;
		if (baseValue)
		{
			text += ":base";
		}
		if (!value.TryGetValue(text, out var value2))
		{
			if (amount == 0)
			{
				return true;
			}
			value2 = statistic.AddShift(amount, displayName, baseValue);
			value.Add(text, value2);
		}
		else if (amount == 0)
		{
			statistic.RemoveShift(value2);
			value.Remove(text);
		}
		else if (!statistic.UpdateShift(value2, amount))
		{
			value[text] = statistic.AddShift(amount, displayName);
		}
		return true;
	}

	public void RemoveStatShifts()
	{
		if (ActiveShifts.Count <= 0)
		{
			return;
		}
		foreach (string item in new List<string>(ActiveShifts.Keys))
		{
			try
			{
				GameObject gameObject = ((Owner != null && Owner.id == item) ? Owner : GameObject.findById(item));
				if (gameObject == null)
				{
					throw new Exception("Can't resolve object id " + item);
				}
				RemoveStatShifts(gameObject);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Unresolved object trying to remove stat shifts", x);
			}
		}
	}

	public void RemoveStatShifts(GameObject target)
	{
		if (!GameObject.validate(ref target) || !ActiveShifts.TryGetValue(target.id, out var value))
		{
			return;
		}
		foreach (KeyValuePair<string, Guid> item in value)
		{
			string key = (item.Key.EndsWith(":base") ? item.Key.Substring(0, item.Key.Length - 5) : item.Key);
			target.Statistics[key].RemoveShift(item.Value);
		}
		ActiveShifts.Remove(target.id);
	}

	public void RemoveStatShift(GameObject target, string stat, bool baseValue = false)
	{
		if (GameObject.validate(ref target) && ActiveShifts.TryGetValue(target.id, out var value))
		{
			string key = stat + (baseValue ? ":base" : "");
			if (value.TryGetValue(key, out var value2))
			{
				target.Statistics[stat].RemoveShift(value2);
				value.Remove(key);
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("[StatShifter Owner:").Append(Owner.id).Append(" Description: ")
			.Append(DefaultDisplayName);
		stringBuilder.Append(" ");
		foreach (KeyValuePair<string, Dictionary<string, Guid>> activeShift in ActiveShifts)
		{
			stringBuilder.Append("[Object:").Append(activeShift.Key).Append(" ");
			foreach (KeyValuePair<string, Guid> item in activeShift.Value)
			{
				stringBuilder.Append(" Stat:").Append(item.Key).Append(":")
					.Append(item.Value);
			}
			stringBuilder.Append("] ");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}

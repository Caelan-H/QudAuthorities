using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;

namespace XRL;

public class PopulationTable : PopulationItem
{
	public string Number;

	public string Chance;

	public override string ToString()
	{
		return "<Table>";
	}

	public override void GetEachUniqueObject(List<string> List)
	{
		if (PopulationManager.Populations.ContainsKey(Name))
		{
			PopulationManager.Populations[Name].GetEachUniqueObject(List);
		}
	}

	public override void GenerateStructured(StructuredPopulationResult result, Dictionary<string, string> Vars)
	{
		result.Objects.AddRange(Generate(Vars, null));
	}

	public override List<PopulationResult> Generate(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (DefaultHint == null)
		{
			DefaultHint = Hint;
		}
		string text = Name;
		if (Vars != null)
		{
			foreach (KeyValuePair<string, string> Var in Vars)
			{
				if (Vars.ContainsKey(Var.Key))
				{
					try
					{
						text = text.Replace("{" + Var.Key + "}", Var.Value);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					Debug.LogError("Unknown key: " + Var.Key);
				}
			}
		}
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		List<PopulationResult> list = new List<PopulationResult>();
		foreach (string item in Chance.CachedCommaExpansion())
		{
			if (string.IsNullOrEmpty(item))
			{
				continue;
			}
			bool flag = false;
			if (item == "100")
			{
				flag = true;
			}
			else if (item.Contains("/"))
			{
				string[] array = item.Split('/');
				int num = Convert.ToInt32(array[0]);
				int high = Convert.ToInt32(array[1]);
				flag = num >= Stat.Roll(1, high);
			}
			else
			{
				flag = Stat.Random(1, 100) <= Convert.ToInt32(item);
			}
			if (!flag)
			{
				continue;
			}
			PopulationManager.RequireTable(text);
			if (text.StartsWith("@"))
			{
				string tableName = text.Substring(1);
				int i = 0;
				for (int num2 = Number.RollCached(); i < num2; i++)
				{
					string text2 = EncounterFactory.Factory.RollOneStringFromTable(tableName);
					if (!string.IsNullOrEmpty(text2))
					{
						list.Add(new PopulationResult(text2, 1, Hint));
					}
				}
			}
			else if (text.StartsWith("#"))
			{
				string table = text.Substring(1);
				int j = 0;
				for (int num3 = Number.RollCached(); j < num3; j++)
				{
					foreach (XRL.World.GameObject @object in EncounterFactory.Factory.CreateEncounterFromTableName(table).Objects)
					{
						list.Add(new PopulationResult(@object.Blueprint, 1, Hint ?? DefaultHint));
					}
				}
			}
			else if (!PopulationManager.Populations.ContainsKey(text))
			{
				Debug.LogWarning("Unknown Population table: " + text);
			}
			else
			{
				int k = 0;
				for (int num4 = Number.RollCached(); k < num4; k++)
				{
					list.AddRange(PopulationManager.Populations[text].Generate(Vars, (Hint == null) ? DefaultHint : Hint));
				}
			}
		}
		return list;
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (DefaultHint == null)
		{
			DefaultHint = Hint;
		}
		string text = Name;
		if (Vars != null)
		{
			foreach (KeyValuePair<string, string> Var in Vars)
			{
				if (Vars.ContainsKey(Var.Key))
				{
					try
					{
						text = text.Replace("{" + Var.Key + "}", Var.Value);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					Debug.LogError("Unknown key: " + Var.Key);
				}
			}
		}
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		foreach (string item in Chance.CachedCommaExpansion())
		{
			if (string.IsNullOrEmpty(item) || (!(item == "100") && Stat.Random(1, 100) > Convert.ToInt32(item)))
			{
				continue;
			}
			PopulationManager.RequireTable(text);
			if (text.StartsWith("@"))
			{
				string tableName = text.Substring(1);
				int i = 0;
				for (int num = Number.RollCached(); i < num; i++)
				{
					string text2 = EncounterFactory.Factory.RollOneStringFromTable(tableName);
					if (!string.IsNullOrEmpty(text2))
					{
						return new PopulationResult(text2, 1, Hint);
					}
				}
				continue;
			}
			if (text.StartsWith("#"))
			{
				string table = text.Substring(1);
				int j = 0;
				for (int num2 = Number.RollCached(); j < num2; j++)
				{
					using List<XRL.World.GameObject>.Enumerator enumerator3 = EncounterFactory.Factory.CreateEncounterFromTableName(table).Objects.GetEnumerator();
					if (enumerator3.MoveNext())
					{
						return new PopulationResult(enumerator3.Current.Blueprint, 1, Hint ?? DefaultHint);
					}
				}
				continue;
			}
			if (!PopulationManager.Populations.ContainsKey(text))
			{
				Debug.LogWarning("Unknown Population table: " + text);
				continue;
			}
			int k = 0;
			for (int num3 = Number.RollCached(); k < num3; k++)
			{
				PopulationResult populationResult = PopulationManager.Populations[text].GenerateOne(Vars, (Hint == null) ? DefaultHint : Hint);
				if (populationResult != null)
				{
					return populationResult;
				}
			}
		}
		return null;
	}
}

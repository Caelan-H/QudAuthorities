using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;

namespace XRL;

public class PopulationObject : PopulationItem
{
	public string Blueprint;

	public string Number;

	public string Chance;

	public string Builder;

	public PopulationObject()
	{
	}

	public PopulationObject(string blueprint, string number, uint weight, string builder)
		: this()
	{
		Blueprint = blueprint;
		Number = number;
		Weight = weight;
		Builder = builder;
	}

	public override string ToString()
	{
		return Blueprint + "[wt=" + Weight + "]";
	}

	public override void GetEachUniqueObject(List<string> List)
	{
		if (!List.CleanContains(Blueprint))
		{
			List.Add(Blueprint);
		}
	}

	public override void GenerateStructured(StructuredPopulationResult result, Dictionary<string, string> Vars)
	{
		result.Objects.AddRange(Generate(Vars, null));
	}

	public override List<PopulationResult> Generate(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		if (Blueprint == null)
		{
			Debug.LogError("NULL blueprint");
		}
		string text = Blueprint;
		if (Vars != null)
		{
			foreach (string key in Vars.Keys)
			{
				if (text.Contains(key))
				{
					text = text.Replace("{" + key + "}", Vars[key]);
				}
			}
		}
		if (text.StartsWith("$CALLBLUEPRINTMETHOD"))
		{
			text = PopulationManager.resolveCallBlueprintSlug(text);
		}
		List<PopulationResult> list = new List<PopulationResult>();
		foreach (string item in Chance.CachedCommaExpansion())
		{
			if (!string.IsNullOrEmpty(item) && (item == "100" || Stat.Random(1, 100) <= Convert.ToInt32(item)))
			{
				list.Add(new PopulationResult(text, Number.RollCached(), Hint ?? DefaultHint, Builder));
			}
		}
		return list;
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		if (Blueprint == null)
		{
			Debug.LogError("NULL blueprint");
		}
		string text = Blueprint;
		if (Vars != null)
		{
			foreach (string key in Vars.Keys)
			{
				if (text.Contains(key))
				{
					text = text.Replace("{" + key + "}", Vars[key]);
				}
			}
		}
		if (text.StartsWith("$CALLBLUEPRINTMETHOD"))
		{
			text = PopulationManager.resolveCallBlueprintSlug(text);
		}
		foreach (string item in Chance.CachedCommaExpansion())
		{
			if (!string.IsNullOrEmpty(item) && (item == "100" || Stat.Random(1, 100) <= Convert.ToInt32(item)))
			{
				return new PopulationResult(text, Number.RollCached(), Hint ?? DefaultHint, Builder);
			}
		}
		return null;
	}
}

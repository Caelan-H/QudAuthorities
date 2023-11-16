using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL;

public class PopulationGroup : PopulationItem
{
	public string Style;

	public string Number;

	public string Chance;

	public string WeightSpec;

	public bool Merge;

	public List<PopulationItem> Items = new List<PopulationItem>();

	public override string ToString()
	{
		string text = "";
		if (!string.IsNullOrEmpty(Name))
		{
			text = text + Name + ", ";
		}
		text = text + Style + " of [";
		foreach (PopulationItem item in Items)
		{
			text = text + item.ToString() + ",";
		}
		return text + "]";
	}

	public void MergeFrom(PopulationGroup o, PopulationInfo i)
	{
		if (o.Style != null)
		{
			Style = o.Style;
		}
		if (o.Number != null)
		{
			Number = o.Number;
		}
		if (o.Chance != null)
		{
			Chance = o.Chance;
		}
		if (o.Hint != null)
		{
			Hint = o.Hint;
		}
		if (o.WeightSpec != null)
		{
			Weight = o.Weight;
		}
		foreach (PopulationItem item in o.Items)
		{
			if (item is PopulationGroup populationGroup && populationGroup.Merge && !string.IsNullOrEmpty(populationGroup.Name) && i.GroupLookup.TryGetValue(populationGroup.Name, out var value))
			{
				value.MergeFrom(populationGroup, i);
			}
			else
			{
				Items.Add(item);
			}
		}
	}

	public override void GetEachUniqueObject(List<string> Ret)
	{
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(Ret);
		}
	}

	public override void GenerateStructured(StructuredPopulationResult result, Dictionary<string, string> Vars)
	{
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		int num = Number.RollCached();
		StructuredPopulationResult structuredPopulationResult = new StructuredPopulationResult();
		structuredPopulationResult.Hint = Hint;
		result.ChildGroups.Add(structuredPopulationResult);
		for (int i = 0; i < num; i++)
		{
			foreach (string item in Chance.CachedCommaExpansion())
			{
				if (string.IsNullOrEmpty(item) || (!(item == "100") && Stat.Random(1, 100) > Convert.ToInt32(item)))
				{
					continue;
				}
				if (string.IsNullOrEmpty(Style))
				{
					Style = "pickeach";
				}
				if (Style.EqualsNoCase("pickone"))
				{
					ulong num2 = 0uL;
					foreach (PopulationItem item2 in Items)
					{
						num2 += item2.Weight;
					}
					ulong num3 = Stat.NextULong(0uL, num2);
					num2 = 0uL;
					foreach (PopulationItem item3 in Items)
					{
						if (num3 >= num2 && num3 < num2 + item3.Weight)
						{
							item3.GenerateStructured(structuredPopulationResult, Vars);
							break;
						}
						num2 += item3.Weight;
					}
				}
				else
				{
					if (!Style.EqualsNoCase("pickeach"))
					{
						continue;
					}
					foreach (PopulationItem item4 in Items)
					{
						item4.GenerateStructured(structuredPopulationResult, Vars);
					}
				}
			}
		}
	}

	public override List<PopulationResult> Generate(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		List<PopulationResult> list = new List<PopulationResult>();
		int num = Number.RollCached();
		for (int i = 0; i < num; i++)
		{
			foreach (string item in Chance.CachedCommaExpansion())
			{
				if (string.IsNullOrEmpty(item) || (!(item == "100") && Stat.Random(1, 100) > Convert.ToInt32(item)))
				{
					continue;
				}
				if (string.IsNullOrEmpty(Style))
				{
					Style = "pickeach";
				}
				if (Style.EqualsNoCase("pickone"))
				{
					ulong num2 = 0uL;
					foreach (PopulationItem item2 in Items)
					{
						num2 += item2.Weight;
					}
					ulong num3 = Stat.NextULong(0uL, num2);
					num2 = 0uL;
					foreach (PopulationItem item3 in Items)
					{
						if (num3 >= num2 && num3 < num2 + item3.Weight)
						{
							list.AddRange(item3.Generate(Vars, Hint ?? DefaultHint));
							break;
						}
						num2 += item3.Weight;
					}
				}
				else
				{
					if (!Style.EqualsNoCase("pickeach"))
					{
						continue;
					}
					foreach (PopulationItem item4 in Items)
					{
						list.AddRange(item4.Generate(Vars, Hint ?? DefaultHint));
					}
				}
			}
		}
		return list;
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (string.IsNullOrEmpty(Number))
		{
			Number = "1";
		}
		if (string.IsNullOrEmpty(Chance))
		{
			Chance = "100";
		}
		int num = Number.RollCached();
		for (int i = 0; i < num; i++)
		{
			foreach (string item in Chance.CachedCommaExpansion())
			{
				if (string.IsNullOrEmpty(item) || (!(item == "100") && Stat.Random(1, 100) > Convert.ToInt32(item)))
				{
					continue;
				}
				if (string.IsNullOrEmpty(Style))
				{
					Style = "pickeach";
				}
				if (Style.EqualsNoCase("pickone"))
				{
					ulong num2 = 0uL;
					foreach (PopulationItem item2 in Items)
					{
						num2 += item2.Weight;
					}
					ulong num3 = Stat.NextULong(0uL, num2);
					num2 = 0uL;
					foreach (PopulationItem item3 in Items)
					{
						if (num3 >= num2 && num3 < num2 + item3.Weight)
						{
							return item3.GenerateOne(Vars, Hint ?? DefaultHint);
						}
						num2 += item3.Weight;
					}
				}
				else
				{
					if (!Style.EqualsNoCase("pickeach"))
					{
						continue;
					}
					foreach (PopulationItem item4 in Items)
					{
						PopulationResult populationResult = item4.GenerateOne(Vars, Hint ?? DefaultHint);
						if (populationResult != null)
						{
							return populationResult;
						}
					}
				}
			}
		}
		return null;
	}
}

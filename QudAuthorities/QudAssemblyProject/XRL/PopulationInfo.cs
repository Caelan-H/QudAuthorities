using System.Collections.Generic;

namespace XRL;

public class PopulationInfo
{
	public string Name;

	public string Hint;

	public bool Merge;

	public List<PopulationItem> Items = new List<PopulationItem>();

	public Dictionary<string, PopulationGroup> GroupLookup = new Dictionary<string, PopulationGroup>();

	public PopulationInfo()
	{
	}

	public PopulationInfo(string Name)
		: this()
	{
		this.Name = Name;
	}

	public void MergeFrom(PopulationInfo o)
	{
		if (o.Hint != null)
		{
			Hint = o.Hint;
		}
		foreach (PopulationItem item in o.Items)
		{
			PopulationGroup populationGroup = item as PopulationGroup;
			if (populationGroup != null && populationGroup.Merge && !string.IsNullOrEmpty(populationGroup.Name) && GroupLookup.TryGetValue(populationGroup.Name, out var value))
			{
				value.MergeFrom(populationGroup, this);
				continue;
			}
			Items.Add(item);
			if (populationGroup != null)
			{
				GroupLookup[populationGroup.Name] = populationGroup;
			}
		}
	}

	public List<PopulationResult> Generate(Dictionary<string, string> Vars, string DefaultHint)
	{
		return PopulationManager.Generate(this, Vars, DefaultHint);
	}

	public PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		return PopulationManager.GenerateOne(this, Vars, DefaultHint);
	}

	public string[] GetEachUniqueObjectRoot()
	{
		List<string> list = new List<string>();
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(list);
		}
		return list.ToArray();
	}

	public void GetEachUniqueObject(List<string> List)
	{
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(List);
		}
	}
}

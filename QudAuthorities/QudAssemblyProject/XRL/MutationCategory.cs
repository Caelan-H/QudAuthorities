using System;
using System.Collections.Generic;

namespace XRL;

[Serializable]
public class MutationCategory
{
	public string Name = "";

	public string DisplayName = "";

	public string Help = "";

	public string Stat = "";

	public string Property = "";

	public string ForceProperty = "";

	public bool IncludeInMutatePool;

	public List<MutationEntry> Entries = new List<MutationEntry>(128);

	public MutationCategory()
	{
	}

	public MutationCategory(string _Name)
	{
		Name = _Name;
	}

	public void Add(MutationEntry Entry)
	{
		Entry.Category = this;
		Entries.Add(Entry);
	}

	public void MergeWith(MutationCategory newCategory)
	{
		Dictionary<string, MutationEntry> dictionary = new Dictionary<string, MutationEntry>(Entries.Count);
		for (int i = 0; i < Entries.Count; i++)
		{
			dictionary.Add(Entries[i].DisplayName, Entries[i]);
		}
		if (!string.IsNullOrEmpty(newCategory.DisplayName))
		{
			DisplayName = newCategory.DisplayName;
		}
		if (!string.IsNullOrEmpty(newCategory.Help))
		{
			Help = newCategory.Help;
		}
		if (!string.IsNullOrEmpty(newCategory.Stat))
		{
			Stat = newCategory.Stat;
		}
		if (!string.IsNullOrEmpty(newCategory.Property))
		{
			Property = newCategory.Property;
		}
		if (!string.IsNullOrEmpty(newCategory.ForceProperty))
		{
			Property = newCategory.ForceProperty;
		}
		if (newCategory.IncludeInMutatePool)
		{
			IncludeInMutatePool = newCategory.IncludeInMutatePool;
		}
		for (int j = 0; j < newCategory.Entries.Count; j++)
		{
			MutationEntry mutationEntry = newCategory.Entries[j];
			if (!string.IsNullOrEmpty(mutationEntry.DisplayName) && mutationEntry.DisplayName[0] == '-')
			{
				if (dictionary.ContainsKey(mutationEntry.DisplayName.Substring(1)))
				{
					Entries.Remove(dictionary[mutationEntry.DisplayName.Substring(1)]);
					dictionary.Remove(mutationEntry.DisplayName.Substring(1));
				}
			}
			else if (dictionary.ContainsKey(mutationEntry.DisplayName))
			{
				dictionary[mutationEntry.DisplayName].MergeWith(mutationEntry);
			}
			else
			{
				Entries.Add(mutationEntry);
				dictionary.Add(mutationEntry.DisplayName, mutationEntry);
			}
		}
	}
}

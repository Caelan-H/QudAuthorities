using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL;

[HasModSensitiveStaticCache]
public static class MutationFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<MutationCategory> _Categories = null;

	private static Dictionary<string, MutationCategory> _CategoriesByName = null;

	private static Dictionary<string, MutationEntry> _MutationsByName = null;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<MutationEntry>> _MutationsByClass = null;

	[ModSensitiveStaticCache(false)]
	public static List<string> StatsUsedByMutations = new List<string>(1);

	public static Dictionary<string, Action<XmlDataHelper>> XMLNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "mutations", HandleMutationsNode },
		{ "category", HandleCategoryNode }
	};

	public static MutationCategory _currentParsingCategory;

	[Obsolete("Use GetCategories() instead of Categories")]
	public static List<MutationCategory> Categories
	{
		get
		{
			CheckInit();
			return _Categories;
		}
	}

	[Obsolete("Use GetCategories() instead of CategoriesByName")]
	public static Dictionary<string, MutationCategory> CategoriesByName
	{
		get
		{
			CheckInit();
			return _CategoriesByName;
		}
	}

	[Obsolete("Use HasMutation(string name) or GetMutationEntryByName(string name)")]
	public static Dictionary<string, MutationEntry> MutationsByName
	{
		get
		{
			CheckInit();
			return _MutationsByName;
		}
	}

	public static void CheckInit()
	{
		if (_Categories == null)
		{
			Loading.LoadTask("Loading Mutations.xml", Init);
		}
	}

	public static IEnumerable<MutationEntry> AllMutationEntries()
	{
		return _MutationsByName.Values;
	}

	public static bool HasMutation(string name)
	{
		CheckInit();
		return _MutationsByName.ContainsKey(name);
	}

	public static MutationEntry GetMutationEntryByName(string name)
	{
		CheckInit();
		_MutationsByName.TryGetValue(name, out var value);
		return value;
	}

	public static List<MutationCategory> GetCategories()
	{
		CheckInit();
		return _Categories;
	}

	public static List<MutationEntry> GetMutationEntry(BaseMutation mutation)
	{
		return GetMutationEntry(mutation.GetType().Name);
	}

	public static List<MutationEntry> GetMutationEntry(string Class)
	{
		CheckInit();
		if (!_MutationsByClass.TryGetValue(Class, out var value))
		{
			return null;
		}
		return value;
	}

	private static void AddFromCategoryRespectingPrerelease(List<MutationEntry> list, MutationCategory category)
	{
		CheckInit();
		if (Options.GetOption("OptionEnablePrereleaseContent", "No") == "Yes")
		{
			list.AddRange(category.Entries);
			return;
		}
		foreach (MutationEntry entry in category.Entries)
		{
			if (!entry.Prerelease)
			{
				list.Add(entry);
			}
		}
	}

	public static List<MutationEntry> GetMutationsOfCategory(string Categories)
	{
		CheckInit();
		List<MutationEntry> list = new List<MutationEntry>(128);
		if (Categories.Contains(","))
		{
			string[] array = Categories.Split(',');
			foreach (string key in array)
			{
				AddFromCategoryRespectingPrerelease(list, _CategoriesByName[key]);
			}
		}
		else
		{
			AddFromCategoryRespectingPrerelease(list, _CategoriesByName[Categories]);
		}
		return list;
	}

	public static BaseMutation GetRandomMutation(string Categories)
	{
		CheckInit();
		return GetMutationsOfCategory(Categories).GetRandomElement()?.CreateInstance();
	}

	private static void Init()
	{
		_Categories = new List<MutationCategory>(5);
		_CategoriesByName = new Dictionary<string, MutationCategory>(5);
		_MutationsByName = new Dictionary<string, MutationEntry>(128);
		_MutationsByClass = new Dictionary<string, List<MutationEntry>>(128);
		StatsUsedByMutations = new List<string>(1);
		using (XmlDataHelper xmlDataHelper = DataManager.GetXMLStream("Mutations.xml", null))
		{
			xmlDataHelper.HandleNodes(XMLNodes);
		}
		ModManager.ForEachFile("Mutations.xml", delegate(string s, ModInfo info)
		{
			using XmlDataHelper xmlDataHelper2 = DataManager.GetXMLStream(s, info);
			xmlDataHelper2.HandleNodes(XMLNodes);
		});
		for (int i = 0; i < _Categories.Count; i++)
		{
			MutationCategory mutationCategory = _Categories[i];
			if (!string.IsNullOrEmpty(mutationCategory.Stat) && !StatsUsedByMutations.Contains(mutationCategory.Stat))
			{
				StatsUsedByMutations.Add(mutationCategory.Stat);
			}
			mutationCategory.Entries.Sort((MutationEntry a, MutationEntry b) => a.DisplayName.CompareTo(b.DisplayName));
			for (int j = 0; j < mutationCategory.Entries.Count; j++)
			{
				MutationEntry mutationEntry = mutationCategory.Entries[j];
				_MutationsByName.Add(mutationEntry.DisplayName, mutationEntry);
				if (!string.IsNullOrEmpty(mutationEntry.Class))
				{
					if (!_MutationsByClass.ContainsKey(mutationEntry.Class))
					{
						_MutationsByClass.Add(mutationEntry.Class, new List<MutationEntry>(1));
					}
					_MutationsByClass[mutationEntry.Class].Add(mutationEntry);
				}
				if (!string.IsNullOrEmpty(mutationEntry.Stat) && !StatsUsedByMutations.Contains(mutationEntry.Stat))
				{
					StatsUsedByMutations.Add(mutationEntry.Stat);
				}
			}
		}
	}

	public static void HandleMutationsNode(XmlDataHelper xml)
	{
		xml.HandleNodes(XMLNodes);
	}

	public static void HandleCategoryNode(XmlDataHelper xml)
	{
		MutationCategory mutationCategory = LoadCategoryNode(xml);
		if (mutationCategory.Name[0] == '-')
		{
			if (_CategoriesByName.ContainsKey(mutationCategory.Name.Substring(1)))
			{
				MutationCategory item = _CategoriesByName[mutationCategory.Name.Substring(1)];
				_CategoriesByName.Remove(mutationCategory.Name.Substring(1));
				_Categories.Remove(item);
			}
		}
		else if (_CategoriesByName.ContainsKey(mutationCategory.Name))
		{
			_CategoriesByName[mutationCategory.Name].MergeWith(mutationCategory);
		}
		else
		{
			_CategoriesByName.Add(mutationCategory.Name, mutationCategory);
			_Categories.Add(mutationCategory);
		}
	}

	public static MutationCategory LoadCategoryNode(XmlDataHelper xml)
	{
		MutationCategory mutationCategory = (_currentParsingCategory = new MutationCategory());
		mutationCategory.Name = xml.GetAttribute("Name");
		mutationCategory.DisplayName = xml.GetAttribute("DisplayName");
		mutationCategory.Help = xml.GetAttribute("Help");
		mutationCategory.Stat = xml.GetAttribute("Stat");
		mutationCategory.Property = xml.GetAttribute("Property");
		mutationCategory.ForceProperty = xml.GetAttribute("ForceProperty");
		mutationCategory.IncludeInMutatePool = xml.GetAttributeBool("IncludeInMutatePool", defaultValue: false);
		xml.HandleNodes(new Dictionary<string, Action<XmlDataHelper>> { { "mutation", HandleMutationNode } });
		_currentParsingCategory = null;
		return mutationCategory;
	}

	public static void HandleMutationNode(XmlDataHelper xml)
	{
		MutationEntry mutationEntry = LoadMutationNode(xml);
		mutationEntry.Category = _currentParsingCategory;
		if (!mutationEntry.Prerelease || Options.GetOption("OptionEnablePrereleaseContent") == "Yes")
		{
			_currentParsingCategory.Entries.Add(mutationEntry);
		}
		xml.DoneWithElement();
	}

	public static MutationEntry LoadMutationNode(XmlDataHelper xml)
	{
		MutationEntry mutationEntry = new MutationEntry();
		mutationEntry.DisplayName = xml.GetAttribute("Name");
		mutationEntry.Class = xml.GetAttribute("Class");
		mutationEntry.Constructor = xml.GetAttribute("Constructor");
		mutationEntry.Tile = xml.GetAttribute("Tile");
		mutationEntry.Foreground = xml.GetAttribute("Foreground") ?? mutationEntry.Foreground;
		mutationEntry.Detail = xml.GetAttribute("Detail") ?? mutationEntry.Detail;
		mutationEntry.Cost = xml.GetAttributeInt("Cost", -999);
		mutationEntry.Stat = xml.GetAttributeString("Stat", "");
		mutationEntry.MutationCode = xml.GetAttribute("Code");
		mutationEntry.Property = xml.GetAttributeString("Property", "");
		mutationEntry.ForceProperty = xml.GetAttributeString("ForceProperty", "");
		mutationEntry.BearerDescription = xml.GetAttributeString("BearerDescription", "");
		mutationEntry.Maximum = xml.GetAttributeInt("MaxSelected", -999);
		mutationEntry.MaxLevel = xml.GetAttributeInt("MaxLevel", -999);
		mutationEntry.Ranked = xml.GetAttribute("Ranked") == "1";
		mutationEntry.Exclusions = xml.GetAttribute("Exclusions");
		mutationEntry.Prerelease = xml.GetAttributeBool("Prerelease", defaultValue: false);
		return mutationEntry;
	}
}

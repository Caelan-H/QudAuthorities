using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Qud.API;
using UnityEngine;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;

namespace XRL;

[HasModSensitiveStaticCache]
public class PopulationManager
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PopulationInfo> _Populations = null;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, MethodInfo> _PopulationCalls = null;

	private static string[] zonetierSplitter = null;

	private static Dictionary<string, Action<XmlDataHelper>> Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "populations", HandleNodes },
		{ "population", HandlePopulationNode }
	};

	private static PopulationInfo CurrentReadingPopulation;

	private static Dictionary<string, Action<XmlDataHelper>> PopulationSubNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "object", HandleObjectNode },
		{ "table", HandleTableNode },
		{ "group", HandleGroupNode }
	};

	private static Dictionary<string, Action<XmlDataHelper>> GroupSubNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "object", HandleGroupObjectNode },
		{ "table", HandleGroupTableNode },
		{ "group", HandleGroupGroupNode }
	};

	private static PopulationGroup CurrentReadingGroup;

	public static Dictionary<string, PopulationInfo> Populations
	{
		get
		{
			CheckInit();
			return _Populations;
		}
	}

	public static Dictionary<string, MethodInfo> PopulationCalls
	{
		get
		{
			if (_PopulationCalls == null)
			{
				_PopulationCalls = new Dictionary<string, MethodInfo>();
			}
			return _PopulationCalls;
		}
	}

	public static string resolveCallBlueprintSlug(string slug)
	{
		if (slug.StartsWith("$CALLBLUEPRINTMETHOD:", StringComparison.InvariantCultureIgnoreCase))
		{
			MethodInfo value = null;
			if (!PopulationCalls.TryGetValue(slug, out value))
			{
				string text = slug.Substring("$CALLBLUEPRINTMETHOD:".Length);
				int num = text.LastIndexOf(".");
				string typeID = text.Substring(0, num);
				string name = text.Substring(num + 1, text.Length - num - 1);
				PopulationCalls.Add(slug, null);
				Type type = ModManager.ResolveType(typeID);
				if (type != null)
				{
					value = type.GetMethod(name);
					PopulationCalls[slug] = value;
				}
			}
			if (value != null)
			{
				if (value.ReturnType == typeof(string))
				{
					int num2 = value.GetParameters().Length;
					return (string)value.Invoke(null, (num2 == 0) ? null : new object[num2]);
				}
				MetricsManager.LogError("Can't resolve resturn value to a string: " + slug);
			}
			else
			{
				MetricsManager.LogError("Undefined population call: " + slug);
			}
		}
		else
		{
			MetricsManager.LogError("Trying to resolve non-call slug: " + slug);
		}
		return null;
	}

	public static List<XRL.World.GameObject> resolveCallObjectSlug(string slug)
	{
		List<XRL.World.GameObject> list = XRL.World.Event.NewGameObjectList();
		if (slug.StartsWith("$CALLOBJECTMETHOD:", StringComparison.InvariantCultureIgnoreCase))
		{
			MethodInfo value = null;
			if (!PopulationCalls.TryGetValue(slug, out value))
			{
				string text = slug.Substring("$CALLOBJECTMETHOD:".Length);
				int num = text.LastIndexOf(".");
				string typeID = text.Substring(0, num);
				string name = text.Substring(num + 1, text.Length - num - 1);
				PopulationCalls.Add(slug, null);
				Type type = ModManager.ResolveType(typeID);
				if (type != null)
				{
					value = type.GetMethod(name);
					PopulationCalls[slug] = value;
				}
			}
			if (value != null)
			{
				if (value.ReturnType == typeof(XRL.World.GameObject))
				{
					int num2 = value.GetParameters().Length;
					list.Add((XRL.World.GameObject)value.Invoke(null, (num2 == 0) ? null : new object[num2]));
				}
				else if ((IEnumerable<XRL.World.GameObject>)value.ReturnType != null)
				{
					int num3 = value.GetParameters().Length;
					list.AddRange((IEnumerable<XRL.World.GameObject>)value.Invoke(null, (num3 == 0) ? null : new object[num3]));
				}
				else
				{
					MetricsManager.LogError("Can't resolve resturn value to a GameObject or GameObject collection: " + slug);
				}
			}
			else
			{
				MetricsManager.LogError("Undefined population call: " + slug);
			}
		}
		else
		{
			MetricsManager.LogError("Trying to resolve non-call slug: " + slug);
		}
		return list;
	}

	public static void CheckInit()
	{
		if (_Populations == null)
		{
			Loading.LoadTask("Loading PopulationTables.xml", LoadFiles);
		}
	}

	public static List<string> ResultToStringList(List<PopulationResult> Result)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < Result.Count; i++)
		{
			for (int j = 0; j < Result[i].Number; j++)
			{
				list.Add(Result[i].Blueprint);
			}
		}
		return list;
	}

	public static void RequireTable(string tableName)
	{
		CheckInit();
		if (_Populations.ContainsKey(tableName))
		{
			return;
		}
		if (tableName.StartsWith("DynamicObjectsTable:"))
		{
			string[] array = tableName.Split(':');
			if (array.Length > 2 && array[2].StartsWith("Tier"))
			{
				if (ResolveTier(array[2], out var low, out var high))
				{
					string targetTag = "DynamicObjectsTable:" + array[1];
					GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(tableName, GameObjectFactory.Factory.Blueprints.Values.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.HasTag(targetTag)), low, high);
				}
			}
			else
			{
				GameObjectFactory.Factory.FabricateDynamicObjectsTable(tableName);
			}
		}
		else if (tableName.StartsWith("DynamicSemanticTable:"))
		{
			GameObjectFactory.Factory.FabricateDynamicSemanticTable(tableName);
		}
		else if (tableName.StartsWith("DynamicArtifactsTable:"))
		{
			GameObjectFactory.Factory.FabricateDynamicArtifactsTable();
		}
		else if (tableName.StartsWith("DynamicHasPartTable:"))
		{
			string[] parts2 = tableName.Split(':');
			if (parts2.Length > 2 && parts2[2].StartsWith("Tier") && ResolveTier(parts2[2], out var low2, out var high2))
			{
				GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(tableName, GameObjectFactory.Factory.Blueprints.Values.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.HasPart(parts2[1])), low2, high2);
			}
			if (!_Populations.ContainsKey(tableName))
			{
				string baseName = tableName.Split(':')[1];
				GameObjectFactory.Factory.FabricateDynamicHasPartTable(baseName);
			}
		}
		else
		{
			if (!tableName.StartsWith("DynamicInheritsTable:"))
			{
				return;
			}
			string[] parts = tableName.Split(':');
			if (parts.Length > 2 && parts[2].StartsWith("Tier") && ResolveTier(parts[2], out var low3, out var high3))
			{
				GameObjectFactory.Factory.FabricateMultitierDynamicPopulationTable(tableName, GameObjectFactory.Factory.Blueprints.Values.Where((GameObjectBlueprint b) => EncountersAPI.IsEligibleForDynamicEncounters(b) && b.DescendsFrom(parts[1])), low3, high3);
			}
			if (!_Populations.ContainsKey(tableName))
			{
				string baseName2 = tableName.Split(':')[1];
				GameObjectFactory.Factory.FabricateDynamicInheritsTable(baseName2);
			}
		}
	}

	public static bool ResolveTier(string origSpec, out int low, out int high)
	{
		low = 0;
		high = 0;
		if (!origSpec.StartsWith("Tier"))
		{
			Debug.LogError("Tier specification does not start with Tier: " + origSpec);
			return false;
		}
		string text = origSpec.Substring(4);
		string text2 = null;
		while (text.Contains("{zonetier"))
		{
			if (text == text2)
			{
				Debug.LogError("Stalled parsing " + origSpec + " at " + text);
				return false;
			}
			text2 = text;
			if (text == "{zonetier}")
			{
				low = (high = ZoneManager.zoneGenerationContextTier);
				return true;
			}
			if (text == "{zonetier+1}")
			{
				low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier + 1));
				return true;
			}
			if (zonetierSplitter == null)
			{
				zonetierSplitter = new string[1] { "{zonetier" };
			}
			string[] array = text.Split(zonetierSplitter, StringSplitOptions.None);
			if (array.Length > 1 || array[0] != "" || (array.Length > 1 && array[1].Length > 3))
			{
				StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
				stringBuilder.Append(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					int num = array[i].IndexOf('}');
					if (num == 0)
					{
						stringBuilder.Append(ZoneManager.zoneGenerationContextTier).Append(array[i].Substring(1));
						continue;
					}
					if (num > 0 && array[i].StartsWith("+"))
					{
						string text3 = array[i].Substring(1, num - 1);
						try
						{
							int num2 = Convert.ToInt32(text3);
							stringBuilder.Append(Tier.Constrain(ZoneManager.zoneGenerationContextTier + num2)).Append(array[i].Substring(num + 1));
						}
						catch (Exception)
						{
							MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text3);
							return false;
						}
						continue;
					}
					if (num > 0 && array[i].StartsWith("-"))
					{
						string text4 = array[i].Substring(1, num - 1);
						try
						{
							int num3 = Convert.ToInt32(text4);
							stringBuilder.Append(Tier.Constrain(ZoneManager.zoneGenerationContextTier - num3)).Append(array[i].Substring(num + 1));
						}
						catch (Exception)
						{
							MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text4);
							return false;
						}
						continue;
					}
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + " at " + array[i]);
					return false;
				}
				text = stringBuilder.ToString();
				continue;
			}
			int num4 = array[1].IndexOf('}');
			if (num4 != array[1].Length - 1)
			{
				MetricsManager.LogError("Internal inconsistency parsing " + origSpec + " at " + array[1]);
				return false;
			}
			if (num4 == 0)
			{
				MetricsManager.LogWarning("This case for " + origSpec + " should be unreachable");
				low = (high = ZoneManager.zoneGenerationContextTier);
				return true;
			}
			if (num4 > 0 && array[1].StartsWith("+"))
			{
				string text5 = array[1].Substring(1, num4 - 1);
				try
				{
					int num5 = Convert.ToInt32(text5);
					low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier + num5));
					return true;
				}
				catch (Exception)
				{
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text5);
					return false;
				}
			}
			if (num4 > 0 && array[1].StartsWith("-"))
			{
				string text6 = array[1].Substring(1, num4 - 1);
				try
				{
					int num6 = Convert.ToInt32(text6);
					low = (high = Tier.Constrain(ZoneManager.zoneGenerationContextTier - num6));
					return true;
				}
				catch (Exception)
				{
					MetricsManager.LogError("Bad zone tier offset in " + origSpec + ": " + text6);
					return false;
				}
			}
			MetricsManager.LogError("Bad zone tier offset in " + origSpec + " at " + array[1]);
			return false;
		}
		int num7 = text.IndexOf('-');
		if (num7 != -1)
		{
			string text7 = text.Substring(0, num7);
			string text8 = text.Substring(num7 + 1);
			try
			{
				low = Tier.Constrain(Convert.ToInt32(text7));
			}
			catch (Exception)
			{
				MetricsManager.LogError("Bad low tier specification in " + origSpec + ": " + text7);
				return false;
			}
			high = low;
			try
			{
				high = Tier.Constrain(Convert.ToInt32(text8));
			}
			catch (Exception)
			{
				MetricsManager.LogError("Bad high tier specification in " + origSpec + ": " + text8);
				return false;
			}
			return true;
		}
		try
		{
			low = (high = Tier.Constrain(Convert.ToInt32(text)));
			return true;
		}
		catch (Exception)
		{
			MetricsManager.LogError("Bad tier specification in " + origSpec + ": " + text);
			return false;
		}
	}

	public static PopulationInfo ResolvePopulation(string tableName, bool missingOkay = false)
	{
		RequireTable(tableName);
		if (_Populations.TryGetValue(tableName, out var value))
		{
			return value;
		}
		if (tableName.StartsWith("@") || tableName.StartsWith("#"))
		{
			PopulationInfo populationInfo = new PopulationInfo();
			PopulationTable item = new PopulationTable
			{
				Name = tableName
			};
			populationInfo.Items.Add(item);
			return populationInfo;
		}
		if (!missingOkay)
		{
			Debug.LogWarning("Unknown Population table: " + tableName);
		}
		return null;
	}

	public static bool HasPopulation(string PopulationName)
	{
		return ResolvePopulation(PopulationName, missingOkay: true) != null;
	}

	public static XRL.World.GameObject CreateOneFrom(string PopulationName, Dictionary<string, string> Variables = null, int BonusModChance = 0, int SetModNumber = 0, string Context = null, Action<XRL.World.GameObject> beforeObjectCreated = null, Action<XRL.World.GameObject> afterObjectCreated = null, List<XRL.World.GameObject> provideInventory = null)
	{
		string blueprint = RollOneFrom(PopulationName, Variables).Blueprint;
		return GameObjectFactory.Factory.CreateObject(blueprint, BonusModChance, SetModNumber, beforeObjectCreated, afterObjectCreated, Context, provideInventory);
	}

	public static PopulationResult RollOneFrom(string PopulationName, Dictionary<string, string> vars = null, string defaultIfNull = null)
	{
		List<PopulationResult> list = null;
		try
		{
			list = Generate(PopulationName, vars);
		}
		catch (Exception message)
		{
			list = new List<PopulationResult>();
			Popup.Show("Error generating population:" + PopulationName + "\n\n, please report this error to support@freeholdgames.com");
			MetricsManager.LogError(message);
		}
		if (list.Count == 0)
		{
			return new PopulationResult(defaultIfNull, 0);
		}
		return list.GetRandomElement();
	}

	public static bool sameAs(List<PopulationResult> r1, List<PopulationResult> r2)
	{
		if (r1.Count != r2.Count)
		{
			return false;
		}
		foreach (PopulationResult entry in r1)
		{
			if (!r2.Any((PopulationResult e) => e.Blueprint == entry.Blueprint && e.Number == entry.Number))
			{
				return false;
			}
		}
		return true;
	}

	public static List<List<PopulationResult>> RollDistinctFrom(string PopulationName, int n, Dictionary<string, string> vars = null, string defaultIfNull = null)
	{
		List<List<PopulationResult>> list = new List<List<PopulationResult>>();
		int num = n;
		int num2 = 100;
		while (num > 0 && num2 > 0)
		{
			List<PopulationResult> Ret = Generate(PopulationName, vars);
			if (!list.Any((List<PopulationResult> r) => sameAs(r, Ret)))
			{
				list.Add(Ret);
				num--;
			}
			if (num <= 0)
			{
				break;
			}
			num2--;
		}
		return list;
	}

	public static List<XRL.World.GameObject> Expand(List<PopulationResult> Input)
	{
		int num = 0;
		List<XRL.World.GameObject> list = new List<XRL.World.GameObject>();
		for (int i = 0; i < Input.Count; i++)
		{
			for (int j = 0; j < Input[i].Number; j++)
			{
				list.Add(GameObjectFactory.Factory.CreateObject(Input[i].Blueprint));
				list[list.Count - 1].SetLongProperty("Batch", num);
				num++;
				if (num >= 2147483646)
				{
					num = 0;
				}
			}
		}
		return list;
	}

	public static StructuredPopulationResult GenerateStructured(string PopulationName, Dictionary<string, string> vars, string DefaultHint = null)
	{
		CheckInit();
		if (!_Populations.ContainsKey(PopulationName))
		{
			Debug.LogError("Unknown population table: " + PopulationName);
			return new StructuredPopulationResult();
		}
		PopulationInfo populationInfo = _Populations[PopulationName];
		StructuredPopulationResult structuredPopulationResult = new StructuredPopulationResult();
		structuredPopulationResult.Hint = populationInfo.Hint;
		foreach (PopulationItem item in populationInfo.Items)
		{
			item.GenerateStructured(structuredPopulationResult, vars);
		}
		return structuredPopulationResult;
	}

	public static StructuredPopulationResult GenerateStructured(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		CheckInit();
		if (PopulationName == null)
		{
			return new StructuredPopulationResult();
		}
		if (!_Populations.ContainsKey(PopulationName))
		{
			Debug.LogError("Unknown population table: " + PopulationName);
			return new StructuredPopulationResult();
		}
		PopulationInfo populationInfo = Populations[PopulationName];
		StructuredPopulationResult structuredPopulationResult = new StructuredPopulationResult();
		structuredPopulationResult.Hint = populationInfo.Hint;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (VariableValue1 != null)
		{
			dictionary.Add(VariableName1, VariableValue1);
		}
		if (VariableValue2 != null)
		{
			dictionary.Add(VariableName2, VariableValue2);
		}
		foreach (PopulationItem item in populationInfo.Items)
		{
			item.GenerateStructured(structuredPopulationResult, dictionary);
		}
		return structuredPopulationResult;
	}

	public static bool HasTable(string PopulationName)
	{
		CheckInit();
		if (string.IsNullOrEmpty(PopulationName))
		{
			return false;
		}
		if (!_Populations.ContainsKey(PopulationName))
		{
			if (!PopulationName.StartsWith("Dynamic"))
			{
				return false;
			}
			PopulationInfo populationInfo = new PopulationInfo();
			PopulationTable item = new PopulationTable
			{
				Name = PopulationName
			};
			populationInfo.Items.Add(item);
			populationInfo.Name = PopulationName;
			populationInfo.Generate(new Dictionary<string, string>(), null);
		}
		return true;
	}

	public static List<string> GetEach(string PopulationName, Dictionary<string, string> vars = null, string DefaultHint = null)
	{
		if (vars != null)
		{
			foreach (KeyValuePair<string, string> var in vars)
			{
				PopulationName = PopulationName.Replace("{" + var.Key + "}", var.Value);
			}
		}
		PopulationInfo populationInfo = ResolvePopulation(PopulationName);
		if (populationInfo == null)
		{
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (PopulationItem item in populationInfo.Items)
		{
			item.GetEachUniqueObject(list);
		}
		return list;
	}

	public static List<PopulationResult> GenerateSemantic(List<string> tags, int tier, int techTier, Dictionary<string, string> vars, string DefaultHint = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DynamicSemanticTable:");
		for (int i = 0; i < tags.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(tags[i]);
		}
		stringBuilder.Append(":").Append(tier).Append(":")
			.Append(techTier);
		return Generate(stringBuilder.ToString(), vars, DefaultHint);
	}

	public static List<PopulationResult> Generate(string PopulationName, Dictionary<string, string> vars, string DefaultHint = null)
	{
		if (PopulationName == null)
		{
			return new List<PopulationResult>();
		}
		if (vars != null)
		{
			foreach (KeyValuePair<string, string> var in vars)
			{
				if (PopulationName.Contains(var.Key))
				{
					PopulationName = PopulationName.Replace("{" + var.Key + "}", var.Value);
				}
			}
		}
		PopulationInfo populationInfo = ResolvePopulation(PopulationName);
		if (populationInfo == null)
		{
			Debug.LogError("Failed to resolve: " + PopulationName);
		}
		List<PopulationResult> list = new List<PopulationResult>();
		foreach (PopulationItem item in populationInfo.Items)
		{
			list.AddRange(item.Generate(vars, DefaultHint));
		}
		return list;
	}

	public static List<PopulationResult> Generate(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		if (PopulationName == null)
		{
			return new List<PopulationResult>();
		}
		PopulationInfo populationInfo = ResolvePopulation(PopulationName);
		if (populationInfo == null)
		{
			return new List<PopulationResult>();
		}
		List<PopulationResult> list = new List<PopulationResult>();
		Dictionary<string, string> dictionary = null;
		if (VariableValue1 != null || VariableValue2 != null)
		{
			dictionary = new Dictionary<string, string>();
			if (VariableValue1 != null)
			{
				dictionary.Add(VariableName1, VariableValue1);
			}
			if (VariableValue2 != null)
			{
				dictionary.Add(VariableName2, VariableValue2);
			}
		}
		foreach (PopulationItem item in populationInfo.Items)
		{
			list.AddRange(item.Generate(dictionary, DefaultHint));
		}
		return list;
	}

	public static PopulationResult GenerateOne(string PopulationName, string VariableName1 = null, string VariableValue1 = null, string VariableName2 = null, string VariableValue2 = null, string DefaultHint = null)
	{
		if (PopulationName == null)
		{
			return null;
		}
		PopulationInfo populationInfo = ResolvePopulation(PopulationName);
		if (populationInfo == null)
		{
			return null;
		}
		Dictionary<string, string> dictionary = null;
		if (VariableValue1 != null || VariableValue2 != null)
		{
			dictionary = new Dictionary<string, string>();
			if (VariableValue1 != null)
			{
				dictionary.Add(VariableName1, VariableValue1);
			}
			if (VariableValue2 != null)
			{
				dictionary.Add(VariableName2, VariableValue2);
			}
		}
		foreach (PopulationItem item in populationInfo.Items)
		{
			PopulationResult populationResult = item.GenerateOne(dictionary, DefaultHint);
			if (populationResult != null)
			{
				return populationResult;
			}
		}
		return null;
	}

	public static List<PopulationResult> Generate(PopulationInfo Population, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		List<PopulationResult> list = new List<PopulationResult>();
		foreach (PopulationItem item in Population.Items)
		{
			list.AddRange(item.Generate(Vars, DefaultHint));
		}
		return list;
	}

	public static PopulationResult GenerateOne(PopulationInfo Population, Dictionary<string, string> Vars, string DefaultHint = null)
	{
		foreach (PopulationItem item in Population.Items)
		{
			PopulationResult populationResult = item.GenerateOne(Vars, DefaultHint);
			if (populationResult != null)
			{
				return populationResult;
			}
		}
		return null;
	}

	public static bool AddToPopulation(string table, string sibling, params PopulationItem[] items)
	{
		if (!Populations.TryGetValue(table, out var value))
		{
			return false;
		}
		PopulationGroup populationGroup = FindGroup(value, sibling);
		if (populationGroup == null)
		{
			return false;
		}
		populationGroup.Items.AddRange(items);
		return true;
	}

	/// <summary>
	///             Recursively searches for a population group with the specified object blueprint or table name.
	///             </summary>
	public static PopulationGroup FindGroup(PopulationGroup target, string needle)
	{
		foreach (PopulationItem item in target.Items)
		{
			if (item.Name == needle && item is PopulationTable)
			{
				return target;
			}
			if ((item as PopulationObject)?.Blueprint == needle)
			{
				return target;
			}
			if (item is PopulationGroup populationGroup && populationGroup.Items.Count > 0)
			{
				PopulationGroup populationGroup2 = FindGroup(populationGroup, needle);
				if (populationGroup2 != null)
				{
					return populationGroup2;
				}
			}
		}
		return null;
	}

	public static PopulationGroup FindGroup(PopulationInfo target, string needle)
	{
		foreach (PopulationItem item in target.Items)
		{
			if (item is PopulationGroup populationGroup && populationGroup.Items.Count > 0)
			{
				PopulationGroup populationGroup2 = FindGroup(populationGroup, needle);
				if (populationGroup2 != null)
				{
					return populationGroup2;
				}
			}
		}
		return null;
	}

	private static void LoadFiles()
	{
		_Populations = new Dictionary<string, PopulationInfo>();
		List<(string, ModInfo)> Paths = new List<(string, ModInfo)>();
		Paths.Add((DataManager.FilePath("PopulationTables.xml"), null));
		ModManager.ForEachFile("PopulationTables.xml", delegate(string path, ModInfo ModInfo)
		{
			Paths.Add((path, ModInfo));
		});
		foreach (var (text, modInfo) in Paths)
		{
			try
			{
				using XmlDataHelper xmlDataHelper = DataManager.GetXMLStream(text, modInfo);
				HandleNodes(xmlDataHelper);
				xmlDataHelper.Close();
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error reading " + text, x);
			}
		}
	}

	private static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(Nodes);
	}

	private static void HandlePopulationNode(XmlDataHelper xml)
	{
		PopulationInfo populationInfo = LoadPopulation(xml);
		if (populationInfo.Merge)
		{
			if (Populations.TryGetValue(populationInfo.Name, out var value))
			{
				value.MergeFrom(populationInfo);
				return;
			}
			xml.ParseWarning("Attempting to Load Merge population " + populationInfo.Name + " but no such population found.");
			Populations[populationInfo.Name] = populationInfo;
		}
		else
		{
			Populations[populationInfo.Name] = populationInfo;
		}
	}

	private static void HandleObjectNode(XmlDataHelper xml)
	{
		PopulationObject item = LoadPopulationObject(xml);
		CurrentReadingPopulation.Items.Add(item);
	}

	private static void HandleTableNode(XmlDataHelper xml)
	{
		PopulationTable item = LoadPopulationTable(xml);
		CurrentReadingPopulation.Items.Add(item);
	}

	private static void HandleGroupNode(XmlDataHelper xml)
	{
		PopulationGroup populationGroup = LoadPopulationGroup(xml, CurrentReadingPopulation);
		if (populationGroup != null)
		{
			CurrentReadingPopulation.Items.Add(populationGroup);
		}
	}

	private static void HandleGroupObjectNode(XmlDataHelper xml)
	{
		PopulationObject item = LoadPopulationObject(xml);
		CurrentReadingGroup.Items.Add(item);
	}

	private static void HandleGroupTableNode(XmlDataHelper xml)
	{
		PopulationTable item = LoadPopulationTable(xml);
		CurrentReadingGroup.Items.Add(item);
	}

	private static void HandleGroupGroupNode(XmlDataHelper xml)
	{
		PopulationGroup populationGroup = LoadPopulationGroup(xml, CurrentReadingPopulation);
		if (populationGroup != null)
		{
			CurrentReadingGroup.Items.Add(populationGroup);
		}
	}

	private static PopulationInfo LoadPopulation(XmlDataHelper Reader)
	{
		if (CurrentReadingPopulation != null)
		{
			Reader.ParseWarning("Loading a new population when we seemingly never parsed the old one.");
		}
		PopulationInfo populationInfo = (CurrentReadingPopulation = new PopulationInfo());
		populationInfo.Name = Reader.GetAttribute("Name");
		populationInfo.Hint = Reader.GetAttribute("Hint");
		populationInfo.Merge = Reader.GetAttribute("Load")?.EqualsNoCase("Merge") ?? false;
		Reader.HandleNodes(PopulationSubNodes);
		CurrentReadingPopulation = null;
		return populationInfo;
	}

	private static PopulationGroup LoadPopulationGroup(XmlDataHelper Reader, PopulationInfo Info)
	{
		PopulationGroup populationGroup = new PopulationGroup();
		PopulationGroup value = null;
		populationGroup.Name = Reader.GetAttribute("Name");
		populationGroup.Style = Reader.GetAttribute("Style");
		populationGroup.Chance = Reader.GetAttribute("Chance");
		populationGroup.Number = Reader.GetAttribute("Number");
		populationGroup.Hint = Reader.GetAttribute("Hint");
		populationGroup.Merge = Reader.GetAttribute("Load").EqualsNoCase("Merge");
		populationGroup.WeightSpec = Reader.GetAttribute("Weight");
		if (uint.TryParse(populationGroup.WeightSpec, out var result))
		{
			populationGroup.Weight = result;
		}
		if (!string.IsNullOrEmpty(populationGroup.Name) && Info.GroupLookup.TryGetValue(populationGroup.Name, out value))
		{
			if (!Reader.IsMod())
			{
				throw new XmlException("Duplicate group name '" + populationGroup.Name + "'", Reader);
			}
			if (!populationGroup.Merge)
			{
				Info.Items.Remove(value);
			}
		}
		PopulationGroup currentReadingGroup = CurrentReadingGroup;
		CurrentReadingGroup = populationGroup;
		Reader.HandleNodes(GroupSubNodes);
		CurrentReadingGroup = currentReadingGroup;
		if (populationGroup.Merge && value != null)
		{
			value.MergeFrom(populationGroup, Info);
			return null;
		}
		if (!string.IsNullOrEmpty(populationGroup.Name))
		{
			Info.GroupLookup[populationGroup.Name] = populationGroup;
		}
		return populationGroup;
	}

	private static PopulationObject LoadPopulationObject(XmlDataHelper Reader)
	{
		PopulationObject populationObject = new PopulationObject();
		populationObject.Name = Reader.GetAttribute("Name");
		populationObject.Blueprint = Reader.GetAttribute("Blueprint");
		populationObject.Number = Reader.GetAttribute("Number");
		populationObject.Chance = Reader.GetAttribute("Chance");
		populationObject.Hint = Reader.GetAttribute("Hint");
		populationObject.Builder = Reader.GetAttribute("Builder");
		if (uint.TryParse(Reader.GetAttribute("Weight"), out var result))
		{
			populationObject.Weight = result;
		}
		Reader.DoneWithElement();
		return populationObject;
	}

	private static PopulationTable LoadPopulationTable(XmlDataHelper Reader)
	{
		PopulationTable populationTable = new PopulationTable();
		populationTable.Name = Reader.GetAttribute("Name");
		populationTable.Number = Reader.GetAttribute("Number");
		populationTable.Chance = Reader.GetAttribute("Chance");
		populationTable.Hint = Reader.GetAttribute("Hint");
		if (uint.TryParse(Reader.GetAttribute("Weight"), out var result))
		{
			populationTable.Weight = result;
		}
		Reader.DoneWithElement();
		return populationTable;
	}
}

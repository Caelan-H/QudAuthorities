using System.Collections.Generic;
using System.Xml;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace XRL.Names;

[HasModSensitiveStaticCache]
public static class NameStyles
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, NameStyle> _NameStyleTable;

	[ModSensitiveStaticCache(false)]
	private static List<NameStyle> _NameStyleList;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<NameValue>> _DefaultTemplateVars;

	public static int NameGenerationFailures;

	public static Dictionary<string, NameStyle> NameStyleTable
	{
		get
		{
			CheckInit();
			return _NameStyleTable;
		}
	}

	public static List<NameStyle> NameStyleList
	{
		get
		{
			CheckInit();
			return _NameStyleList;
		}
	}

	public static Dictionary<string, List<NameValue>> DefaultTemplateVars
	{
		get
		{
			CheckInit();
			return _DefaultTemplateVars;
		}
	}

	public static void CheckInit()
	{
		if (_NameStyleTable == null)
		{
			Loading.LoadTask("Loading Naming.xml", Init);
		}
	}

	private static void Init()
	{
		_NameStyleTable = new Dictionary<string, NameStyle>(16);
		_NameStyleList = new List<NameStyle>(16);
		_DefaultTemplateVars = new Dictionary<string, List<NameValue>>();
		ProcessNamingXmlFile("Naming.xml", mod: false);
		ModManager.ForEachFile("Naming.xml", delegate(string file)
		{
			ProcessNamingXmlFile(file, mod: true);
		});
	}

	private static void ProcessNamingXmlFile(string file, bool mod)
	{
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element)
			{
				if (!(xmlTextReader.Name == "naming"))
				{
					throw new XmlUnsupportedElementException(xmlTextReader);
				}
				LoadNamingNode(xmlTextReader, mod);
			}
		}
		xmlTextReader.Close();
	}

	public static void LoadNamingNode(XmlTextReader Reader, bool mod = false)
	{
		string loadMode = (mod ? Reader.GetAttribute("Load") : null);
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "namestyles")
				{
					LoadNameStylesNode(Reader, mod, loadMode);
					continue;
				}
				if (!(Reader.Name == "defaulttemplatevars"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateVarsNode(Reader, mod, loadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylesNode(XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "namestyle"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleNode(Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleNode(XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
		}
		if (_NameStyleTable.TryGetValue(attribute, out var value))
		{
			if (!mod)
			{
				MetricsManager.LogError("duplicate name style " + attribute);
				return;
			}
			if (LoadMode != "Merge")
			{
				_NameStyleList.Remove(value);
				value = new NameStyle();
				value.Name = attribute;
				_NameStyleTable[attribute] = value;
			}
		}
		else
		{
			value = new NameStyle();
			value.Name = attribute;
			_NameStyleTable.Add(attribute, value);
			_NameStyleList.Add(value);
		}
		string attribute2 = Reader.GetAttribute("HyphenationChance");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out value.HyphenationChance))
		{
			throw new XmlException("invalid HyphenationChance: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("TwoNameChance");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out value.TwoNameChance))
		{
			throw new XmlException("invalid TwoNameChance: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Base");
		if (!string.IsNullOrEmpty(attribute2))
		{
			value.Base = attribute2;
		}
		attribute2 = Reader.GetAttribute("Format");
		if (!string.IsNullOrEmpty(attribute2))
		{
			value.Format = attribute2;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "prefixes")
				{
					LoadNameStylePrefixesNode(value, Reader, mod, LoadMode);
					continue;
				}
				if (Reader.Name == "infixes")
				{
					LoadNameStyleInfixesNode(value, Reader, mod, LoadMode);
					continue;
				}
				if (Reader.Name == "postfixes")
				{
					LoadNameStylePostfixesNode(value, Reader, mod, LoadMode);
					continue;
				}
				if (Reader.Name == "titletemplates")
				{
					LoadNameStyleTitleTemplatesNode(value, Reader, mod, LoadMode);
					continue;
				}
				if (Reader.Name == "templatevars")
				{
					LoadNameStyleTemplateVarsNode(value, Reader, mod, LoadMode);
					continue;
				}
				if (!(Reader.Name == "scopes"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleScopesNode(value, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePrefixesNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Prefixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!string.IsNullOrEmpty(attribute))
		{
			style.PrefixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "prefix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStylePrefixNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePrefixNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NamePrefix namePrefix = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			namePrefix = style.Prefixes.Find(attribute);
			if (namePrefix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Prefixes.Remove(namePrefix);
					namePrefix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Prefixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (namePrefix == null)
		{
			namePrefix = new NamePrefix();
			namePrefix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out namePrefix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Prefixes.Add(namePrefix);
		}
	}

	public static void LoadNameStyleInfixesNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Infixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!string.IsNullOrEmpty(attribute))
		{
			style.InfixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "infix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleInfixNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleInfixNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameInfix nameInfix = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameInfix = style.Infixes.Find(attribute);
			if (nameInfix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Infixes.Remove(nameInfix);
					nameInfix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Infixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameInfix == null)
		{
			nameInfix = new NameInfix();
			nameInfix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out nameInfix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Infixes.Add(nameInfix);
		}
	}

	public static void LoadNameStylePostfixesNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Postfixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!string.IsNullOrEmpty(attribute))
		{
			style.PostfixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "postfix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStylePostfixNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePostfixNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NamePostfix namePostfix = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			namePostfix = style.Postfixes.Find(attribute);
			if (namePostfix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Postfixes.Remove(namePostfix);
					namePostfix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Postfixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (namePostfix == null)
		{
			namePostfix = new NamePostfix();
			namePostfix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out namePostfix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Postfixes.Add(namePostfix);
		}
	}

	public static void LoadNameStyleTitleTemplatesNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.TitleTemplates.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "titletemplate"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTitleTemplateNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleTitleTemplateNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameTemplate nameTemplate = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameTemplate = style.TitleTemplates.Find(attribute);
			if (nameTemplate != null)
			{
				if (LoadMode != "Merge")
				{
					style.TitleTemplates.Remove(nameTemplate);
					nameTemplate = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.TitleTemplates.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameTemplate == null)
		{
			nameTemplate = new NameTemplate();
			nameTemplate.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out nameTemplate.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.TitleTemplates.Add(nameTemplate);
		}
	}

	public static void LoadNameStyleTemplateVarsNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.TemplateVars.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "templatevar"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTemplateVarNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleTemplateVarNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		List<NameValue> value = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (style.TemplateVars != null && style.TemplateVars.TryGetValue(attribute, out value))
			{
				if (LoadMode != "Merge")
				{
					value.Clear();
				}
				flag = true;
			}
		}
		else if (style.TemplateVars.ContainsKey(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (value == null)
		{
			value = new List<NameValue>();
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "value"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTemplateValueNode(style, value, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
		if (!flag)
		{
			if (style.TemplateVars == null)
			{
				style.TemplateVars = new Dictionary<string, List<NameValue>>();
			}
			style.TemplateVars[attribute] = value;
		}
	}

	public static void LoadNameStyleTemplateValueNode(NameStyle style, List<NameValue> list, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameValue nameValue = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameValue = list.Find(attribute);
			if (nameValue != null)
			{
				if (LoadMode != "Merge")
				{
					style.TemplateVars.Remove(attribute);
					nameValue = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (list.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameValue == null)
		{
			nameValue = new NameValue();
			nameValue.Name = attribute;
		}
		if (!flag)
		{
			list.Add(nameValue);
		}
	}

	public static void LoadNameStyleScopesNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Scopes.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "scope"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleScopeNode(style, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleScopeNode(NameStyle style, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameScope nameScope = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameScope = style.Scopes.Find(attribute);
			if (nameScope != null)
			{
				if (LoadMode != "Merge")
				{
					style.Scopes.Remove(nameScope);
					nameScope = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Scopes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameScope == null)
		{
			nameScope = new NameScope();
			nameScope.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out nameScope.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Genotype");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Genotype = attribute2;
		}
		attribute2 = Reader.GetAttribute("Subtype");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Subtype = attribute2;
		}
		attribute2 = Reader.GetAttribute("Species");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Species = attribute2;
		}
		attribute2 = Reader.GetAttribute("Culture");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Culture = attribute2;
		}
		attribute2 = Reader.GetAttribute("Faction");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Faction = attribute2;
		}
		attribute2 = Reader.GetAttribute("Gender");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Gender = attribute2;
		}
		attribute2 = Reader.GetAttribute("Mutation");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Mutation = attribute2;
		}
		attribute2 = Reader.GetAttribute("Tag");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Tag = attribute2;
		}
		attribute2 = Reader.GetAttribute("Special");
		if (!string.IsNullOrEmpty(attribute2))
		{
			nameScope.Special = attribute2;
		}
		attribute2 = Reader.GetAttribute("Priority");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out nameScope.Priority))
		{
			throw new XmlException("invalid Priority: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Chance");
		if (!string.IsNullOrEmpty(attribute2) && !int.TryParse(attribute2, out nameScope.Chance))
		{
			throw new XmlException("invalid Chance: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Combine");
		if (!string.IsNullOrEmpty(attribute2) && !bool.TryParse(attribute2, out nameScope.Combine))
		{
			throw new XmlException("invalid Combine: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Scopes.Add(nameScope);
		}
	}

	public static void LoadDefaultTemplateVarsNode(XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				_DefaultTemplateVars.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "templatevar"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateVarNode(Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadDefaultTemplateVarNode(XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		List<NameValue> value = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (_DefaultTemplateVars != null && _DefaultTemplateVars.TryGetValue(attribute, out value))
			{
				if (LoadMode != "Merge")
				{
					value.Clear();
				}
				flag = true;
			}
		}
		else if (_DefaultTemplateVars.ContainsKey(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (value == null)
		{
			value = new List<NameValue>();
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "value"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateValueNode(value, Reader, mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
		if (!flag)
		{
			if (_DefaultTemplateVars == null)
			{
				_DefaultTemplateVars = new Dictionary<string, List<NameValue>>();
			}
			_DefaultTemplateVars[attribute] = value;
		}
	}

	public static void LoadDefaultTemplateValueNode(List<NameValue> list, XmlTextReader Reader, bool mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameValue nameValue = null;
		bool flag = false;
		if (mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameValue = list.Find(attribute);
			if (nameValue != null)
			{
				if (LoadMode != "Merge")
				{
					_DefaultTemplateVars.Remove(attribute);
					nameValue = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (list.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameValue == null)
		{
			nameValue = new NameValue();
			nameValue.Name = attribute;
		}
		if (!flag)
		{
			list.Add(nameValue);
		}
	}

	public static string Generate(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> TitleContext = null, bool FailureOkay = false, bool SpecialFaildown = false, NameStyle Skip = null, List<NameStyle> SkipList = null, bool ForProcessed = false)
	{
		if (!ForProcessed && GameObject.validate(ref For))
		{
			Genotype = For.GetGenotype();
			Subtype = For.GetSubtype();
			Species = For.GetSpecies();
			Culture = For.GetCulture();
			Faction = For.GetPrimaryFaction();
			Gender = For.GetGender().Name;
			Mutations = For.GetMutationNames();
			Tag = For.GetPropertyOrTag("NamingTag");
		}
		while (true)
		{
			List<(NameStyle, NameScope)> list = new List<(NameStyle, NameScope)>();
			bool flag = true;
			foreach (NameStyle nameStyle in NameStyleList)
			{
				if (nameStyle == Skip || (SkipList != null && SkipList.Contains(nameStyle)))
				{
					continue;
				}
				NameScope nameScope = nameStyle.CheckApply(Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special);
				if (nameScope == null)
				{
					continue;
				}
				if (nameScope.Combine && flag)
				{
					list.Add((nameStyle, nameScope));
					continue;
				}
				bool flag2 = true;
				foreach (var item in list)
				{
					if ((!nameScope.Combine || !item.Item2.Combine) && item.Item2.Priority > nameScope.Priority)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					list.Clear();
					list.Add((nameStyle, nameScope));
					flag = nameScope.Combine;
				}
			}
			switch (list.Count)
			{
			case 1:
			{
				string text3 = list[0].Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay, SpecialFaildown, Skip, SkipList);
				if (!string.IsNullOrEmpty(text3))
				{
					return text3;
				}
				break;
			}
			default:
			{
				int num = 0;
				foreach (var item2 in list)
				{
					if (item2.Item2.Priority > 0)
					{
						num += item2.Item2.Priority;
					}
				}
				if (num <= 0)
				{
					break;
				}
				int num2 = Stat.Random(0, num);
				int num3 = 0;
				foreach (var item3 in list)
				{
					if (item3.Item2.Priority <= 0)
					{
						continue;
					}
					num3 += item3.Item2.Priority;
					if (num2 < num3)
					{
						string text = item3.Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay, SpecialFaildown, Skip, SkipList);
						if (!string.IsNullOrEmpty(text))
						{
							return text;
						}
					}
				}
				foreach (var item4 in list)
				{
					if (item4.Item2.Priority > 0)
					{
						string text2 = item4.Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay, SpecialFaildown, Skip, SkipList);
						if (!string.IsNullOrEmpty(text2))
						{
							return text2;
						}
					}
				}
				break;
			}
			case 0:
				break;
			}
			if (!SpecialFaildown || string.IsNullOrEmpty(Special))
			{
				break;
			}
			Special = null;
		}
		if (FailureOkay)
		{
			return null;
		}
		return "NameGenFail" + ++NameGenerationFailures;
	}
}

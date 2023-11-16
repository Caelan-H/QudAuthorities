using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace XRL;

public class XmlDataHelper : XmlTextReader
{
	/// <summary>
	///             The mod responsible for this stream input.
	///             </summary>
	public readonly ModInfo modInfo;

	public bool sanityChecks = true;

	public readonly Regex TrimSpaceRegex = new Regex("^\\s+|\\s+$", RegexOptions.Multiline);

	protected HashSet<string> attributeChecked = new HashSet<string>();

	private static readonly Dictionary<string, Action<XmlDataHelper>> noNodesExpected = new Dictionary<string, Action<XmlDataHelper>>();

	/// <summary>
	///             Create XmlDataHelper from Stream
	///             </summary><param name="input">Input stream</param><param name="modInfo">Mod (or null for base game)</param><returns />
	public XmlDataHelper(Stream input, ModInfo modInfo = null)
		: base(input)
	{
		this.modInfo = modInfo;
		base.WhitespaceHandling = WhitespaceHandling.None;
	}

	public XmlDataHelper(string uri, ModInfo modInfo = null)
		: base(uri)
	{
		this.modInfo = modInfo;
		base.WhitespaceHandling = WhitespaceHandling.None;
	}

	public virtual bool IsMod()
	{
		return modInfo != null;
	}

	/// <summary>
	///             Logs Exception using mod channel or metrics.
	///             </summary><param name="e" />
	public void HandleException(Exception e)
	{
		if (modInfo != null)
		{
			modInfo.Error(e);
		}
		else
		{
			MetricsManager.LogException(GetSourcePoint(), e, "XML_Parse");
		}
	}

	public string GetSourcePoint()
	{
		return GetType().Name + ":: " + BaseURI + " line " + base.LineNumber + " char " + base.LinePosition;
	}

	/// <summary>
	///             Generate a parser warning.
	///             </summary><param name="msg" />
	public void ParseWarning(object msg)
	{
		string msg2 = GetSourcePoint() + "\n" + msg;
		if (modInfo != null)
		{
			modInfo.Warn(msg2);
		}
		else
		{
			MetricsManager.LogException(GetSourcePoint(), new Exception(msg.ToString()), "XML_Parse");
		}
	}

	public override bool Read()
	{
		attributeChecked.Clear();
		return base.Read();
	}

	public static void Parse(string path, Dictionary<string, Action<XmlDataHelper>> handlers, bool includeMods = false)
	{
		List<(string, ModInfo)> Paths = new List<(string, ModInfo)>();
		Paths.Add((DataManager.FilePath(path), null));
		if (includeMods)
		{
			ModManager.ForEachFile(path, delegate(string modPath, ModInfo modInfo)
			{
				Paths.Add((modPath, modInfo));
			});
		}
		foreach (var (fileName, modInfo2) in Paths)
		{
			using XmlDataHelper xmlDataHelper = DataManager.GetXMLStream(fileName, modInfo2);
			xmlDataHelper.HandleNodes(handlers);
			xmlDataHelper.Close();
		}
	}

	public void AssertExtraAttributes()
	{
		if (!sanityChecks || AttributeCount == 0)
		{
			return;
		}
		for (int i = 0; i < AttributeCount; i++)
		{
			MoveToAttribute(i);
			if (!attributeChecked.Contains(Name))
			{
				ParseWarning($"Unused attribute \"{Name}\" detected.");
			}
		}
		MoveToElement();
	}

	public override string GetAttribute(string name)
	{
		attributeChecked.Add(name);
		return base.GetAttribute(name);
	}

	public virtual bool HasAttribute(string name)
	{
		return GetAttribute(name) != null;
	}

	public virtual string GetAttributeString(string name, string defaultValue)
	{
		return GetAttribute(name) ?? defaultValue;
	}

	public virtual int GetAttributeInt(string name, int defaultValue)
	{
		int result = defaultValue;
		attributeChecked.Add(name);
		if (MoveToAttribute(name))
		{
			try
			{
				result = Convert.ToInt32(Value);
			}
			catch (Exception innerException)
			{
				HandleException(new Exception($"Error parsing attribute {name}=\"{Value}\" as an Int32", innerException));
			}
			MoveToElement();
		}
		return result;
	}

	public virtual bool GetAttributeBool(string name, bool defaultValue)
	{
		bool result = defaultValue;
		attributeChecked.Add(name);
		if (MoveToAttribute(name))
		{
			try
			{
				result = Convert.ToBoolean(Value);
			}
			catch (Exception innerException)
			{
				HandleException(new Exception($"Error parsing attribute {name}=\"{Value}\" as a boolean", innerException));
			}
			MoveToElement();
		}
		return result;
	}

	/// <summary>
	///             Current XML element is done. Ensure a self-closing tag, or otherwise empty tag.  Advances the reader past the current node.
	///             </summary>
	public void DoneWithElement()
	{
		HandleNodes(noNodesExpected);
	}

	public string GetTextNode()
	{
		AssertExtraAttributes();
		string name = Name;
		Read();
		string result = null;
		if (NodeType == XmlNodeType.Text)
		{
			result = TrimSpaceRegex.Replace(Value, "");
		}
		else
		{
			ParseWarning("Unexpected node type: " + NodeType.ToString() + " expecting Text.");
		}
		Read();
		if (NodeType != XmlNodeType.EndElement || Name != name)
		{
			ParseWarning("Expected closing tag for " + name + ".");
		}
		return result;
	}

	/// <summary>
	///             Handle children nodes of the current node given a dictionary of node name to Action handler.   Advances the reader past the current node.
	///             </summary><param name="nodeHandlers">Map of xml node names to action handlers</param>
	public void HandleNodes(Dictionary<string, Action<XmlDataHelper>> nodeHandlers)
	{
		AssertExtraAttributes();
		string name = Name;
		if (IsEmptyElement || NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		while (Read())
		{
			switch (NodeType)
			{
			case XmlNodeType.EndElement:
				if (Name == name)
				{
					return;
				}
				ParseWarning($"Unexpected EndElement for \"{Name}\"");
				break;
			case XmlNodeType.Element:
			{
				Action<XmlDataHelper> value = null;
				if (nodeHandlers.TryGetValue(Name, out value))
				{
					try
					{
						value(this);
					}
					catch (Exception ex)
					{
						HandleException(ex);
						throw ex;
					}
				}
				else
				{
					ParseWarning($"Unexpected \"{Name}\" node");
				}
				break;
			}
			default:
				ParseWarning("Unexpected node type: " + NodeType);
				break;
			case XmlNodeType.Comment:
			case XmlNodeType.XmlDeclaration:
				break;
			}
		}
	}
}

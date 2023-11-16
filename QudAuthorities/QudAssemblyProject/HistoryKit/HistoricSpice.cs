using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using XRL;
using XRL.UI;

namespace HistoryKit;

[HasModSensitiveStaticCache]
public static class HistoricSpice
{
	[ModSensitiveStaticCache(false)]
	private static JSONClass _root;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, JSONNode> _roots;

	public static JSONClass root
	{
		get
		{
			CheckInit();
			return _root;
		}
	}

	public static Dictionary<string, JSONNode> roots
	{
		get
		{
			CheckInit();
			return _roots;
		}
	}

	public static void CheckInit()
	{
		if (_roots == null)
		{
			Loading.LoadTask("Loading HistorySpice.json", Init);
		}
	}

	private static void Init()
	{
		if (_roots != null)
		{
			return;
		}
		_roots = new Dictionary<string, JSONNode>();
		string text = "";
		using (StreamReader streamReader = DataManager.GetStreamingAssetsStreamReader("HistorySpice.json"))
		{
			text = streamReader.ReadToEnd();
		}
		if (!(text != ""))
		{
			return;
		}
		foreach (KeyValuePair<string, JSONNode> childNode in (_root = (JSON.Parse(text) as JSONClass)["spice"] as JSONClass).ChildNodes)
		{
			_roots.Add(childNode.Key, childNode.Value);
		}
		foreach (string key in _roots.Keys)
		{
			List<string> obj = new List<string> { "spice", key };
			ResolveRelativeLinks(obj, _roots[key]);
			obj.RemoveAt(obj.Count - 1);
		}
	}

	private static void ResolveRelativeLinks(List<string> parents, JSONNode current)
	{
		foreach (JSONNode child in current.Childs)
		{
			if (!child.Value.Contains("^."))
			{
				continue;
			}
			Match match = Regex.Match(child.Value, "<.*?>");
			while (match != null && !string.IsNullOrEmpty(match.Value))
			{
				if (match.Value.Contains("^."))
				{
					string text = match.Groups[0].Value.Replace("<", "").Replace(">", "").Split('.')[0];
					string text2 = match.Groups[0].Value.Substring(text.Length + 2).Replace(">", "");
					int length = text.Length;
					text = "";
					for (int i = 0; i < parents.Count - length; i++)
					{
						text += parents[i];
						text += ".";
					}
					string newValue = "<" + text + text2 + ">";
					child.Value = child.Value.Replace(match.Groups[0].Value, newValue);
				}
				match = match.NextMatch();
			}
		}
		JSONClass jSONClass = current as JSONClass;
		if (!(jSONClass != null))
		{
			return;
		}
		foreach (KeyValuePair<string, JSONNode> childNode in jSONClass.ChildNodes)
		{
			parents.Add(childNode.Key);
			ResolveRelativeLinks(parents, childNode.Value);
			parents.RemoveAt(parents.Count - 1);
		}
	}
}

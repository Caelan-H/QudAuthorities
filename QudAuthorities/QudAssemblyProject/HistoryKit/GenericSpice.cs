using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using XRL;

namespace HistoryKit;

public class GenericSpice
{
	public JSONClass root;

	public Dictionary<string, JSONNode> roots;

	public void Init(string fileName)
	{
		if (roots != null)
		{
			return;
		}
		roots = new Dictionary<string, JSONNode>();
		Debug.Log("Loading global config");
		string text = "";
		using (StreamReader streamReader = new StreamReader(DataManager.FilePath(fileName)))
		{
			text = streamReader.ReadToEnd();
		}
		if (!(text != ""))
		{
			return;
		}
		foreach (KeyValuePair<string, JSONNode> childNode in (root = (JSON.Parse(text) as JSONClass)["spice"] as JSONClass).ChildNodes)
		{
			roots.Add(childNode.Key, childNode.Value);
			Debug.Log("Loaded " + childNode.Key);
		}
		foreach (string key in roots.Keys)
		{
			List<string> obj = new List<string> { "spice", key };
			ResolveRelativeLinks(obj, roots[key]);
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

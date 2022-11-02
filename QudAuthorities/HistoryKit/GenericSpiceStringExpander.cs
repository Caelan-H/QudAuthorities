using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using XRL.Language;

namespace HistoryKit;

public static class GenericSpiceStringExpander
{
	public static void SetVar(Dictionary<string, JSONNode> vars, string key, JSONNode val)
	{
		if (vars.ContainsKey(key))
		{
			vars[key] = val;
		}
		else
		{
			vars.Add(key, val);
		}
	}

	public static string ExpandQuery(string query, HistoricEntitySnapshot entity, History history, Dictionary<string, string> vars, Dictionary<string, JSONNode> nodeVars, GenericSpice spice)
	{
		string text = query;
		string text2 = query;
		string text3 = null;
		bool flag = false;
		bool flag2 = false;
		query = query.Substring(1, query.Length - 2);
		if (query.EndsWith(".capitalize"))
		{
			query = query.Remove(query.Length - ".capitalize".Length);
			flag2 = true;
		}
		if (query.EndsWith(".article"))
		{
			query = query.Remove(query.Length - ".article".Length);
			flag = true;
		}
		string[] array = query.Split('.');
		if (array.Length < 2)
		{
			string text4 = null;
			if (array[0].Contains("="))
			{
				string[] array2 = array[0].Split('=');
				array[0] = array2[1];
				text3 = array2[0];
			}
			else
			{
				text3 = null;
			}
			text4 = array[0];
			text2 = (nodeVars.ContainsKey(text4) ? nodeVars[text4].Value : ((!vars.ContainsKey(text4)) ? text4 : vars[text4]));
		}
		else
		{
			string text5 = null;
			if (query.Contains("="))
			{
				string[] array3 = query.Split('=');
				text3 = array3[0];
				array = array3[1].Split('.');
			}
			else
			{
				text3 = null;
			}
			text5 = array[0];
			if (text5.StartsWith("spice"))
			{
				JSONNode jSONNode = ((!(text5 == "spice")) ? nodeVars[text5] : spice.root);
				for (int i = 1; i < array.Length; i++)
				{
					if (array[i] == "!random")
					{
						if (i == array.Length - 1)
						{
							JSONClass jSONClass = jSONNode as JSONClass;
							if (jSONClass != null)
							{
								List<string> keys = jSONClass.GetKeys();
								text2 = keys[history.Random(0, keys.Count - 1)];
								if (string.IsNullOrEmpty(text3) || text3[0] != '$')
								{
									break;
								}
								SetVar(nodeVars, text3, keys[history.Random(0, keys.Count - 1)]);
								return "";
							}
							JSONNode jSONNode2 = jSONNode[history.Random(0, jSONNode.Count - 1)];
							if (!string.IsNullOrEmpty(text3) && text3[0] == '$')
							{
								SetVar(nodeVars, text3, jSONNode2);
								return "";
							}
							text2 = jSONNode2.Value;
							break;
						}
						int num = 0;
						num = history.Random(0, jSONNode.Count - 1);
						jSONNode = jSONNode[num];
						if (jSONNode == null)
						{
							Debug.LogError("no spice root after random in " + text);
							return "";
						}
						continue;
					}
					if (array[i].StartsWith("entity$"))
					{
						string[] array4 = array[i].Split('$');
						if (array4[1].Contains("["))
						{
							string[] array5 = array4[1].Split('[');
							array5[1] = array5[1].Replace("]", "");
							if (!entity.listProperties.ContainsKey(array5[0]))
							{
								return "<undefined entity list " + array5[0] + ">";
							}
							if (entity.GetList(array5[0]).Count == 0)
							{
								return "<empty entity list " + array5[0] + ">";
							}
							int num2 = 0;
							num2 = ((!(array5[1] == "random")) ? int.Parse(array5[1]) : history.Random(0, entity.GetList(array5[0]).Count - 1));
							jSONNode = jSONNode[entity.GetList(array5[0])[num2]];
						}
						else
						{
							if (!entity.properties.ContainsKey(array4[1]))
							{
								return "<undefined entity property " + array4[1] + ">";
							}
							jSONNode = jSONNode[entity.GetProperty(array4[1])];
						}
					}
					else
					{
						jSONNode = jSONNode[array[i]];
					}
					if (jSONNode == null)
					{
						Debug.LogError("spice reference " + array[i] + " in " + text + " wasn't a node");
						return "";
					}
					if (i == array.Length - 1)
					{
						text2 = jSONNode.Value;
						if (string.IsNullOrEmpty(text3) || text3[0] != '$')
						{
							break;
						}
						SetVar(nodeVars, text3, jSONNode);
						return "";
					}
				}
			}
			else if (text5.StartsWith("entity"))
			{
				HistoricEntitySnapshot historicEntitySnapshot = entity;
				if (text5.StartsWith("entity["))
				{
					historicEntitySnapshot = history.GetEntity(text5.Split('[')[1].Replace("]", "")).GetSnapshotAtYear(history.currentYear);
				}
				if (historicEntitySnapshot == null)
				{
					return "<unknown entity>";
				}
				if (array.Length != 2)
				{
					return "<unknown format " + query + ">";
				}
				if (array[1].Contains("["))
				{
					string[] array6 = array[1].Split('[');
					array6[1] = array6[1].Replace("]", "");
					if (!entity.listProperties.ContainsKey(array6[0]))
					{
						return "<undefined entity list " + array6[0] + ">";
					}
					if (entity.GetList(array6[0]).Count == 0)
					{
						return "<empty entity list " + array6[0] + ">";
					}
					int num3 = 0;
					num3 = ((!(array6[1] == "random")) ? int.Parse(array6[1]) : history.Random(0, entity.GetList(array6[0]).Count - 1));
					text2 = historicEntitySnapshot.GetList(array6[0])[num3];
				}
				else
				{
					text2 = historicEntitySnapshot.GetProperty(array[1]);
				}
			}
			else
			{
				text5.StartsWith("history");
			}
		}
		if (text2 == " ")
		{
			text2 = "";
		}
		if (flag && text2.Length > 0)
		{
			text2 = ((text2[0] != '=') ? Grammar.A(text2, flag2) : ((flag2 ? "=Article=" : "=article=") + text2));
			flag2 = false;
		}
		if (flag2 && text2.Length > 0)
		{
			text2 = ((text2[0] != '=') ? (char.ToUpper(text2[0]) + text2.Substring(1)) : ("=capitalize=" + text2));
		}
		if (text3 != null)
		{
			if (vars.ContainsKey(text3))
			{
				vars[text3] = text2;
			}
			else
			{
				vars.Add(text3, text2);
			}
			return "";
		}
		return text2;
	}

	public static string ExpandString(string input, HistoricEntitySnapshot entity, History history, GenericSpice spice)
	{
		StringBuilder stringBuilder = new StringBuilder(input);
		Dictionary<string, string> vars = new Dictionary<string, string>();
		Dictionary<string, JSONNode> nodeVars = new Dictionary<string, JSONNode>();
		Match match = Regex.Match(input, "<.*?>");
		int num = 0;
		while (match != null && !string.IsNullOrEmpty(match.Value))
		{
			while (match != null && !string.IsNullOrEmpty(match.Value))
			{
				stringBuilder.Replace(match.Groups[0].Value, ExpandQuery(match.Groups[0].Value, entity, history, vars, nodeVars, spice));
				match = match.NextMatch();
			}
			if (stringBuilder.Equals(input))
			{
				break;
			}
			num++;
			if (num > 25)
			{
				Debug.LogError("Maximum recursion reached on " + input + " current " + stringBuilder.ToString());
				break;
			}
			match = Regex.Match(stringBuilder.ToString(), "<.*?>");
		}
		return stringBuilder.ToString();
	}
}

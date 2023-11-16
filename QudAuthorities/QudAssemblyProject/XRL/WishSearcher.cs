using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.Language;
using XRL.World;

namespace XRL;

public class WishSearcher
{
	public static int WishResultSort(WishResult x, WishResult y)
	{
		if (x == y)
		{
			return 0;
		}
		if (x == null)
		{
			return 1;
		}
		if (y == null)
		{
			return -1;
		}
		int num = x.Distance.CompareTo(y.Distance);
		if (num != 0)
		{
			return num;
		}
		int num2 = x.NegativeMarks.CompareTo(y.NegativeMarks);
		if (num2 != 0)
		{
			return num2;
		}
		return x.AddOrder.CompareTo(y.AddOrder);
	}

	public static WishResult SearchForWish(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		list.Add(SearchForZone(Search));
		list.Add(SearchForMutation(Search));
		list.Add(SearchForBlueprint(Search));
		list.Add(SearchForQuest(Search));
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForZone(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("zone:", "");
		foreach (string key in WorldFactory.Factory.ZoneDisplayToID.Keys)
		{
			WishResult wishResult = new WishResult();
			wishResult.Distance = Grammar.LevenshteinDistance(text, key);
			wishResult.Result = WorldFactory.Factory.ZoneDisplayToID[key];
			wishResult.Type = WishResultType.Zone;
			wishResult.AddOrder = list.Count;
			list.Add(wishResult);
		}
		list.Sort(WishResultSort);
		if (list.Count == 0)
		{
			return null;
		}
		return list[0];
	}

	public static WishResult SearchForCrayonBlueprint(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("object:", "");
		foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
		{
			if (!value.HasPart("Physics") || value.HasTag("BaseObject") || value.Name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			int num = Grammar.LevenshteinDistance(text, value.Name);
			int num2 = 99999;
			string text2 = null;
			string partParameter = value.GetPartParameter("Render", "DisplayName");
			if (!string.IsNullOrEmpty(partParameter))
			{
				text2 = partParameter.ToLower();
				num2 = Grammar.LevenshteinDistance(text, text2);
			}
			if (text2 != null && !text2.StartsWith("["))
			{
				WishResult wishResult = new WishResult();
				if (num < num2)
				{
					wishResult.Distance = num;
				}
				else
				{
					wishResult.Distance = num2;
				}
				wishResult.Result = value.Name;
				wishResult.Type = WishResultType.Blueprint;
				wishResult.AddOrder = list.Count;
				list.Add(wishResult);
			}
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForBlueprint(string Search)
	{
		if (Search.Contains("object:"))
		{
			Search = Search.Replace("object:", "");
		}
		else if (Search.Contains("Object:"))
		{
			Search = Search.Replace("Object:", "");
		}
		else if (Search.Contains("OBJECT:"))
		{
			Search = Search.Replace("OBJECT:", "");
		}
		WishResult wishResult = null;
		if (GameObjectFactory.Factory.Blueprints.ContainsKey(Search))
		{
			wishResult = new WishResult();
			wishResult.Distance = 0;
			wishResult.Result = Search;
			wishResult.Type = WishResultType.Blueprint;
			return wishResult;
		}
		int num = int.MaxValue;
		string s = Search.ToLower();
		List<WishResult> list = new List<WishResult>();
		foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
		{
			if (!value.HasPart("Physics"))
			{
				continue;
			}
			int num2 = Math.Min(Grammar.LevenshteinDistance(s, value.CachedNameLC), Grammar.LevenshteinDistance(s, value.CachedDisplayNameStrippedLC));
			if (num2 > 10)
			{
				continue;
			}
			if (num2 > 0)
			{
				num2 = Math.Min(num2, Grammar.LevenshteinDistance(Search, value.Name));
			}
			if (num2 > 0)
			{
				num2 = Math.Min(num2, Grammar.LevenshteinDistance(Search, value.CachedDisplayNameStripped, caseInsensitive: false));
			}
			if (num2 <= num)
			{
				wishResult = new WishResult();
				wishResult.Distance = num2;
				wishResult.Result = value.Name;
				wishResult.Type = WishResultType.Blueprint;
				if (!value.GetPartParameter("Physics", "IsReal", "false").EqualsNoCase("true"))
				{
					wishResult.NegativeMarks++;
				}
				if (value.Name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
				{
					wishResult.NegativeMarks++;
				}
				if (value.HasTag("BaseObject"))
				{
					wishResult.NegativeMarks++;
				}
				if (!value.HasPart("Render"))
				{
					wishResult.NegativeMarks++;
				}
				if (num2 < num)
				{
					list.Clear();
					num = num2;
				}
				else
				{
					wishResult.AddOrder = list.Count;
				}
				list.Add(wishResult);
			}
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForMutation(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("mutation:", "");
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (type.FullName.Contains("Parts.Mutation"))
			{
				WishResult wishResult = new WishResult();
				wishResult.Type = WishResultType.Mutation;
				wishResult.Result = type.FullName;
				string[] array = type.FullName.Split('.');
				string t = array[array.Length - 1];
				wishResult.Distance = Grammar.LevenshteinDistance(text, t);
				list.Add(wishResult);
			}
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForQuest(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("quest:", "");
		foreach (string key in QuestLoader.Loader.QuestsByID.Keys)
		{
			WishResult wishResult = new WishResult();
			wishResult.Type = WishResultType.Quest;
			wishResult.Result = key;
			wishResult.Distance = Grammar.LevenshteinDistance(text, key);
			list.Add(wishResult);
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForEffect(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}
}

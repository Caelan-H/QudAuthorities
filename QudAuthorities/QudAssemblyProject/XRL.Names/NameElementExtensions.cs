using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.Names;

public static class NameElementExtensions
{
	public static string GetRandomNameElement<T>(this List<T> list, Random R = null) where T : NameElement
	{
		switch (list.Count)
		{
		case 0:
			return null;
		case 1:
			if (list[0].Weight <= 0)
			{
				return null;
			}
			return list[0].Name;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int num = 0;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].Weight > 0)
				{
					num += list[i].Weight;
				}
			}
			if (num <= 0)
			{
				return null;
			}
			int num2 = R.Next(0, num);
			int num3 = 0;
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (list[j].Weight > 0)
				{
					num3 += list[j].Weight;
					if (num2 < num3)
					{
						return list[j].Name;
					}
				}
			}
			return null;
		}
		}
	}

	public static T Find<T>(this List<T> list, string Name) where T : NameElement
	{
		foreach (T item in list)
		{
			if (item.Name == Name)
			{
				return item;
			}
		}
		return null;
	}

	public static bool Has<T>(this List<T> list, string Name) where T : NameElement
	{
		foreach (T item in list)
		{
			if (item.Name == Name)
			{
				return true;
			}
		}
		return false;
	}
}

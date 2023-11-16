using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using ConsoleLib.Console;
using Newtonsoft.Json;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.UI;
using XRL.World;

public static class Extensions
{
	private static Dictionary<string, List<string>> CachedCommaExpansions = new Dictionary<string, List<string>>(16);

	private static string LastCommaExpansionRequest;

	private static List<string> LastCommaExpansionResult;

	private static char[] splitterColon = new char[1] { ':' };

	private static Dictionary<string, Dictionary<string, string>> CachedDictionaryExpansions = new Dictionary<string, Dictionary<string, string>>(8);

	private static string LastDictionaryExpansionRequest;

	private static Dictionary<string, string> LastDictionaryExpansionResult;

	private static Dictionary<string, Dictionary<string, int>> CachedNumericDictionaryExpansions = new Dictionary<string, Dictionary<string, int>>(8);

	private static string LastNumericDictionaryExpansionRequest;

	private static Dictionary<string, int> LastNumericDictionaryExpansionResult;

	public static string Strip(this string s)
	{
		return ConsoleLib.Console.ColorUtility.StripFormatting(s);
	}

	public static string Capitalize(this string s)
	{
		return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(s);
	}

	public static int StrippedLength(this string s)
	{
		return ConsoleLib.Console.ColorUtility.LengthExceptFormatting(s);
	}

	public static int CompareExceptFormatting(this string s, string vs)
	{
		return ConsoleLib.Console.ColorUtility.CompareExceptFormatting(s, vs);
	}

	public static bool IsNullOrEmpty<T>(this ICollection<T> list)
	{
		if (list != null)
		{
			return list.Count < 1;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
	{
		if (list != null)
		{
			return !list.Any();
		}
		return true;
	}

	public static bool IsNullOrEmpty(this string value)
	{
		if (value != null)
		{
			return value.Length == 0;
		}
		return true;
	}

	public static bool IsDirectorySeparator(this char Value)
	{
		if (Value != Path.DirectorySeparatorChar)
		{
			return Value == Path.AltDirectorySeparatorChar;
		}
		return true;
	}

	public static bool Contains(this string text, string find, CompareOptions comp)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, find, comp) >= 0;
	}

	public static bool TryParseRange(this string Range, out int Low, out int High)
	{
		High = 0;
		int num = Range.IndexOf('-');
		if (num < 0)
		{
			bool result = int.TryParse(Range, out Low);
			High = Low;
			return result;
		}
		if (int.TryParse(Range.Substring(0, num), out Low))
		{
			return int.TryParse(Range.Substring(num + 1), out High);
		}
		return false;
	}

	public static void SetBit(this ref int field, int mask, bool value)
	{
		if (value)
		{
			field |= mask;
		}
		else
		{
			field &= ~mask;
		}
	}

	public static bool HasBit(this int field, int mask)
	{
		return (field & mask) != 0;
	}

	public static bool HasAllBits(this int field, int mask)
	{
		return (field & mask) == mask;
	}

	public static int CountBits(this int field)
	{
		field -= (field >> 1) & 0x55555555;
		field = (field & 0x33333333) + ((field >> 2) & 0x33333333);
		field = (field + (field >> 4)) & 0xF0F0F0F;
		return field * 16843009 >> 24;
	}

	public static Dictionary<T, int> Increment<T>(this Dictionary<T, int> dict, T key)
	{
		dict.TryGetValue(key, out var value);
		dict[key] = value + 1;
		return dict;
	}

	public static bool TryAdd<K, V>(this Dictionary<K, V> Self, K Key, V Value)
	{
		if (Self.ContainsKey(Key))
		{
			return false;
		}
		Self[Key] = Value;
		return true;
	}

	public static V GetValue<K, V>(this Dictionary<K, V> Self, K Key, V Default = default(V))
	{
		if (Key == null || !Self.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value;
	}

	public static List<string> CachedCommaExpansion(this string text)
	{
		if (text == LastCommaExpansionRequest && text != null)
		{
			return LastCommaExpansionResult;
		}
		if (!CachedCommaExpansions.TryGetValue(text, out var value))
		{
			value = new List<string>(text.Split(','));
			CachedCommaExpansions.Add(text, value);
		}
		LastCommaExpansionRequest = text;
		LastCommaExpansionResult = value;
		return value;
	}

	public static Dictionary<string, string> CachedDictionaryExpansion(this string text)
	{
		if (text == LastDictionaryExpansionRequest && text != null)
		{
			return LastDictionaryExpansionResult;
		}
		if (!CachedDictionaryExpansions.TryGetValue(text, out var value))
		{
			string[] array = text.Split(',');
			value = new Dictionary<string, string>(array.Length);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string[] array3 = text2.Split(splitterColon, 2);
				if (array3.Length < 2)
				{
					Debug.LogError("Bad dictionary expansion part '" + text2 + "'");
				}
				else if (value.ContainsKey(array3[0]))
				{
					Debug.LogError("Duplicate dictionary expansion entry '" + array3[0] + "'");
				}
				else
				{
					value.Add(array3[0], array3[1]);
				}
			}
			CachedDictionaryExpansions.Add(text, value);
		}
		LastDictionaryExpansionRequest = text;
		LastDictionaryExpansionResult = value;
		return value;
	}

	public static Dictionary<string, int> CachedNumericDictionaryExpansion(this string text)
	{
		if (text == LastNumericDictionaryExpansionRequest && text != null)
		{
			return LastNumericDictionaryExpansionResult;
		}
		if (!CachedNumericDictionaryExpansions.TryGetValue(text, out var value))
		{
			string[] array = text.Split(',');
			value = new Dictionary<string, int>(array.Length);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string[] array3 = text2.Split(splitterColon, 2);
				if (array3.Length < 2)
				{
					Debug.LogError("Bad dictionary expansion part '" + text2 + "'");
				}
				else if (value.ContainsKey(array3[0]))
				{
					Debug.LogError("Duplicate dictionary expansion entry '" + array3[0] + "'");
				}
				else
				{
					value.Add(array3[0], Convert.ToInt32(array3[1]));
				}
			}
			CachedNumericDictionaryExpansions.Add(text, value);
		}
		LastNumericDictionaryExpansionRequest = text;
		LastNumericDictionaryExpansionResult = value;
		return value;
	}

	public static int ReverseIndexOf<T>(this List<T> list, T item)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Equals(item))
			{
				return num;
			}
		}
		return -1;
	}

	public static string Snippet(this string text, int howManySpaces = 4, int ifNoSpaces = 10)
	{
		int num = text.UpToNthIndex(' ', howManySpaces);
		if (num <= 0)
		{
			return text.Substring(0, ifNoSpaces);
		}
		return text.Substring(0, num);
	}

	public static int UpToNthIndex(this string Text, char Char, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int length = Text.Length; i < length; i++)
		{
			if (Text[i] == Char)
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int UpToNthIndex(this string Text, Func<char, bool> Predicate, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int length = Text.Length; i < length; i++)
		{
			if (Predicate(Text[i]))
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int UpToNthIndex<T>(this List<T> List, T Value, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int count = List.Count; i < count; i++)
		{
			if (List[i].Equals(Value))
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int FindCount<T>(this List<T> list, Predicate<T> filter)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static T RemoveRandomElement<T>(this List<T> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default(T);
		case 1:
			return list[0];
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int index = R.Next(0, list.Count);
			T result = list[index];
			list.RemoveAt(index);
			return result;
		}
		}
	}

	public static T GetRandomElement<T>(this List<T> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default(T);
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Count)];
		}
	}

	public static T GetRandomElement<T>(this Dictionary<T, int> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default(T);
		case 1:
			foreach (T key in list.Keys)
			{
				if (list[key] > 0)
				{
					return key;
				}
			}
			return default(T);
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int num = 0;
			foreach (int value in list.Values)
			{
				num += value;
			}
			int num2 = R.Next(0, num);
			int num3 = 0;
			foreach (KeyValuePair<T, int> item in list)
			{
				num3 += item.Value;
				if (num3 >= num2)
				{
					return item.Key;
				}
			}
			throw new Exception("should be unreachable");
		}
		}
	}

	public static T GetRandomElement<T>(this Dictionary<T, int> list, ref int total, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default(T);
		case 1:
			foreach (T key in list.Keys)
			{
				if (list[key] > 0)
				{
					return key;
				}
			}
			return default(T);
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (total == 0)
			{
				foreach (int value in list.Values)
				{
					total += value;
				}
			}
			int num = R.Next(0, total);
			int num2 = 0;
			foreach (KeyValuePair<T, int> item in list)
			{
				num2 += item.Value;
				if (num2 >= num)
				{
					return item.Key;
				}
			}
			throw new Exception("should be unreachable");
		}
		}
	}

	public static T GetRandomElement<T>(this T[] list, System.Random R = null)
	{
		switch (list.Length)
		{
		case 0:
			return default(T);
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Length)];
		}
	}

	public static T GetRandomElementCosmetic<T>(this T[] list)
	{
		return list.GetRandomElement(Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this IEnumerable<T> enumerable, System.Random R = null)
	{
		int num = enumerable.Count();
		switch (num)
		{
		case 0:
			return default(T);
		case 1:
			return enumerable.ElementAt(0);
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int index = R.Next(0, num);
			return enumerable.ElementAt(index);
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.GetRandomElement(Stat.Rnd2);
	}

	public static char GetRandomElement(this string list, System.Random R = null)
	{
		switch (list.Length)
		{
		case 0:
			return '\0';
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Length)];
		}
	}

	public static char GetRandomElementCosmetic(this string list)
	{
		return list.GetRandomElement(Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this List<T> list, Predicate<T> filter, System.Random R = null)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		int num = 0;
		T result = default(T);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
				result = list[i];
			}
		}
		switch (num)
		{
		case 0:
			return default(T);
		case 1:
			return result;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (num == list.Count)
			{
				return list[R.Next(0, list.Count)];
			}
			int num2 = R.Next(0, num);
			if (num2 == num - 1)
			{
				return result;
			}
			int num3 = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null && filter(list[j]) && num3++ == num2)
				{
					return list[j];
				}
			}
			return default(T);
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this List<T> list, Predicate<T> filter)
	{
		return list.GetRandomElement(filter, Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this T[] list, Predicate<T> filter, System.Random R = null)
	{
		if (list.Length == 0)
		{
			return default(T);
		}
		int num = 0;
		T result = default(T);
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
				result = list[i];
			}
		}
		switch (num)
		{
		case 0:
			return default(T);
		case 1:
			return result;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (num == list.Length)
			{
				return list[R.Next(0, list.Length)];
			}
			int num2 = R.Next(0, num);
			if (num2 == num - 1)
			{
				return result;
			}
			int num3 = 0;
			for (int j = 0; j < list.Length; j++)
			{
				if (list[j] != null && filter(list[j]) && num3++ == num2)
				{
					return list[j];
				}
			}
			return default(T);
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this T[] list, Predicate<T> filter)
	{
		return list.GetRandomElement(filter, Stat.Rnd2);
	}

	public static string GetRandomSubstring(this string Text, char Separator, bool Trim = false, System.Random R = null)
	{
		if (Text.IsNullOrEmpty())
		{
			return "";
		}
		int i = 0;
		int num = 1;
		int num2;
		for (num2 = Text.Length - 1; i < num2; i++)
		{
			if (Text[i] == Separator)
			{
				num++;
			}
		}
		if (num == 1)
		{
			return Text;
		}
		int j = 0;
		int num3 = (R ?? Stat.Rand).Next(0, num);
		i = 0;
		num = 0;
		for (; i < num2; i++)
		{
			if (Text[i] == Separator)
			{
				if (num++ == num3)
				{
					i--;
					break;
				}
				j = i + 1;
			}
		}
		if (Trim)
		{
			for (; char.IsWhiteSpace(Text[j]); j++)
			{
			}
			while (char.IsWhiteSpace(Text[i]))
			{
				i--;
			}
		}
		return Text.Substring(j, i - j + 1);
	}

	public static string GetDelimitedSubstring(this string Text, char Separator, int Index, bool Trim = false)
	{
		if (Text.IsNullOrEmpty())
		{
			if (Index <= 0)
			{
				return Text;
			}
			return null;
		}
		int i = 0;
		int j = 0;
		int num = 0;
		for (int num2 = Text.Length - 1; j < num2; j++)
		{
			if (Text[j] == Separator)
			{
				if (num++ == Index)
				{
					j--;
					break;
				}
				i = j + 1;
			}
		}
		if (num == 0 && Index > 0)
		{
			return null;
		}
		if (Trim)
		{
			for (; char.IsWhiteSpace(Text[i]); i++)
			{
			}
			while (char.IsWhiteSpace(Text[j]))
			{
				j--;
			}
		}
		return Text.Substring(i, j - i + 1);
	}

	public static T GetCyclicElement<T>(this IList<T> list, int Index)
	{
		return list.Count switch
		{
			0 => default(T), 
			1 => list[0], 
			_ => list[Index % list.Count], 
		};
	}

	public static T GetCyclicElement<T>(this T[] list, int Index)
	{
		return list.Length switch
		{
			0 => default(T), 
			1 => list[0], 
			_ => list[Index % list.Length], 
		};
	}

	public static void Fill<T>(this IList<T> List, T Value)
	{
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			List[i] = Value;
		}
	}

	public static StringBuilder Clear(this StringBuilder SB)
	{
		SB.Length = 0;
		return SB;
	}

	public static StringBuilder AppendSigned(this StringBuilder SB, int val)
	{
		if (val >= 0)
		{
			SB.Append('+');
		}
		SB.Append(val);
		return SB;
	}

	public static StringBuilder AppendRange(this StringBuilder SB, IEnumerable<string> Range, string Separator)
	{
		bool flag = true;
		foreach (string item in Range)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				SB.Append(Separator);
			}
			SB.Append(item);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (SB.Length > 0)
			{
				SB.Append(' ');
			}
			SB.Append(text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, string text, string with)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (SB.Length > 0)
			{
				SB.Append(with);
			}
			SB.Append(text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, string text, char with)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (SB.Length > 0)
			{
				SB.Append(with);
			}
			SB.Append(text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char ch)
	{
		if (SB.Length > 0)
		{
			SB.Append(' ');
		}
		SB.Append(ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char ch, string with)
	{
		if (SB.Length > 0)
		{
			SB.Append(with);
		}
		SB.Append(ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char ch, char with)
	{
		if (SB.Length > 0)
		{
			SB.Append(with);
		}
		SB.Append(ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int num)
	{
		if (SB.Length > 0)
		{
			SB.Append(' ');
		}
		SB.Append(num);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int num, string with)
	{
		if (SB.Length > 0)
		{
			SB.Append(with);
		}
		SB.Append(num);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int num, char with)
	{
		if (SB.Length > 0)
		{
			SB.Append(with);
		}
		SB.Append(num);
		return SB;
	}

	public static StringBuilder Unindent(this StringBuilder SB)
	{
		int i = 0;
		int length;
		for (length = SB.Length; i < length; i++)
		{
			if (!char.IsWhiteSpace(SB[i]))
			{
				SB.Remove(0, i);
				break;
			}
		}
		for (i = SB.Length - 1; i >= 0; i--)
		{
			if (!char.IsWhiteSpace(SB[i]))
			{
				SB.Remove(i + 1, SB.Length - i - 1);
				break;
			}
		}
		i = 0;
		length = SB.Length;
		int num = length;
		for (; i < length; i++)
		{
			char c = SB[i];
			if (c == '\n')
			{
				if (i - num <= 1)
				{
					num = i;
					continue;
				}
			}
			else if (num >= length || char.IsWhiteSpace(c))
			{
				continue;
			}
			SB.Remove(num + 1, i - num - 1);
			length -= i - num - 1;
			i = num;
			num = length;
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, IEnumerable<string> Values, string With = " ")
	{
		foreach (string Value in Values)
		{
			if (SB.Length > 0)
			{
				SB.Append(With);
			}
			SB.Append(Value);
		}
		return SB;
	}

	public static StringBuilder AppendMask(this StringBuilder Builder, int Mask, int Length = 32)
	{
		for (int num = Length - 1; num >= 0; num--)
		{
			Builder.Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return Builder;
	}

	public static StringBuilder AppendMask(this StringBuilder Builder, int Mask, int Start, int Length)
	{
		for (int num = Start + Length - 1; num >= Start; num--)
		{
			Builder.Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return Builder;
	}

	public static StringBuilder AppendJoin(this StringBuilder SB, string Separator, IEnumerable<string> Values)
	{
		using IEnumerator<string> enumerator = Values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return SB;
		}
		string current = enumerator.Current;
		if (current != null)
		{
			SB.Append(current);
		}
		while (enumerator.MoveNext())
		{
			SB.Append(Separator);
			current = enumerator.Current;
			if (current != null)
			{
				SB.Append(current);
			}
		}
		return SB;
	}

	public static StringBuilder AppendItVerb(this StringBuilder SB, XRL.World.GameObject Object, string Verb, bool Capitalized = false)
	{
		return SB.Append(Capitalized ? Object.It : Object.it).Append(' ').Append(Object.GetVerb(Verb, PrependSpace: false, PronounAntecedent: true));
	}

	public static StringBuilder CompoundItVerb(this StringBuilder SB, XRL.World.GameObject Object, string Verb, char With = ' ', bool Capitalized = true)
	{
		return SB.Compound(Capitalized ? Object.It : Object.it, With).Append(' ').Append(Object.GetVerb(Verb, PrependSpace: false, PronounAntecedent: true));
	}

	public static bool EndsWith(this StringBuilder SB, char Value)
	{
		return SB[SB.Length - 1] == Value;
	}

	public static void Set<K, V>(this Dictionary<K, V> dictionary, K key, V value)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
		}
		else
		{
			dictionary.Add(key, value);
		}
	}

	public static bool in10(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static void AddIf<T>(this List<T> list, T element, Func<T, bool> conditional)
	{
		if (conditional(element))
		{
			list.Add(element);
		}
	}

	public static void AddIfNot<T>(this List<T> list, T element, Func<T, bool> conditional)
	{
		if (!conditional(element))
		{
			list.Add(element);
		}
	}

	public static void AddIfNotNull<T>(this List<T> list, T element)
	{
		if (element != null)
		{
			list.Add(element);
		}
	}

	public static bool ShowSuccess(this XRL.World.GameObject Object, string Message, bool Log = false)
	{
		if (Object != null && Object.IsPlayer())
		{
			Popup.Show(Message, CopyScrap: true, Capitalize: true, DimBackground: true, Log);
		}
		return true;
	}

	public static bool ShowFailure(this XRL.World.GameObject Object, string Message, bool Log = false)
	{
		if (Object != null && Object.IsPlayer())
		{
			Popup.Show(Message, CopyScrap: true, Capitalize: true, DimBackground: true, Log);
		}
		return false;
	}

	public static List<T> ShuffleInPlace<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			Algorithms.RandomShuffleInPlace(list);
		}
		else
		{
			Algorithms.RandomShuffleInPlace(list, R);
		}
		return list;
	}

	public static IEnumerable<int> ShuffledRange(int low, int high, System.Random rng = null)
	{
		if (rng == null)
		{
			rng = Stat.Rnd;
		}
		int count = high - low;
		int num = count + 3;
		if (num % 2 == 0)
		{
			num++;
		}
		bool flag;
		do
		{
			num += 2;
			flag = true;
			int num2 = (int)Math.Sqrt(num) + 1;
			int num3 = 3;
			while (flag && num3 <= num2)
			{
				flag &= num % num3 != 0;
				num3 += 2;
			}
		}
		while (!flag);
		long a = rng.Next(2, num);
		long pl = num;
		int i = 0;
		int found = 0;
		while (found < count)
		{
			i++;
			int num4 = (int)(i * a % pl);
			if (num4 < count)
			{
				found++;
				yield return num4 + low;
			}
		}
	}

	public static IEnumerable<T> InRandomOrderNoAlloc<T>(this List<T> list, System.Random rng = null)
	{
		if (rng == null)
		{
			rng = Stat.Rnd;
		}
		foreach (int item in ShuffledRange(0, list.Count - 1, rng))
		{
			yield return list[item];
		}
	}

	public static IEnumerable<T> InRandomOrder<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			R = Stat.Rnd;
		}
		foreach (int item in Enumerable.Range(0, list.Count).ToList().Shuffle(R))
		{
			yield return list[item];
		}
	}

	public static List<T> Shuffle<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			return new List<T>(Algorithms.RandomShuffle(list));
		}
		return new List<T>(Algorithms.RandomShuffle(list, R));
	}

	public static double toRadians(this double angle)
	{
		return Math.PI * angle / 180.0;
	}

	public static float toRadians(this float angle)
	{
		return (float)((double)angle).toRadians();
	}

	public static float toRadians(this int angle)
	{
		return (float)((double)angle).toRadians();
	}

	public static double toDegrees(this double angle)
	{
		return angle * (180.0 / Math.PI);
	}

	public static float toDegrees(this float angle)
	{
		return (float)((double)angle).toDegrees();
	}

	public static double normalizeRadians(this double angle)
	{
		return angle - Math.PI * 2.0 * Math.Floor((angle + Math.PI) / (Math.PI * 2.0));
	}

	public static float normalizeRadians(this float angle)
	{
		return (float)((double)angle).normalizeRadians();
	}

	public static int normalizeDegrees(this int angle)
	{
		return angle % 360;
	}

	public static string DebugNode(this XmlTextReader Reader)
	{
		if (Reader.NodeType == XmlNodeType.Element || Reader.NodeType == XmlNodeType.EndElement)
		{
			string text = Reader.LineNumber + ":" + Reader.LinePosition + ((Reader.NodeType == XmlNodeType.EndElement && !Reader.IsEmptyElement) ? "</" : "<") + Reader.Name;
			bool isEmptyElement = Reader.IsEmptyElement;
			if (Reader.HasAttributes)
			{
				while (Reader.MoveToNextAttribute())
				{
					text = text + " " + Reader.Name + "=\"" + Reader.Value + "\"";
				}
			}
			return text + (isEmptyElement ? " />" : ">");
		}
		return "[" + Reader.NodeType.ToString() + "]";
	}

	public static double DiminishingReturns(this double val, double scale)
	{
		if (val < 0.0)
		{
			return 0.0 - (0.0 - val).DiminishingReturns(scale);
		}
		double num = val / scale;
		return (Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale;
	}

	public static float DiminishingReturns(this float val, double scale)
	{
		if (val < 0f)
		{
			return 0f - (0f - val).DiminishingReturns(scale);
		}
		double num = (double)val / scale;
		return (float)((Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale);
	}

	public static int DiminishingReturns(this int val, double scale)
	{
		if (val < 0)
		{
			return -(-val).DiminishingReturns(scale);
		}
		double num = (double)val / scale;
		return (int)((Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale);
	}

	public static bool EqualsNoCase(this string val, string cmp)
	{
		return string.Equals(val, cmp, StringComparison.CurrentCultureIgnoreCase);
	}

	public static int GetStableHashCode(this string Value, int Hash = 0)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			Hash = Value[i] + (Hash << 6) + (Hash << 16) - Hash;
		}
		return Hash;
	}

	public static bool EqualsEmptyEqualsNull(this string val, string cmp)
	{
		if (val == cmp)
		{
			return true;
		}
		if (string.IsNullOrEmpty(val) && string.IsNullOrEmpty(cmp))
		{
			return true;
		}
		return false;
	}

	public static T Deserialize<T>(this JsonSerializer serializer, string file)
	{
		using StreamReader reader = new StreamReader(file);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return serializer.Deserialize<T>(reader2);
	}

	public static T Decompress<T>(this JsonSerializer serializer, string file)
	{
		using FileStream stream = new FileStream(file, FileMode.Open);
		using GZipStream stream2 = new GZipStream(stream, CompressionLevel.Fastest);
		using StreamReader reader = new StreamReader(stream2);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return serializer.Deserialize<T>(reader2);
	}

	public static void Serialize(this JsonSerializer serializer, string file, object value)
	{
		using StreamWriter textWriter = new StreamWriter(file);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		serializer.Serialize(jsonWriter, value);
	}

	public static void Compress(this JsonSerializer serializer, string file, object value)
	{
		using FileStream stream = new FileStream(file, FileMode.Create);
		using GZipStream stream2 = new GZipStream(stream, CompressionLevel.Fastest);
		using StreamWriter textWriter = new StreamWriter(stream2);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		serializer.Serialize(jsonWriter, value);
	}

	public static void Populate(this JsonSerializer serializer, string file, object target)
	{
		using StreamReader reader = new StreamReader(file);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		serializer.Populate(reader2, target);
	}
}

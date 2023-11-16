using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XRL;
using XRL.UI;
using XRL.World;

namespace ConsoleLib.Console;

[HasModSensitiveStaticCache]
public static class ColorUtility
{
	public static readonly string DEFAULT_FOREGROUND = "default foreground";

	public static readonly string DEFAULT_BACKGROUND = "default background";

	public static readonly string DEFAULT_DETAIL = "default detail";

	public static Dictionary<char, ushort> CharToColorMap;

	public static Dictionary<ushort, char> ColorAttributeToCharMap;

	public static Dictionary<Color, char> ColorToCharMap;

	public static Dictionary<char, Color> _ColorMap = null;

	public static Dictionary<string, Color> ColorAliasMap;

	public static Color[] usfColorMap;

	private static Dictionary<string, string> stripCache = new Dictionary<string, string>();

	private static Dictionary<char, int> ForegroundColorCounts = new Dictionary<char, int>();

	private static StringBuilder ClipSB = new StringBuilder(256);

	private static StringBuilder EscSB = new StringBuilder(256);

	private static Dictionary<string, List<string>> CachedForegroundExpansions = new Dictionary<string, List<string>>(16);

	private static string LastForegroundExpansionRequest;

	private static List<string> LastForegroundExpansionResult;

	private static Dictionary<string, List<string>> CachedBackgroundExpansions = new Dictionary<string, List<string>>(16);

	private static string LastBackgroundExpansionRequest;

	private static List<string> LastBackgroundExpansionResult;

	public static Dictionary<char, Color> ColorMap
	{
		get
		{
			if (_ColorMap == null)
			{
				Init();
			}
			return _ColorMap;
		}
		set
		{
			_ColorMap = value;
		}
	}

	public static Color ColorFromString(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return ColorMap['y'];
		}
		return ColorMap[str[0]];
	}

	public static Color colorFromTextColor(TextColor c)
	{
		return usfColorMap[(uint)c];
	}

	public static Color colorFromChar(char c)
	{
		return ColorMap[c];
	}

	public static string StripBackgroundFormatting(string s)
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			if (s[i] != '^')
			{
				stringBuilder.Append(s[i]);
			}
			else if (s[i] == '^')
			{
				i++;
				if (s[i] == '^')
				{
					stringBuilder.Append('^');
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static StringBuilder StripFormatting(StringBuilder s)
	{
		Markup.Transform(s);
		int i = 0;
		for (int num = s.Length - 1; i < num; i++)
		{
			if (s[i] == '^')
			{
				if (s[i + 1] == '^')
				{
					i++;
					continue;
				}
				s.Remove(i, 2);
				i--;
			}
			else if (s[i] == '&')
			{
				if (s[i + 1] == '&')
				{
					i++;
					continue;
				}
				s.Remove(i, 2);
				i--;
			}
		}
		return s;
	}

	public static string StripFormatting(string s)
	{
		if (s == null)
		{
			return "";
		}
		lock (stripCache)
		{
			if (stripCache.ContainsKey(s))
			{
				return stripCache[s];
			}
		}
		s = Markup.Transform(s);
		int length = s.Length;
		int num = s.IndexOf('&');
		int num2 = s.IndexOf('^');
		if (num != -1)
		{
			length = ((num2 != -1) ? Math.Min(num2, num) : num);
		}
		else
		{
			if (num2 == -1)
			{
				return s;
			}
			length = num2;
		}
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		if (length > 0)
		{
			stringBuilder.Append(s, 0, length);
		}
		int i = length;
		for (int length2 = s.Length; i < length2; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= length2 || s[i] == '&')
				{
					stringBuilder.Append("&&");
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= length2 || s[i] == '^')
				{
					stringBuilder.Append("^^");
				}
			}
			else
			{
				stringBuilder.Append(s[i]);
			}
		}
		string text = stringBuilder.ToString();
		lock (stripCache)
		{
			if (stripCache.Count > 50)
			{
				stripCache.Clear();
			}
			if (!stripCache.ContainsKey(s))
			{
				stripCache.Add(s, text);
				return text;
			}
			return text;
		}
	}

	public static string GetMainForegroundColor(string s)
	{
		if (s == null)
		{
			return null;
		}
		s = Markup.Transform(s);
		ForegroundColorCounts.Clear();
		char c = 'y';
		int value = 0;
		char c2 = c;
		int num = 0;
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			bool flag = false;
			if (s[i] == '&')
			{
				i++;
				if (i < length)
				{
					if (s[i] == '&')
					{
						flag = true;
					}
					else if (s[i] != c)
					{
						ForegroundColorCounts[c] = value;
						c = s[i];
						ForegroundColorCounts.TryGetValue(c, out value);
					}
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i < length && s[i] == '^')
				{
					flag = true;
				}
			}
			else if (s[i] != ' ')
			{
				flag = true;
			}
			if (flag)
			{
				value++;
				if (c == c2)
				{
					num++;
				}
				else if (value > num)
				{
					c2 = c;
					num = value;
				}
			}
		}
		return c2.ToString() ?? "";
	}

	public static string GetMainForegroundColor(StringBuilder s)
	{
		return GetMainForegroundColor(s.ToString());
	}

	public static bool HasFormatting(string s)
	{
		if (s.Contains("{{"))
		{
			return true;
		}
		int i = 0;
		for (int num = s.Length - 1; i < num; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i < s.Length && s[i] != '&')
				{
					return true;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i < s.Length && s[i] != '^')
				{
					return true;
				}
			}
		}
		return false;
	}

	public static char FirstCharacterExceptFormatting(string s)
	{
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				i++;
				if (i < length && s[i] != '&')
				{
					continue;
				}
				return '&';
			}
			if (s[i] == '^')
			{
				i++;
				if (i < length && s[i] != '^')
				{
					continue;
				}
				return '^';
			}
			return s[i];
		}
		return '\0';
	}

	public static int LengthExceptFormatting(string s)
	{
		int num = 0;
		bool flag = false;
		int num2 = 0;
		int i = 0;
		int length = s.Length;
		int num3 = length - 1;
		for (; i < length; i++)
		{
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num3)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					flag = true;
					num2++;
					i++;
					continue;
				}
				if (num2 > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					num2--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				i++;
				if (i >= length || s[i] == '&')
				{
					num++;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= length || s[i] == '^')
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		return num;
	}

	public static int LengthExceptFormatting(StringBuilder s)
	{
		int num = 0;
		bool flag = false;
		int num2 = 0;
		int i = 0;
		int length = s.Length;
		int num3 = length - 1;
		for (; i < length; i++)
		{
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num3)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					flag = true;
					num2++;
					i++;
					continue;
				}
				if (num2 > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					num2--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				i++;
				if (i >= length || s[i] == '&')
				{
					num++;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= length || s[i] == '^')
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		return num;
	}

	public static string ClipExceptFormatting(string s, int want)
	{
		ClipSB.Clear();
		int num = 0;
		bool flag = false;
		int num2 = 0;
		int i = 0;
		int length = s.Length;
		int num3 = length - 1;
		for (; i < length; i++)
		{
			ClipSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num3)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					ClipSB.Append('{');
					flag = true;
					num2++;
					i++;
					continue;
				}
				if (num2 > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					ClipSB.Append('}');
					num2--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if ((i >= length || s[i] == '&') && ++num >= want)
				{
					break;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if ((i >= length || s[i] == '^') && ++num >= want)
				{
					break;
				}
			}
			else if (++num >= want)
			{
				break;
			}
		}
		for (int j = 0; j < num2; j++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipExceptFormatting(StringBuilder s, int want)
	{
		ClipSB.Clear();
		int num = 0;
		bool flag = false;
		int num2 = 0;
		int i = 0;
		int length = s.Length;
		int num3 = length - 1;
		for (; i < length; i++)
		{
			ClipSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num3)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					ClipSB.Append('{');
					flag = true;
					num2++;
					i++;
					continue;
				}
				if (num2 > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					ClipSB.Append('}');
					num2--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if ((i >= length || s[i] == '&') && ++num >= want)
				{
					break;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if ((i >= length || s[i] == '^') && ++num >= want)
				{
					break;
				}
			}
			else if (++num >= want)
			{
				break;
			}
		}
		for (int j = 0; j < num2; j++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipToFirstExceptFormatting(string s, char ch)
	{
		ClipSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			if (flag)
			{
				ClipSB.Append(s[i]);
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					ClipSB.Append("{{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					ClipSB.Append("}}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				ClipSB.Append('&');
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if (ch == '&' && s[i] == '&')
				{
					break;
				}
			}
			else if (s[i] == '^')
			{
				ClipSB.Append('^');
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if (ch == '^' && s[i] == '^')
				{
					break;
				}
			}
			else
			{
				if (s[i] == ch)
				{
					break;
				}
				ClipSB.Append(s[i]);
			}
		}
		for (int j = 0; j < num; j++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipToFirstExceptFormatting(StringBuilder s, char ch)
	{
		ClipSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			if (flag)
			{
				ClipSB.Append(s[i]);
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					ClipSB.Append("{{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					ClipSB.Append("}}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				ClipSB.Append('&');
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if (ch == '&' && s[i] == '&')
				{
					break;
				}
			}
			else if (s[i] == '^')
			{
				ClipSB.Append('^');
				i++;
				if (i < length)
				{
					ClipSB.Append(s[i]);
				}
				if (ch == '^' && s[i] == '^')
				{
					break;
				}
			}
			else
			{
				if (s[i] == ch)
				{
					break;
				}
				ClipSB.Append(s[i]);
			}
		}
		for (int j = 0; j < num; j++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string EscapeFormatting(string s)
	{
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append("\\{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append("\\}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeFormatting(StringBuilder s)
	{
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append("\\{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append("\\}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeNonMarkupFormatting(string s)
	{
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append('{');
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append('}');
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeNonMarkupFormatting(StringBuilder s)
	{
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append('{');
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append('}');
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string ToUpperExceptFormatting(string s)
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= length || s[i] == '&')
				{
					stringBuilder.Append('&');
				}
				else
				{
					stringBuilder.Append('&').Append(s[i]);
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= length || s[i] == '^')
				{
					stringBuilder.Append('^');
				}
				else
				{
					stringBuilder.Append('^').Append(s[i]);
				}
			}
			else
			{
				stringBuilder.Append(char.ToUpper(s[i]));
			}
		}
		return stringBuilder.ToString();
	}

	public static string ToLowerExceptFormatting(string s)
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			if (s[i] == '^')
			{
				i++;
				if (s[i] == '^')
				{
					stringBuilder.Append('^');
				}
				else
				{
					stringBuilder.Append('^').Append(s[i]);
				}
			}
			else if (s[i] == '&')
			{
				i++;
				if (s[i] == '&')
				{
					stringBuilder.Append('&');
				}
				else
				{
					stringBuilder.Append('&').Append(s[i]);
				}
			}
			else
			{
				stringBuilder.Append(char.ToLower(s[i]));
			}
		}
		return stringBuilder.ToString();
	}

	public static string ReplaceExceptFormatting(string s, char search, char replace)
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= length || s[i] == '&')
				{
					stringBuilder.Append('&');
				}
				else
				{
					stringBuilder.Append('&').Append(s[i]);
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= length || s[i] == '^')
				{
					stringBuilder.Append('^');
				}
				else
				{
					stringBuilder.Append('^').Append(s[i]);
				}
			}
			else if (s[i] == search)
			{
				stringBuilder.Append(replace);
			}
			else
			{
				stringBuilder.Append(s[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static string CapitalizeExceptFormatting(string s)
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		bool flag = true;
		bool flag2 = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			if (flag2)
			{
				if (s[i] == '|')
				{
					flag2 = false;
				}
				stringBuilder.Append(s[i]);
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					flag2 = true;
					num++;
					i++;
					stringBuilder.Append("{{");
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					num--;
					i++;
					stringBuilder.Append("}}");
					continue;
				}
			}
			if (s[i] == '&')
			{
				stringBuilder.Append('&');
				i++;
				if (i < length)
				{
					stringBuilder.Append(s[i]);
				}
			}
			else if (s[i] == '^')
			{
				stringBuilder.Append('^');
				i++;
				if (i < length)
				{
					stringBuilder.Append(s[i]);
				}
			}
			else if (flag)
			{
				stringBuilder.Append(char.ToUpper(s[i]));
				flag = false;
			}
			else
			{
				stringBuilder.Append(s[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static void SortExceptFormatting(List<string> list)
	{
		list.Sort(CompareExceptFormatting);
	}

	public static int CompareExceptFormatting(string a, string b)
	{
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		int num2 = 0;
		int length = a.Length;
		int num3 = a.Length - 1;
		int length2 = b.Length;
		int num4 = b.Length - 1;
		int num5 = 0;
		int num6 = 0;
		int num7 = num5;
		int num8 = num6;
		int num9 = 0;
		while (num5 < length || num6 < length2)
		{
			if (num5 >= length)
			{
				return 1;
			}
			if (num6 >= length2)
			{
				return -1;
			}
			if (num7 != num5 || num8 != num6)
			{
				num9 = 0;
				num7 = num5;
				num8 = num6;
			}
			else if (++num9 > 10)
			{
				MetricsManager.LogError("hit limiter at " + num5 + " of " + length + " and " + num6 + " of " + length2 + " in " + a + " and " + b);
				return 0;
			}
			if (flag)
			{
				if (a[num5] == '|')
				{
					flag = false;
				}
				num5++;
				continue;
			}
			if (num5 < num3)
			{
				if (a[num5] == '{' && a[num5 + 1] == '{')
				{
					flag = true;
					num++;
					num5++;
					num5++;
					continue;
				}
				if (num > 0 && a[num5] == '}' && a[num5 + 1] == '}')
				{
					num--;
					num5++;
					num5++;
					continue;
				}
			}
			if (a[num5] == '&')
			{
				num5++;
				if (num5 < length && a[num5] != '&')
				{
					num5++;
					continue;
				}
			}
			else if (a[num5] == '^')
			{
				num5++;
				if (num5 < length && a[num5] != '^')
				{
					num5++;
					continue;
				}
			}
			if (flag2)
			{
				if (b[num6] == '|')
				{
					flag2 = false;
				}
				num6++;
				continue;
			}
			if (num6 < num4)
			{
				if (b[num6] == '{' && b[num6 + 1] == '{')
				{
					flag2 = true;
					num2++;
					num6++;
					num6++;
					continue;
				}
				if (num2 > 0 && b[num6] == '}' && b[num6 + 1] == '}')
				{
					num2--;
					num6++;
					num6++;
					continue;
				}
			}
			if (b[num6] == '&')
			{
				num6++;
				if (num6 < length2 && b[num6] != '&')
				{
					num6++;
					continue;
				}
			}
			else if (b[num6] == '^')
			{
				num6++;
				if (num6 < length2 && b[num6] != '^')
				{
					num6++;
					continue;
				}
			}
			int num10 = a[num5].CompareTo(b[num6]);
			if (num10 != 0)
			{
				return num10;
			}
			num5++;
			num6++;
		}
		return 0;
	}

	public static void SortExceptFormattingAndCase(List<string> list)
	{
		list.Sort(CompareExceptFormattingAndCase);
	}

	public static int CompareExceptFormattingAndCase(string a, string b)
	{
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		int num2 = 0;
		int length = a.Length;
		int num3 = a.Length - 1;
		int length2 = b.Length;
		int num4 = b.Length - 1;
		int num5 = 0;
		int num6 = 0;
		int num7 = num5;
		int num8 = num6;
		int num9 = 0;
		while (num5 < length || num6 < length2)
		{
			if (num5 >= length)
			{
				return 1;
			}
			if (num6 >= length2)
			{
				return -1;
			}
			if (num7 != num5 || num8 != num6)
			{
				num9 = 0;
				num7 = num5;
				num8 = num6;
			}
			else if (++num9 > 10)
			{
				MetricsManager.LogError("hit limiter at " + num5 + " of " + length + " and " + num6 + " of " + length2 + " in " + a + " and " + b);
				return 0;
			}
			if (flag)
			{
				if (a[num5] == '|')
				{
					flag = false;
				}
				num5++;
				continue;
			}
			if (num5 < num3)
			{
				if (a[num5] == '{' && a[num5 + 1] == '{')
				{
					flag = true;
					num++;
					num5++;
					num5++;
					continue;
				}
				if (num > 0 && a[num5] == '}' && a[num5 + 1] == '}')
				{
					num--;
					num5++;
					num5++;
					continue;
				}
			}
			if (a[num5] == '&')
			{
				num5++;
				if (num5 < length && a[num5] != '&')
				{
					num5++;
					continue;
				}
			}
			else if (a[num5] == '^')
			{
				num5++;
				if (num5 < length && a[num5] != '^')
				{
					num5++;
					continue;
				}
			}
			if (flag2)
			{
				if (b[num6] == '|')
				{
					flag2 = false;
				}
				num6++;
				continue;
			}
			if (num6 < num4)
			{
				if (b[num6] == '{' && b[num6 + 1] == '{')
				{
					flag2 = true;
					num2++;
					num6++;
					num6++;
					continue;
				}
				if (num2 > 0 && b[num6] == '}' && b[num6 + 1] == '}')
				{
					num2--;
					num6++;
					num6++;
					continue;
				}
			}
			if (b[num6] == '&')
			{
				num6++;
				if (num6 < length2 && b[num6] != '&')
				{
					num6++;
					continue;
				}
			}
			else if (b[num6] == '^')
			{
				num6++;
				if (num6 < length2 && b[num6] != '^')
				{
					num6++;
					continue;
				}
			}
			char c = a[num5];
			char c2 = b[num6];
			if (char.IsLetter(c) && char.IsLetter(c2))
			{
				c = char.ToUpper(c);
				c2 = char.ToUpper(c2);
			}
			int num10 = c.CompareTo(c2);
			if (num10 != 0)
			{
				return num10;
			}
			num5++;
			num6++;
		}
		return 0;
	}

	public static bool HasUpperExceptFormatting(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '^' || s[i] == '&')
			{
				i++;
			}
			else if (char.IsUpper(s[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasLowerExceptFormatting(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '^' || s[i] == '&')
			{
				i++;
			}
			else if (char.IsUpper(s[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAllUpperExceptFormatting(string s)
	{
		bool result = false;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '^' || s[i] == '&')
			{
				i++;
			}
			else if (char.IsUpper(s[i]))
			{
				result = true;
			}
			else if (char.IsLower(s[i]))
			{
				return false;
			}
		}
		return result;
	}

	public static bool IsAllLowerExceptFormatting(string s)
	{
		bool result = false;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '^' || s[i] == '&')
			{
				i++;
			}
			else if (char.IsLower(s[i]))
			{
				result = true;
			}
			else if (char.IsUpper(s[i]))
			{
				return false;
			}
		}
		return result;
	}

	public static bool IsFirstUpperExceptFormatting(string s)
	{
		int num = 0;
		while (num < s.Length)
		{
			if (s[num] == '^' || s[num] == '&')
			{
				num++;
				num++;
				continue;
			}
			return char.IsUpper(s[num]);
		}
		return false;
	}

	public static bool IsFirstLowerExceptFormatting(string s)
	{
		int num = 0;
		while (num < s.Length)
		{
			if (s[num] == '^' || s[num] == '&')
			{
				num++;
				num++;
				continue;
			}
			return char.IsLower(s[num]);
		}
		return false;
	}

	public static void ComponentsFromAttributeColor(ushort color, out float r, out float g, out float b)
	{
		r = 0f;
		g = 0f;
		b = 0f;
		switch (color)
		{
		case 1:
			b = 128f;
			break;
		case 2:
			g = 128f;
			break;
		case 3:
			b = 128f;
			g = 128f;
			break;
		case 4:
			r = 128f;
			break;
		case 5:
			r = 128f;
			b = 128f;
			break;
		case 6:
			g = 128f;
			r = 128f;
			break;
		case 7:
			r = 192f;
			g = 192f;
			b = 192f;
			break;
		case 8:
			r = 128f;
			g = 128f;
			b = 128f;
			break;
		case 9:
			b = 255f;
			break;
		case 10:
			g = 255f;
			break;
		case 11:
			b = 255f;
			g = 255f;
			break;
		case 12:
			r = 255f;
			break;
		case 13:
			r = 255f;
			b = 255f;
			break;
		case 14:
			g = 255f;
			r = 255f;
			break;
		case 15:
			r = 255f;
			g = 255f;
			b = 255f;
			break;
		}
	}

	public static void ForegroundFromAttribute(ushort A, out float r, out float g, out float b)
	{
		ComponentsFromAttributeColor(GetForeground(A), out r, out g, out b);
	}

	public static void BackgroundFromAttribute(ushort A, out float r, out float g, out float b)
	{
		ComponentsFromAttributeColor(GetBackground(A), out r, out g, out b);
	}

	public static Color FromWebColor(string s)
	{
		float num = (int)byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
		float num2 = (int)byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
		return new Color(b: (float)(int)byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber) / 255f, r: num / 255f, g: num2 / 255f);
	}

	[ModSensitiveCacheInit]
	public static void CheckInit()
	{
		if (ColorMap == null)
		{
			Loading.LoadTask("Loading Display.txt", Init);
		}
	}

	private static List<string> GetPaths()
	{
		List<string> paths = new List<string> { DataManager.FilePath("Display.txt") };
		ModManager.ForEachFile("Display.txt", delegate(string path)
		{
			paths.Add(path);
		});
		string text = DataManager.SavePath("Display.txt");
		if (File.Exists(text))
		{
			paths.Add(text);
		}
		return paths;
	}

	private static void LoadDisplaySettings(string Path, GameManager Manager, UnityEngine.GameObject MainCamera)
	{
		using StreamReader streamReader = new StreamReader(Path);
		JSONClass jSONClass = JSON.Parse(streamReader.ReadToEnd()) as JSONClass;
		if (jSONClass["colors"] is JSONClass jSONClass2)
		{
			foreach (KeyValuePair<string, JSONNode> item in jSONClass2)
			{
				Color color = FromWebColor(item.Value);
				ColorMap.Add(item.Key[0], color);
				ColorToCharMap.Add(color, item.Key[0]);
			}
		}
		if (!MainCamera)
		{
			MetricsManager.LogError("Main camera not found, skipping display settings from " + Path + ".");
			return;
		}
		if (jSONClass["camera"] is JSONClass jSONClass3 && jSONClass3["background"] != null)
		{
			MainCamera.GetComponent<Camera>().backgroundColor = FromWebColor(jSONClass3["background"]);
		}
		if (jSONClass["tiles"] is JSONClass jSONClass4)
		{
			if (jSONClass4["width"] != null)
			{
				Manager.tileWidth = Convert.ToInt32(jSONClass4["width"]);
				LetterboxCamera component = MainCamera.GetComponent<LetterboxCamera>();
				component.DesiredWidth = Manager.tileWidth * 80;
				component.Refresh();
			}
			if (jSONClass4["height"] != null)
			{
				Manager.tileHeight = Convert.ToInt32(jSONClass4["height"]);
			}
		}
		if (!(jSONClass["shaders"] is JSONClass jSONClass5))
		{
			return;
		}
		if (jSONClass5["scanlines"] != null)
		{
			CC_AnalogTV component2 = MainCamera.GetComponent<CC_AnalogTV>();
			if (jSONClass5["scanlines"]["enable"] != null)
			{
				if (jSONClass5["scanlines"]["enable"].Value.EqualsNoCase("true"))
				{
					component2.enabled = true;
				}
				else
				{
					component2.enabled = false;
				}
			}
			if (jSONClass5["scanlines"]["greyscale"] != null)
			{
				component2.grayscale = jSONClass5["scanlines"]["greyscale"].Value.EqualsNoCase("true");
			}
			if (jSONClass5["scanlines"]["noise"] != null)
			{
				component2.noiseIntensity = Convert.ToSingle(jSONClass5["scanlines"]["noise"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["intensity"] != null)
			{
				component2.scanlinesIntensity = Convert.ToSingle(jSONClass5["scanlines"]["intensity"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["count"] != null)
			{
				component2.scanlinesCount = Convert.ToSingle(jSONClass5["scanlines"]["count"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["offset"] != null)
			{
				component2.scanlinesOffset = Convert.ToSingle(jSONClass5["scanlines"]["offset"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["distortion"] != null)
			{
				component2.distortion = Convert.ToSingle(jSONClass5["scanlines"]["distortion"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["cubicdistortion"] != null)
			{
				component2.cubicDistortion = Convert.ToSingle(jSONClass5["scanlines"]["cubicdistortion"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["zoom"] != null)
			{
				component2.scale = Convert.ToSingle(jSONClass5["scanlines"]["zoom"].Value, CultureInfo.InvariantCulture);
			}
		}
		if (jSONClass5["vignette"] != null)
		{
			CC_FastVignette component3 = MainCamera.GetComponent<CC_FastVignette>();
			if (jSONClass5["vignette"]["enable"] != null)
			{
				if (jSONClass5["vignette"]["enable"].Value.EqualsNoCase("true"))
				{
					component3.enabled = true;
				}
				else
				{
					component3.enabled = false;
				}
			}
			if (jSONClass5["vignette"]["sharpness"] != null)
			{
				component3.sharpness = Convert.ToSingle(jSONClass5["vignette"]["sharpness"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["vignette"]["darkness"] != null)
			{
				component3.darkness = Convert.ToSingle(jSONClass5["vignette"]["darkness"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["vignette"]["desaturate"] != null)
			{
				component3.desaturate = jSONClass5["vignette"]["desaturate"].Value.EqualsNoCase("true");
			}
		}
		if (!(jSONClass5["settings"] != null))
		{
			return;
		}
		CC_BrightnessContrastGamma component4 = MainCamera.GetComponent<CC_BrightnessContrastGamma>();
		if (jSONClass5["settings"]["enable"] != null)
		{
			if (jSONClass5["settings"]["enable"].Value.EqualsNoCase("true"))
			{
				component4.enabled = true;
			}
			else
			{
				component4.enabled = false;
			}
		}
		if (jSONClass5["settings"]["brightness"] != null)
		{
			component4.brightness = Convert.ToSingle(jSONClass5["settings"]["brightness"].Value, CultureInfo.InvariantCulture);
		}
		if (jSONClass5["settings"]["contrast"] != null)
		{
			component4.contrast = Convert.ToSingle(jSONClass5["settings"]["contrast"].Value, CultureInfo.InvariantCulture);
		}
		if (jSONClass5["settings"]["gamma"] != null)
		{
			component4.gamma = Convert.ToSingle(jSONClass5["settings"]["gamma"].Value, CultureInfo.InvariantCulture);
		}
	}

	private static void Init()
	{
		ColorMap = new Dictionary<char, Color>();
		ColorToCharMap = new Dictionary<Color, char>();
		CharToColorMap = new Dictionary<char, ushort>();
		ColorAttributeToCharMap = new Dictionary<ushort, char>();
		ColorAliasMap = new Dictionary<string, Color>();
		usfColorMap = new Color[32];
		GameManager component = UnityEngine.GameObject.Find("GameManager").GetComponent<GameManager>();
		UnityEngine.GameObject mainCamera = UnityEngine.GameObject.Find("Main Camera");
		foreach (string path in GetPaths())
		{
			try
			{
				LoadDisplaySettings(path, component, mainCamera);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error loading " + path, x);
			}
		}
		usfColorMap[0] = ColorMap['k'];
		usfColorMap[1] = ColorMap['b'];
		usfColorMap[4] = ColorMap['r'];
		usfColorMap[2] = ColorMap['g'];
		usfColorMap[3] = ColorMap['c'];
		usfColorMap[5] = ColorMap['m'];
		usfColorMap[6] = ColorMap['w'];
		usfColorMap[7] = ColorMap['y'];
		usfColorMap[8] = ColorMap['o'];
		usfColorMap[16] = ColorMap['K'];
		usfColorMap[17] = ColorMap['B'];
		usfColorMap[20] = ColorMap['R'];
		usfColorMap[18] = ColorMap['G'];
		usfColorMap[19] = ColorMap['C'];
		usfColorMap[21] = ColorMap['M'];
		usfColorMap[22] = ColorMap['W'];
		usfColorMap[23] = ColorMap['Y'];
		usfColorMap[24] = ColorMap['O'];
		CharToColorMap.Add('k', 0);
		CharToColorMap.Add('b', 1);
		CharToColorMap.Add('r', 4);
		CharToColorMap.Add('g', 2);
		CharToColorMap.Add('c', 3);
		CharToColorMap.Add('m', 5);
		CharToColorMap.Add('w', 6);
		CharToColorMap.Add('y', 7);
		CharToColorMap.Add('o', 8);
		CharToColorMap.Add('K', Bright((ushort)0));
		CharToColorMap.Add('B', Bright(1));
		CharToColorMap.Add('R', Bright(4));
		CharToColorMap.Add('G', Bright(2));
		CharToColorMap.Add('C', Bright(3));
		CharToColorMap.Add('M', Bright(5));
		CharToColorMap.Add('W', Bright(6));
		CharToColorMap.Add('Y', Bright(7));
		CharToColorMap.Add('O', Bright(8));
		ColorAttributeToCharMap.Add(0, 'k');
		ColorAttributeToCharMap.Add(1, 'b');
		ColorAttributeToCharMap.Add(4, 'r');
		ColorAttributeToCharMap.Add(2, 'g');
		ColorAttributeToCharMap.Add(3, 'c');
		ColorAttributeToCharMap.Add(5, 'm');
		ColorAttributeToCharMap.Add(6, 'w');
		ColorAttributeToCharMap.Add(7, 'y');
		ColorAttributeToCharMap.Add(8, 'o');
		ColorAttributeToCharMap.Add(Bright((ushort)0), 'K');
		ColorAttributeToCharMap.Add(Bright(1), 'B');
		ColorAttributeToCharMap.Add(Bright(4), 'R');
		ColorAttributeToCharMap.Add(Bright(2), 'G');
		ColorAttributeToCharMap.Add(Bright(3), 'C');
		ColorAttributeToCharMap.Add(Bright(5), 'M');
		ColorAttributeToCharMap.Add(Bright(6), 'W');
		ColorAttributeToCharMap.Add(Bright(7), 'Y');
		ColorAttributeToCharMap.Add(Bright(8), 'O');
		ColorAliasMap.Add("default background", ColorMap['k']);
		ColorAliasMap.Add("default foreground", ColorMap['y']);
		ColorAliasMap.Add("default detail", ColorMap['W']);
	}

	public static ushort MakeColor(ushort foreground, ushort background)
	{
		return (ushort)(foreground + (background << 5));
	}

	public static ushort MakeColor(TextColor foreground, TextColor background)
	{
		return (ushort)((uint)foreground + ((uint)background << 5));
	}

	public static ushort MakeColor(ushort foreground, TextColor background)
	{
		return (ushort)(foreground + ((uint)background << 5));
	}

	public static ushort MakeColor(TextColor foreground, ushort background)
	{
		return (ushort)((uint)foreground + (uint)(background << 5));
	}

	public static ushort Bright(ushort c)
	{
		return (ushort)(c | 0x10u);
	}

	public static ushort Bright(TextColor c)
	{
		return (ushort)(c | (TextColor)16);
	}

	public static ushort MakeBackgroundColor(ushort c)
	{
		return (ushort)(c << 5);
	}

	public static ushort MakeBackgroundColor(TextColor c)
	{
		return (ushort)((uint)c << 5);
	}

	public static char ParseForegroundColor(string str, char defaultColor = 'y')
	{
		if (string.IsNullOrEmpty(str))
		{
			return 'y';
		}
		char result = defaultColor;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '&')
			{
				i++;
				if (i < str.Length && str[i] != '&')
				{
					result = str[i];
				}
			}
		}
		return result;
	}

	public static char? FindLastForeground(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		char? result = null;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					result = text[i];
				}
			}
		}
		return result;
	}

	public static void FindLastForeground(string text, ref char? result)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					result = text[i];
				}
			}
		}
	}

	public static char? FindLastBackground(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		char? result = null;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					result = text[i];
				}
			}
		}
		return result;
	}

	public static void FindLastBackground(string text, ref char? result)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					result = text[i];
				}
			}
		}
	}

	public static void FindLastForegroundAndBackground(string text, ref char? foreground, ref char? background)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					foreground = text[i];
				}
			}
			else if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					background = text[i];
				}
			}
		}
	}

	public static ushort GetForeground(ushort c)
	{
		return (ushort)(c & 0x1Fu);
	}

	public static ushort GetBackground(ushort c)
	{
		return (ushort)(c >> 5);
	}

	public static List<string> CachedForegroundExpansion(string Text, char Separator = ',')
	{
		if (Text == null)
		{
			return null;
		}
		if (Text == LastForegroundExpansionRequest)
		{
			return LastForegroundExpansionResult;
		}
		if (!CachedForegroundExpansions.TryGetValue(Text, out var value))
		{
			string[] array = Text.Split(Separator);
			value = new List<string>(array.Length);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				value.Add("&" + array[i]);
			}
			CachedForegroundExpansions.Add(Text, value);
		}
		LastForegroundExpansionRequest = Text;
		return LastForegroundExpansionResult = value;
	}

	public static List<string> CachedBackgroundExpansion(string Text, char Separator = ',')
	{
		if (Text == null)
		{
			return null;
		}
		if (Text == LastBackgroundExpansionRequest)
		{
			return LastBackgroundExpansionResult;
		}
		if (!CachedBackgroundExpansions.TryGetValue(Text, out var value))
		{
			string[] array = Text.Split(Separator);
			value = new List<string>(array.Length);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				value.Add("^" + array[i]);
			}
			CachedBackgroundExpansions.Add(Text, value);
		}
		LastBackgroundExpansionRequest = Text;
		return LastBackgroundExpansionResult = value;
	}
}

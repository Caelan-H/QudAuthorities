using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using XRL.Language;
using XRL.Rules;
using XRL.World;

namespace XRL;

public static class Extensions
{
	public static string Things(this int num, string what, string whatPlural = null)
	{
		if (num == 1)
		{
			return "1 " + what;
		}
		return num + " " + (whatPlural ?? Grammar.Pluralize(what));
	}

	public static StringBuilder DumpStringBuilder(this List<GameObject> list, StringBuilder SB = null)
	{
		if (SB == null)
		{
			SB = new StringBuilder();
		}
		for (int i = 0; i < list.Count; i++)
		{
			SB.Append(i).Append(": ").Append(list[i].DebugName)
				.Append('\n');
		}
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			SB.Append("\n{{rules|").Append(text).Append("}}");
		}
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, string text, Action<StringBuilder> proc)
	{
		if (!string.IsNullOrEmpty(text))
		{
			SB.Append("\n{{rules|").Append(text);
			proc(SB);
			SB.Append("}}");
		}
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender)
	{
		SB.Append("\n{{rules|");
		appender(SB);
		SB.Append("}}");
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender, Action<StringBuilder> proc)
	{
		SB.Append("\n{{rules|");
		appender(SB);
		proc(SB);
		SB.Append("}}");
		return SB;
	}

	public static string Dump(this List<GameObject> list)
	{
		return list.DumpStringBuilder().ToString();
	}

	public static int Roll(this string Dice)
	{
		return Stat.Roll(Dice);
	}

	public static int RollMin(this string Dice)
	{
		return Stat.RollMin(Dice);
	}

	public static int RollMax(this string Dice)
	{
		return Stat.RollMax(Dice);
	}

	public static int RollCached(this string Dice)
	{
		return Stat.RollCached(Dice);
	}

	public static int RollMinCached(this string Dice)
	{
		return Stat.RollMinCached(Dice);
	}

	public static int RollMaxCached(this string Dice)
	{
		return Stat.RollMaxCached(Dice);
	}

	public static DieRoll GetCachedDieRoll(this string Dice)
	{
		return Stat.GetCachedDieRoll(Dice);
	}

	public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
	{
		return new SynchronizationContextAwaiter(context);
	}
}
